using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PadsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public PadsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/Pads
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<pad>>> GetPads()
        // {
        //     return await _context.pads.ToListAsync();
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pad>>> GetPads([FromQuery] string? search)
        {
            var query = _context.Pads.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.pad_name.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }



        // GET: api/Pads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pad>> GetPad(int id)
        {
            var pad = await _context.Pads.FindAsync(id);

            if (pad == null)
                return NotFound();

            return pad;
        }

        // POST: api/Pads
        // [HttpPost]
        // public async Task<ActionResult<Pad>> PostPad(Pad p)
        // {
        //     _context.Pads.Add(p);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetPad), new { id = p.pad_id }, p);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<Pad>> PostPad(Pad p)
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

            // === INSERT Pad ===
            p.create_by = currentUserId;
            p.create_at = DateTime.Now;
            p.update_by = null;
            p.update_at = null;

            _context.Pads.Add(p);
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
                target_table = "pads",
                target_id = p.pad_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_id = p.pad_id,
                //     pad_name = p.pad_name,
                //     create_by = p.create_by,
                //     create_at = p.create_at
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

            return CreatedAtAction(nameof(GetPad), new { id = p.pad_id }, p);
        }


        // PUT: api/Pads/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutPad(int id, Pad p)
        // {
        //     if (id != p.pad_id)
        //         return BadRequest();

        //     _context.Entry(p).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!PadExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }


        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPad(int id, Pad p)
        {
            if (id != p.pad_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingPad = await _context.Pads
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.pad_id == id);

            if (existingPad == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            p.create_by = existingPad.create_by;
            p.create_at = existingPad.create_at;

            p.update_by = currentUserId;
            p.update_at = DateTime.Now;

            _context.Entry(p).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PadExists(id))
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
                    pad_id = existingPad.pad_id,
                    pad_name = existingPad.pad_name,
                    update_by = existingPad.update_by,
                    update_at = existingPad.update_at
                },
                @new = new
                {
                    pad_id = p.pad_id,
                    pad_name = p.pad_name,
                    update_by = p.update_by,
                    update_at = p.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "pads",
                target_id = p.pad_id.ToString(),
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


        // DELETE: api/Pads/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeletePad(int id)
        // {
        //     var pad = await _context.Pads.FindAsync(id);
        //     if (pad == null)
        //         return NotFound();

        //     _context.Pads.Remove(pad);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePad(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.pad_id == id);

            if (pad == null)
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
                target_table = "pads",
                target_id = pad.pad_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_id = pad.pad_id,
                //     pad_name = pad.pad_name,
                //     create_by = pad.create_by,
                //     create_at = pad.create_at,
                //     update_by = pad.update_by,
                //     update_at = pad.update_at
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

            // === DELETE Pad ===
            _context.Pads.Remove(pad);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool PadExists(int id)
        {
            return _context.Pads.Any(e => e.pad_id == id);
        }
    }
}
