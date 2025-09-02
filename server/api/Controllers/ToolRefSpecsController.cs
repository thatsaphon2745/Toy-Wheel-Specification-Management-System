using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolRefSpecsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolRefSpecsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: api/ToolRefSpecs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var data = await _context.ToolRefSpecs
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Tool)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Type)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.SizeRef)
                .Include(t => t.AxleType)
                .ToListAsync();

            var result = data.Select(t => new
            {
                t.tool_ref_spec_id,

                // ✅ decoded fields
                tool_type = t.ToolKey?.Type?.type_name,
                tool_name = t.ToolKey?.Tool?.tool_name,
                size_ref = t.ToolKey?.SizeRef?.size_ref,
                axle_type = t.AxleType?.axle_type,

                // ✅ spec fields
                t.overall_a,
                t.overall_b,
                t.overall_c,
                t.tolerance_a,
                t.tolerance_b,
                t.tolerance_c,
                t.f_shank_min,
                t.f_shank_max,
                t.chassis_span,
                t.chassis_span1,
                t.chassis_span2,
                t.b2b_min,
                t.b2b_max,
                t.h2h_min,
                t.h2h_max,

                // ✅ other fields
                t.source,
                t.is_original_spec,
                t.knurling_type
            }).ToList();

            return Ok(result);
        }

        // ✅ GET: api/ToolRefSpecs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOne(int id)
        {
            var t = await _context.ToolRefSpecs
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Tool)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Type)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.SizeRef)
                .Include(t => t.AxleType)
                .FirstOrDefaultAsync(t => t.tool_ref_spec_id == id);

            if (t == null)
                return NotFound();

            var result = new
            {
                t.tool_ref_spec_id,
                tool_type = t.ToolKey?.Type?.type_name,
                tool_name = t.ToolKey?.Tool?.tool_name,
                size_ref = t.ToolKey?.SizeRef?.size_ref,
                axle_type = t.AxleType?.axle_type,

                t.overall_a,
                t.overall_b,
                t.overall_c,
                t.tolerance_a,
                t.tolerance_b,
                t.tolerance_c,
                t.f_shank_min,
                t.f_shank_max,
                t.chassis_span,
                t.chassis_span1,
                t.chassis_span2,
                t.b2b_min,
                t.b2b_max,
                t.h2h_min,
                t.h2h_max,

                t.source,
                t.is_original_spec,
                t.knurling_type,
                t.description
            };

            return Ok(result);
        }


        // ✅ POST: api/ToolRefSpecs
        // [HttpPost]
        // public async Task<ActionResult<ToolRefSpec>> PostToolRefSpec(ToolRefSpec toolRefSpec)
        // {
        //     _context.ToolRefSpecs.Add(toolRefSpec);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetOne), new { id = toolRefSpec.tool_ref_spec_id }, toolRefSpec);
        // }


        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> PostToolRefSpec([FromBody] ToolRefSpec toolRefSpec)
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

            toolRefSpec.create_by = currentUserId;
            toolRefSpec.create_at = DateTime.Now;
            toolRefSpec.update_by = null;
            toolRefSpec.update_at = null;

            _context.ToolRefSpecs.Add(toolRefSpec);
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
                target_table = "toolRefSpecs",
                target_id = toolRefSpec.tool_ref_spec_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_ref_spec_id = toolRefSpec.tool_ref_spec_id,
                //     tool_key_id = toolRefSpec.tool_key_id,
                //     axle_type_id = toolRefSpec.axle_type_id,
                //     overall_a = toolRefSpec.overall_a,
                //     overall_b = toolRefSpec.overall_b,
                //     overall_c = toolRefSpec.overall_c,
                //     tolerance_a = toolRefSpec.tolerance_a,
                //     tolerance_b = toolRefSpec.tolerance_b,
                //     tolerance_c = toolRefSpec.tolerance_c,
                //     f_shank_min = toolRefSpec.f_shank_min,
                //     f_shank_max = toolRefSpec.f_shank_max,
                //     chassis_span = toolRefSpec.chassis_span,
                //     b2b_min = toolRefSpec.b2b_min,
                //     b2b_max = toolRefSpec.b2b_max,
                //     h2h_min = toolRefSpec.h2h_min,
                //     h2h_max = toolRefSpec.h2h_max,
                //     source = toolRefSpec.source,
                //     is_original_spec = toolRefSpec.is_original_spec,
                //     knurling_type = toolRefSpec.knurling_type,
                //     create_by = toolRefSpec.create_by,
                //     create_at = toolRefSpec.create_at
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
                toolRefSpec.tool_ref_spec_id,
                toolRefSpec.tool_key_id,
                toolRefSpec.axle_type_id,
                toolRefSpec.overall_a,
                toolRefSpec.overall_b,
                toolRefSpec.overall_c,
                toolRefSpec.tolerance_a,
                toolRefSpec.tolerance_b,
                toolRefSpec.tolerance_c,
                toolRefSpec.f_shank_min,
                toolRefSpec.f_shank_max,
                toolRefSpec.chassis_span,
                toolRefSpec.b2b_min,
                toolRefSpec.b2b_max,
                toolRefSpec.h2h_min,
                toolRefSpec.h2h_max,
                toolRefSpec.source,
                toolRefSpec.is_original_spec,
                toolRefSpec.knurling_type
            };

            return CreatedAtAction(nameof(GetOne), new { id = toolRefSpec.tool_ref_spec_id }, result);
        }



        // ✅ GET: api/ToolRefSpecs/check-duplicate
        [HttpGet("check-duplicate")]
        public async Task<ActionResult<object>> CheckDuplicateSpec(
            [FromQuery] int type_id,
            [FromQuery] int tool_id,
            [FromQuery] int size_ref_id,
            [FromQuery] int axle_type_id)
        {
            var exists = await _context.ToolRefSpecs
                .Include(r => r.ToolKey)
                .AnyAsync(r =>
                    r.ToolKey != null &&
                    r.ToolKey.type_id == type_id &&
                    r.ToolKey.tool_id == tool_id &&
                    r.ToolKey.size_ref_id == size_ref_id &&
                    r.axle_type_id == axle_type_id
                );

            return Ok(new { exists });
        }

        // GET: api/ToolRefSpecs/find?tool_key_id=1&axle_type_id=2
        [HttpGet("find")]
        public async Task<IActionResult> FindToolRefSpec(
            [FromQuery] int tool_key_id,
            [FromQuery] int axle_type_id)
        {
            Console.WriteLine($"🔍 [ToolRefSpecs/find] Called with tool_key_id={tool_key_id}, axle_type_id={axle_type_id}");

            var result = await _context.ToolRefSpecs
                .FirstOrDefaultAsync(r =>
                    r.tool_key_id == tool_key_id &&
                    r.axle_type_id == axle_type_id);

            if (result == null)
            {
                Console.WriteLine($"❌ [ToolRefSpecs/find] Not found for tool_key_id={tool_key_id}, axle_type_id={axle_type_id}");
                return NotFound();
            }

            Console.WriteLine($"✅ [ToolRefSpecs/find] Found tool_ref_spec_id={result.tool_ref_spec_id}");
            return Ok(result);
        }


        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutToolRefSpec(int id, [FromBody] ToolRefSpec toolRefSpec)
        // {
        //     if (id != toolRefSpec.tool_ref_spec_id)
        //         return BadRequest("tool_ref_spec_id mismatch");

        //     var existing = await _context.ToolRefSpecs.FindAsync(id);
        //     if (existing == null) return NotFound();

        //     // 🔐 อัปเดตเฉพาะ field ที่อนุญาต
        //     existing.overall_a = toolRefSpec.overall_a;
        //     existing.overall_b = toolRefSpec.overall_b;
        //     existing.overall_c = toolRefSpec.overall_c;
        //     existing.tolerance_a = toolRefSpec.tolerance_a;
        //     existing.tolerance_b = toolRefSpec.tolerance_b;
        //     existing.tolerance_c = toolRefSpec.tolerance_c;
        //     existing.f_shank_min = toolRefSpec.f_shank_min;
        //     existing.f_shank_max = toolRefSpec.f_shank_max;
        //     existing.b2b_min = toolRefSpec.b2b_min;
        //     existing.b2b_max = toolRefSpec.b2b_max;
        //     existing.h2h_min = toolRefSpec.h2h_min;
        //     existing.h2h_max = toolRefSpec.h2h_max;
        //     existing.chassis_span1 = toolRefSpec.chassis_span1;
        //     existing.chassis_span2 = toolRefSpec.chassis_span2;
        //     existing.source = toolRefSpec.source;

        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolRefSpec(int id, [FromBody] ToolRefSpec toolRefSpec)
        {
            if (id != toolRefSpec.tool_ref_spec_id)
                return BadRequest("tool_ref_spec_id mismatch");

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

            var existing = await _context.ToolRefSpecs
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.tool_ref_spec_id == id);

            if (existing == null)
                return NotFound("ToolRefSpec not found.");

            // Load entity for update
            var entity = await _context.ToolRefSpecs.FindAsync(id);

            // Update fields
            entity.overall_a = toolRefSpec.overall_a;
            entity.overall_b = toolRefSpec.overall_b;
            entity.overall_c = toolRefSpec.overall_c;
            entity.tolerance_a = toolRefSpec.tolerance_a;
            entity.tolerance_b = toolRefSpec.tolerance_b;
            entity.tolerance_c = toolRefSpec.tolerance_c;
            entity.f_shank_min = toolRefSpec.f_shank_min;
            entity.f_shank_max = toolRefSpec.f_shank_max;
            entity.b2b_min = toolRefSpec.b2b_min;
            entity.b2b_max = toolRefSpec.b2b_max;
            entity.h2h_min = toolRefSpec.h2h_min;
            entity.h2h_max = toolRefSpec.h2h_max;
            entity.chassis_span1 = toolRefSpec.chassis_span1;
            entity.chassis_span2 = toolRefSpec.chassis_span2;
            entity.source = toolRefSpec.source;
            entity.update_by = currentUserId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            // Prepare log
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
                    tool_ref_spec_id = existing.tool_ref_spec_id,
                    overall_a = existing.overall_a,
                    overall_b = existing.overall_b,
                    overall_c = existing.overall_c,
                    tolerance_a = existing.tolerance_a,
                    tolerance_b = existing.tolerance_b,
                    tolerance_c = existing.tolerance_c,
                    f_shank_min = existing.f_shank_min,
                    f_shank_max = existing.f_shank_max,
                    b2b_min = existing.b2b_min,
                    b2b_max = existing.b2b_max,
                    h2h_min = existing.h2h_min,
                    h2h_max = existing.h2h_max,
                    chassis_span1 = existing.chassis_span1,
                    chassis_span2 = existing.chassis_span2,
                    source = existing.source,
                    update_by = existing.update_by,
                    update_at = existing.update_at
                },
                @new = new
                {
                    tool_ref_spec_id = entity.tool_ref_spec_id,
                    overall_a = entity.overall_a,
                    overall_b = entity.overall_b,
                    overall_c = entity.overall_c,
                    tolerance_a = entity.tolerance_a,
                    tolerance_b = entity.tolerance_b,
                    tolerance_c = entity.tolerance_c,
                    f_shank_min = entity.f_shank_min,
                    f_shank_max = entity.f_shank_max,
                    b2b_min = entity.b2b_min,
                    b2b_max = entity.b2b_max,
                    h2h_min = entity.h2h_min,
                    h2h_max = entity.h2h_max,
                    chassis_span1 = entity.chassis_span1,
                    chassis_span2 = entity.chassis_span2,
                    source = entity.source,
                    update_by = entity.update_by,
                    update_at = entity.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "toolRefSpecs",
                target_id = entity.tool_ref_spec_id.ToString(),
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
                message = "ToolRefSpec updated successfully",
                tool_ref_spec_id = entity.tool_ref_spec_id
            };

            return Ok(result);
        }



        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolRefSpec(int id)
        // {
        //     var spec = await _context.ToolRefSpecs.FindAsync(id);
        //     if (spec == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.ToolRefSpecs.Remove(spec);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        // DELETE: api/ToolRefSpecs/{id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolRefSpec(int id)
        // {
        //     var refSpec = await _context.ToolRefSpecs.FindAsync(id);
        //     if (refSpec == null)
        //         return NotFound();

        //     // ✅ Check if tool_ref_spec_id is used in toolSpecs
        //     bool isUsedInToolSpecs = await _context.ToolSpecs
        //         .AnyAsync(ts => ts.tool_ref_spec_id == id);

        //     if (isUsedInToolSpecs)
        //     {
        //         return Conflict("Cannot delete: This original spec is being used in Tool Specs.");
        //     }

        //     _context.ToolRefSpecs.Remove(refSpec);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolRefSpec(int id)
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

            var refSpec = await _context.ToolRefSpecs
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.tool_ref_spec_id == id);

            if (refSpec == null)
                return NotFound("ToolRefSpec not found.");

            // ✅ Check if tool_ref_spec_id is used in toolSpecs
            bool isUsedInToolSpecs = await _context.ToolSpecs
                .AnyAsync(ts => ts.tool_ref_spec_id == id);

            if (isUsedInToolSpecs)
            {
                return Conflict("Cannot delete: This spec is being used in Tool Specs.");
            }

            // Load entity for delete
            var entity = await _context.ToolRefSpecs.FindAsync(id);
            if (entity != null)
            {
                _context.ToolRefSpecs.Remove(entity);
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
                target_table = "toolRefSpecs",
                target_id = refSpec.tool_ref_spec_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_ref_spec_id = refSpec.tool_ref_spec_id,
                //     tool_key_id = refSpec.tool_key_id,
                //     axle_type_id = refSpec.axle_type_id,
                //     overall_a = refSpec.overall_a,
                //     overall_b = refSpec.overall_b,
                //     overall_c = refSpec.overall_c,
                //     tolerance_a = refSpec.tolerance_a,
                //     tolerance_b = refSpec.tolerance_b,
                //     tolerance_c = refSpec.tolerance_c,
                //     f_shank_min = refSpec.f_shank_min,
                //     f_shank_max = refSpec.f_shank_max,
                //     b2b_min = refSpec.b2b_min,
                //     b2b_max = refSpec.b2b_max,
                //     h2h_min = refSpec.h2h_min,
                //     h2h_max = refSpec.h2h_max,
                //     chassis_span1 = refSpec.chassis_span1,
                //     chassis_span2 = refSpec.chassis_span2,
                //     source = refSpec.source,
                //     knurling_type = refSpec.knurling_type,
                //     is_original_spec = refSpec.is_original_spec,
                //     create_by = refSpec.create_by,
                //     create_at = refSpec.create_at,
                //     update_by = refSpec.update_by,
                //     update_at = refSpec.update_at
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
                message = "ToolRefSpec deleted successfully",
                tool_ref_spec_id = id
            });
        }


        // GET: api/ToolRefSpecs/check-original-usage/{tool_key_id}
        // [HttpGet("check-original-usage/{tool_key_id}")]
        // public async Task<IActionResult> CheckIfOriginalToolKeyIsUsed(int tool_key_id)
        // {
        //     var used = await (
        //         from spec in _context.ToolRefSpecs
        //         join toolSpec in _context.ToolSpecs on spec.tool_ref_spec_id equals toolSpec.tool_ref_spec_id
        //         where spec.tool_key_id == tool_key_id
        //         select toolSpec.tool_spec_id
        //     ).AnyAsync();

        //     return Ok(new { used });
        // }


        // ✅ แก้ให้ตรวจสอบเฉพาะ ref ที่ใช้ tool_key_id + axle_type_id เดียวกับ row ที่จะลบ
        // [HttpGet("check-original-usage")]
        // public async Task<IActionResult> CheckOriginalUsage([FromQuery] int tool_key_id, [FromQuery] int axle_type_id)
        // {
        //     // ToolRefSpecs ที่ใช้ original key นี้ + axle_type_id นี้
        //     var refSpecsUsed = await _context.ToolRefSpecs
        //         .AnyAsync(r => r.tool_key_id == tool_key_id && r.axle_type_id == axle_type_id);

        //     // ToolSpecs ที่ใช้ reference key ที่อ้างอิง tool_key_id นี้
        //     var toolSpecsUsed = await _context.ToolSpecs
        //         .AnyAsync(t => _context.ToolRefSpecs
        //             .Any(r => r.tool_ref_spec_id == t.tool_ref_spec_id &&
        //                       r.tool_key_id == tool_key_id &&
        //                       r.axle_type_id == axle_type_id));

        //     return Ok(new { used = refSpecsUsed || toolSpecsUsed });
        // }


        // [HttpGet("check-original-usage")]
        // public async Task<IActionResult> CheckOriginalUsage([FromQuery] int tool_key_id, [FromQuery] int axle_type_id)
        // {
        //     bool refSpecsUsed;
        //     bool toolSpecsUsed;

        //     if (axle_type_id == 0)
        //     {
        //         refSpecsUsed = await _context.ToolRefSpecs.AnyAsync(r => r.tool_key_id == tool_key_id);
        //         toolSpecsUsed = await _context.ToolSpecs
        //             .AnyAsync(t => _context.ToolRefSpecs
        //                 .Any(r => r.tool_ref_spec_id == t.tool_ref_spec_id && r.tool_key_id == tool_key_id));
        //     }
        //     else
        //     {
        //         refSpecsUsed = await _context.ToolRefSpecs.AnyAsync(r => r.tool_key_id == tool_key_id && r.axle_type_id == axle_type_id);
        //         toolSpecsUsed = await _context.ToolSpecs
        //             .AnyAsync(t => _context.ToolRefSpecs
        //                 .Any(r => r.tool_ref_spec_id == t.tool_ref_spec_id && r.tool_key_id == tool_key_id && r.axle_type_id == axle_type_id));
        //     }

        //     return Ok(new { used = refSpecsUsed || toolSpecsUsed });
        // }

        [HttpGet("check-original-usage")]
        public async Task<IActionResult> CheckOriginalUsage([FromQuery] int tool_ref_spec_id)
        {
            bool usedInToolSpecs = await _context.ToolSpecs
                .AnyAsync(ts => ts.tool_ref_spec_id == tool_ref_spec_id);

            return Ok(new { used = usedInToolSpecs });
        }

        [HttpGet("check-toolspec")]
        public async Task<IActionResult> CheckToolSpec(int tool_ref_spec_id)
        {
            var exists = await _context.ToolSpecs
                .AnyAsync(x => x.tool_ref_spec_id == tool_ref_spec_id);

            return Ok(new { used = exists });
        }

        [HttpGet("check-toolmachinemap")]
        public async Task<IActionResult> CheckToolMachineMap(int original_tool_key_id)
        {
            var exists = await _context.ToolMachineMaps
                .AnyAsync(x => x.tool_key_id == original_tool_key_id);

            return Ok(new { used = exists });
        }

        [HttpGet("check-toolpadmap")]
        public async Task<IActionResult> CheckToolPadMap(int original_tool_key_id)
        {
            var exists = await _context.ToolPadMaps
                .AnyAsync(x => x.tool_key_id == original_tool_key_id);

            return Ok(new { used = exists });
        }



        // [HttpGet("check-original-usage")]
        // public async Task<IActionResult> CheckOriginalUsage([FromQuery] int tool_ref_spec_id)
        // {
        //     try
        //     {
        //         // 1. หา tool_ref_spec record
        //         var toolRefSpec = await _context.ToolRefSpecs
        //             .FirstOrDefaultAsync(x => x.tool_ref_spec_id == tool_ref_spec_id);

        //         if (toolRefSpec == null)
        //         {
        //             return Ok(new
        //             {
        //                 used = false,
        //                 isUsedInToolSpecs = false,
        //                 isUsedInToolMachineMap = false,
        //                 isUsedInToolPadMap = false,
        //                 message = "OriginalSpec not found."
        //             });
        //         }

        //         var originalToolKeyId = toolRefSpec.tool_key_id;

        //         var linkedToolKeyAlls = new List<int>();

        //         // ถ้า originalToolKeyId มีค่า → เพิ่มเข้าไปก่อน
        //         if (originalToolKeyId.HasValue)
        //         {
        //             linkedToolKeyAlls.Add(originalToolKeyId.Value);
        //         }

        //         // หา toolKeyAlls ที่อ้างอิง original นี้
        //         var additionalKeys = await _context.ToolKeyAlls
        //             .Where(x => x.source_original_key_id == originalToolKeyId)
        //             .Select(x => x.tool_key_id)
        //             .ToListAsync();

        //         linkedToolKeyAlls.AddRange(additionalKeys);

        //         // ถ้าไม่มี linkedToolKey เลย → ใช้ค่า dummy เพื่อกัน empty list
        //         if (!linkedToolKeyAlls.Any())
        //         {
        //             linkedToolKeyAlls.Add(-9999); // ค่าไม่จริงที่ไม่มีใน table
        //         }

        //         // เช็ค ToolMachineMap
        //         var isUsedInToolMachineMap = await _context.ToolMachineMaps
        //             .AnyAsync(x =>
        //                 x.tool_key_id.HasValue &&
        //                 linkedToolKeyAlls.Contains(x.tool_key_id.Value));

        //         // เช็ค ToolPadMap
        //         var isUsedInToolPadMap = await _context.ToolPadMaps
        //             .AnyAsync(x =>
        //                 x.tool_key_id.HasValue &&
        //                 linkedToolKeyAlls.Contains(x.tool_key_id.Value));

        //         // เช็ค ToolSpecs
        //         var isUsedInToolSpecs = await _context.ToolSpecs
        //             .AnyAsync(x => x.tool_ref_spec_id == tool_ref_spec_id);

        //         var used = isUsedInToolSpecs || isUsedInToolMachineMap || isUsedInToolPadMap;

        //         return Ok(new
        //         {
        //             used,
        //             isUsedInToolSpecs,
        //             isUsedInToolMachineMap,
        //             isUsedInToolPadMap,
        //             message = used
        //                 ? "This OriginalSpec is still referenced in one or more related tables."
        //                 : "This OriginalSpec is not referenced and can be deleted."
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             message = "An error occurred while checking OriginalSpec usage.",
        //             details = ex.Message
        //         });
        //     }
        // }





        [HttpGet("byToolKeyId/{toolKeyId}")]
        public async Task<IActionResult> GetByToolKeyId(int toolKeyId)
        {
            var refs = await _context.ToolRefSpecs
                .Where(r => r.tool_key_id == toolKeyId)
                .ToListAsync();

            return Ok(refs);
        }

    }
}
