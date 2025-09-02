using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SecretController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSecret()
        {
            var username = User.Identity?.Name;
            return Ok(new
            {
                message = $"Hello {username}, you have accessed a protected API!"
            });
        }
    }
}
