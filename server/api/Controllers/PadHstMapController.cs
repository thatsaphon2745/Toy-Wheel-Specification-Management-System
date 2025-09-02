using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;
using api.Services;


namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PadHstMapController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        private readonly PadHstService _padHstService;


        public PadHstMapController(
            MbkBarbell9Context context,
            PadHstService padHstService
        )
        {
            _context = context;
            _padHstService = padHstService;
        }


        // GET: api/PadHstMap
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetAll()
        // {
        //     var result = await _context.PadHstMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.HstType)
        //         .Select(m => new
        //         {
        //             m.pad_hst_id,
        //             m.pad_id,
        //             pad_name = m.Pad.pad_name,
        //             m.hst_type_id,
        //             hst_type = m.HstType.hst_type
        //         })
        //         .ToListAsync();

        //     return Ok(result);
        // }
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetAll(
        //     [FromQuery] string? pad_name,
        //     [FromQuery] string? hst_type,
        //     [FromQuery] string? pending_request
        // )
        // {
        //     var padNames = string.IsNullOrWhiteSpace(pad_name)
        //         ? new List<string>()
        //         : pad_name.Split(',').Select(x => x.Trim()).ToList();

        //     var hstTypes = string.IsNullOrWhiteSpace(hst_type)
        //         ? new List<string>()
        //         : hst_type.Split(',').Select(x => x.Trim()).ToList();

        //     var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //         ? new List<string>()
        //         : pending_request.Split(',').Select(x => x.Trim().ToLower()).ToList();

        //     // 🔍 ดึง pending request (เฉพาะ UPDATE หรือ DELETE) จาก Request table
        //     var pendingRequestsDict = await _context.Requests
        //         .Where(r =>
        //             r.target_table == "PadHstMap" &&
        //             r.request_status == "Pending" &&
        //             (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //         .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //     // 🔍 Query หลัก
        //     var result = await _context.PadHstMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.HstType)
        //         .Select(m => new
        //         {
        //             m.pad_hst_id,
        //             m.pad_id,
        //             pad_name = m.Pad.pad_name,
        //             m.hst_type_id,
        //             hst_type = m.HstType.hst_type,
        //             pending_request = (string?)null
        //         })
        //         .ToListAsync();

        //     // 🧠 เติม pending_request จาก dictionary
        //     var final = result
        //         .Select(x => new
        //         {
        //             x.pad_hst_id,
        //             x.pad_id,
        //             x.pad_name,
        //             x.hst_type_id,
        //             x.hst_type,
        //             pending_request = pendingRequestsDict.TryGetValue(x.pad_hst_id, out var pr) ? pr : null
        //         });

        //     // ✅ Apply filters
        //     if (padNames.Any())
        //         final = final.Where(x => padNames.Contains(x.pad_name));

        //     if (hstTypes.Any())
        //         final = final.Where(x => hstTypes.Contains(x.hst_type));

        //     if (pendingRequests.Any())
        //     {
        //         final = final.Where(x =>
        //             (x.pending_request == null && pendingRequests.Contains("null")) ||
        //             (x.pending_request != null && pendingRequests.Contains(x.pending_request.ToLower()))
        //         );
        //     }

        //     return Ok(final.ToList());
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll(
            [FromQuery] List<string> pad_name,
            [FromQuery] List<string> hst_type,
            [FromQuery] List<string> pending_request,
            [FromQuery] string? created_at_start,
            [FromQuery] string? created_at_end,
            [FromQuery] string? updated_at_start,
            [FromQuery] string? updated_at_end,
            [FromQuery] List<string> create_by,
            [FromQuery] List<string> update_by,
            [FromQuery] List<string> description // ✅ เพิ่มตรงนี้
        )
        {
            // ✅ Normalize filters
            var padNames = pad_name.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            var hstTypes = hst_type.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            var descriptions = description.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList(); // ✅
            var pendingRequestsRaw = pending_request.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLower()).ToList();

            // ✅ Parse date filters
            DateTime? createdStart = DateTime.TryParse(created_at_start, out var cs) ? cs : null;
            DateTime? createdEnd = DateTime.TryParse(created_at_end, out var ce) ? ce.Date.AddDays(1).AddTicks(-1) : null;
            DateTime? updatedStart = DateTime.TryParse(updated_at_start, out var us) ? us : null;
            DateTime? updatedEnd = DateTime.TryParse(updated_at_end, out var ue) ? ue.Date.AddDays(1).AddTicks(-1) : null;

            // ✅ Load user_id → employee_id
            var userDict = await _context.Users
                .Select(u => new { u.user_id, u.employee_id })
                .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

            var createByEmployees = create_by.Select(x => x.Trim().ToLower() == "null" ? "" : x.Trim()).ToList();
            var updateByEmployees = update_by.Select(x => x.Trim().ToLower() == "null" ? "" : x.Trim()).ToList();

            var createByUserIds = userDict.Where(x => createByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();
            var updateByUserIds = userDict.Where(x => updateByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();

            // ✅ Load pending requests
            var pendingRequestsDict = await _context.Requests
                .Where(r =>
                    r.target_table == "PadHstMap" &&
                    r.request_status == "Pending" &&
                    (r.request_type == "UPDATE" || r.request_type == "DELETE"))
                .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

            // ✅ Main Query
            var query = from m in _context.PadHstMaps
                        join pad in _context.Pads on m.pad_id equals pad.pad_id
                        join hst in _context.HstTypes on m.hst_type_id equals hst.hst_type_id
                        where
                            (createdStart == null || m.create_at >= createdStart.Value) &&
                            (createdEnd == null || m.create_at <= createdEnd.Value) &&
                            (updatedStart == null || m.update_at >= updatedStart.Value) &&
                            (updatedEnd == null || m.update_at <= updatedEnd.Value)
                        select new
                        {
                            m.pad_hst_id,
                            m.pad_id,
                            pad_name = pad.pad_name,
                            m.hst_type_id,
                            hst_type = hst.hst_type,
                            m.description, // ✅ ดึง description จาก PadBrassMap
                            m.create_by,
                            m.update_by,
                            m.create_at,
                            m.update_at
                        };

            var result = await query.ToListAsync();

            // ✅ Map user_id → employee_id + pending_request
            var final = result
                .Select(x => new
                {
                    x.pad_hst_id,
                    x.pad_id,
                    x.pad_name,
                    x.hst_type_id,
                    x.hst_type,
                    x.description, // ✅ ส่งออกไป frontend
                    create_by = userDict.ContainsKey(x.create_by) ? userDict[x.create_by] : null,
                    update_by = x.update_by.HasValue && userDict.ContainsKey(x.update_by.Value) ? userDict[x.update_by.Value] : null,
                    x.create_at,
                    x.update_at,
                    pending_request = pendingRequestsDict.TryGetValue(x.pad_hst_id, out var pr) ? pr : null
                })
                .Where(x =>
                    (createByEmployees.Count == 0 || (
                        (x.create_by == null && createByEmployees.Contains("")) ||
                        (x.create_by != null && createByEmployees.Contains(x.create_by))
                    )) &&
                    (updateByEmployees.Count == 0 || (
                        (x.update_by == null && updateByEmployees.Contains("")) ||
                        (x.update_by != null && updateByEmployees.Contains(x.update_by))
                    ))
                );

            // ✅ Apply additional filters
            if (padNames.Any())
                final = final.Where(x => padNames.Contains(x.pad_name));

            if (hstTypes.Any())
                final = final.Where(x => hstTypes.Contains(x.hst_type));

            if (descriptions.Any()) // ✅ Filter description
            {
                final = final.Where(x =>
                    (x.description == null && descriptions.Contains("null")) ||
                    (x.description != null && descriptions.Contains(x.description))
                );
            }

            if (pendingRequestsRaw.Any())
            {
                final = final.Where(x =>
                    (x.pending_request == null && pendingRequestsRaw.Contains("null")) ||
                    (x.pending_request != null && pendingRequestsRaw.Contains(x.pending_request.ToLower()))
                );
            }

            return Ok(final.ToList());
        }


        // GET: api/PadHstMap/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOne(int id)
        {
            var map = await _context.PadHstMaps
                .Include(m => m.Pad)
                .Include(m => m.HstType)
                .Where(m => m.pad_hst_id == id)
                .Select(m => new
                {
                    m.pad_hst_id,
                    pad_name = m.Pad.pad_name,
                    hst_type = m.HstType.hst_type
                })
                .FirstOrDefaultAsync();

            if (map == null)
                return NotFound();

            return Ok(map);
        }

        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetPivotedPadHst()
        // {
        //     var data = await _context.PadHstMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.HstType)
        //         .ToListAsync();

        //     var pivot = data
        //         .Where(m => m.Pad != null && m.HstType != null)
        //         .GroupBy(m => m.Pad!.pad_name)
        //         .Select(g => new Dictionary<string, object>
        //         {
        //     { "PAD_NAME", g.Key ?? "" },
        //     { "HST", g.Any(x => x.HstType!.hst_type.ToUpper() == "HST") ? 1 : 0 },
        //     { "RIM", g.Any(x => x.HstType!.hst_type.ToUpper() == "RIM") ? 1 : 0 },
        //     { "INNER", g.Any(x => x.HstType!.hst_type.ToUpper() == "INNER") ? 1 : 0 },
        //     { "EXTRA_RIM", g.Any(x => x.HstType!.hst_type.ToUpper() == "EXTRA_RIM") ? 1 : 0 }
        //         })
        //         .ToList();

        //     return Ok(pivot);
        // }

        [HttpGet("pivot")]
        public async Task<ActionResult<IEnumerable<object>>> GetPivotedPadHst(
            [FromQuery] string? pad_name,
            [FromQuery] int? HST,
            [FromQuery] int? RIM,
            [FromQuery] int? INNER,
            [FromQuery] int? EXTRA_RIM
        )
        {
            var padNames = string.IsNullOrWhiteSpace(pad_name)
                ? new List<string>()
                : pad_name.Split(',').Select(x => x.Trim()).ToList();

            var data = await _context.PadHstMaps
                .Include(m => m.Pad)
                .Include(m => m.HstType)
                .Where(m => m.Pad != null && m.HstType != null)
                .ToListAsync();

            if (padNames.Any())
                data = data.Where(x => padNames.Contains(x.Pad!.pad_name)).ToList();

            var pivoted = data
                .GroupBy(m => m.Pad!.pad_name)
                .Select(g =>
                {
                    var hstSet = g
                        .Select(x => x.HstType!.hst_type.ToUpper())
                        .Distinct()
                        .ToHashSet();

                    return new
                    {
                        PAD_NAME = g.Key ?? "",
                        HST = hstSet.Contains("HST") ? 1 : 0,
                        RIM = hstSet.Contains("RIM") ? 1 : 0,
                        INNER = hstSet.Contains("INNER") ? 1 : 0,
                        EXTRA_RIM = hstSet.Contains("EXTRA_RIM") ? 1 : 0
                    };
                })
                .ToList();

            // ✅ กรองเฉพาะ type ที่ส่งมาเท่านั้น
            var result = pivoted.Where(x =>
                (!HST.HasValue || x.HST == HST.Value) &&
                (!RIM.HasValue || x.RIM == RIM.Value) &&
                (!INNER.HasValue || x.INNER == INNER.Value) &&
                (!EXTRA_RIM.HasValue || x.EXTRA_RIM == EXTRA_RIM.Value)
            ).OrderBy(x => x.PAD_NAME).ToList();

            return Ok(result);
        }





        // POST: api/PadHstMap
        [HttpPost]
        public async Task<ActionResult<PadHstMap>> Post(PadHstMap map)
        {
            _context.PadHstMaps.Add(map);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = map.pad_hst_id }, map);
        }

        // ✅ POST: api/PadHstMap/by-id
        // [HttpPost("by-id")]
        // public async Task<ActionResult<object>> PostById([FromBody] PadHstMap map)
        // {
        //     // เช็คว่า pad_id และ hst_type_id มีอยู่จริง
        //     if (!_context.Pads.Any(p => p.pad_id == map.pad_id))
        //         return BadRequest("Invalid pad_id.");

        //     if (!_context.HstTypes.Any(h => h.hst_type_id == map.hst_type_id))
        //         return BadRequest("Invalid hst_type_id.");

        //     _context.PadHstMaps.Add(map);
        //     await _context.SaveChangesAsync();

        //     // ส่งผลลัพธ์แบบรวมข้อมูล string
        //     var result = new
        //     {
        //         map.pad_hst_id,
        //         pad_name = (await _context.Pads.FindAsync(map.pad_id))?.pad_name,
        //         hst_type = (await _context.HstTypes.FindAsync(map.hst_type_id))?.hst_type
        //     };

        //     return CreatedAtAction(nameof(GetOne), new { id = map.pad_hst_id }, result);
        // }


        [Authorize(Roles = "admin,editor")]
        [HttpPost("by-id")]
        public async Task<ActionResult<object>> PostById([FromBody] PadHstMap map)
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

            // ===== ตรวจ foreign key =====

            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == map.pad_id);
            if (pad == null)
                return BadRequest("Invalid pad_id.");

            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == map.hst_type_id);
            if (hstType == null)
                return BadRequest("Invalid hst_type_id.");

            // ===== ตรวจซ้ำ =====

            bool exists = await _context.PadHstMaps.AnyAsync(m =>
                m.pad_id == map.pad_id &&
                m.hst_type_id == map.hst_type_id);

            if (exists)
                return Conflict("This mapping already exists.");

            // ===== บันทึก =====

            map.create_by = currentUserId;
            map.create_at = DateTime.Now;
            map.update_by = null;
            map.update_at = null;

            _context.PadHstMaps.Add(map);
            await _context.SaveChangesAsync();

            var result = new
            {
                map.pad_hst_id,
                pad_name = pad?.pad_name,
                hst_type = hstType?.hst_type
            };

            // ====== PARSE OS ======

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

            // ====== INSERT LOG ======

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "padHstMap",
                target_id = map.pad_hst_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_hst_id = map.pad_hst_id,
                //     pad_id = map.pad_id,
                //     pad_name = pad?.pad_name,
                //     hst_type_id = map.hst_type_id,
                //     hst_type = hstType?.hst_type,
                //     create_by = map.create_by,
                //     create_at = map.create_at
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

            return CreatedAtAction(nameof(GetOne), new { id = map.pad_hst_id }, result);
        }

        // PUT: api/PadHstMap/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> Put(int id, [FromBody] PadHstMap incoming)
        // {
        //     if (id != incoming.pad_hst_id)
        //         return BadRequest("Mismatched ID");

        //     var existing = await _context.PadHstMaps.FindAsync(id);
        //     if (existing == null)
        //         return NotFound();

        //     // ✅ อัปเดตเฉพาะ hst_type_id
        //     existing.hst_type_id = incoming.hst_type_id;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //         return NoContent();
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         return StatusCode(500, $"Update failed: {ex.Message}");
        //     }
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PadHstMap incoming)
        {
            if (id != incoming.pad_hst_id)
                return BadRequest("pad_hst_id mismatch");

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ===== ดึงข้อมูลเดิม =====
            var existing = await _context.PadHstMaps
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.pad_hst_id == id);

            if (existing == null)
                return NotFound("Mapping not found");

            // ===== ตรวจ foreign key =====

            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == incoming.hst_type_id);

            if (hstType == null)
                return BadRequest("Invalid hst_type_id.");

            // ===== ตรวจ duplicate =====
            bool isDuplicate = await _context.PadHstMaps.AnyAsync(m =>
                m.pad_id == existing.pad_id &&
                m.hst_type_id == incoming.hst_type_id &&
                m.pad_hst_id != id
            );

            if (isDuplicate)
                return Conflict("This pad-hst mapping already exists.");

            // ===== Update =====

            var updated = await _context.PadHstMaps.FindAsync(id);
            updated.hst_type_id = incoming.hst_type_id;
            updated.update_by = currentUserId;
            updated.update_at = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PadHstMapExists(id))
                    return NotFound();
                else
                    throw;
            }

            // ===== Load ข้อมูลจริงเพื่อเขียน Log =====

            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == existing.pad_id);

            var oldHstType = await _context.HstTypes
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(h => h.hst_type_id == existing.hst_type_id);

            var newHstType = await _context.HstTypes
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(h => h.hst_type_id == incoming.hst_type_id);

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

            var details = new
            {
                old = new
                {
                    pad_hst_id = existing.pad_hst_id,
                    pad_id = existing.pad_id,
                    pad_name = pad?.pad_name,
                    hst_type_id = existing.hst_type_id,
                    hst_type = oldHstType?.hst_type,
                    update_by = existing.update_by,
                    update_at = existing.update_at
                },
                @new = new
                {
                    pad_hst_id = updated.pad_hst_id,
                    pad_id = updated.pad_id,
                    pad_name = pad?.pad_name,
                    hst_type_id = updated.hst_type_id,
                    hst_type = newHstType?.hst_type,
                    update_by = updated.update_by,
                    update_at = updated.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "padHstMap",
                target_id = updated.pad_hst_id.ToString(),
                // details = JsonConvert.SerializeObject(details),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 204,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/PadHstMap/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     var map = await _context.PadHstMaps.FindAsync(id);
        //     if (map == null)
        //         return NotFound();

        //     _context.PadHstMaps.Remove(map);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ===== ดึงข้อมูลเดิม =====
            var map = await _context.PadHstMaps
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.pad_hst_id == id);

            if (map == null)
                return NotFound("Mapping not found");

            // ===== Load ข้อมูลจริง สำหรับ log =====
            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == map.pad_id);

            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == map.hst_type_id);

            // ===== Prepare Log Info =====
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
                target_table = "padHstMap",
                target_id = map.pad_hst_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_hst_id = map.pad_hst_id,
                //     pad_id = map.pad_id,
                //     pad_name = pad?.pad_name,
                //     hst_type_id = map.hst_type_id,
                //     hst_type = hstType?.hst_type,
                //     create_by = map.create_by,
                //     create_at = map.create_at,
                //     update_by = map.update_by,
                //     update_at = map.update_at
                // }),
                // ip_address = ip,
                // device = device,
                // os_info = os,
                // endpoint_url = endpoint,
                // http_method = method,
                // response_status = 204,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // ===== DELETE PadHstMap =====
            var entity = await _context.PadHstMaps.FindAsync(id);
            if (entity != null)
            {
                _context.PadHstMaps.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }


        private bool PadHstMapExists(int id)
        {
            return _context.PadHstMaps.Any(e => e.pad_hst_id == id);
        }

        [HttpGet("can-delete/{id}")]
        [Authorize]
        public async Task<IActionResult> CanDeletePadHst(int id)
        {
            var canDelete = await _padHstService.CanDeletePadHstAsync(id);

            if (!canDelete)
            {
                return Ok(new
                {
                    canDelete = false,
                    reason = "Cannot delete PadHstMap because it is still used by tools."
                });
            }

            return Ok(new { canDelete = true });
        }


    }
}
