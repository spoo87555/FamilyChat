using Microsoft.AspNetCore.Mvc;

namespace FamilyChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<T> HandleResult<T>(T result)
    {
        if (result == null)
            return NotFound();

        return Ok(result);
    }
} 