using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HstTypesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public HstTypesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/HstTypes
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<hstType>>> GetHstTypes()
        // {
        //     return await _context.hstTypes.ToListAsync();
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HstType>>> GetHstTypes([FromQuery] string? search)
        {
            var query = _context.HstTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h =>
                    EF.Functions.Like(h.hst_type.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }




        // GET: api/HstTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HstType>> GetHstType(int id)
        {
            var hst = await _context.HstTypes.FindAsync(id);

            if (hst == null)
                return NotFound();

            return hst;
        }

        // POST: api/HstTypes
        // [HttpPost]
        // public async Task<ActionResult<HstType>> PostHstType(HstType hst)
        // {
        //     _context.HstTypes.Add(hst);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetHstType), new { id = hst.hst_type_id }, hst);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<HstType>> PostHstType(HstType hst)
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

            // === INSERT HstType ===
            hst.create_by = currentUserId;
            hst.create_at = DateTime.Now;
            hst.update_by = null;
            hst.update_at = null;

            _context.HstTypes.Add(hst);
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
                target_table = "hstTypes",
                target_id = hst.hst_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     hst_type_id = hst.hst_type_id,
                //     hst_type = hst.hst_type,
                //     create_by = hst.create_by,
                //     create_at = hst.create_at
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

            return CreatedAtAction(nameof(GetHstType), new { id = hst.hst_type_id }, hst);
        }


        // PUT: api/HstTypes/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutHstType(int id, HstType hst)
        // {
        //     if (id != hst.hst_type_id)
        //         return BadRequest();

        //     _context.Entry(hst).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!HstTypeExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHstType(int id, HstType hst)
        {
            if (id != hst.hst_type_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingHst = await _context.HstTypes
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.hst_type_id == id);

            if (existingHst == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            hst.create_by = existingHst.create_by;
            hst.create_at = existingHst.create_at;

            hst.update_by = currentUserId;
            hst.update_at = DateTime.Now;

            _context.Entry(hst).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HstTypeExists(id))
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
                    hst_type_id = existingHst.hst_type_id,
                    hst_type = existingHst.hst_type,
                    update_by = existingHst.update_by,
                    update_at = existingHst.update_at
                },
                @new = new
                {
                    hst_type_id = hst.hst_type_id,
                    hst_type = hst.hst_type,
                    update_by = hst.update_by,
                    update_at = hst.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "hstTypes",
                target_id = hst.hst_type_id.ToString(),
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

        // DELETE: api/HstTypes/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteHstType(int id)
        // {
        //     var hst = await _context.HstTypes.FindAsync(id);
        //     if (hst == null)
        //         return NotFound();

        //     _context.HstTypes.Remove(hst);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHstType(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var hst = await _context.HstTypes
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.hst_type_id == id);

            if (hst == null)
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
                target_table = "hstTypes",
                target_id = hst.hst_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     hst_type_id = hst.hst_type_id,
                //     hst_type = hst.hst_type,
                //     create_by = hst.create_by,
                //     create_at = hst.create_at,
                //     update_by = hst.update_by,
                //     update_at = hst.update_at
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

            // === DELETE HstType ===
            _context.HstTypes.Remove(hst);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool HstTypeExists(int id)
        {
            return _context.HstTypes.Any(e => e.hst_type_id == id);
        }
    }
}
