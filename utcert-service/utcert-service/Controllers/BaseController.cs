using Microsoft.AspNetCore.Mvc;
using UTCert.Model.Database;

namespace utcert_service.Controllers;

[Controller]
public abstract class BaseController : ControllerBase
{
    // returns the current authenticated account (null if not logged in)
    protected User User => (User)HttpContext.Items["User"];
    
    protected Guid CurrentUserId => User.Id;
}