using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Model.Database;

namespace UTCert.Data.Repository.Interface;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<List<RefreshToken>> GetRefreshTokens(Guid userId);

    Task<RefreshToken?> GetToken(string token);
}