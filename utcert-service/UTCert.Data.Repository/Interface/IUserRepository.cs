using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Model.Database;

namespace UTCert.Data.Repository.Interface;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetUserByStakeId(string stakeId);

    Task<User> GetUserById(Guid id);
}