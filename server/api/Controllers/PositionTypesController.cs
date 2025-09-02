using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionTypesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public PositionTypesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/PositionTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionType>>> GetPositionTypes([FromQuery] string? search)
        {
            var query = _context.PositionTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.position_type.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }


        // GET: api/PositionTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PositionType>> GetPositionType(int id)
        {
            var position = await _context.PositionTypes.FindAsync(id);

            if (position == null)
                return NotFound();

            return position;
        }

        // POST: api/PositionTypes
        // [HttpPost]
        // public async Task<ActionResult<PositionType>> PostPositionType(PositionType position)
        // {
        //     _context.PositionTypes.Add(position);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetPositionType), new { id = position.position_type_id }, position);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<PositionType>> PostPositionType(PositionType position)
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

            // === INSERT PositionType ===
            position.create_by = currentUserId;
            position.create_at = DateTime.Now;
            position.update_by = null;
            position.update_at = null;

            _context.PositionTypes.Add(position);
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
                target_table = "positionTypes",
                target_id = position.position_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     position_type_id = position.position_type_id,
                //     position_type = position.position_type,
                //     create_by = position.create_by,
                //     create_at = position.create_at
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

            return CreatedAtAction(nameof(GetPositionType), new { id = position.position_type_id }, position);
        }

        // PUT: api/PositionTypes/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutPositionType(int id, PositionType position)
        // {
        //     if (id != position.position_type_id)
        //         return BadRequest();

        //     _context.Entry(position).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!PositionTypeExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPositionType(int id, PositionType position)
        {
            if (id != position.position_type_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingPosition = await _context.PositionTypes
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(x => x.position_type_id == id);

            if (existingPosition == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            position.create_by = existingPosition.create_by;
            position.create_at = existingPosition.create_at;

            position.update_by = currentUserId;
            position.update_at = DateTime.Now;

            _context.Entry(position).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionTypeExists(id))
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
                    position_type_id = existingPosition.position_type_id,
                    position_type = existingPosition.position_type,
                    update_by = existingPosition.update_by,
                    update_at = existingPosition.update_at
                },
                @new = new
                {
                    position_type_id = position.position_type_id,
                    position_type = position.position_type,
                    update_by = position.update_by,
                    update_at = position.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "positionTypes",
                target_id = position.position_type_id.ToString(),
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


        // DELETE: api/PositionTypes/5
        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePositionType(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var position = await _context.PositionTypes
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.position_type_id == id);

            if (position == null)
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
                target_table = "positionTypes",
                target_id = position.position_type_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     position_type_id = position.position_type_id,
                //     position_type = position.position_type,
                //     create_by = position.create_by,
                //     create_at = position.create_at,
                //     update_by = position.update_by,
                //     update_at = position.update_at
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

            // === DELETE PositionType ===
            _context.PositionTypes.Remove(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool PositionTypeExists(int id)
        {
            return _context.PositionTypes.Any(e => e.position_type_id == id);
        }
    }
}
