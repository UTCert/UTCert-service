using UTCert.Model.Database;

namespace UTCert.Service.Helper.JwtUtils;

public interface IJwtUtils
{
    public string GenerateJwtToken(User user);
    public Guid ValidateJwtToken(string token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}