using Microsoft.AspNetCore.Mvc;
using UTCert.Model.Database;

namespace utcert_service.Controllers;

[Controller]
public abstract class BaseController : ControllerBase
{
    protected User User => (User)HttpContext.Items["User"];
    
    protected Guid CurrentUserId => User.Id;
    protected string CurrentUserStakeId => User.StakeId;
}