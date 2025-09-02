using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AxleTypesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public AxleTypesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/AxleTypes
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<axleType>>> GetAxleTypes()
        // {
        //     return await _context.axleTypes.ToListAsync();
        // }

        // GET: api/AxleTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AxleType>>> GetAxleTypes([FromQuery] string? search)
        {
            var query = _context.AxleTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    EF.Functions.Like(a.axle_type.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }


        // GET: api/AxleTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AxleType>> GetAxleType(int id)
        {
            var axle = await _context.AxleTypes.FindAsync(id);

            if (axle == null)
                return NotFound();

            return axle;
        }

        // // POST: api/AxleTypes
        // [HttpPost]
        // public async Task<ActionResult<AxleType>> PostAxleType(AxleType axle)
        // {
        //     _context.AxleTypes.Add(axle);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetAxleType), new { id = axle.axle_type_id }, axle);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<AxleType>> PostAxleType(AxleType axle)
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

            // === INSERT AxleType ===
            axle.create_by = currentUserId;
            axle.create_at = DateTime.Now;
            axle.update_by = null;
            axle.update_at = null;

            _context.AxleTypes.Add(axle);
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
                target_table = "axleTypes",
                target_id = axle.axle_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     axle_type_id = axle.axle_type_id,
                //     axle_type = axle.axle_type,
                //     create_by = axle.create_by,
                //     create_at = axle.create_at
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

            return CreatedAtAction(nameof(GetAxleType), new { id = axle.axle_type_id }, axle);
        }

        // // PUT: api/AxleTypes/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutAxleType(int id, AxleType axle)
        // {
        //     if (id != axle.axle_type_id)
        //         return BadRequest();

        //     _context.Entry(axle).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!AxleTypeExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAxleType(int id, AxleType axle)
        {
            if (id != axle.axle_type_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingAxle = await _context.AxleTypes
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(x => x.axle_type_id == id);

            if (existingAxle == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            axle.create_by = existingAxle.create_by;
            axle.create_at = existingAxle.create_at;

            axle.update_by = currentUserId;
            axle.update_at = DateTime.Now;

            _context.Entry(axle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AxleTypeExists(id))
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
                    axle_type_id = existingAxle.axle_type_id,
                    axle_type = existingAxle.axle_type,
                    update_by = existingAxle.update_by,
                    update_at = existingAxle.update_at
                },
                @new = new
                {
                    axle_type_id = axle.axle_type_id,
                    axle_type = axle.axle_type,
                    update_by = axle.update_by,
                    update_at = axle.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "axleTypes",
                target_id = axle.axle_type_id.ToString(),
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


        // // DELETE: api/AxleTypes/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteAxleType(int id)
        // {
        //     var axle = await _context.AxleTypes.FindAsync(id);
        //     if (axle == null)
        //         return NotFound();

        //     _context.AxleTypes.Remove(axle);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAxleType(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var axle = await _context.AxleTypes
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.axle_type_id == id);
            if (axle == null)
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
                target_table = "axleTypes",
                target_id = axle.axle_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     axle_type_id = axle.axle_type_id,
                //     axle_type = axle.axle_type,
                //     create_by = axle.create_by,
                //     create_at = axle.create_at,
                //     update_by = axle.update_by,
                //     update_at = axle.update_at
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

            // === DELETE AxleType ===
            _context.AxleTypes.Remove(axle);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool AxleTypeExists(int id)
        {
            return _context.AxleTypes.Any(e => e.axle_type_id == id);
        }
    }
}
