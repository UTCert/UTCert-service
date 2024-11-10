using UTCert.Data.Repository.Common.BaseUnitOfWork;

namespace UTCert.Data.Repository.Interface;

public interface IUnitOfWork: IUnitOfWorkBase
{
    IUserRepository UserRepository { get; }
    
    IRefreshTokenRepository RefreshTokenRepository { get; }
    
    ICertificateRepository CertificateRepository { get; }
    
    IContactRepository ContactRepository { get; }
}