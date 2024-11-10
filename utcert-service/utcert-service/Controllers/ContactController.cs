using Microsoft.AspNetCore.Mvc;
using utcert_service.Authorization;
using utcert_service.ResponseModel;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Shared.Enum;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.BusinessLogic.Dtos;
using UTCert.Model.Web.Dtos;
using UTCert.Model.Web.Contact;

namespace utcert_service.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ContactController : BaseController
{
    private readonly IContactService _contactService;
    private readonly IUnitOfWork _unitOfWork;
    
    public ContactController(IContactService contactService, IUnitOfWork unitOfWork)
    {
        _contactService = contactService;
        _unitOfWork = unitOfWork;
    }
    
    [HttpPost("get-contacts")]
    public async Task<ApiResponse<PagedResultDto<ContactDto>>> GetContacts(ContactFilterDto input)
    {
        input.UserId = CurrentUserId; 
        var contacts = await _contactService.GetContacts(input);
        return new ApiResponse<PagedResultDto<ContactDto>>(contacts);
    }
    
    [HttpPost("create")]
    public async Task<ApiResponse<Guid>> CreateContact(string stakeId)
    {
        var newContactId = await _contactService.CreateContact(stakeId, CurrentUserId);
        return new ApiResponse<Guid>(newContactId);
    }
    
    [HttpPut("update-status")]
    public async Task<ApiResponse<bool>> UpdateContactStatus(ContactInputDto input)
    {
        var success = await _contactService.UpdateContactStatus(input);
        return new ApiResponse<bool>()
        {
            Success = success, 
            Data = success, 
        };
    }
    
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> DeleteContact(Guid id)
    {
        var success = await _contactService.DeleteContact(id);
        return new ApiResponse<bool>(success);
    }
}