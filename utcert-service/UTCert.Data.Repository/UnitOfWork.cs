﻿using UTCert.Data.Repository.Common.BaseUnitOfWork;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;

namespace UTCert.Data.Repository;

public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
{
    private UserRepository _userRepository;
    private RefreshTokenRepository _refreshTokenRepository;
    private CertificateRepository _certificateRepository;
    private ContactRepository _contactRepository;
    
    public UnitOfWork(IDbContext context) : base(context)
    {
    }

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(DbContext);
    public IRefreshTokenRepository RefreshTokenRepository => _refreshTokenRepository ??= new RefreshTokenRepository(DbContext);
    public ICertificateRepository CertificateRepository => _certificateRepository ??= new CertificateRepository(DbContext);
    public IContactRepository ContactRepository => _contactRepository ??= new ContactRepository(DbContext);
}