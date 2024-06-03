using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Common;

namespace UTCert.Data.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IDbContext context) : base(context)
    {
    }
    
    public async Task<User> GetUserByStakeId(string stakeId)
    {
        var user = await DbSet.FirstOrDefaultAsync(x => x.StakeId == stakeId);
        
        if (user == null)
        {
            throw new AppException("User not found!");
        }

        return user;
    }
    
    public async Task<User> GetUserById(Guid id)
    {
        var user = await DbSet.FirstOrDefaultAsync(x => x.Id == id);
        
        if (user == null)
        {
            throw new AppException("User not found!");
        }

        return user;
    }
}