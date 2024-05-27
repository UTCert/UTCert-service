using UTCert.Model.Web.User;

namespace UTCert.Service.BusinessLogic.Interface;

public interface IUserService
{
    Task<string> Register(RegisterDto model);
    Task<UserResponseDto> Authenticate(string stakeId, string ipAddress);
    Task<UserResponseDto> RefreshToken(string token, string ipAddress);
    Task RevokeToken(string token, string ipAddress);
    IEnumerable<UserResponseDto> GetAll();
}