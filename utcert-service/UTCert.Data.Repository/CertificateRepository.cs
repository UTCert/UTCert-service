using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;

namespace UTCert.Data.Repository;

public class CertificateRepository : GenericRepository<Certificate>, ICertificateRepository
{
    public CertificateRepository(IDbContext context) : base(context)
    {
    }
    
    public async Task<int> CountCertificate()
    {
        return await DbSet.CountAsync();
    }
}