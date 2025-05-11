using Microsoft.AspNetCore.Mvc;

namespace TroubleTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected string GetUserId()
    {
        return User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException();
    }

    protected bool IsAdmin()
    {
        return User.IsInRole("admin");
    }

    protected bool IsServiceAgent()
    {
        return User.IsInRole("agent");
    }
}