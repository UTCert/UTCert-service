using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.Contact;
using UTCert.Model.Web.Dtos;
using UTCert.Service.BusinessLogic.Dtos;

namespace UTCert.Service.BusinessLogic.Interface;

public interface IContactService
{
    Task<PagedResultDto<ContactDto>> GetContacts(ContactFilterDto input);
    Task<Guid> CreateContact(string stakeId, Guid issuerId);
    Task<bool> UpdateContactStatus(ContactInputDto input);
    Task<bool> DeleteContact(Guid contactId);
}