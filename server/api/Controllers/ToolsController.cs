using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/Tools // ✅ ทุกคนเข้าถึงได้
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tool>>> GetTools([FromQuery] string? search)
        {
            var query = _context.Tools.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    EF.Functions.Like(t.tool_name.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/Tools/5 // ✅ ทุกคนเข้าถึงได้
        [HttpGet("{id}")]
        public async Task<ActionResult<Tool>> GetTool(int id)
        {
            var t = await _context.Tools.FindAsync(id);

            if (t == null)
                return NotFound();

            return t;
        }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<Tool>> PostTool(Tool t)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity.Name;

            Console.WriteLine("===== CLAIMS =====");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            var isAdmin = User.IsInRole("admin");
            Console.WriteLine($"Is user in role 'admin'? {isAdmin}");

            // === INSERT Tool ===
            t.create_by = currentUserId;
            // ดึงเวลา utc+7
            t.create_at = DateTime.Now;
            t.update_by = null;
            t.update_at = null;

            _context.Tools.Add(t);
            await _context.SaveChangesAsync();

            // === PARSE OS FROM USER-AGENT ===
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

            // === INSERT Log ===
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "tools",
                target_id = t.tool_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_id = t.tool_id,
                //     tool_name = t.tool_name
                //     // เพิ่ม field อื่นตามต้องการ เช่น create_by, create_at
                // }),
                // ip_address = ip,
                // device = device,
                // os_info = os,             // ✅ บันทึก OS info ที่ parse มาได้
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 201,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTool), new { id = t.tool_id }, t);
        }

        // // ✅ ต้องเป็น admin หรือ editor
        // [Authorize(Roles = "admin,editor")]
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutTool(int id, Tool t)
        // {
        //     if (id != t.tool_id)
        //         return BadRequest();

        //     var userIdClaim = User.FindFirst("user_id");
        //     if (userIdClaim == null)
        //         return Unauthorized();

        //     var currentUserId = int.Parse(userIdClaim.Value);

        //     // ดึง record เก่าก่อน
        //     var existingTool = await _context.Tools.AsNoTracking()
        //                                            .FirstOrDefaultAsync(x => x.tool_id == id);
        //     if (existingTool == null)
        //         return NotFound();

        //     // อย่าทับค่า create_by / create_at
        //     t.create_by = existingTool.create_by;
        //     t.create_at = existingTool.create_at;

        //     t.update_by = currentUserId;
        //     t.update_at = DateTime.Now;

        //     _context.Entry(t).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!ToolExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTool(int id, Tool t)
        {
            if (id != t.tool_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity.Name;

            // ดึง record เก่าก่อน
            var existingTool = await _context.Tools.AsNoTracking()
                                                   .FirstOrDefaultAsync(x => x.tool_id == id);
            if (existingTool == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            t.create_by = existingTool.create_by;
            t.create_at = existingTool.create_at;

            t.update_by = currentUserId;
            t.update_at = DateTime.Now;

            _context.Entry(t).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToolExists(id))
                    return NotFound();
                else
                    throw;
            }

            // === INSERT LOG ===
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            // Parse OS
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

            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            // สร้าง object ค่าเก่า-ค่าใหม่
            var details = new
            {
                old = new
                {
                    tool_id = existingTool.tool_id,
                    tool_name = existingTool.tool_name,
                    update_by = existingTool.update_by,
                    update_at = existingTool.update_at
                },
                @new = new
                {
                    tool_id = t.tool_id,
                    tool_name = t.tool_name,
                    update_by = t.update_by,
                    update_at = t.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "tools",
                target_id = t.tool_id.ToString(),
                // details = JsonConvert.SerializeObject(details),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 204, // NoContent
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // // ✅ ต้องเป็น admin หรือ editor
        // [Authorize(Roles = "admin,editor")]
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteTool(int id)
        // {
        //     var userIdClaim = User.FindFirst("user_id");
        //     if (userIdClaim == null)
        //         return Unauthorized();

        //     var currentUserId = int.Parse(userIdClaim.Value);

        //     var t = await _context.Tools.FindAsync(id);
        //     if (t == null)
        //         return NotFound();

        //     // อาจจะเลือก soft delete ได้ หรือ log ก่อนลบ
        //     _context.Tools.Remove(t);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity.Name;

            var t = await _context.Tools.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.tool_id == id);
            if (t == null)
                return NotFound();

            // === INSERT LOG BEFORE DELETE ===

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            // Parse OS
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

            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "DELETE",
                target_table = "tools",
                target_id = t.tool_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_id = t.tool_id,
                //     tool_name = t.tool_name,
                //     create_by = t.create_by,
                //     create_at = t.create_at,
                //     update_by = t.update_by,
                //     update_at = t.update_at
                // }),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 204,  // NoContent
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // === DELETE Tool ===
            _context.Tools.Remove(t);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool ToolExists(int id)
        {
            return _context.Tools.Any(e => e.tool_id == id);
        }


        [HttpGet("check")]
        [Authorize]
        public IActionResult Check()
        {
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            var isAdmin = User.IsInRole("admin");

            return Ok(new
            {
                isAdmin,
                claims
            });
        }
    }
}
