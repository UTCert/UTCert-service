using UTCert.Data.Repository.Common.BaseRepository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;

namespace UTCert.Data.Repository;

public class ContactRepository : GenericRepository<Contact>, IContactRepository
{
    public ContactRepository(IDbContext context) : base(context)
    {
    }
}