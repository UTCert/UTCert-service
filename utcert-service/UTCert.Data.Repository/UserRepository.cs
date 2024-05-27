using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;

namespace UTCert.Data.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IDbContext context) : base(context)
    {
    }
}