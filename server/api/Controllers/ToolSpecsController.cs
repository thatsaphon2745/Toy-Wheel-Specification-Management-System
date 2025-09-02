using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models; // ✅ เปลี่ยนตาม namespace ที่ใช้
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs; // ✅ ใส่ไว้ด้านบน ToolSpecsController.cs
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
// using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolSpecsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolSpecsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: api/ToolSpecs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolSpec>>> GetToolSpecs()
        {
            return await _context.ToolSpecs.ToListAsync();
        }

        // ✅ GET: api/ToolSpecs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToolSpec>> GetToolSpec(int id)
        {
            var toolSpec = await _context.ToolSpecs.FindAsync(id);

            if (toolSpec == null)
            {
                return NotFound();
            }

            return toolSpec;
        }

        // ✅ POST: api/ToolSpecs
        // [HttpPost]
        // public async Task<ActionResult<ToolSpec>> PostToolSpec(ToolSpec toolSpec)
        // {
        //     _context.ToolSpecs.Add(toolSpec);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetToolSpec), new { id = toolSpec.tool_spec_id }, toolSpec);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> PostToolSpec([FromBody] ToolSpec toolSpec)
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

            toolSpec.create_by = currentUserId;
            toolSpec.create_at = DateTime.Now;
            toolSpec.update_by = null;
            toolSpec.update_at = null;

            _context.ToolSpecs.Add(toolSpec);
            await _context.SaveChangesAsync();

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
                target_table = "toolSpecs",
                target_id = toolSpec.tool_spec_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     tool_spec_id = toolSpec.tool_spec_id,
                //     ref_key_id = toolSpec.ref_key_id,
                //     tool_ref_spec_id = toolSpec.tool_ref_spec_id,
                //     position_type_id = toolSpec.position_type_id,
                //     axle_type_id = toolSpec.axle_type_id,
                //     chassis_span_override = toolSpec.chassis_span_override,
                //     create_by = toolSpec.create_by,
                //     create_at = toolSpec.create_at
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
                toolSpec.tool_spec_id,
                toolSpec.ref_key_id,
                toolSpec.tool_ref_spec_id,
                toolSpec.position_type_id,
                toolSpec.axle_type_id,
                toolSpec.chassis_span_override
            };

            return CreatedAtAction(nameof(GetToolSpec), new { id = toolSpec.tool_spec_id }, result);
        }


        // // ✅ PUT: api/ToolSpecs/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutToolSpec(int id, ToolSpec toolSpec)
        // {
        //     if (id != toolSpec.tool_spec_id)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(toolSpec).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!ToolSpecExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }

        //     return NoContent();
        // }

        // ✅ PUT: api/ToolSpecs/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutToolSpec(int id, [FromBody] ToolSpec updatedSpec)
        // {
        //     if (id != updatedSpec.tool_spec_id)
        //         return BadRequest("ID mismatch");

        //     var existing = await _context.ToolSpecs.FindAsync(id);
        //     if (existing == null)
        //         return NotFound();

        //     // ✅ อัปเดตเฉพาะฟิลด์ที่แก้ (เช่น chassis_span_override)
        //     existing.chassis_span_override = updatedSpec.chassis_span_override;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //         return NoContent();
        //     }
        //     catch (DbUpdateConcurrencyException ex)
        //     {
        //         return StatusCode(500, $"Update failed: {ex.Message}");
        //     }
        // }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutToolSpec(int id, [FromBody] ChassisSpanUpdateDto dto)
        // {
        //     var existing = await _context.ToolSpecs.FindAsync(id);
        //     if (existing == null)
        //         return NotFound();

        //     existing.chassis_span_override = dto.chassis_span_override;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //         return NoContent();
        //     }
        //     catch (DbUpdateConcurrencyException ex)
        //     {
        //         return StatusCode(500, $"Update failed: {ex.Message}");
        //     }
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolSpec(int id, [FromBody] ChassisSpanUpdateDto dto)
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

            var existing = await _context.ToolSpecs
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.tool_spec_id == id);

            if (existing == null)
                return NotFound("ToolSpec not found.");

            var entity = await _context.ToolSpecs.FindAsync(id);

            // save old value for logging
            var oldValue = existing.chassis_span_override;

            entity.chassis_span_override = dto.chassis_span_override;
            entity.update_by = currentUserId;
            entity.update_at = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, $"Update failed: {ex.Message}");
            }

            // Log
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
                tool_spec_id = entity.tool_spec_id,
                old_value = oldValue,
                new_value = dto.chassis_span_override,
                update_by = entity.update_by,
                update_at = entity.update_at
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "toolSpecs",
                target_id = entity.tool_spec_id.ToString(),
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
                message = "ToolSpec updated successfully",
                tool_spec_id = entity.tool_spec_id,
                chassis_span_override = entity.chassis_span_override
            };

            return Ok(result);
        }


        // ✅ DELETE: api/ToolSpecs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolSpec(int id)
        {
            var toolSpec = await _context.ToolSpecs.FindAsync(id);
            if (toolSpec == null)
            {
                return NotFound();
            }

            _context.ToolSpecs.Remove(toolSpec);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ToolSpecExists(int id)
        {
            return _context.ToolSpecs.Any(e => e.tool_spec_id == id);
        }



        // // ✅ DELETE: api/ToolSpecs/delete-ref-spec?tool_spec_id=xx&ref_key_id=yy
        // [HttpDelete("delete-ref-spec")]
        // public async Task<IActionResult> DeleteReferenceSpec([FromQuery] int tool_spec_id, [FromQuery] int ref_key_id)
        // {
        //     using var transaction = await _context.Database.BeginTransactionAsync();
        //     try
        //     {
        //         // 🔥 Step 1: ลบ toolSpecs (แถวที่เลือก)
        //         var toolSpec = await _context.ToolSpecs.FindAsync(tool_spec_id);
        //         if (toolSpec != null)
        //         {
        //             _context.ToolSpecs.Remove(toolSpec);
        //         }

        //         // 🔥 Step 2: ตรวจสอบว่ามี toolSpecs ตัวอื่นใช้ ref_key_id นี้ไหม
        //         var otherSpecsUsingRef = await _context.ToolSpecs
        //             .Where(s => s.ref_key_id == ref_key_id && s.tool_spec_id != tool_spec_id)
        //             .AnyAsync();

        //         if (!otherSpecsUsingRef)
        //         {
        //             // ✅ ไม่มีใครใช้อีกแล้ว → ลบ toolKeyReferences
        //             var refKey = await _context.ToolKeyReferences.FindAsync(ref_key_id);
        //             if (refKey != null)
        //             {
        //                 _context.ToolKeyReferences.Remove(refKey);
        //             }

        //             // ✅ และลบ toolKeyAlls ที่อิง ref นี้
        //             var toolAlls = await _context.ToolKeyAlls
        //                 .Where(t => t.source_ref_key_id == ref_key_id)
        //                 .ToListAsync();

        //             if (toolAlls.Any())
        //             {
        //                 _context.ToolKeyAlls.RemoveRange(toolAlls);
        //             }
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return NoContent();
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, $"Delete failed: {ex.Message}");
        //     }
        // }


        // [HttpDelete("delete-ref-spec")]
        // public async Task<IActionResult> DeleteReferenceSpec([FromQuery] int tool_spec_id, [FromQuery] int ref_key_id)
        // {
        //     using var transaction = await _context.Database.BeginTransactionAsync();
        //     var deletedLogs = new List<string>();

        //     try
        //     {
        //         // 🔥 Step 1: ลบ toolSpecs (แถวที่เลือก)
        //         var toolSpec = await _context.ToolSpecs.FindAsync(tool_spec_id);
        //         if (toolSpec != null)
        //         {
        //             _context.ToolSpecs.Remove(toolSpec);
        //             deletedLogs.Add($"Deleted from ToolSpecs (tool_spec_id: {tool_spec_id})");
        //         }

        //         // 🔥 Step 2: ตรวจสอบว่ามี toolSpecs ตัวอื่นใช้ ref_key_id นี้ไหม
        //         var otherSpecsUsingRef = await _context.ToolSpecs
        //             .Where(s => s.ref_key_id == ref_key_id && s.tool_spec_id != tool_spec_id)
        //             .AnyAsync();

        //         if (!otherSpecsUsingRef)
        //         {
        //             // ✅ ไม่มีใครใช้อีกแล้ว → ลบ toolKeyReferences
        //             var refKey = await _context.ToolKeyReferences.FindAsync(ref_key_id);
        //             if (refKey != null)
        //             {
        //                 _context.ToolKeyReferences.Remove(refKey);
        //                 deletedLogs.Add($"Deleted from ToolKeyReferences (ref_key_id: {ref_key_id})");
        //             }

        //             // ✅ และลบ toolKeyAlls ที่อิง ref นี้
        //             var toolAlls = await _context.ToolKeyAlls
        //                 .Where(t => t.source_ref_key_id == ref_key_id)
        //                 .ToListAsync();

        //             if (toolAlls.Any())
        //             {
        //                 _context.ToolKeyAlls.RemoveRange(toolAlls);
        //                 deletedLogs.Add($"Deleted {toolAlls.Count} row(s) from ToolKeyAlls (source_ref_key_id: {ref_key_id})");
        //             }
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return Ok(new
        //         {
        //             message = "Delete completed.",
        //             deleted = deletedLogs
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, new
        //         {
        //             message = "Delete failed.",
        //             error = ex.Message,
        //             deleted = deletedLogs
        //         });
        //     }
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("delete-ref-spec")]
        public async Task<IActionResult> DeleteReferenceSpec([FromQuery] int tool_spec_id, [FromQuery] int ref_key_id)
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            var deletedLogs = new List<string>();

            try
            {
                // ✅ Step 1: Find and delete ToolSpec
                var entity = await _context.ToolSpecs.FindAsync(tool_spec_id);
                if (entity != null)
                {
                    _context.ToolSpecs.Remove(entity);
                    deletedLogs.Add($"Deleted from ToolSpecs (tool_spec_id: {tool_spec_id})");
                }
                else
                {
                    return NotFound(new
                    {
                        message = $"ToolSpec not found (tool_spec_id: {tool_spec_id})"
                    });
                }

                // ✅ Step 2: Check for other specs using same ref_key_id
                var otherSpecsUsingRef = await _context.ToolSpecs
                    .AnyAsync(s => s.ref_key_id == ref_key_id && s.tool_spec_id != tool_spec_id);

                if (!otherSpecsUsingRef)
                {
                    var entityRef = await _context.ToolKeyReferences.FindAsync(ref_key_id);
                    if (entityRef != null)
                    {
                        _context.ToolKeyReferences.Remove(entityRef);
                        deletedLogs.Add($"Deleted from ToolKeyReferences (ref_key_id: {ref_key_id})");
                    }

                    var toolAlls = await _context.ToolKeyAlls
                        .Where(t => t.source_ref_key_id == ref_key_id)
                        .ToListAsync();

                    if (toolAlls.Any())
                    {
                        _context.ToolKeyAlls.RemoveRange(toolAlls);
                        deletedLogs.Add($"Deleted {toolAlls.Count} row(s) from ToolKeyAlls (source_ref_key_id: {ref_key_id})");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ Log
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
                    target_table = "toolSpecs + related",
                    target_id = tool_spec_id.ToString(),
                    // details = JsonConvert.SerializeObject(new
                    // {
                    //     tool_spec_id,
                    //     ref_key_id,
                    //     deleted = deletedLogs
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
                    message = "Delete completed.",
                    deleted = deletedLogs
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Delete failed.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message,
                    deleted = deletedLogs
                });
            }
        }

        [HttpGet("check-toolmachinemap-ref")]
        public async Task<IActionResult> CheckToolMachineMapRef(int tool_key_id)
        {
            try
            {
                var exists = await _context.ToolMachineMaps
                    .AnyAsync(x => x.tool_key_id == tool_key_id);

                return Ok(new { used = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error checking ToolMachineMap usage for ref tool.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("check-toolpadmap-ref")]
        public async Task<IActionResult> CheckToolPadMapRef(int tool_key_id)
        {
            try
            {
                var exists = await _context.ToolPadMaps
                    .AnyAsync(x => x.tool_key_id == tool_key_id);

                return Ok(new { used = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error checking ToolPadMap usage for ref tool.",
                    details = ex.Message
                });
            }
        }



    }
}
