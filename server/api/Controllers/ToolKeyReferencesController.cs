using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolKeyReferencesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolKeyReferencesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ POST: api/ToolKeyReferences
        // [HttpPost]
        // public async Task<ActionResult<ToolKeyReference>> PostToolKeyReference(ToolKeyReference reference)
        // {
        //     _context.ToolKeyReferences.Add(reference);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetToolKeyReference), new { id = reference.ref_key_id }, reference);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> PostToolKeyReference([FromBody] ToolKeyReference reference)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            Console.WriteLine("===== CLAIMS =====");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            var isAdmin = User.IsInRole("admin");
            Console.WriteLine($"Is user in role 'admin'? {isAdmin}");

            reference.create_by = currentUserId;
            reference.create_at = DateTime.Now;
            reference.update_by = null;
            reference.update_at = null;

            _context.ToolKeyReferences.Add(reference);
            await _context.SaveChangesAsync();

            var device = Request.Headers["User-Agent"].ToString();

            string os = null;
            if (device.Contains("Windows"))
                os = "Windows";
            else if (device.Contains("Macintosh"))
                os = "macOS";
            else if (device.Contains("iPhone"))
                os = "iOS";
            else if (device.Contains("Android"))
                os = "Android";
            else if (device.Contains("Linux"))
                os = "Linux";
            else
                os = "Unknown";

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "toolKeyReferences",
                target_id = reference.ref_key_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     ref_key_id = reference.ref_key_id,
                //     tool_id = reference.tool_id,
                //     type_id = reference.type_id,
                //     position_type_id = reference.position_type_id,
                //     knurling_type = reference.knurling_type,
                //     tool_key_id = reference.tool_key_id,
                //     create_by = reference.create_by,
                //     create_at = reference.create_at
                // }),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 201,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            var result = new
            {
                reference.ref_key_id,
                reference.tool_id,
                reference.type_id,
                reference.position_type_id,
                reference.knurling_type,
                reference.tool_key_id
            };

            return CreatedAtAction(nameof(GetToolKeyReference), new { id = reference.ref_key_id }, result);
        }


        // ✅ GET: api/ToolKeyReferences/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ToolKeyReference>> GetToolKeyReference(int id)
        {
            var reference = await _context.ToolKeyReferences.FindAsync(id);

            if (reference == null)
                return NotFound();

            return Ok(reference);
        }

        // (Optional) ✅ GET: api/ToolKeyReferences
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolKeyReference>>> GetAllToolKeyReferences()
        {
            return await _context.ToolKeyReferences.ToListAsync();
        }
    }
}
