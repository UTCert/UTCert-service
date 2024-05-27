using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;

namespace UTCert.Data.Repository;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbContext context) : base(context)
    {
    }
    
    public async Task<List<RefreshToken>> GetRefreshTokens(Guid userId)
    {
        return await DbSet.Where(r => r.UserId == userId).ToListAsync();
    }

    public async Task<RefreshToken?> GetToken(string token)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Token == token);
    }
}