using UTCert.Data.Repository.Common.DbContext;
using UTCert.Service.Helper.JwtUtils;

namespace utcert_service.Authorization;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, DatabaseContext dataContext, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var userId = jwtUtils.ValidateJwtToken(token);
            if (userId != Guid.Empty)
            {
                // attach account to context on successful jwt validation
                context.Items["User"] = await dataContext.Users.FindAsync(userId);
                var a = context.Items["User"];
            }
        }
        await _next(context);
    }
}