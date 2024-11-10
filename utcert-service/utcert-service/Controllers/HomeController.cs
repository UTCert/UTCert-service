using Microsoft.AspNetCore.Mvc;
using utcert_service.Authorization;
using utcert_service.ResponseModel;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Shared.Enum;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.BusinessLogic.Dtos;

namespace utcert_service.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HomeController : BaseController
{
    private readonly IHomeService _homeService;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IHomeService homeService, IUnitOfWork unitOfWork)
    {
        _homeService = homeService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ApiResponse<DashboardDto>> GetDashboardDataAsync()
    {
        var res = await _homeService.GetDashboardDataAsync(CurrentUserId);
        return new ApiResponse<DashboardDto>
        {
            Success = true,
            Data = res
        };
    }
}