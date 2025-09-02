using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PadBrassMapController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public PadBrassMapController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: api/PadBrassMap
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetAll()
        // {
        //     var result = await _context.PadBrassMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.Brass)
        //         .Select(m => new
        //         {
        //             m.pad_brass_id,
        //             pad_name = m.Pad.pad_name,
        //             brass_no = m.Brass.brass_no
        //         })
        //         .ToListAsync();

        //     return Ok(result);
        // }

        // ✅ GET: api/PadBrassMap
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetAll()
        // {
        //     var result = await _context.PadBrassMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.Brass)
        //         .Select(m => new
        //         {
        //             m.pad_brass_id,
        //             m.pad_id,                         // ✅ เพิ่มตรงนี้
        //             pad_name = m.Pad.pad_name,
        //             m.brass_id,                       // ✅ เพิ่มด้วยถ้าอยากได้
        //             brass_no = m.Brass.brass_no
        //         })
        //         .ToListAsync();

        //     return Ok(result);
        // }

        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetAll()
        // {
        //     var result = await (
        //         from m in _context.PadBrassMaps
        //         join pad in _context.Pads on m.pad_id equals pad.pad_id
        //         join brass in _context.Brasses on m.brass_id equals brass.brass_id
        //         join r in _context.Requests
        //             .Where(r =>
        //                 r.target_table == "PadBrassMap" &&
        //                 r.request_status == "Pending" &&
        //                 (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //             on m.pad_brass_id equals r.target_pk_id into requestJoin
        //         from request in requestJoin.DefaultIfEmpty() // LEFT JOIN
        //         select new
        //         {
        //             m.pad_brass_id,
        //             m.pad_id,
        //             pad_name = pad.pad_name,
        //             m.brass_id,
        //             brass_no = brass.brass_no,
        //             pending_request = request.request_type // null, "UPDATE", or "DELETE"
        //         }
        //     ).ToListAsync();

        //     return Ok(result);
        // }

        // [HttpGet]
        // public async Task<IActionResult> GetAll(
        //     [FromQuery] string? pad_name,
        //     [FromQuery] string? brass_no,
        //     [FromQuery] string? pending_request
        // )
        // {
        //     // 🔹 แปลง comma-separated string เป็น List<string>
        //     var padNames = string.IsNullOrWhiteSpace(pad_name)
        //         ? new List<string>()
        //         : pad_name.Split(',').Select(x => x.Trim()).ToList();

        //     var brassNos = string.IsNullOrWhiteSpace(brass_no)
        //         ? new List<string>()
        //         : brass_no.Split(',').Select(x => x.Trim()).ToList();

        //     var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //         ? new List<string>()
        //         : pending_request.Split(',').Select(x => x.Trim().ToUpper()).ToList();

        //     var query = from m in _context.PadBrassMaps
        //                 join pad in _context.Pads on m.pad_id equals pad.pad_id
        //                 join brass in _context.Brasses on m.brass_id equals brass.brass_id
        //                 join r in _context.Requests
        //                     .Where(r =>
        //                         r.target_table == "PadBrassMap" &&
        //                         r.request_status == "Pending" &&
        //                         (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //                     on m.pad_brass_id equals r.target_pk_id into requestJoin
        //                 from request in requestJoin.DefaultIfEmpty()
        //                 select new
        //                 {
        //                     m.pad_brass_id,
        //                     m.pad_id,
        //                     pad_name = pad.pad_name,
        //                     m.brass_id,
        //                     brass_no = brass.brass_no,
        //                     pending_request = request.request_type
        //                 };

        //     // 🔍 Filter เฉพาะถ้ามีค่า
        //     if (padNames.Any())
        //         query = query.Where(x => padNames.Contains(x.pad_name));

        //     if (brassNos.Any())
        //         query = query.Where(x => brassNos.Contains(x.brass_no));

        //     if (pendingRequests.Any())
        //         query = query.Where(x => pendingRequests.Contains(x.pending_request ?? ""));

        //     var result = await query.ToListAsync();
        //     return Ok(result);
        // }

        // [HttpGet]
        // public async Task<IActionResult> GetAll(
        //      [FromQuery] string? pad_name,
        //      [FromQuery] string? brass_no,
        //      [FromQuery] string? pending_request
        //  )
        // {
        //     var padNames = string.IsNullOrWhiteSpace(pad_name)
        //         ? new List<string>()
        //         : pad_name.Split(',').Select(x => x.Trim()).ToList();

        //     var brassNos = string.IsNullOrWhiteSpace(brass_no)
        //         ? new List<string>()
        //         : brass_no.Split(',').Select(x => x.Trim()).ToList();

        //     var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //         ? new List<string>()
        //         : pending_request.Split(',').Select(x => x.Trim().ToUpper()).ToList();

        //     // ✅ ดึง Requests ก่อนแยกต่างหาก (ไม่เอาไว้ใน join)
        //     var pendingRequestsDict = await _context.Requests
        //         .Where(r =>
        //             r.target_table == "PadBrassMap" &&
        //             r.request_status == "Pending" &&
        //             (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //         .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //     // ✅ Query หลักแบบไม่มี CTE หรือ WITH
        //     var query = from m in _context.PadBrassMaps
        //                 join pad in _context.Pads on m.pad_id equals pad.pad_id
        //                 join brass in _context.Brasses on m.brass_id equals brass.brass_id
        //                 select new
        //                 {
        //                     m.pad_brass_id,
        //                     m.pad_id,
        //                     pad_name = pad.pad_name,
        //                     m.brass_id,
        //                     brass_no = brass.brass_no,
        //                     pending_request = (string?)null // จะ override ทีหลัง
        //                 };

        //     var result = await query.ToListAsync();

        //     // ✅ ใส่ pending_request แบบ manual หลังจาก query เสร็จ
        //     var final = result
        //         .Select(x => new
        //         {
        //             x.pad_brass_id,
        //             x.pad_id,
        //             x.pad_name,
        //             x.brass_id,
        //             x.brass_no,
        //             pending_request = pendingRequestsDict.TryGetValue(x.pad_brass_id, out var pr) ? pr : null
        //         });

        //     // ✅ Filter ใน memory
        //     if (padNames.Any())
        //         final = final.Where(x => padNames.Contains(x.pad_name));

        //     if (brassNos.Any())
        //         final = final.Where(x => brassNos.Contains(x.brass_no));

        //     if (pendingRequests.Any())
        //     {
        //         final = final.Where(x =>
        //             // ✅ กรณี null จริง หรือ frontend ส่งมาเป็น "(Blanks)"
        //             ((x.pending_request == null || x.pending_request == "(Blanks)") && pendingRequests.Contains("null")) ||

        //             // ✅ กรณีมีค่า และค่าตรงกับ filter
        //             (x.pending_request != null && pendingRequests.Contains(x.pending_request))
        //         );
        //     }



        //     return Ok(final.ToList());
        // }

        // [HttpGet]
        // public async Task<IActionResult> GetAll(
        //     [FromQuery] string? pad_name,
        //     [FromQuery] string? brass_no,
        //     [FromQuery] string? pending_request
        // )
        // {
        //     var padNames = string.IsNullOrWhiteSpace(pad_name)
        //         ? new List<string>()
        //         : pad_name.Split(',').Select(x => x.Trim()).ToList();

        //     var brassNos = string.IsNullOrWhiteSpace(brass_no)
        //         ? new List<string>()
        //         : brass_no.Split(',').Select(x => x.Trim()).ToList();

        //     // ✅ ใช้ .ToLower() เพื่อให้ตรงกับ "null" จาก frontend
        //     var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //         ? new List<string>()
        //         : pending_request.Split(',').Select(x => x.Trim().ToLower()).ToList();

        //     // ✅ ดึง Pending Request
        //     var pendingRequestsDict = await _context.Requests
        //         .Where(r =>
        //             r.target_table == "PadBrassMap" &&
        //             r.request_status == "Pending" &&
        //             (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //         .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //     var query = from m in _context.PadBrassMaps
        //                 join pad in _context.Pads on m.pad_id equals pad.pad_id
        //                 join brass in _context.Brasses on m.brass_id equals brass.brass_id
        //                 select new
        //                 {
        //                     m.pad_brass_id,
        //                     m.pad_id,
        //                     pad_name = pad.pad_name,
        //                     m.brass_id,
        //                     brass_no = brass.brass_no,
        //                     pending_request = (string?)null
        //                 };

        //     var result = await query.ToListAsync();

        //     var final = result
        //         .Select(x => new
        //         {
        //             x.pad_brass_id,
        //             x.pad_id,
        //             x.pad_name,
        //             x.brass_id,
        //             x.brass_no,
        //             pending_request = pendingRequestsDict.TryGetValue(x.pad_brass_id, out var pr) ? pr : null
        //         });

        //     // ✅ Apply filters
        //     if (padNames.Any())
        //         final = final.Where(x => padNames.Contains(x.pad_name));

        //     if (brassNos.Any())
        //         final = final.Where(x => brassNos.Contains(x.brass_no));

        //     if (pendingRequests.Any())
        //     {
        //         final = final.Where(x =>
        //             (x.pending_request == null && pendingRequests.Contains("null")) ||
        //             (x.pending_request != null && pendingRequests.Contains(x.pending_request.ToLower()))
        //         );
        //     }

        //     return Ok(final.ToList());
        // }

        // [HttpGet]
        // public async Task<IActionResult> GetAll(
        //     [FromQuery] List<string> pad_name,
        //     [FromQuery] List<string> brass_no,
        //     [FromQuery] List<string> pending_request
        // )
        // {
        //     // ✅ Clean & Normalize
        //     var padNames = pad_name
        //         .Where(x => !string.IsNullOrWhiteSpace(x))
        //         .Select(x => x.Trim())
        //         .ToList();

        //     var brassNos = brass_no
        //         .Where(x => !string.IsNullOrWhiteSpace(x))
        //         .Select(x => x.Trim())
        //         .ToList();

        //     var pendingRequestsRaw = pending_request
        //         .Where(x => !string.IsNullOrWhiteSpace(x))
        //         .Select(x => x.Trim().ToLower())
        //         .ToList();

        //     // ✅ ดึง Pending Request
        //     var pendingRequestsDict = await _context.Requests
        //         .Where(r =>
        //             r.target_table == "PadBrassMap" &&
        //             r.request_status == "Pending" &&
        //             (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //         .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //     var query = from m in _context.PadBrassMaps
        //                 join pad in _context.Pads on m.pad_id equals pad.pad_id
        //                 join brass in _context.Brasses on m.brass_id equals brass.brass_id
        //                 select new
        //                 {
        //                     m.pad_brass_id,
        //                     m.pad_id,
        //                     pad_name = pad.pad_name,
        //                     m.brass_id,
        //                     brass_no = brass.brass_no,
        //                     pending_request = (string?)null
        //                 };

        //     var result = await query.ToListAsync();

        //     var final = result
        //         .Select(x => new
        //         {
        //             x.pad_brass_id,
        //             x.pad_id,
        //             x.pad_name,
        //             x.brass_id,
        //             x.brass_no,
        //             pending_request = pendingRequestsDict.TryGetValue(x.pad_brass_id, out var pr) ? pr : null
        //         });

        //     // ✅ Apply filters
        //     if (padNames.Any())
        //         final = final.Where(x => padNames.Contains(x.pad_name));

        //     if (brassNos.Any())
        //         final = final.Where(x => brassNos.Contains(x.brass_no));

        //     if (pendingRequestsRaw.Any())
        //     {
        //         final = final.Where(x =>
        //             (x.pending_request == null && pendingRequestsRaw.Contains("null")) ||
        //             (x.pending_request != null && pendingRequestsRaw.Contains(x.pending_request.ToLower()))
        //         );
        //     }

        //     return Ok(final.ToList());
        // }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] List<string> pad_name,
            [FromQuery] List<string> brass_no,
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
            var brassNos = brass_no.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            var descriptions = description.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList(); // ✅
            var pendingRequestsRaw = pending_request.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLower()).ToList();

            // ✅ Parse date filters
            DateTime? createdStart = DateTime.TryParse(created_at_start, out var cs) ? cs : null;
            DateTime? createdEnd = DateTime.TryParse(created_at_end, out var ce) ? ce : null;
            DateTime? updatedStart = DateTime.TryParse(updated_at_start, out var us) ? us : null;
            DateTime? updatedEnd = DateTime.TryParse(updated_at_end, out var ue) ? ue : null;

            if (createdEnd != null)
                createdEnd = createdEnd.Value.Date.AddDays(1).AddTicks(-1);
            if (updatedEnd != null)
                updatedEnd = updatedEnd.Value.Date.AddDays(1).AddTicks(-1);

            // ✅ Load user dictionary: user_id → employee_id
            var userDict = await _context.Users
                .Select(u => new { u.user_id, u.employee_id })
                .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

            // ✅ Convert employee_id → user_id
            // var createByEmployees = create_by.Select(x => x.Trim()).ToList();
            // var updateByEmployees = update_by.Select(x => x.Trim()).ToList();

            var createByEmployees = create_by
                .Select(x => x.Trim().ToLower() == "null" ? "" : x.Trim())
                .ToList();

            var updateByEmployees = update_by
                .Select(x => x.Trim().ToLower() == "null" ? "" : x.Trim())
                .ToList();


            var createByUserIds = userDict.Where(x => createByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();
            var updateByUserIds = userDict.Where(x => updateByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();

            // ✅ Load pending request
            var pendingRequestsDict = await _context.Requests
                .Where(r =>
                    r.target_table == "PadBrassMap" &&
                    r.request_status == "Pending" &&
                    (r.request_type == "UPDATE" || r.request_type == "DELETE"))
                .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

            // ✅ Query
            var query = from m in _context.PadBrassMaps
                        join pad in _context.Pads on m.pad_id equals pad.pad_id
                        join brass in _context.Brasses on m.brass_id equals brass.brass_id
                        where
                            (createdStart == null || m.create_at >= createdStart.Value) &&
                            (createdEnd == null || m.create_at <= createdEnd.Value) &&
                            (updatedStart == null || m.update_at >= updatedStart.Value) &&
                            (updatedEnd == null || m.update_at <= updatedEnd.Value)
                        select new
                        {
                            m.pad_brass_id,
                            m.pad_id,
                            pad_name = pad.pad_name,
                            m.brass_id,
                            brass_no = brass.brass_no,
                            m.description, // ✅ ดึง description จาก PadBrassMap
                            m.create_by,
                            m.update_by,
                            m.create_at,
                            m.update_at
                        };

            var result = await query.ToListAsync();

            // ✅ Map + Apply create_by, update_by filter (with support for "null")
            var final = result
                .Select(x => new
                {
                    x.pad_brass_id,
                    x.pad_id,
                    x.pad_name,
                    x.brass_id,
                    x.brass_no,
                    x.description, // ✅ ส่งออกไป frontend
                    create_by = userDict.ContainsKey(x.create_by)
                        ? userDict[x.create_by]
                        : null,
                    x.create_at,
                    x.update_at,
                    update_by = x.update_by.HasValue && userDict.ContainsKey(x.update_by.Value)
                        ? userDict[x.update_by.Value]
                        : null,
                    pending_request = pendingRequestsDict.TryGetValue(x.pad_brass_id, out var pr) ? pr : null
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

            // ✅ Additional filters
            if (padNames.Any())
                final = final.Where(x => padNames.Contains(x.pad_name));

            if (brassNos.Any())
                final = final.Where(x => brassNos.Contains(x.brass_no));

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





        // ✅ GET: api/PadBrassMap/pivot
        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetPivot()
        // {
        //     var data = await _context.PadBrassMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.Brass)
        //         .Where(m => m.Pad != null && m.Brass != null)
        //         .ToListAsync();

        //     var result = data
        //         .GroupBy(m => m.Pad!.pad_name)
        //         .Select(g => new
        //         {
        //             pad_name = g.Key ?? "",
        //             brass_no = string.Join(", ", g
        //                 .Select(x => x.Brass.brass_no)
        //                 .Where(brass => !string.IsNullOrEmpty(brass))
        //                 .Distinct()
        //                 .OrderBy(x => x))
        //         })
        //         .OrderBy(x => x.pad_name)
        //         .ToList();

        //     return Ok(result);
        // }

        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetPivot(
        //     [FromQuery] string? pad_name,
        //     [FromQuery] string? brass_no
        // )
        // {
        //     var padNames = string.IsNullOrWhiteSpace(pad_name)
        //         ? new List<string>()
        //         : pad_name.Split(',').Select(x => x.Trim()).ToList();

        //     // ✅ filter brass_no แบบ exact match หลัง group
        //     var targetBrassSets = string.IsNullOrWhiteSpace(brass_no)
        //         ? new List<string>()
        //         : brass_no.Split(',').Select(x => x.Trim()).ToList();

        //     var data = await _context.PadBrassMaps
        //         .Include(m => m.Pad)
        //         .Include(m => m.Brass)
        //         .Where(m => m.Pad != null && m.Brass != null)
        //         .ToListAsync();

        //     if (padNames.Any())
        //         data = data.Where(x => padNames.Contains(x.Pad.pad_name)).ToList();

        //     var grouped = data
        //         .GroupBy(m => m.Pad!.pad_name)
        //         .Select(g =>
        //         {
        //             var brassList = g
        //                 .Select(x => x.Brass.brass_no)
        //                 .Where(x => !string.IsNullOrEmpty(x))
        //                 .Distinct()
        //                 .OrderBy(x => x)
        //                 .ToList();

        //             var brassJoined = string.Join(", ", brassList);

        //             return new
        //             {
        //                 pad_name = g.Key ?? "",
        //                 brass_no = brassJoined,
        //                 brass_list = brassList
        //             };
        //         });

        //     // ✅ filter จาก group แล้ว
        //     var result = grouped
        //         .Where(x =>
        //             !targetBrassSets.Any() || // ไม่มี filter = ผ่านหมด
        //             string.Join(", ", targetBrassSets.OrderBy(x => x)) == x.brass_no
        //         )
        //         .Select(x => new
        //         {
        //             x.pad_name,
        //             x.brass_no
        //         })
        //         .OrderBy(x => x.pad_name)
        //         .ToList();

        //     return Ok(result);
        // }

        [HttpGet("pivot")]
        public async Task<ActionResult<IEnumerable<object>>> GetPivot(
            [FromQuery] List<string> pad_name,
            [FromQuery] List<string> brass_no
        )
        {
            // ✅ Clean & normalize pad_name
            var padNames = pad_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            // ✅ Clean & normalize brass_no (for exact match)
            var targetBrassSets = brass_no
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            var data = await _context.PadBrassMaps
                .Include(m => m.Pad)
                .Include(m => m.Brass)
                .Where(m => m.Pad != null && m.Brass != null)
                .ToListAsync();

            if (padNames.Any())
                data = data.Where(x => padNames.Contains(x.Pad.pad_name)).ToList();

            var grouped = data
                .GroupBy(m => m.Pad!.pad_name)
                .Select(g =>
                {
                    var brassList = g
                        .Select(x => x.Brass.brass_no)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var brassJoined = string.Join(", ", brassList);

                    return new
                    {
                        pad_name = g.Key ?? "",
                        brass_no = brassJoined,
                        brass_list = brassList
                    };
                });

            // ✅ filter จาก group แล้วด้วย exact match
            var result = grouped
                .Where(x =>
                    !targetBrassSets.Any() || // ไม่มี filter = ผ่านหมด
                                              // targetBrassSets.Count == x.brass_list.Count &&
                                              // targetBrassSets.All(b => x.brass_list.Contains(b))
                    targetBrassSets.Contains(x.brass_no)
                )
                .Select(x => new
                {
                    x.pad_name,
                    x.brass_no
                })
                .OrderBy(x => x.pad_name)
                .ToList();

            return Ok(result);
        }





        // ✅ POST: api/PadBrassMap
        [HttpPost]
        public async Task<ActionResult<PadBrassMap>> Post(PadBrassMap map)
        {
            _context.PadBrassMaps.Add(map);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = map.pad_brass_id }, map);
        }

        // POST: api/PadBrassMap/by-id
        // [HttpPost("by-id")]
        // public async Task<ActionResult<object>> PostById([FromBody] PadBrassMap map)
        // {
        //     if (!_context.Pads.Any(p => p.pad_id == map.pad_id))
        //         return BadRequest("Invalid pad_id.");

        //     if (!_context.Brasses.Any(b => b.brass_id == map.brass_id))
        //         return BadRequest("Invalid brass_id.");

        //     // ❗ ป้องกัน insert ซ้ำ
        //     bool exists = await _context.PadBrassMaps.AnyAsync(m =>
        //         m.pad_id == map.pad_id && m.brass_id == map.brass_id);
        //     if (exists)
        //         return Conflict("This mapping already exists.");

        //     _context.PadBrassMaps.Add(map);
        //     await _context.SaveChangesAsync();

        //     var result = new
        //     {
        //         map.pad_brass_id,
        //         pad_name = (await _context.Pads.FindAsync(map.pad_id))?.pad_name,
        //         brass_no = (await _context.Brasses.FindAsync(map.brass_id))?.brass_no
        //     };

        //     return CreatedAtAction(nameof(GetAll), new { id = map.pad_brass_id }, result);
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost("by-id")]
        public async Task<ActionResult<object>> PostById([FromBody] PadBrassMap map)
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

            if (!_context.Pads.Any(p => p.pad_id == map.pad_id))
                return BadRequest("Invalid pad_id.");

            if (!_context.Brasses.Any(b => b.brass_id == map.brass_id))
                return BadRequest("Invalid brass_id.");

            // ❗ ป้องกัน insert ซ้ำ
            bool exists = await _context.PadBrassMaps.AnyAsync(m =>
                m.pad_id == map.pad_id && m.brass_id == map.brass_id);
            if (exists)
                return Conflict("This mapping already exists.");

            map.create_by = currentUserId;
            map.create_at = DateTime.Now;
            map.update_by = null;
            map.update_at = null;

            _context.PadBrassMaps.Add(map);
            await _context.SaveChangesAsync();

            var pad = await _context.Pads.FindAsync(map.pad_id);
            var brass = await _context.Brasses.FindAsync(map.brass_id);

            var result = new
            {
                map.pad_brass_id,
                pad_name = pad?.pad_name,
                brass_no = brass?.brass_no
            };

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

            // === INSERT LOG ===
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = HttpContext.Request.Path;
            var method = HttpContext.Request.Method;

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "INSERT",
                target_table = "padBrassMap",
                target_id = map.pad_brass_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_brass_id = map.pad_brass_id,
                //     pad_id = map.pad_id,
                //     pad_name = pad?.pad_name,
                //     brass_id = map.brass_id,
                //     brass_no = brass?.brass_no,
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

            return CreatedAtAction(nameof(GetAll), new { id = map.pad_brass_id }, result);
        }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> Put(int id, PadBrassMap map)
        // {
        //     if (id != map.pad_brass_id)
        //         return BadRequest("pad_brass_id mismatch");

        //     var existingMap = await _context.PadBrassMaps.FindAsync(id);
        //     if (existingMap == null)
        //         return NotFound("Mapping not found");

        //     // ❌ ตรวจสอบว่ามี mapping เดิม (pad_id, brass_id) ซ้ำอยู่หรือยัง
        //     bool isDuplicate = await _context.PadBrassMaps.AnyAsync(m =>
        //         m.pad_id == existingMap.pad_id &&
        //         m.brass_id == map.brass_id &&
        //         m.pad_brass_id != id // ต้องไม่ใช่ record เดิม
        //     );

        //     if (isDuplicate)
        //         return Conflict("This pad-brass mapping already exists.");

        //     // ✅ อัปเดตเฉพาะ brass_id
        //     existingMap.brass_id = map.brass_id;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         return StatusCode(500, $"Update failed: {ex.Message}");
        //     }

        //     return Ok(new
        //     {
        //         existingMap.pad_brass_id,
        //         pad_id = existingMap.pad_id,
        //         brass_id = existingMap.brass_id
        //     });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, PadBrassMap map)
        {
            if (id != map.pad_brass_id)
                return BadRequest("pad_brass_id mismatch");

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var currentUserId = int.Parse(userIdClaim.Value);
            var username = User.Identity?.Name;

            // ===== ดึงข้อมูลเดิม =====
            var existingMap = await _context.PadBrassMaps
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.pad_brass_id == id);

            if (existingMap == null)
                return NotFound("Mapping not found");

            // ===== ตรวจ duplicate =====
            bool isDuplicate = await _context.PadBrassMaps.AnyAsync(m =>
                m.pad_id == existingMap.pad_id &&
                m.brass_id == map.brass_id &&
                m.pad_brass_id != id
            );

            if (isDuplicate)
                return Conflict("This pad-brass mapping already exists.");

            // ===== Update เฉพาะ brass_id =====
            map.pad_id = existingMap.pad_id; // ห้ามแก้ pad_id
            map.create_by = existingMap.create_by;
            map.create_at = existingMap.create_at;
            map.update_by = currentUserId;
            map.update_at = DateTime.Now;

            _context.Entry(map).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PadBrassMapExists(id))
                    return NotFound();
                else
                    throw;
            }

            // ====== เตรียมข้อมูลสำหรับ Logs ======

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

            // ============ Load ชื่อจริง pad_name และ brass_no ============

            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == existingMap.pad_id);

            var oldBrass = await _context.Brasses
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(b => b.brass_id == existingMap.brass_id);

            var newBrass = await _context.Brasses
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(b => b.brass_id == map.brass_id);

            var details = new
            {
                old = new
                {
                    pad_brass_id = existingMap.pad_brass_id,
                    pad_id = existingMap.pad_id,
                    pad_name = pad?.pad_name,
                    brass_id = existingMap.brass_id,
                    brass_no = oldBrass?.brass_no,
                    update_by = existingMap.update_by,
                    update_at = existingMap.update_at
                },
                @new = new
                {
                    pad_brass_id = map.pad_brass_id,
                    pad_id = map.pad_id,
                    pad_name = pad?.pad_name, // pad_id ไม่เปลี่ยน → ใช้ชื่อเดิม
                    brass_id = map.brass_id,
                    brass_no = newBrass?.brass_no,
                    update_by = map.update_by,
                    update_at = map.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "padBrassMap",
                target_id = map.pad_brass_id.ToString(),
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



        // ✅ DELETE: api/PadBrassMap/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     var map = await _context.PadBrassMaps.FindAsync(id);
        //     if (map == null)
        //         return NotFound();

        //     _context.PadBrassMaps.Remove(map);
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

            var map = await _context.PadBrassMaps
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.pad_brass_id == id);

            if (map == null)
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

            var pad = await _context.Pads.FindAsync(map.pad_id);
            var brass = await _context.Brasses.FindAsync(map.brass_id);

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "DELETE",
                target_table = "padBrassMap",
                target_id = map.pad_brass_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     pad_brass_id = map.pad_brass_id,
                //     pad_id = map.pad_id,
                //     pad_name = pad?.pad_name,
                //     brass_id = map.brass_id,
                //     brass_no = brass?.brass_no,
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
                // response_status = 204,  // NoContent
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // === DELETE PadBrassMap ===
            _context.PadBrassMaps.Remove(map);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        private bool PadBrassMapExists(int id)
        {
            return _context.PadBrassMaps.Any(e => e.pad_brass_id == id);
        }

    }
}
