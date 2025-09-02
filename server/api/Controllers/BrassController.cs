using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;
namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrassController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public BrassController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/Brass
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brass>>> GetBrasses([FromQuery] string? search)
        {
            var query = _context.Brasses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    EF.Functions.Like(b.brass_no.ToLower(), $"%{search.ToLower()}%")
                    || EF.Functions.Like(b.description.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/Brass/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Brass>> GetBrass(int id)
        {
            var b = await _context.Brasses.FindAsync(id);

            if (b == null)
                return NotFound();

            return b;
        }

        // POST: api/Brass
        // [HttpPost]
        // public async Task<ActionResult<Brass>> PostBrass(Brass b)
        // {
        //     _context.Brasses.Add(b);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetBrass), new { id = b.brass_id }, b);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<Brass>> PostBrass(Brass b)
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

            // === INSERT Brass ===
            b.create_by = currentUserId;
            b.create_at = DateTime.Now;
            b.update_by = null;
            b.update_at = null;

            _context.Brasses.Add(b);
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
                target_table = "brasses",
                target_id = b.brass_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     brass_id = b.brass_id,
                //     brass_no = b.brass_no,
                //     description = b.description,
                //     create_by = b.create_by,
                //     create_at = b.create_at
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

            return CreatedAtAction(nameof(GetBrass), new { id = b.brass_id }, b);
        }


        // PUT: api/Brass/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutBrass(int id, Brass b)
        // {
        //     if (id != b.brass_id)
        //         return BadRequest();

        //     _context.Entry(b).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!BrassExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrass(int id, Brass b)
        {
            if (id != b.brass_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingBrass = await _context.Brasses
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(x => x.brass_id == id);

            if (existingBrass == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            b.create_by = existingBrass.create_by;
            b.create_at = existingBrass.create_at;

            b.update_by = currentUserId;
            b.update_at = DateTime.Now;

            _context.Entry(b).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrassExists(id))
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
                    brass_id = existingBrass.brass_id,
                    brass_no = existingBrass.brass_no,
                    description = existingBrass.description,
                    update_by = existingBrass.update_by,
                    update_at = existingBrass.update_at
                },
                @new = new
                {
                    brass_id = b.brass_id,
                    brass_no = b.brass_no,
                    description = b.description,
                    update_by = b.update_by,
                    update_at = b.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "brasses",
                target_id = b.brass_id.ToString(),
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


        // DELETE: api/Brass/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteBrass(int id)
        // {
        //     var b = await _context.Brasses.FindAsync(id);
        //     if (b == null)
        //         return NotFound();

        //     _context.Brasses.Remove(b);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrass(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var b = await _context.Brasses
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(x => x.brass_id == id);

            if (b == null)
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
                target_table = "brasses",
                target_id = b.brass_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     brass_id = b.brass_id,
                //     brass_no = b.brass_no,
                //     description = b.description,
                //     create_by = b.create_by,
                //     create_at = b.create_at,
                //     update_by = b.update_by,
                //     update_at = b.update_at
                // }),
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

            // === DELETE Brass ===
            _context.Brasses.Remove(b);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool BrassExists(int id)
        {
            return _context.Brasses.Any(e => e.brass_id == id);
        }
    }
}
