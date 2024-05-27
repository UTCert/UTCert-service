using Microsoft.AspNetCore.Mvc;
using utcert_service.Authorization;
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
    public async Task<string> Register(RegisterDto model)
    {
        return await _userService.Register(model);
    }
    
    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<UserResponseDto> Authenticate(string stakeId)
    {
        var response = await _userService.Authenticate(stakeId, ipAddress());
        setTokenCookie(response.RefreshToken);
        return response;
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<UserResponseDto> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var response = await _userService.RefreshToken(refreshToken, ipAddress());
        setTokenCookie(response.RefreshToken);
        return response;
    }

    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(string token)
    {
        var refreshToken = string.IsNullOrEmpty(token) ? token : Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Token is required" });

        if (!User.OwnsToken(refreshToken))
            return Unauthorized(new { message = "Unauthorized" });

        _userService.RevokeToken(refreshToken, ipAddress());
        return Ok(new { message = "Token revoked" });
    }
    
    [Authorize(Role.Admin)]  
    [HttpGet]
    public async Task<RefreshToken?> GetAll()
    {
        var g = new Guid("E7653248-FDFF-4939-B472-A424D4ABF74A");
        return await _unitOfWork.RefreshTokenRepository.GetByIdAsync(g);
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