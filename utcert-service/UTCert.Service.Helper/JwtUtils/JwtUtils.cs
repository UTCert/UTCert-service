using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Authorization;

namespace UTCert.Service.Helper.JwtUtils;

public class JwtUtils : IJwtUtils
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppSettings _appSettings;

    public JwtUtils(
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings)
    {
        _unitOfWork = unitOfWork;
        _appSettings = appSettings.Value;
    }

    public string GenerateJwtToken(User user)
    {
        // generate token that is valid for 15 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Guid ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clock skew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = new Guid(jwtToken.Claims.First(x => x.Type == "id").Value);

            // return account id from JWT token if validation successful
            return userId;
        }
        catch
        {
            // return Guid.Empty if validation fails
            return Guid.Empty;
        }
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            // token is a cryptographically strong random sequence of values
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            // token is valid for 7 days
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        // ensure token is unique by checking against db
        // var tokenIsUnique = !_context.Users.Any(a => a.RefreshTokens.Any(t => t.Token == refreshToken.Token));

        // var tokenIsUnique = await _unitOfWork.UserRepository
        //     .AnyAsync(a => a.RefreshTokens.Any(t => t.Token == refreshToken.Token));
        //
        //
        // if (!tokenIsUnique)
        //     return await GenerateRefreshToken(ipAddress);

        return refreshToken;
    }
}