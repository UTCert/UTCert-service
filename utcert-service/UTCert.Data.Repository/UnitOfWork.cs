using UTCert.Data.Repository.Common.BaseUnitOfWork;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;

namespace UTCert.Data.Repository;

public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
{
    private UserRepository _userRepository;
    private RefreshTokenRepository _refreshTokenRepository;
    
    public UnitOfWork(IDbContext context) : base(context)
    {
    }

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(DbContext);
    
    public IRefreshTokenRepository RefreshTokenRepository => _refreshTokenRepository ??= new RefreshTokenRepository(DbContext);

}