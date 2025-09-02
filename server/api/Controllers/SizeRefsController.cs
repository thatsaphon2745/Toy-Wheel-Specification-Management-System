using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeRefsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public SizeRefsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/SizeRefs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SizeRef>>> GetSizeRefs([FromQuery] string? search)
        {
            var query = _context.SizeRefs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    EF.Functions.Like(s.size_ref.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/SizeRefs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SizeRef>> GetSizeRef(int id)
        {
            var s = await _context.SizeRefs.FindAsync(id);

            if (s == null)
                return NotFound();

            return s;
        }

        // POST: api/SizeRefs
        // [HttpPost]
        // public async Task<ActionResult<SizeRef>> PostSizeRef(SizeRef s)
        // {
        //     _context.SizeRefs.Add(s);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetSizeRef), new { id = s.size_ref_id }, s);
        // }
        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<SizeRef>> PostSizeRef(SizeRef s)
        {
            // --- ดึง user info ---
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

            // --- Set Audit Fields ---
            s.create_by = currentUserId;
            s.create_at = DateTime.Now;
            s.update_by = null;
            s.update_at = null;

            _context.SizeRefs.Add(s);
            await _context.SaveChangesAsync();

            // --- PARSE OS FROM USER-AGENT ---
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

            // --- INSERT Log ---
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "sizeRefs",
                target_id = s.size_ref_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     size_ref_id = s.size_ref_id,
                //     size_ref = s.size_ref
                //     // เพิ่ม field อื่นตามต้องการ เช่น create_by, create_at
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

            return CreatedAtAction(nameof(GetSizeRef), new { id = s.size_ref_id }, s);
        }


        // PUT: api/SizeRefs/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutSizeRef(int id, SizeRef s)
        // {
        //     if (id != s.size_ref_id)
        //         return BadRequest();

        //     _context.Entry(s).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!SizeRefExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSizeRef(int id, SizeRef s)
        {
            if (id != s.size_ref_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity.Name;

            // ดึง record เก่าก่อน
            var existingSizeRef = await _context.SizeRefs.AsNoTracking()
                                                         .FirstOrDefaultAsync(x => x.size_ref_id == id);
            if (existingSizeRef == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            s.create_by = existingSizeRef.create_by;
            s.create_at = existingSizeRef.create_at;

            s.update_by = currentUserId;
            s.update_at = DateTime.Now;

            _context.Entry(s).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SizeRefExists(id))
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
                    size_ref_id = existingSizeRef.size_ref_id,
                    size_ref = existingSizeRef.size_ref,
                    update_by = existingSizeRef.update_by,
                    update_at = existingSizeRef.update_at
                },
                @new = new
                {
                    size_ref_id = s.size_ref_id,
                    size_ref = s.size_ref,
                    update_by = s.update_by,
                    update_at = s.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "sizeRefs",
                target_id = s.size_ref_id.ToString(),
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

        // DELETE: api/SizeRefs/5
        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSizeRef(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity.Name;

            var s = await _context.SizeRefs.AsNoTracking()
                                           .FirstOrDefaultAsync(x => x.size_ref_id == id);
            if (s == null)
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
                target_table = "sizeRefs",
                target_id = s.size_ref_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     size_ref_id = s.size_ref_id,
                //     size_ref = s.size_ref,
                //     create_by = s.create_by,
                //     create_at = s.create_at,
                //     update_by = s.update_by,
                //     update_at = s.update_at
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

            // === DELETE SizeRef ===
            _context.SizeRefs.Remove(s);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool SizeRefExists(int id)
        {
            return _context.SizeRefs.Any(e => e.size_ref_id == id);
        }
    }
}
