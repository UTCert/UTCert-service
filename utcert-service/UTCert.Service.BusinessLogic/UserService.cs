using System.Security.Cryptography;
using AutoMapper;
using Microsoft.Extensions.Options;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Authorization;
using UTCert.Model.Shared.Common;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.User;
using UTCert.Service.BusinessLogic.Common;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.Helper.JwtUtils;


namespace UTCert.Service.BusinessLogic;

public class UserService : EntityService<User>, IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    
    public UserService(IJwtUtils jwtUtils,
                        IMapper mapper,
                        IOptions<AppSettings> appSettings, 
                        IUnitOfWork unitOfWork) 
                        : base(unitOfWork, unitOfWork.UserRepository)
    {
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Register(RegisterDto model)
    {
        var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.StakeId == model.StakeId);
        if (user != null)
        {
            throw new AppException("User has been existed!");
        }

        var newUser = _mapper.Map<User>(model);
        var token = await generateVerificationToken();

        newUser.Id =  Guid.NewGuid();
        newUser.Role = Role.User;
        newUser.CreatedDate = DateTime.UtcNow;
        newUser.IsDeleted = false;
        newUser.IsVerified = false;
        newUser.VerificationToken = token;
        
        await CreateAsync(newUser);
        return token;
    }

    public async Task<UserResponseDto> Authenticate(string stakeId, string ipAddress)
    {
        var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.StakeId == stakeId);

        if (user == null)
            throw new AppException("User not found!");

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        var refreshToken =  _jwtUtils.GenerateRefreshToken(ipAddress);
        refreshToken.Id = Guid.NewGuid();
        refreshToken.UserId = user.Id;
        
        await removeOldRefreshTokens(user.Id);
        await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.CommitAsync();

        var response = _mapper.Map<UserResponseDto>(user);
        response.JwtToken = jwtToken;
        response.RefreshToken = refreshToken.Token;
        return response;
    }

    public async Task<UserResponseDto> RefreshToken(string token, string ipAddress)
    {
        var refreshToken = await _unitOfWork.RefreshTokenRepository.GetToken(token);
        if (refreshToken != null || !refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        var user = await _unitOfWork.UserRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null)
            throw new AppException("User not found");
        
        if (refreshToken.IsRevoked)
        {
            await revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
        }

        var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
        await removeOldRefreshTokens(user.Id);
        
        await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.CommitAsync();
        
        var jwtToken = _jwtUtils.GenerateJwtToken(user);

        var response = _mapper.Map<UserResponseDto>(user);
        response.JwtToken = jwtToken;
        response.RefreshToken = newRefreshToken.Token;
        return response;
    }

    public async Task RevokeToken(string token, string ipAddress)
    {
        var refreshToken = await _unitOfWork.RefreshTokenRepository.GetToken(token);
        if (refreshToken != null || !refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");

        _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);
        await _unitOfWork.CommitAsync();
    }

    public IEnumerable<UserResponseDto> GetAll()
    {
        var users =  _unitOfWork.UserRepository.GetAll();
        return  _mapper.Map<IList<UserResponseDto>>(users);
    }

    #region private Functions

    private async Task<string> generateVerificationToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // TODO: it will be slow when the number of users becomes large
        var tokenIsUnique = await _unitOfWork.UserRepository.AnyAsync(x => x.VerificationToken == token);
        if (tokenIsUnique)
        {
            return await generateVerificationToken();
        }
        return token;
    }
    
    private async Task removeOldRefreshTokens(Guid userId)
    {
        var refreshTokens = await _unitOfWork.RefreshTokenRepository.GetRefreshTokens(userId);
        var oldRefreshTokens = refreshTokens.Where(x =>
            !x.IsActive &&
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow).ToList();

        _unitOfWork.RefreshTokenRepository.DeleteRange(oldRefreshTokens);
        await _unitOfWork.CommitAsync();
    }
    
    private async Task revokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            return;
        }
        
        var childToken = await _unitOfWork.RefreshTokenRepository
            .FirstOrDefaultAsync(x => x.Token == refreshToken.ReplacedByToken);
        if (childToken.IsActive)
        {
            revokeRefreshToken(childToken, ipAddress, reason);
        }
        else
        {
            await revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
        }
    }
    
    private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
    
    private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    #endregion
}