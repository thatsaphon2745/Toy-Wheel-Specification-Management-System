using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolKeyOriginalsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolKeyOriginalsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // POST: api/ToolKeyOriginals
        // [HttpPost]
        // public async Task<ActionResult<ToolKeyOriginal>> PostToolKeyOriginal(ToolKeyOriginal toolKeyOriginal)
        // {
        //     _context.ToolKeyOriginals.Add(toolKeyOriginal);
        //     await _context.SaveChangesAsync();

        //     // ✅ ส่ง id กลับให้ React ใช้ต่อ (ใช้ใน step ถัดไป)
        //     return CreatedAtAction(nameof(PostToolKeyOriginal), new { id = toolKeyOriginal.tool_key_id }, toolKeyOriginal);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> PostToolKeyOriginal([FromBody] ToolKeyOriginal toolKeyOriginal)
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

            // ✅ Insert record
            toolKeyOriginal.create_by = currentUserId;
            toolKeyOriginal.create_at = DateTime.Now;
            toolKeyOriginal.update_by = null;
            toolKeyOriginal.update_at = null;

            _context.ToolKeyOriginals.Add(toolKeyOriginal);
            await _context.SaveChangesAsync();

            // ✅ Prepare log
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
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

            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "toolKeyOriginals",
                target_id = toolKeyOriginal.tool_key_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_key_id = toolKeyOriginal.tool_key_id,
                //     type_id = toolKeyOriginal.type_id,
                //     tool_id = toolKeyOriginal.tool_id,
                //     size_ref_id = toolKeyOriginal.size_ref_id,
                //     knurling_type = toolKeyOriginal.knurling_type,
                //     create_by = toolKeyOriginal.create_by,
                //     create_at = toolKeyOriginal.create_at
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
                toolKeyOriginal.tool_key_id,
                toolKeyOriginal.type_id,
                toolKeyOriginal.tool_id,
                toolKeyOriginal.size_ref_id,
                toolKeyOriginal.knurling_type
            };

            return CreatedAtAction(nameof(PostToolKeyOriginal), new { id = toolKeyOriginal.tool_key_id }, result);
        }



        [HttpGet("find")]
        public async Task<IActionResult> FindToolKeyOriginal(
        [FromQuery] int type_id,
        [FromQuery] int tool_id,
        [FromQuery] int size_ref_id)
        {
            Console.WriteLine($"📥 FIND: type_id={type_id}, tool_id={tool_id}, size_ref_id={size_ref_id}");

            var result = await _context.ToolKeyOriginals
                .FirstOrDefaultAsync(t =>
                    t.type_id == type_id &&
                    t.tool_id == tool_id &&
                    t.size_ref_id == size_ref_id);

            if (result == null)
            {
                Console.WriteLine("❌ NOT FOUND");
                return NotFound();
            }

            Console.WriteLine($"✅ FOUND tool_key_id={result.tool_key_id}");
            return Ok(result);
        }
        // PUT: api/ToolKeyOriginals/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutToolKeyOriginal(int id, ToolKeyOriginal toolKeyOriginal)
        // {
        //     if (id != toolKeyOriginal.tool_key_id)
        //     {
        //         return BadRequest("tool_key_id mismatch");
        //     }

        //     _context.Entry(toolKeyOriginal).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!_context.ToolKeyOriginals.Any(e => e.tool_key_id == id))
        //             return NotFound();

        //         throw;
        //     }

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolKeyOriginal(int id, [FromBody] ToolKeyOriginal toolKeyOriginal)
        {
            if (id != toolKeyOriginal.tool_key_id)
            {
                return BadRequest("tool_key_id mismatch");
            }

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

            // === Load old data ===
            var existing = await _context.ToolKeyOriginals
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.tool_key_id == id);

            if (existing == null)
                return NotFound("ToolKeyOriginal not found.");

            // Update fields
            toolKeyOriginal.create_by = existing.create_by;
            toolKeyOriginal.create_at = existing.create_at;
            toolKeyOriginal.update_by = currentUserId;
            toolKeyOriginal.update_at = DateTime.Now;

            _context.Entry(toolKeyOriginal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ToolKeyOriginals.Any(e => e.tool_key_id == id))
                    return NotFound();

                throw;
            }

            // === Prepare Log ===
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
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

            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var details = new
            {
                old = new
                {
                    tool_key_id = existing.tool_key_id,
                    type_id = existing.type_id,
                    tool_id = existing.tool_id,
                    size_ref_id = existing.size_ref_id,
                    knurling_type = existing.knurling_type,
                    update_by = existing.update_by,
                    update_at = existing.update_at
                },
                @new = new
                {
                    tool_key_id = toolKeyOriginal.tool_key_id,
                    type_id = toolKeyOriginal.type_id,
                    tool_id = toolKeyOriginal.tool_id,
                    size_ref_id = toolKeyOriginal.size_ref_id,
                    knurling_type = toolKeyOriginal.knurling_type,
                    update_by = toolKeyOriginal.update_by,
                    update_at = toolKeyOriginal.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "toolKeyOriginals",
                target_id = toolKeyOriginal.tool_key_id.ToString(),
                // details = JsonConvert.SerializeObject(details),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 200,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            var result = new
            {
                message = "ToolKeyOriginal updated successfully",
                tool_key_id = toolKeyOriginal.tool_key_id,
                type_id = toolKeyOriginal.type_id,
                tool_id = toolKeyOriginal.tool_id,
                size_ref_id = toolKeyOriginal.size_ref_id,
                knurling_type = toolKeyOriginal.knurling_type
            };

            return Ok(result);
        }


        // DELETE: api/ToolKeyOriginals/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolKeyOriginal(int id)
        // {
        //     var original = await _context.ToolKeyOriginals.FindAsync(id);
        //     if (original == null) return NotFound();

        //     _context.ToolKeyOriginals.Remove(original);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }


        // DELETE: api/ToolKeyOriginals/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolKeyOriginal(int id)
        // {
        //     var original = await _context.ToolKeyOriginals.FindAsync(id);
        //     if (original == null)
        //         return NotFound();

        //     bool isReferenced = await _context.ToolKeyReferences.AnyAsync(r => r.tool_key_id == id);
        //     if (isReferenced)
        //         return Conflict("This original tool is referenced by one or more Tool References.");

        //     _context.ToolKeyOriginals.Remove(original);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        // // DELETE: api/ToolKeyOriginals/{id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolKeyOriginal(int id)
        // {
        //     var original = await _context.ToolKeyOriginals.FindAsync(id);
        //     if (original == null)
        //         return NotFound();

        //     // ✅ Step 1: Check if any toolRefSpecs reference this tool_key_id
        //     bool usedInRefSpecs = await _context.ToolRefSpecs
        //         .AnyAsync(spec => spec.tool_key_id == id);

        //     if (usedInRefSpecs)
        //     {
        //         return Conflict("Cannot delete: Tool Ref Specs are still using this original tool.");
        //     }

        //     // ✅ Step 2: Check if any toolSpecs reference toolRefSpec which is based on this original tool
        //     bool usedInToolSpecs = await _context.ToolSpecs
        //         .Include(ts => ts.ToolRefSpec) // ต้องมี navigation property ชื่อ ToolRefSpec
        //         .AnyAsync(ts => ts.ToolRefSpec.tool_key_id == id);

        //     if (usedInToolSpecs)
        //     {
        //         return Conflict("Cannot delete: Tool Specs are using reference specs from this original tool.");
        //     }

        //     _context.ToolKeyOriginals.Remove(original);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }


        // DELETE: api/ToolKeyOriginals/{id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolKeyOriginal(int id)
        // {
        //     var original = await _context.ToolKeyOriginals.FindAsync(id);
        //     if (original == null)
        //         return NotFound();

        //     // Step 1: เช็ก ToolRefSpec ที่ใช้ key นี้
        //     bool usedInRefSpecs = await _context.ToolRefSpecs
        //         .AnyAsync(spec => spec.tool_key_id == id);

        //     if (usedInRefSpecs)
        //         return Conflict("Cannot delete: Tool Ref Specs are still using this original tool.");

        //     // Step 2: เช็ก ToolSpecs ที่อ้างอิง ToolRefSpec ซึ่งใช้ original key นี้
        //     bool usedInToolSpecs = await _context.ToolSpecs
        //         .Where(ts => ts.tool_ref_spec_id != null)
        //         .Include(ts => ts.ToolRefSpec)
        //         .AnyAsync(ts => ts.ToolRefSpec.tool_key_id == id);

        //     if (usedInToolSpecs)
        //     {
        //         return Conflict("Cannot delete: Tool Specs are using reference specs from this original tool.");
        //     }

        //     _context.ToolKeyOriginals.Remove(original);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }


        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolKeyOriginal(int id)
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

            var original = await _context.ToolKeyOriginals
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.tool_key_id == id);

            if (original == null)
                return NotFound("ToolKeyOriginal not found.");

            // Step 1: เช็ก ToolRefSpec ที่ใช้ key นี้
            bool usedInRefSpecs = await _context.ToolRefSpecs
                .AnyAsync(spec => spec.tool_key_id == id);

            if (usedInRefSpecs)
                return Conflict("Cannot delete: Tool Ref Specs are still using this original tool.");

            // Step 2: เช็ก ToolSpecs ที่อ้างอิง ToolRefSpec ซึ่งใช้ original key นี้
            bool usedInToolSpecs = await _context.ToolSpecs
                .Where(ts => ts.tool_ref_spec_id != null)
                .Include(ts => ts.ToolRefSpec)
                .AnyAsync(ts => ts.ToolRefSpec.tool_key_id == id);

            if (usedInToolSpecs)
            {
                return Conflict("Cannot delete: Tool Specs are using reference specs from this original tool.");
            }

            var entity = await _context.ToolKeyOriginals.FindAsync(id);
            if (entity != null)
            {
                _context.ToolKeyOriginals.Remove(entity);
                await _context.SaveChangesAsync();
            }

            // === Prepare Log ===
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
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

            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "DELETE",
                target_table = "toolKeyOriginals",
                target_id = original.tool_key_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_key_id = original.tool_key_id,
                //     type_id = original.type_id,
                //     tool_id = original.tool_id,
                //     size_ref_id = original.size_ref_id,
                //     knurling_type = original.knurling_type,
                //     create_by = original.create_by,
                //     create_at = original.create_at,
                //     update_by = original.update_by,
                //     update_at = original.update_at
                // }),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 200,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ToolKeyOriginal deleted successfully",
                tool_key_id = id
            });
        }


        // GET: api/ToolKeyOriginals/check
        [HttpGet("check")]
        public async Task<IActionResult> CheckDuplicateOriginalKey([FromQuery] int tool_id, [FromQuery] int type_id, [FromQuery] int size_ref_id, [FromQuery] int knurling_type)
        {
            var existing = await _context.ToolKeyOriginals
                .FirstOrDefaultAsync(k =>
                    k.tool_id == tool_id &&
                    k.type_id == type_id &&
                    k.size_ref_id == size_ref_id &&
                    k.knurling_type == knurling_type);

            if (existing != null)
            {
                return Ok(new { exists = true, tool_key_id = existing.tool_key_id });
            }

            return Ok(new { exists = false });
        }


    }
}
