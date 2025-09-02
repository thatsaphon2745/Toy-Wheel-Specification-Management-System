using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public MachinesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/Machines
        [HttpGet]
        // public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
        // {
        //     return await _context.Machines.ToListAsync();
        // }
        public async Task<ActionResult<IEnumerable<Machine>>> GetMachines([FromQuery] string? search)
        {
            var query = _context.Machines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    EF.Functions.Like(m.machine_no.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/Machines/01
        [HttpGet("{id}")]
        // public async Task<ActionResult<Machine>> GetMachine(string id)
        // {
        //     var machine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineNo == id);

        //     if (machine == null)
        //         return NotFound();

        //     return machine;
        // }

        public async Task<ActionResult<Machine>> GetMachine(int id)
        {
            var machine = await _context.Machines.FindAsync(id);

            if (machine == null)
                return NotFound();

            return machine;
        }

        // POST: api/Machines
        // [HttpPost]
        // public async Task<ActionResult<Machine>> PostMachine(Machine machine)
        // {
        //     _context.Machines.Add(machine);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetMachine), new { id = machine.machine_id }, machine);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<ActionResult<Machine>> PostMachine(Machine machine)
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

            // === INSERT Machine ===
            machine.create_by = currentUserId;
            machine.create_at = DateTime.Now;
            machine.update_by = null;
            machine.update_at = null;

            _context.Machines.Add(machine);
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
                target_table = "machines",
                target_id = machine.machine_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     machine_id = machine.machine_id,
                //     machine_no = machine.machine_no,
                //     create_by = machine.create_by,
                //     create_at = machine.create_at
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

            return CreatedAtAction(nameof(GetMachine), new { id = machine.machine_id }, machine);
        }


        // PUT: api/Machines/01
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutMachine(int id, Machine machine)
        // {
        //     if (id != machine.machine_id)
        //         return BadRequest();

        //     _context.Entry(machine).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!MachineExists(id))
        //             return NotFound();
        //         else
        //             throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMachine(int id, Machine machine)
        {
            if (id != machine.machine_id)
                return BadRequest();

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ดึง record เก่าก่อน
            var existingMachine = await _context.Machines
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.machine_id == id);

            if (existingMachine == null)
                return NotFound();

            // อย่าทับค่า create_by / create_at
            machine.create_by = existingMachine.create_by;
            machine.create_at = existingMachine.create_at;

            machine.update_by = currentUserId;
            machine.update_at = DateTime.Now;

            _context.Entry(machine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MachineExists(id))
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
                    machine_id = existingMachine.machine_id,
                    machine_no = existingMachine.machine_no,
                    update_by = existingMachine.update_by,
                    update_at = existingMachine.update_at
                },
                @new = new
                {
                    machine_id = machine.machine_id,
                    machine_no = machine.machine_no,
                    update_by = machine.update_by,
                    update_at = machine.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "machines",
                target_id = machine.machine_id.ToString(),
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


        // DELETE: api/Machines/01
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteMachine(int id)
        // {
        //     var machine = await _context.Machines.FindAsync(id);
        //     if (machine == null)
        //         return NotFound();

        //     _context.Machines.Remove(machine);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            var machine = await _context.Machines
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.machine_id == id);

            if (machine == null)
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
                target_table = "machines",
                target_id = machine.machine_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     machine_id = machine.machine_id,
                //     machine_no = machine.machine_no,
                //     create_by = machine.create_by,
                //     create_at = machine.create_at,
                //     update_by = machine.update_by,
                //     update_at = machine.update_at
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

            // === DELETE Machine ===
            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.machine_id == id);
        }
    }
}
