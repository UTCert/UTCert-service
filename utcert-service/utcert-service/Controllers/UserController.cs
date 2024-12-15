using System.Net;
using Microsoft.AspNetCore.Mvc;
using utcert_service.Authorization;
using utcert_service.ResponseModel;
using UTCert.Data.Repository.Interface;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;
using UTCert.Model.Web.User;
using UTCert.Service.BusinessLogic.Interface;

namespace utcert_service.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public UserController(IUserService userService, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ApiResponse<Guid>> Register([FromForm] RegisterDto model)
    {
        var newUserId = await _userService.Register(model);
        return new ApiResponse<Guid>(newUserId);
    }

    [AllowAnonymous]
    [HttpGet("authenticate")]
    public async Task<ApiResponse<UserResponseDto>> Authenticate(string stakeId)
    {
        var response = await _userService.Authenticate(stakeId, ipAddress());
        setTokenCookie(response.RefreshToken);
        return new ApiResponse<UserResponseDto>(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<UserResponseDto> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? "";
        var response = await _userService.RefreshToken(refreshToken, ipAddress());
        setTokenCookie(response.RefreshToken);
        return response;
    }

    [AllowAnonymous]
    [HttpPost("revoke-token")]
    public async Task<BoolApiResponse> RevokeToken(string token)
    {
        var curToken = string.IsNullOrWhiteSpace(token) ? Request.Cookies["refreshToken"] : token;

        if (string.IsNullOrEmpty(curToken))
        {
            return new BoolApiResponse(false, "Token is required", HttpStatusCode.BadRequest);
        }

        var result = await _userService.RevokeToken(curToken, ipAddress());
        return new BoolApiResponse(result);
    }

    //comment [Authorize] at class level if want to set permission admin only
    /* [Authorize(Role.Admin)]  
     [HttpGet]
     public async Task<RefreshToken?> GetAll()
     {
         var g = new Guid("E7653248-FDFF-4939-B472-A424D4ABF74A");
         return await _unitOfWork.RefreshTokenRepository.GetByIdAsync(g);
     }*/

    [AllowAnonymous]
    [HttpGet("has-account")]
    public async Task<ApiResponse<bool>> HasAccount(string stakeId)
    {
        var result = await _userService.HasAccount(stakeId);
        return new ApiResponse<bool>()
        {
            Success = result
        };
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{id:guid}")]
    public async Task<ApiResponse<UserResponseDto>> GetById(Guid id)
    {
        var response = new ApiResponse<UserResponseDto>();
        try
        {
            var result = await _userService.GetById(id);
            response.Success = true;
            response.Data = result;
            return response;

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return response;
        }

    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update(Guid id, [FromForm] UserCrudDto input) 
    {
        try
        {
            var result = await _userService.Update(id, input);
            return new ApiResponse<bool>() { Success = result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    #region private Functions

    private void setTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }

    #endregion
}