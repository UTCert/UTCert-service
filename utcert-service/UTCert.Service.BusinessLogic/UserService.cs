﻿using System.Security.Cryptography;
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
using UTCert.Service.Helper.Interface;
using UTCert.Service.Helper.JwtUtils;


namespace UTCert.Service.BusinessLogic;

public class UserService : EntityService<User>, IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly ICloudinaryService _cloudinaryService;

    public UserService(IJwtUtils jwtUtils,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinaryService)
        : base(unitOfWork, unitOfWork.UserRepository)
    {
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Guid> Register(RegisterDto model)
    {
        var imageUrl = "";
        try
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.StakeId == model.StakeId);

            if (user != null)
            {
                throw new AppException("User has been existed!");
            }

            var newUser = _mapper.Map<User>(model);
            newUser.Id = Guid.NewGuid();
            newUser.Role = Role.User;
            newUser.CreatedDate = DateTime.UtcNow;
            newUser.IsDeleted = false;
            newUser.IsVerified = false;

            if (model.AvatarUri != null && model.AvatarUri.Length > 0)
            {
                var res = await _cloudinaryService.UploadFromFile(model.AvatarUri, Constants.AvatarFolderName);
                newUser.AvatarUri = res;
                imageUrl = res;
            }

            await CreateAsync(newUser);
            return newUser.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if(!string.IsNullOrEmpty(imageUrl))
            {
                await _cloudinaryService.Delete(imageUrl);
            }
            throw;
        }
    }

    public async Task<UserResponseDto> Authenticate(string stakeId, string ipAddress)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.StakeId == stakeId);

            if (user == null)
                throw new AppException("User not found!");

            var jwtToken = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            refreshToken.Id = Guid.NewGuid();
            refreshToken.UserId = user.Id;

            // remove old refresh tokens
            await RemoveOldRefreshTokens(user.Id);

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<UserResponseDto>(user);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UserResponseDto> RefreshToken(string token, string ipAddress)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetToken(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new AppException("Invalid token");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                throw new AppException("User not found");
            }

            await RemoveOldRefreshTokens(user.Id);
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReasonRevoked = "Replaced by new token";
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);

            newRefreshToken.Id = Guid.NewGuid();
            newRefreshToken.UserId = user.Id;

            await _unitOfWork.RefreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<UserResponseDto>(user);
            response.JwtToken = _jwtUtils.GenerateJwtToken(user);
            response.RefreshToken = newRefreshToken.Token;

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> RevokeToken(string token, string ipAddress)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetToken(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new AppException("Invalid token");
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReasonRevoked = "Revoked without replacement";

            _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);
            return await _unitOfWork.CommitAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public IEnumerable<UserResponseDto> GetAll()
    {
        var users =  _unitOfWork.UserRepository.GetAll();
        return  _mapper.Map<IList<UserResponseDto>>(users);
    }

    public async Task<bool> HasAccount(string stakeId)
    {
        return await _unitOfWork.UserRepository.AnyAsync(x => x.StakeId == stakeId);
    }

    public async Task<UserResponseDto> GetById(Guid id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User does not exist or has been deleted");

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<bool> Update(Guid id, UserCrudDto input)
    {
        var _userRepos = _unitOfWork.UserRepository;
        var user = await _userRepos.FirstOrDefaultAsync(x => x.Id == id) ?? throw new AppException("User does not exist or has been deleted!");

        if (input.AvatarUri != null && input.AvatarUri.Length > 0)
        {
            if (!string.IsNullOrEmpty(user.AvatarUri))
            {
                await _cloudinaryService.Delete(user.AvatarUri);
            }
            var res = await _cloudinaryService.UploadFromFile(input.AvatarUri, Constants.AvatarFolderName);
            user.AvatarUri = res;
        }

        user.Name = input.Name;
        user.ModifiedDate = DateTime.Now;
        _userRepos.UpdateAsync(user);

        return await _unitOfWork.CommitAsync() > 0;
    }

    #region private Functions

    private async Task RemoveOldRefreshTokens(Guid userId)
    {
        var refreshTokens = await _unitOfWork.RefreshTokenRepository.GetRefreshTokens(userId);
        var oldRefreshTokens = refreshTokens.Where(x =>
            !x.IsActive &&
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow).ToList();

        if (oldRefreshTokens.Any())
        {
            _unitOfWork.RefreshTokenRepository.DeleteRange(oldRefreshTokens);
            await _unitOfWork.CommitAsync();
        }
    }

    #endregion
}