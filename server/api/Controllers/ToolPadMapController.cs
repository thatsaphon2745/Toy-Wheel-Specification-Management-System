using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;


namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolPadMapController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolPadMapController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: /api/ToolPadMap/raw
        // [HttpGet("raw")]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolPadMapRaw()
        // {
        //     var raw = await _context.ToolPadMaps
        //         .Include(x => x.ToolKey).ThenInclude(k => k.Type)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.Tool)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.TypeRef)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.ToolRef)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.SizeRef)
        //         .Include(x => x.Pad)
        //         .Include(x => x.HstType)
        //         .ToListAsync();

        //     var result = raw.Select(x => new
        //     {
        //         map_id = x.map_id, // ✅ เพิ่มบรรทัดนี้
        //         tool_type = x.ToolKey?.Type?.type_name,
        //         tool_name = x.ToolKey?.Tool?.tool_name,
        //         type_ref = x.ToolKey?.TypeRef?.type_name,
        //         tool_ref = x.ToolKey?.ToolRef?.tool_name,
        //         size_ref = x.ToolKey?.SizeRef?.size_ref,
        //         pad_name = x.Pad?.pad_name,
        //         hst_type = x.HstType?.hst_type
        //     });

        //     return Ok(result);
        // }

        //         [HttpGet("raw")]
        //         public async Task<ActionResult<IEnumerable<object>>> GetToolPadMapRaw(
        //     [FromQuery] string? tool_name,
        //     [FromQuery] string? tool_type,
        //     [FromQuery] string? type_ref,
        //     [FromQuery] string? tool_ref,
        //     [FromQuery] string? size_ref,
        //     [FromQuery] string? pad_name,
        //     [FromQuery] string? hst_type,
        //     [FromQuery] string? pending_request
        // )
        //         {
        //             // 🔹 แปลง string ให้เป็น list
        //             var toolNames = string.IsNullOrWhiteSpace(tool_name) ? new List<string>() : tool_name.Split(',').Select(x => x.Trim()).ToList();
        //             var toolTypes = string.IsNullOrWhiteSpace(tool_type) ? new List<string>() : tool_type.Split(',').Select(x => x.Trim()).ToList();
        //             var typeRefs = string.IsNullOrWhiteSpace(type_ref) ? new List<string>() : type_ref.Split(',').Select(x => x.Trim()).ToList();
        //             var toolRefs = string.IsNullOrWhiteSpace(tool_ref) ? new List<string>() : tool_ref.Split(',').Select(x => x.Trim()).ToList();
        //             var sizeRefs = string.IsNullOrWhiteSpace(size_ref) ? new List<string>() : size_ref.Split(',').Select(x => x.Trim()).ToList();
        //             var padNames = string.IsNullOrWhiteSpace(pad_name) ? new List<string>() : pad_name.Split(',').Select(x => x.Trim()).ToList();
        //             var hstTypes = string.IsNullOrWhiteSpace(hst_type) ? new List<string>() : hst_type.Split(',').Select(x => x.Trim()).ToList();
        //             var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //                 ? new List<string>()
        //                 : pending_request.Split(',').Select(x => x.Trim().ToLower()).ToList();

        //             // 🔸 ดึงคำร้อง pending สำหรับ ToolPadMap
        //             var pendingRequestsDict = await _context.Requests
        //                 .Where(r =>
        //                     r.target_table == "ToolPadMap" &&
        //                     r.request_status == "Pending" &&
        //                     (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //                 .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //             // 🔹 ดึงข้อมูลหลัก
        //             var raw = await _context.ToolPadMaps
        //                 .Include(x => x.ToolKey).ThenInclude(k => k.Type)
        //                 .Include(x => x.ToolKey).ThenInclude(k => k.Tool)
        //                 .Include(x => x.ToolKey).ThenInclude(k => k.TypeRef)
        //                 .Include(x => x.ToolKey).ThenInclude(k => k.ToolRef)
        //                 .Include(x => x.ToolKey).ThenInclude(k => k.SizeRef)
        //                 .Include(x => x.Pad)
        //                 .Include(x => x.HstType)
        //                 .ToListAsync();

        //             var result = raw.Select(x => new
        //             {
        //                 map_id = x.map_id,
        //                 tool_type = x.ToolKey?.Type?.type_name,
        //                 tool_name = x.ToolKey?.Tool?.tool_name,
        //                 type_ref = x.ToolKey?.TypeRef?.type_name,
        //                 tool_ref = x.ToolKey?.ToolRef?.tool_name,
        //                 size_ref = x.ToolKey?.SizeRef?.size_ref,
        //                 pad_name = x.Pad?.pad_name,
        //                 hst_type = x.HstType?.hst_type,
        //                 pending_request = pendingRequestsDict.TryGetValue(x.map_id, out var pr) ? pr : null
        //             });

        //             // ✅ Apply Filters with null-safe support
        //             if (toolNames.Any())
        //                 result = result.Where(x => toolNames.Contains(x.tool_name ?? "null"));

        //             if (toolTypes.Any())
        //                 result = result.Where(x => toolTypes.Contains(x.tool_type ?? "null"));

        //             if (typeRefs.Any())
        //                 result = result.Where(x => typeRefs.Contains(x.type_ref ?? "null"));

        //             if (toolRefs.Any())
        //                 result = result.Where(x => toolRefs.Contains(x.tool_ref ?? "null"));

        //             if (sizeRefs.Any())
        //                 result = result.Where(x => sizeRefs.Contains(x.size_ref ?? "null"));

        //             if (padNames.Any())
        //                 result = result.Where(x => padNames.Contains(x.pad_name ?? "null"));

        //             if (hstTypes.Any())
        //                 result = result.Where(x => hstTypes.Contains(x.hst_type ?? "null"));

        //             if (pendingRequests.Any())
        //             {
        //                 result = result.Where(x =>
        //                     (x.pending_request == null && pendingRequests.Contains("null")) ||
        //                     (x.pending_request != null && pendingRequests.Contains(x.pending_request.ToLower())));
        //             }

        //             return Ok(result.ToList());
        //         }


        [HttpGet("{id}/after-hst")]
        public async Task<IActionResult> GetAfterHstImage(int id)
        {
            var row = await _context.ToolPadMaps.FindAsync(id);
            if (row == null || row.pic_after_hst == null) return NotFound();

            var fileName = string.IsNullOrEmpty(row.pic_after_hst_file_name)
                ? "after_hst.png"
                : row.pic_after_hst_file_name;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";

            return File(row.pic_after_hst, contentType, fileName);
        }


        [HttpGet("raw")]
        public async Task<ActionResult<IEnumerable<object>>> GetToolPadMapRaw(
            [FromQuery] List<string> tool_name,
            [FromQuery] List<string> tool_type,
            [FromQuery] List<string> type_ref,
            [FromQuery] List<string> tool_ref,
            [FromQuery] List<string> size_ref,
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
            // ✅ แปลงค่าทุกตัวที่อาจมี "null", "(Blanks)" ให้เป็น ""
            var toolNames = tool_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolTypes = tool_type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var typeRefs = type_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolRefs = tool_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var sizeRefs = size_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var padNames = pad_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var hstTypes = hst_type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var descriptions = description
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
               .ToList();

            var pendingRequestsRaw = pending_request
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "null" : x.Trim().ToLower())
                .ToList();

            // ✅ Parse date
            DateTime? createdStart = DateTime.TryParse(created_at_start, out var cs) ? cs : null;
            DateTime? createdEnd = DateTime.TryParse(created_at_end, out var ce) ? ce.Date.AddDays(1).AddTicks(-1) : null;
            DateTime? updatedStart = DateTime.TryParse(updated_at_start, out var us) ? us : null;
            DateTime? updatedEnd = DateTime.TryParse(updated_at_end, out var ue) ? ue.Date.AddDays(1).AddTicks(-1) : null;

            // ✅ Load user map
            var userDict = await _context.Users
                .Select(u => new { u.user_id, u.employee_id })
                .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

            var createByEmployees = create_by
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var updateByEmployees = update_by
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var createByUserIds = userDict.Where(x => createByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();
            var updateByUserIds = userDict.Where(x => updateByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();

            // ✅ Load pending request
            var pendingRequestsDict = await _context.Requests
                .Where(r =>
                    r.target_table == "ToolPadMap" &&
                    r.request_status == "Pending" &&
                    (r.request_type == "UPDATE" || r.request_type == "DELETE"))
                .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

            // ✅ Load data
            var maps = await _context.ToolPadMaps
                .Include(x => x.ToolKey).ThenInclude(k => k.Type)
                .Include(x => x.ToolKey).ThenInclude(k => k.Tool)
                .Include(x => x.ToolKey).ThenInclude(k => k.TypeRef)
                .Include(x => x.ToolKey).ThenInclude(k => k.ToolRef)
                .Include(x => x.ToolKey).ThenInclude(k => k.SizeRef)
                .Include(x => x.Pad)
                .Include(x => x.HstType)
                .ToListAsync();

            var result = maps
                .Where(m =>
                    (createdStart == null || m.create_at >= createdStart) &&
                    (createdEnd == null || m.create_at <= createdEnd) &&
                    (updatedStart == null || m.update_at >= updatedStart) &&
                    (updatedEnd == null || m.update_at <= updatedEnd)
                )
                .Select(x => new
                {
                    map_id = x.map_id,
                    tool_type = x.ToolKey?.Type?.type_name,
                    tool_name = x.ToolKey?.Tool?.tool_name,
                    type_ref = x.ToolKey?.TypeRef?.type_name,
                    tool_ref = x.ToolKey?.ToolRef?.tool_name,
                    size_ref = x.ToolKey?.SizeRef?.size_ref,
                    pad_name = x.Pad?.pad_name,
                    hst_type = x.HstType?.hst_type,
                    description = x.description, // ✅ เพิ่มตรงนี้

                    // ❌ ไม่ส่ง byte[] pic_after_hst
                    // ✅ ส่ง file name + URL สำหรับโหลดรูปแทน

                    pic_after_hst_file_name = x.pic_after_hst_file_name,
                    image_url = x.pic_after_hst != null
                    ? Url.Action(
                        nameof(GetAfterHstImage),
                        "ToolPadMap",
                        new { id = x.map_id },
                        Request.Scheme
                    )
                    : null,

                    create_by = userDict.ContainsKey(x.create_by) ? userDict[x.create_by] : null,
                    update_by = x.update_by.HasValue && userDict.ContainsKey(x.update_by.Value) ? userDict[x.update_by.Value] : null,
                    x.create_at,
                    x.update_at,
                    pending_request = pendingRequestsDict.TryGetValue(x.map_id, out var pr) ? pr : null
                })
                .Where(x =>
                    (createByEmployees.Count == 0 || ((x.create_by == null && createByEmployees.Contains("")) || (x.create_by != null && createByEmployees.Contains(x.create_by)))) &&
                    (updateByEmployees.Count == 0 || ((x.update_by == null && updateByEmployees.Contains("")) || (x.update_by != null && updateByEmployees.Contains(x.update_by))))
                );

            // ✅ Apply string filters
            if (toolTypes.Any()) result = result.Where(x => toolTypes.Contains(x.tool_type ?? ""));
            if (toolNames.Any()) result = result.Where(x => toolNames.Contains(x.tool_name ?? ""));
            if (typeRefs.Any()) result = result.Where(x => typeRefs.Contains(x.type_ref ?? ""));
            if (toolRefs.Any()) result = result.Where(x => toolRefs.Contains(x.tool_ref ?? ""));
            if (sizeRefs.Any()) result = result.Where(x => sizeRefs.Contains(x.size_ref ?? ""));
            if (padNames.Any()) result = result.Where(x => padNames.Contains(x.pad_name ?? ""));
            if (hstTypes.Any()) result = result.Where(x => hstTypes.Contains(x.hst_type ?? ""));

            if (descriptions.Any())
                result = result.Where(x => descriptions.Contains(x.description ?? ""));

            if (pendingRequestsRaw.Any())
            {
                result = result.Where(x =>
                    (x.pending_request == null && pendingRequestsRaw.Contains("null")) ||
                    (x.pending_request != null && pendingRequestsRaw.Contains(x.pending_request.ToLower())));
            }

            return Ok(result.ToList());
        }



        // ✅ GET: /api/ToolPadMap/pivot
        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolPadMapPivot()
        // {
        //     var raw = await _context.ToolPadMaps
        //         .Include(x => x.ToolKey).ThenInclude(k => k.Type)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.Tool)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.TypeRef)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.ToolRef)
        //         .Include(x => x.ToolKey).ThenInclude(k => k.SizeRef)
        //         .Include(x => x.Pad)
        //         .Include(x => x.HstType)
        //         .ToListAsync();

        //     var pivoted = raw
        //         .GroupBy(x => new
        //         {
        //             tool_type = x.ToolKey?.Type?.type_name,
        //             tool_name = x.ToolKey?.Tool?.tool_name,
        //             type_ref = x.ToolKey?.TypeRef?.type_name,
        //             tool_ref = x.ToolKey?.ToolRef?.tool_name,
        //             size_ref = x.ToolKey?.SizeRef?.size_ref
        //         })
        //         .Select(g => new
        //         {
        //             g.Key.tool_type,
        //             g.Key.tool_name,
        //             g.Key.type_ref,
        //             g.Key.tool_ref,
        //             g.Key.size_ref,
        //             HST_pad = g.FirstOrDefault(x => x.HstType?.hst_type == "HST")?.Pad?.pad_name,
        //             RIM_pad = g.FirstOrDefault(x => x.HstType?.hst_type == "RIM")?.Pad?.pad_name,
        //             INNER_pad = g.FirstOrDefault(x => x.HstType?.hst_type == "INNER")?.Pad?.pad_name,
        //             EXTRA_RIM_pad = g.FirstOrDefault(x => x.HstType?.hst_type == "EXTRA_RIM")?.Pad?.pad_name
        //         });

        //     return Ok(pivoted.ToList()); // ✅ ต้องมี ToList() ตรงนี้
        // }

        [HttpGet("pivot")]
        public async Task<ActionResult<IEnumerable<object>>> GetToolPadMapPivot(
            [FromQuery] List<string> tool_name,
            [FromQuery] List<string> tool_type,
            [FromQuery] List<string> type_ref,
            [FromQuery] List<string> tool_ref,
            [FromQuery] List<string> size_ref,
            [FromQuery] List<string> HST_pad,
            [FromQuery] List<string> RIM_pad,
            [FromQuery] List<string> INNER_pad,
            [FromQuery] List<string> EXTRA_RIM_pad
        )
        {
            // ✅ Normalize filters
            var toolTypes = tool_type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolNames = tool_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var typeRefs = type_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolRefs = tool_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var sizeRefs = size_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var targetHST = HST_pad
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "null" : x.Trim())
                .OrderBy(x => x)
                .ToList();

            var targetRIM = RIM_pad
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "null" : x.Trim())
                .OrderBy(x => x)
                .ToList();

            var targetINNER = INNER_pad
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "null" : x.Trim())
                .OrderBy(x => x)
                .ToList();

            var targetEXTRA = EXTRA_RIM_pad
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x == "(Blanks)" ? "null" : x.Trim())
                .OrderBy(x => x)
                .ToList();


            // 🧱 Load + filter เบื้องต้น
            var data = await _context.ToolPadMaps
                .Include(x => x.ToolKey).ThenInclude(k => k.Type)
                .Include(x => x.ToolKey).ThenInclude(k => k.Tool)
                .Include(x => x.ToolKey).ThenInclude(k => k.TypeRef)
                .Include(x => x.ToolKey).ThenInclude(k => k.ToolRef)
                .Include(x => x.ToolKey).ThenInclude(k => k.SizeRef)
                .Include(x => x.Pad)
                .Include(x => x.HstType)
                .Where(x => x.ToolKey != null)
                .ToListAsync();

            if (toolTypes.Any())
                data = data.Where(x => toolTypes.Contains(x.ToolKey.Type?.type_name ?? "")).ToList();
            if (toolNames.Any())
                data = data.Where(x => toolNames.Contains(x.ToolKey.Tool?.tool_name ?? "")).ToList();
            if (typeRefs.Any())
                data = data.Where(x => typeRefs.Contains(x.ToolKey.TypeRef?.type_name ?? "")).ToList();
            if (toolRefs.Any())
                data = data.Where(x => toolRefs.Contains(x.ToolKey.ToolRef?.tool_name ?? "")).ToList();
            if (sizeRefs.Any())
                data = data.Where(x => sizeRefs.Contains(x.ToolKey.SizeRef?.size_ref ?? "")).ToList();

            // 🧮 Group โดย ToolKey + Extract pad per HST type
            var grouped = data
                .GroupBy(x => new
                {
                    tool_type = x.ToolKey.Type?.type_name ?? "",
                    tool_name = x.ToolKey.Tool?.tool_name ?? "",
                    type_ref = x.ToolKey.TypeRef?.type_name ?? "",
                    tool_ref = x.ToolKey.ToolRef?.tool_name ?? "",
                    size_ref = x.ToolKey.SizeRef?.size_ref ?? ""
                })
            //     .Select(g =>
            //     {
            //         var padByHst = g
            //             .GroupBy(x => x.HstType?.hst_type)
            //             .ToDictionary(
            //                 k => k.Key ?? "null",
            //                 v => v.FirstOrDefault()?.Pad?.pad_name ?? "null"
            //             );

            //     return new
            //     {
            //         g.Key.tool_type,
            //         g.Key.tool_name,
            //         g.Key.type_ref,
            //         g.Key.tool_ref,
            //         g.Key.size_ref,
            //         HST_pad = padByHst.TryGetValue("HST", out var h) ? h : null,
            //         RIM_pad = padByHst.TryGetValue("RIM", out var r) ? r : null,
            //         INNER_pad = padByHst.TryGetValue("INNER", out var i) ? i : null,
            //         EXTRA_RIM_pad = padByHst.TryGetValue("EXTRA_RIM", out var e) ? e : null
            //     };
            // });
            // .Select(g =>
            //     {
            //         var padByHst = g
            //             .GroupBy(x => x.HstType?.hst_type)
            //             .ToDictionary(
            //                 k => k.Key ?? "null",
            //                 v => new
            //                 {
            //                     PadName = v.FirstOrDefault()?.Pad?.pad_name ?? "null",
            //                     PicAfter = v.FirstOrDefault()?.pic_after_hst,
            //                     PicAfterFile = v.FirstOrDefault()?.pic_after_hst_file_name
            //                 }
            //             );

            //         return new
            //         {
            //             g.Key.tool_type,
            //             g.Key.tool_name,
            //             g.Key.type_ref,
            //             g.Key.tool_ref,
            //             g.Key.size_ref,

            //             HST_pad = padByHst.TryGetValue("HST", out var h) ? h.PadName : null,
            //             HST_pic_after_hst = padByHst.TryGetValue("HST", out h) ? h.PicAfter : null,
            //             HST_pic_after_hst_file_name = padByHst.TryGetValue("HST", out h) ? h.PicAfterFile : null,

            //             RIM_pad = padByHst.TryGetValue("RIM", out var r) ? r.PadName : null,
            //             RIM_pic_after_hst = padByHst.TryGetValue("RIM", out r) ? r.PicAfter : null,
            //             RIM_pic_after_hst_file_name = padByHst.TryGetValue("RIM", out r) ? r.PicAfterFile : null,

            //             INNER_pad = padByHst.TryGetValue("INNER", out var i) ? i.PadName : null,
            //             INNER_pic_after_hst = padByHst.TryGetValue("INNER", out i) ? i.PicAfter : null,
            //             INNER_pic_after_hst_file_name = padByHst.TryGetValue("INNER", out i) ? i.PicAfterFile : null,

            //             EXTRA_RIM_pad = padByHst.TryGetValue("EXTRA_RIM", out var e) ? e.PadName : null,
            //             EXTRA_RIM_pic_after_hst = padByHst.TryGetValue("EXTRA_RIM", out e) ? e.PicAfter : null,
            //             EXTRA_RIM_pic_after_hst_file_name = padByHst.TryGetValue("EXTRA_RIM", out e) ? e.PicAfterFile : null
            //         };
            //     });
                
                .Select(g =>
                    {
                        // helper สร้าง URL เฉพาะตอนมี map_id และ file name
                        string? AfterUrl(int? mapId, string? fileName) =>
                            (mapId == null || string.IsNullOrWhiteSpace(fileName))
                                ? null
                                : Url.Action(
                                    nameof(GetAfterHstImage), // ✅ ชี้ไป action ส่งรูป AFTER
                                    "ToolPadMap",
                                    new { id = mapId.Value },
                                    Request.Scheme
                                );

                        // ถ้าจะเอารูป BEFORE ด้วย ให้ปลดคอมเมนต์และใส่ field ด้านล่างเพิ่ม
                        // string? BeforeUrl(int? mapId, string? fileName) =>
                        //     (mapId == null || string.IsNullOrWhiteSpace(fileName))
                        //         ? null
                        //         : Url.Action(nameof(GetBeforeHstImage), "ToolPadMap", new { id = mapId.Value }, Request.Scheme);

                        var recH = g.FirstOrDefault(z => z.HstType?.hst_type == "HST");
                        var recR = g.FirstOrDefault(z => z.HstType?.hst_type == "RIM");
                        var recI = g.FirstOrDefault(z => z.HstType?.hst_type == "INNER");
                        var recE = g.FirstOrDefault(z => z.HstType?.hst_type == "EXTRA_RIM");

                        return new
                        {
                            g.Key.tool_type,
                            g.Key.tool_name,
                            g.Key.type_ref,
                            g.Key.tool_ref,
                            g.Key.size_ref,

                            // HST
                            HST_pad = recH?.Pad?.pad_name ?? "null",
                            HST_after_image_url = AfterUrl(recH?.map_id, recH?.pic_after_hst_file_name),
                            HST_after_file_name = recH?.pic_after_hst_file_name,
                            // HST_before_image_url = BeforeUrl(recH?.map_id, recH?.pic_before_hst_file_name),
                            // HST_before_file_name = recH?.pic_before_hst_file_name,

                            // RIM
                            RIM_pad = recR?.Pad?.pad_name ?? "null",
                            RIM_after_image_url = AfterUrl(recR?.map_id, recR?.pic_after_hst_file_name),
                            RIM_after_file_name = recR?.pic_after_hst_file_name,
                            // RIM_before_image_url = BeforeUrl(recR?.map_id, recR?.pic_before_hst_file_name),
                            // RIM_before_file_name = recR?.pic_before_hst_file_name,

                            // INNER
                            INNER_pad = recI?.Pad?.pad_name ?? "null",
                            INNER_after_image_url = AfterUrl(recI?.map_id, recI?.pic_after_hst_file_name),
                            INNER_after_file_name = recI?.pic_after_hst_file_name,
                            // INNER_before_image_url = BeforeUrl(recI?.map_id, recI?.pic_before_hst_file_name),
                            // INNER_before_file_name = recI?.pic_before_hst_file_name,

                            // EXTRA_RIM
                            EXTRA_RIM_pad = recE?.Pad?.pad_name ?? "null",
                            EXTRA_RIM_after_image_url = AfterUrl(recE?.map_id, recE?.pic_after_hst_file_name),
                            EXTRA_RIM_after_file_name = recE?.pic_after_hst_file_name,
                            // EXTRA_RIM_before_image_url = BeforeUrl(recE?.map_id, recE?.pic_before_hst_file_name),
                            // EXTRA_RIM_before_file_name = recE?.pic_before_hst_file_name
                        };
                    });

            // 🧠 Optional filtering exact match
            var filtered = grouped
                .Where(x =>
                    (!targetHST.Any() || targetHST.Contains(x.HST_pad ?? "null")) &&
                    (!targetRIM.Any() || targetRIM.Contains(x.RIM_pad ?? "null")) &&
                    (!targetINNER.Any() || targetINNER.Contains(x.INNER_pad ?? "null")) &&
                    (!targetEXTRA.Any() || targetEXTRA.Contains(x.EXTRA_RIM_pad ?? "null"))
                )
                .OrderBy(x => x.tool_type)
                .ThenBy(x => x.tool_name)
                .ToList();

            return Ok(filtered);
        }


        // ✅ POST: /api/ToolPadMap
        // [HttpPost]
        // public async Task<ActionResult> AddToolPadMap([FromBody] ToolPadMap newMap)
        // {
        //     // 🧪 ตรวจสอบ field ที่จำเป็น
        //     if (newMap.tool_key_id == 0 || newMap.pad_id == 0 || newMap.hst_type_id == 0)
        //     {
        //         return BadRequest("Missing required fields.");
        //     }

        //     // 🔁 ตรวจสอบซ้ำ
        //     var exists = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == newMap.tool_key_id &&
        //         x.pad_id == newMap.pad_id &&
        //         x.hst_type_id == newMap.hst_type_id
        //     );

        //     if (exists)
        //     {
        //         return Conflict("Mapping already exists.");
        //     }

        //     // 🔐 ตรวจสอบว่า pad_id กับ hst_type_id สามารถใช้คู่กันได้ใน padHstMap
        //     var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
        //         x.pad_id == newMap.pad_id &&
        //         x.hst_type_id == newMap.hst_type_id
        //     );

        //     if (!isValidPadHst)
        //     {
        //         return BadRequest("This pad is not valid for the given HST type.");
        //     }

        //     // ✅ Insert
        //     _context.ToolPadMaps.Add(newMap);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Mapping added successfully", id = newMap.map_id });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> AddToolPadMap([FromBody] ToolPadMap newMap)
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

            if (newMap.tool_key_id == 0 || newMap.pad_id == 0 || newMap.hst_type_id == 0)
            {
                return BadRequest("Missing required fields.");
            }

            // ตรวจ FK: tool_key_id
            var toolKey = await _context.ToolKeyAlls
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.tool_key_id == newMap.tool_key_id);

            if (toolKey == null)
                return BadRequest("Invalid tool_key_id.");

            // ตรวจ FK: pad_id
            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == newMap.pad_id);

            if (pad == null)
                return BadRequest("Invalid pad_id.");

            // ตรวจ FK: hst_type_id
            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == newMap.hst_type_id);

            if (hstType == null)
                return BadRequest("Invalid hst_type_id.");

            // ตรวจซ้ำ
            var exists = await _context.ToolPadMaps.AnyAsync(x =>
                x.tool_key_id == newMap.tool_key_id &&
                x.pad_id == newMap.pad_id &&
                x.hst_type_id == newMap.hst_type_id
            );

            if (exists)
            {
                return Conflict("Mapping already exists.");
            }

            // ตรวจว่า pad_id กับ hst_type_id ใช้คู่กันได้จริงหรือไม่
            var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
                x.pad_id == newMap.pad_id &&
                x.hst_type_id == newMap.hst_type_id
            );

            if (!isValidPadHst)
            {
                return BadRequest("This pad is not valid for the given HST type.");
            }

            newMap.create_by = currentUserId;
            newMap.create_at = DateTime.Now;
            newMap.update_by = null;
            newMap.update_at = null;

            _context.ToolPadMaps.Add(newMap);
            await _context.SaveChangesAsync();

            var result = new
            {
                newMap.map_id,
                tool_key_id = newMap.tool_key_id,
                pad_id = newMap.pad_id,
                pad_name = pad?.pad_name,
                hst_type_id = newMap.hst_type_id,
                hst_type = hstType?.hst_type
            };

            // === LOGGING ===
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
                target_table = "toolPadMap",
                target_id = newMap.map_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     map_id = newMap.map_id,
                //     tool_key_id = newMap.tool_key_id,
                //     pad_id = newMap.pad_id,
                //     pad_name = pad?.pad_name,
                //     hst_type_id = newMap.hst_type_id,
                //     hst_type = hstType?.hst_type,
                //     create_by = newMap.create_by,
                //     create_at = newMap.create_at
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

            return Ok(result);
        }


        // POST: /api/ToolPadMap/{map_id}/after-hst
        [Authorize(Roles = "admin,editor")]
        [HttpPost("{map_id:int}/after-hst")]
        public async Task<IActionResult> UpsertAfterHst(
            int map_id,
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var row = await _context.ToolPadMaps.FindAsync(map_id);
            if (row == null) return NotFound("map_id not found.");

            var allow = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allow.Contains(ext)) return BadRequest("Only .jpg/.jpeg/.png/.webp allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            // ✅ อัปเดตเฉพาะรูปและชื่อไฟล์เท่านั้น
            row.pic_after_hst = ms.ToArray();
            row.pic_after_hst_file_name = Path.GetFileName(file.FileName);

            // ❌ ไม่แตะ user_id/update_by/create_by เลย
            // (ถ้าตารางมี update_at และอยากอัปเดตเวลาเก็บไว้ ก็ทำได้โดยไม่แตะ user)
            // row.update_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { map_id, file_name = row.pic_after_hst_file_name, size = row.pic_after_hst?.Length ?? 0 });
        }


        // DELETE: /api/ToolPadMap/{tool_pad_id}/after-hst
        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{map_id:int}/after-hst")]
        public async Task<IActionResult> DeleteAfterHst(int map_id)
        {
            var row = await _context.ToolPadMaps.FindAsync(map_id);
            if (row == null) return NotFound("map_id not found.");

            row.pic_after_hst = null;
            row.pic_after_hst_file_name = null;

            // ❌ ไม่แตะ user_id/update_by/create_by
            // row.update_at = DateTime.Now; // จะใส่ก็ได้

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // ✅ DELETE: /api/ToolPadMap/{id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolPadMap(int id)
        // {
        //     var map = await _context.ToolPadMaps.FindAsync(id);
        //     if (map == null)
        //     {
        //         return NotFound("Mapping not found.");
        //     }

        //     _context.ToolPadMaps.Remove(map);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Mapping deleted successfully", id = id });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolPadMap(int id)
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

            // ===== Load mapping =====
            var map = await _context.ToolPadMaps
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.map_id == id);

            if (map == null)
                return NotFound("Mapping not found.");

            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == map.pad_id);

            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == map.hst_type_id);

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
                target_table = "toolPadMap",
                target_id = map.map_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     map_id = map.map_id,
                //     tool_key_id = map.tool_key_id,
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
                // response_status = 200,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // ===== Delete =====
            var entity = await _context.ToolPadMaps.FindAsync(id);
            if (entity != null)
            {
                _context.ToolPadMaps.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Mapping deleted successfully",
                map_id = id
            });
        }





        // ✅ PUT: /api/ToolPadMap/{id}
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateToolPadMap(int id, [FromBody] ToolPadMap updatedMap)
        // {
        //     if (updatedMap.pad_id == 0 || updatedMap.hst_type_id == 0)
        //         return BadRequest("Missing required fields.");

        //     var existingMap = await _context.ToolPadMaps.FindAsync(id);
        //     if (existingMap == null)
        //         return NotFound("Mapping not found.");

        //     // 🔁 ตรวจสอบซ้ำ (ต้องไม่ชนกับ mapping อื่นที่มี combination เดียวกัน)
        //     var duplicate = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == existingMap.tool_key_id &&
        //         x.pad_id == updatedMap.pad_id &&
        //         x.hst_type_id == updatedMap.hst_type_id &&
        //         x.map_id != id
        //     );

        //     if (duplicate)
        //         return Conflict("Mapping with the same pad and HST type already exists.");

        //     // 🔐 ตรวจสอบ pad กับ hst_type ว่าจับคู่กันได้
        //     var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
        //         x.pad_id == updatedMap.pad_id &&
        //         x.hst_type_id == updatedMap.hst_type_id
        //     );

        //     if (!isValidPadHst)
        //         return BadRequest("This pad is not valid for the given HST type.");

        //     // ✅ อัปเดต
        //     existingMap.pad_id = updatedMap.pad_id;
        //     existingMap.hst_type_id = updatedMap.hst_type_id;

        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Mapping updated successfully", id = id });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToolPadMap(int id, [FromBody] ToolPadMap updatedMap)
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

            if (updatedMap.pad_id == 0 || updatedMap.hst_type_id == 0)
                return BadRequest("Missing required fields.");

            var existingMap = await _context.ToolPadMaps
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.map_id == id);

            if (existingMap == null)
                return NotFound("Mapping not found.");

            // ตรวจว่า pad_id มีจริงไหม
            var pad = await _context.Pads
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.pad_id == updatedMap.pad_id);

            if (pad == null)
                return BadRequest("Invalid pad_id.");

            // ตรวจว่า hst_type_id มีจริงไหม
            var hstType = await _context.HstTypes
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(h => h.hst_type_id == updatedMap.hst_type_id);

            if (hstType == null)
                return BadRequest("Invalid hst_type_id.");

            // ตรวจ duplicate
            var duplicate = await _context.ToolPadMaps.AnyAsync(x =>
                x.tool_key_id == existingMap.tool_key_id &&
                x.pad_id == updatedMap.pad_id &&
                x.hst_type_id == updatedMap.hst_type_id &&
                x.map_id != id
            );

            if (duplicate)
                return Conflict("Mapping with the same pad and HST type already exists.");

            // ตรวจว่าจับคู่ pad_id กับ hst_type_id ได้จริงหรือไม่
            var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
                x.pad_id == updatedMap.pad_id &&
                x.hst_type_id == updatedMap.hst_type_id
            );

            if (!isValidPadHst)
                return BadRequest("This pad is not valid for the given HST type.");

            // ===== Update =====
            var mapToUpdate = await _context.ToolPadMaps.FindAsync(id);

            var oldPad = await _context.Pads
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.pad_id == mapToUpdate.pad_id);

            var oldHstType = await _context.HstTypes
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(h => h.hst_type_id == mapToUpdate.hst_type_id);

            mapToUpdate.pad_id = updatedMap.pad_id;
            mapToUpdate.hst_type_id = updatedMap.hst_type_id;
            mapToUpdate.update_by = currentUserId;
            mapToUpdate.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            // ===== Prepare log =====
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
                    map_id = existingMap.map_id,
                    tool_key_id = existingMap.tool_key_id,
                    pad_id = existingMap.pad_id,
                    pad_name = oldPad?.pad_name,
                    hst_type_id = existingMap.hst_type_id,
                    hst_type = oldHstType?.hst_type,
                    update_by = existingMap.update_by,
                    update_at = existingMap.update_at
                },
                @new = new
                {
                    map_id = mapToUpdate.map_id,
                    tool_key_id = mapToUpdate.tool_key_id,
                    pad_id = mapToUpdate.pad_id,
                    pad_name = pad?.pad_name,
                    hst_type_id = mapToUpdate.hst_type_id,
                    hst_type = hstType?.hst_type,
                    update_by = mapToUpdate.update_by,
                    update_at = mapToUpdate.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "toolPadMap",
                target_id = mapToUpdate.map_id.ToString(),
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
                message = "Mapping updated successfully",
                map_id = mapToUpdate.map_id,
                tool_key_id = mapToUpdate.tool_key_id,
                pad_id = mapToUpdate.pad_id,
                pad_name = pad?.pad_name,
                hst_type_id = mapToUpdate.hst_type_id,
                hst_type = hstType?.hst_type
            };

            return Ok(result);
        }


    }
}
