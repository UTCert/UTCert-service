using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository.Common.ExtensionMethod;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Common;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.Contact;
using UTCert.Model.Web.Dtos;
using UTCert.Service.BusinessLogic.Common;
using UTCert.Service.BusinessLogic.Dtos;
using UTCert.Service.BusinessLogic.Interface;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UTCert.Service.BusinessLogic;

public class ContactService : EntityService<Contact>, IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly IMapper _mapper; 
    
    public ContactService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper) : base(unitOfWork, unitOfWork.ContactRepository)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _mapper = mapper; 
    }
    
    //get all contacts of a user
    public async Task<PagedResultDto<ContactDto>> GetContacts(ContactFilterDto input)
    {
        var query = _unitOfWork.ContactRepository.GetAll().AsNoTracking()
            .Where(x => x.IssuerId == input.UserId || x.ReceiverId == input.UserId);

        query = query
            .WhereIf(!string.IsNullOrEmpty(input.ContactName), 
                x => x.IssuerName.Contains(input.ContactName) || x.ReceiverName.Contains(input.ContactName))
            .WhereIf(input.ContactStatus.HasValue, x => (int)x.Status == input.ContactStatus.Value); 


        if (!string.IsNullOrEmpty(input.Sorting))
        {
            query = query.OrderByDynamic(input.Sorting);
        }
        else
        {
            query = query.OrderBy(x => x.Id);
        }

        var rowCount = await query.CountAsync();
        var dataList = await query
            .PageBy((input.PageNumber - 1) * input.PageSize, input.PageSize)
            .Select(x => new ContactDto
            {
                Id = x.Id, 
                Issuer = x.Issuer, 
                Receiver = x.Receiver, 
                IssuerName = x.IssuerName, 
                ReceiverName = x.ReceiverName,
                ContactName = x.IssuerId == input.UserId ? x.ReceiverName : x.IssuerName, 
                IssuerId = x.IssuerId, 
                ReceiverId = x.ReceiverId, 
                Status = x.Status, 
                CreatedDate = x.CreatedDate, 
            })
            .ToListAsync();

        return new PagedResultDto<ContactDto>
        {
            Items = dataList,
            TotalCount = rowCount,
        };
    }
    
    public async Task<Guid> CreateContact(string stakeId, Guid currentUserId)
    {
        try
        {
            var receiver = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.StakeId == stakeId) ?? throw new AppException("User not found!");
            var issuer = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Id == currentUserId) ?? throw new AppException("User not found!");
            var isExistContact = await _unitOfWork.ContactRepository.AnyAsync(x => x.ReceiverId == issuer.Id);
            if (isExistContact)
            {
                throw new AppException("Contact has been existed!");
            }
            
            var newContact = new Contact
            {
                Id = new Guid(),
                IssuerId = issuer.Id,
                ReceiverId = receiver.Id,
                Status = ContactStatus.Pending,
                CreatedDate = DateTime.Now, 
                IssuerName = issuer.Name, 
                ReceiverName = issuer.Name,
            };
            
            await CreateAsync(newContact);
            return newContact.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<bool> UpdateContactStatus(ContactInputDto input)
    {
        var contact = await _unitOfWork.ContactRepository.FirstOrDefaultAsync(x => x.Id == input.Id) ?? throw new AppException("Contact not found!");
        contact.Status = input.Status;
        contact.ModifiedDate = DateTime.Now;
        return await UpdateAsync(contact);
    }
    
    public async Task<bool> DeleteContact(Guid contactId)
    {
        var contact = await _unitOfWork.ContactRepository.FirstOrDefaultAsync(x => x.Id == contactId) ?? throw new AppException("Contact not found!");
        await DeleteAsync(contact);
        return await _unitOfWork.CommitAsync() > 0;
    }
}