using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolMachineMapController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolMachineMapController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/ToolMachineMap
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMap()
        // {
        //     var maps = await _context.ToolMachineMaps
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.Type)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.Tool)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.TypeRef)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.ToolRef)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.SizeRef)
        //         .Include(map => map.Machine)
        //         .ToListAsync(); // 👈 ดึงมาก่อน แล้วค่อย map ภายหลัง

        //     var result = maps.Select(map => new
        //     {
        //         map_id = map.map_id, // ✅ เพิ่มบรรทัดนี้
        //         tool_type = map.ToolKey.Type?.type_name,
        //         tool_name = map.ToolKey.Tool?.tool_name,
        //         type_ref = map.ToolKey.TypeRef?.type_name,
        //         tool_ref = map.ToolKey.ToolRef?.tool_name,
        //         size_ref = map.ToolKey.SizeRef?.size_ref,
        //         machine_no = map.Machine?.machine_no // 👈 ใส่ ? เผื่อ null ด้วย
        //     });

        //     return Ok(result);
        // }

        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMap(
        //     [FromQuery] string? tool_name,
        //     [FromQuery] string? tool_type,
        //     [FromQuery] string? type_ref,
        //     [FromQuery] string? tool_ref,
        //     [FromQuery] string? size_ref,
        //     [FromQuery] string? machine_no,
        //     [FromQuery] string? pending_request
        // )
        // {
        //     // 🔹 แปลง string ให้เป็น list
        //     var toolNames = string.IsNullOrWhiteSpace(tool_name) ? new List<string>() : tool_name.Split(',').Select(x => x.Trim()).ToList();
        //     var toolTypes = string.IsNullOrWhiteSpace(tool_type) ? new List<string>() : tool_type.Split(',').Select(x => x.Trim()).ToList();
        //     var typeRefs = string.IsNullOrWhiteSpace(type_ref) ? new List<string>() : type_ref.Split(',').Select(x => x.Trim()).ToList();
        //     var toolRefs = string.IsNullOrWhiteSpace(tool_ref) ? new List<string>() : tool_ref.Split(',').Select(x => x.Trim()).ToList();
        //     var sizeRefs = string.IsNullOrWhiteSpace(size_ref) ? new List<string>() : size_ref.Split(',').Select(x => x.Trim()).ToList();
        //     var machineNos = string.IsNullOrWhiteSpace(machine_no) ? new List<string>() : machine_no.Split(',').Select(x => x.Trim()).ToList();
        //     var pendingRequests = string.IsNullOrWhiteSpace(pending_request)
        //         ? new List<string>()
        //         : pending_request.Split(',').Select(x => x.Trim().ToLower()).ToList();

        //     // 🔸 Map request pending จาก Request Table
        //     var pendingRequestsDict = await _context.Requests
        //         .Where(r =>
        //             r.target_table == "ToolMachineMap" &&
        //             r.request_status == "Pending" &&
        //             (r.request_type == "UPDATE" || r.request_type == "DELETE"))
        //         .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

        //     // 🔹 ดึงข้อมูลหลัก
        //     var maps = await _context.ToolMachineMaps
        //         .Include(map => map.ToolKey).ThenInclude(tk => tk.Type)
        //         .Include(map => map.ToolKey).ThenInclude(tk => tk.Tool)
        //         .Include(map => map.ToolKey).ThenInclude(tk => tk.TypeRef)
        //         .Include(map => map.ToolKey).ThenInclude(tk => tk.ToolRef)
        //         .Include(map => map.ToolKey).ThenInclude(tk => tk.SizeRef)
        //         .Include(map => map.Machine)
        //         .ToListAsync();

        //     var result = maps.Select(map => new
        //     {
        //         map_id = map.map_id,
        //         tool_type = map.ToolKey.Type?.type_name,
        //         tool_name = map.ToolKey.Tool?.tool_name,
        //         type_ref = map.ToolKey.TypeRef?.type_name,
        //         tool_ref = map.ToolKey.ToolRef?.tool_name,
        //         size_ref = map.ToolKey.SizeRef?.size_ref,
        //         machine_no = map.Machine?.machine_no,
        //         pending_request = pendingRequestsDict.TryGetValue(map.map_id, out var pr) ? pr : null
        //     });

        //     // ✅ Apply Filters with null-safe support
        //     if (toolNames.Any())
        //         result = result.Where(x => toolNames.Contains(x.tool_name ?? "null"));

        //     if (toolTypes.Any())
        //         result = result.Where(x => toolTypes.Contains(x.tool_type ?? "null"));

        //     if (typeRefs.Any())
        //         result = result.Where(x => typeRefs.Contains(x.type_ref ?? "null"));

        //     if (toolRefs.Any())
        //         result = result.Where(x => toolRefs.Contains(x.tool_ref ?? "null"));

        //     if (sizeRefs.Any())
        //         result = result.Where(x => sizeRefs.Contains(x.size_ref ?? "null"));

        //     if (machineNos.Any())
        //         result = result.Where(x => machineNos.Contains(x.machine_no ?? "null"));

        //     if (pendingRequests.Any())
        //     {
        //         result = result.Where(x =>
        //             (x.pending_request == null && pendingRequests.Contains("null")) ||
        //             (x.pending_request != null && pendingRequests.Contains(x.pending_request.ToLower())));
        //     }


        //     return Ok(result.ToList());
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMap(
            [FromQuery] List<string> tool_name,
            [FromQuery] List<string> tool_type,
            [FromQuery] List<string> type_ref,
            [FromQuery] List<string> tool_ref,
            [FromQuery] List<string> size_ref,
            [FromQuery] List<string> machine_no,
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
            var toolNames = tool_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolTypes = tool_type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var typeRefs = type_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolRefs = tool_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var sizeRefs = size_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var machineNos = machine_no
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var descriptions = description
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();


            var pendingRequestsRaw = pending_request
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "null" : x.Trim().ToLower())
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
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var updateByEmployees = update_by
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();


            var createByUserIds = userDict.Where(x => createByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();
            var updateByUserIds = userDict.Where(x => updateByEmployees.Contains(x.Value)).Select(x => x.Key).ToList();

            // ✅ Load pending request
            var pendingRequestsDict = await _context.Requests
                .Where(r =>
                    r.target_table == "ToolMachineMap" &&
                    r.request_status == "Pending" &&
                    (r.request_type == "UPDATE" || r.request_type == "DELETE"))
                .ToDictionaryAsync(r => r.target_pk_id, r => r.request_type);

            // ✅ Load main data
            var query = _context.ToolMachineMaps
                .Include(map => map.ToolKey).ThenInclude(tk => tk.Type)
                .Include(map => map.ToolKey).ThenInclude(tk => tk.Tool)
                .Include(map => map.ToolKey).ThenInclude(tk => tk.TypeRef)
                .Include(map => map.ToolKey).ThenInclude(tk => tk.ToolRef)
                .Include(map => map.ToolKey).ThenInclude(tk => tk.SizeRef)
                .Include(map => map.Machine);

            var maps = await query.ToListAsync();

            var result = maps
                .Where(m =>
                    (createdStart == null || m.create_at >= createdStart) &&
                    (createdEnd == null || m.create_at <= createdEnd) &&
                    (updatedStart == null || m.update_at >= updatedStart) &&
                    (updatedEnd == null || m.update_at <= updatedEnd)
                )
                .Select(map => new
                {
                    map_id = map.map_id,
                    tool_type = map.ToolKey.Type?.type_name,
                    tool_name = map.ToolKey.Tool?.tool_name,
                    type_ref = map.ToolKey.TypeRef?.type_name,
                    tool_ref = map.ToolKey.ToolRef?.tool_name,
                    size_ref = map.ToolKey.SizeRef?.size_ref,
                    machine_no = map.Machine?.machine_no,
                    description = map.description ?? null, // ✅ เพิ่มตรงนี้ (หรือแล้วแต่ชื่อ field จริง)
                    create_by = userDict.ContainsKey(map.create_by) ? userDict[map.create_by] : null,
                    update_by = map.update_by.HasValue && userDict.ContainsKey(map.update_by.Value) ? userDict[map.update_by.Value] : null,
                    map.create_at,
                    map.update_at,
                    pending_request = pendingRequestsDict.TryGetValue(map.map_id, out var pr) ? pr : null
                })
                .Where(x =>
                    (createByEmployees.Count == 0 || ((x.create_by == null && createByEmployees.Contains("")) || (x.create_by != null && createByEmployees.Contains(x.create_by)))) &&
                    (updateByEmployees.Count == 0 || ((x.update_by == null && updateByEmployees.Contains("")) || (x.update_by != null && updateByEmployees.Contains(x.update_by))))
                );

            // ✅ Field filters (string match)
            if (toolNames.Any())
                result = result.Where(x => toolNames.Contains(x.tool_name ?? ""));

            if (toolTypes.Any())
                result = result.Where(x => toolTypes.Contains(x.tool_type ?? ""));

            if (typeRefs.Any())
                result = result.Where(x => typeRefs.Contains(x.type_ref ?? ""));

            if (toolRefs.Any())
                result = result.Where(x => toolRefs.Contains(x.tool_ref ?? ""));

            if (sizeRefs.Any())
                result = result.Where(x => sizeRefs.Contains(x.size_ref ?? ""));

            if (machineNos.Any())
                result = result.Where(x => machineNos.Contains(x.machine_no ?? ""));

            if (descriptions.Any())
            {
                result = result.Where(x =>
                    (x.description == null && descriptions.Contains("")) ||
                    (x.description != null && descriptions.Contains(x.description))
                );
            }

            if (pendingRequestsRaw.Any())
            {
                result = result.Where(x =>
                    (x.pending_request == null && pendingRequestsRaw.Contains("null")) ||
                    (x.pending_request != null && pendingRequestsRaw.Contains(x.pending_request.ToLower())));
            }

            return Ok(result.ToList());
        }



        // GET: api/ToolMachineMap/pivot
        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMapPivot()
        // {
        //     var pivotData = await _context.ToolMachineMaps
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.Type)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.Tool)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.TypeRef)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.ToolRef)
        //         .Include(map => map.ToolKey)
        //             .ThenInclude(tk => tk.SizeRef)
        //         .Include(map => map.Machine)
        //         .ToListAsync();

        //     var grouped = pivotData
        //         .GroupBy(map => new
        //         {
        //             tool_type = map.ToolKey.Type?.type_name,
        //             tool_name = map.ToolKey.Tool?.tool_name,
        //             type_ref = map.ToolKey.TypeRef?.type_name,
        //             tool_ref = map.ToolKey.ToolRef?.tool_name,
        //             size_ref = map.ToolKey.SizeRef?.size_ref
        //         })
        //         .Select(g => new
        //         {
        //             g.Key.tool_type,
        //             g.Key.tool_name,
        //             g.Key.type_ref,
        //             g.Key.tool_ref,
        //             g.Key.size_ref,
        //             machine_no = string.Join(", ",
        //                         g.Select(x => x.Machine?.machine_no)
        //                         .Where(m => !string.IsNullOrEmpty(m))
        //                         .Distinct()
        //                         .OrderBy(m => int.TryParse(m, out var n) ? n : int.MaxValue)
        //             )
        //         });

        //     return Ok(grouped);
        // }

        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMapPivot(
        //     [FromQuery] string? tool_type,
        //     [FromQuery] string? tool_name,
        //     [FromQuery] string? type_ref,
        //     [FromQuery] string? tool_ref,
        //     [FromQuery] string? size_ref,
        //     [FromQuery] string? machine_no
        // )
        // [HttpGet("pivot")]
        // public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMapPivot(
        //     [FromQuery] List<string> tool_name,
        //     [FromQuery] List<string> tool_type,
        //     [FromQuery] List<string> type_ref,
        //     [FromQuery] List<string> tool_ref,
        //     [FromQuery] List<string> size_ref,
        //     [FromQuery] List<string> machine_no,
        //     [FromQuery] List<string> pending_request,
        //     [FromQuery] string? created_at_start,
        //     [FromQuery] string? created_at_end,
        //     [FromQuery] string? updated_at_start,
        //     [FromQuery] string? updated_at_end,
        //     [FromQuery] List<string> create_by,
        //     [FromQuery] List<string> update_by
        // )
        // {
        //     var toolTypes = string.IsNullOrWhiteSpace(tool_type)
        //         ? new List<string>()
        //         : tool_type.Split(',').Select(x => x.Trim()).ToList();

        //     var toolNames = string.IsNullOrWhiteSpace(tool_name)
        //         ? new List<string>()
        //         : tool_name.Split(',').Select(x => x.Trim()).ToList();

        //     var typeRefs = string.IsNullOrWhiteSpace(type_ref)
        //         ? new List<string>()
        //         : type_ref.Split(',').Select(x => x.Trim()).ToList();

        //     var toolRefs = string.IsNullOrWhiteSpace(tool_ref)
        //         ? new List<string>()
        //         : tool_ref.Split(',').Select(x => x.Trim()).ToList();

        //     var sizeRefs = string.IsNullOrWhiteSpace(size_ref)
        //         ? new List<string>()
        //         : size_ref.Split(',').Select(x => x.Trim()).ToList();

        //     // 🟨 เตรียม machine_no filter สำหรับ exact match (กรองหลัง group)
        //     var targetMachineSet = string.IsNullOrWhiteSpace(machine_no)
        //         ? new List<string>()
        //         : machine_no.Split(',').Select(x => x.Trim()).ToList();

        //     // 🧱 Load + filter เบื้องต้นก่อน group
        //     var data = await _context.ToolMachineMaps
        //         .Include(m => m.ToolKey).ThenInclude(k => k.Type)
        //         .Include(m => m.ToolKey).ThenInclude(k => k.Tool)
        //         .Include(m => m.ToolKey).ThenInclude(k => k.TypeRef)
        //         .Include(m => m.ToolKey).ThenInclude(k => k.ToolRef)
        //         .Include(m => m.ToolKey).ThenInclude(k => k.SizeRef)
        //         .Include(m => m.Machine)
        //         .Where(m => m.ToolKey != null)
        //         .ToListAsync();

        //     if (toolTypes.Any())
        //         data = data.Where(x => toolTypes.Contains(x.ToolKey.Type?.type_name ?? "null")).ToList();
        //     if (toolNames.Any())
        //         data = data.Where(x => toolNames.Contains(x.ToolKey.Tool?.tool_name ?? "null")).ToList();
        //     if (typeRefs.Any())
        //         data = data.Where(x => typeRefs.Contains(x.ToolKey.TypeRef?.type_name ?? "null")).ToList();
        //     if (toolRefs.Any())
        //         data = data.Where(x => toolRefs.Contains(x.ToolKey.ToolRef?.tool_name ?? "null")).ToList();
        //     if (sizeRefs.Any())
        //         data = data.Where(x => sizeRefs.Contains(x.ToolKey.SizeRef?.size_ref ?? "null")).ToList();

        //     // 🧮 Group และรวม machine_no
        //     var grouped = data
        //         .GroupBy(x => new
        //         {
        //             tool_type = x.ToolKey.Type?.type_name ?? null,
        //             tool_name = x.ToolKey.Tool?.tool_name ?? null,
        //             type_ref = x.ToolKey.TypeRef?.type_name ?? null,
        //             tool_ref = x.ToolKey.ToolRef?.tool_name ?? null,
        //             size_ref = x.ToolKey.SizeRef?.size_ref ?? null
        //         })
        //         .Select(g =>
        //         {
        //             var machineList = g
        //                 .Select(x => x.Machine?.machine_no ?? "null")
        //                 .Distinct()
        //                 .OrderBy(x => int.TryParse(x, out var n) ? n : int.MaxValue)
        //                 .ToList();

        //             var machineJoined = string.Join(", ", machineList);

        //             return new
        //             {
        //                 g.Key.tool_type,
        //                 g.Key.tool_name,
        //                 g.Key.type_ref,
        //                 g.Key.tool_ref,
        //                 g.Key.size_ref,
        //                 machine_no = machineJoined,
        //                 machine_list = machineList
        //             };
        //         });

        //     // 🧠 Filter exact match แบบ string เป๊ะๆ
        //     var result = grouped
        //         .Where(x =>
        //             !targetMachineSet.Any() ||  // ถ้าไม่ได้ filter ก็ผ่านหมด
        //             string.Join(", ", targetMachineSet.OrderBy(x => x)) == x.machine_no
        //         )
        //         .Select(x => new
        //         {
        //             x.tool_type,
        //             x.tool_name,
        //             x.type_ref,
        //             x.tool_ref,
        //             x.size_ref,
        //             x.machine_no
        //         })
        //         .OrderBy(x => x.tool_type)
        //         .ThenBy(x => x.tool_name)
        //         .ToList();

        //     return Ok(result);
        // }

        [HttpGet("pivot")]
        public async Task<ActionResult<IEnumerable<object>>> GetToolMachineMapPivot(
            [FromQuery] List<string> tool_name,
            [FromQuery] List<string> tool_type,
            [FromQuery] List<string> type_ref,
            [FromQuery] List<string> tool_ref,
            [FromQuery] List<string> size_ref,
            [FromQuery] List<string> machine_no
        // [FromQuery] List<string> pending_request,
        // [FromQuery] string? created_at_start,
        // [FromQuery] string? created_at_end,
        // [FromQuery] string? updated_at_start,
        // [FromQuery] string? updated_at_end,
        // [FromQuery] List<string> create_by,
        // [FromQuery] List<string> update_by
        )
        {
            // ✅ Normalize filters
            var toolTypes = tool_type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolNames = tool_name
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var typeRefs = type_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var toolRefs = tool_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var sizeRefs = size_ref
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "" : x.Trim())
                .ToList();

            var targetMachineSet = machine_no
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower() == "null" || x.Trim() == "(Blanks)" ? "null" : x.Trim())
                .OrderBy(x => x)
                .ToList();

            // 🧱 Load + filter เบื้องต้นก่อน group
            var data = await _context.ToolMachineMaps
                .Include(m => m.ToolKey).ThenInclude(k => k.Type)
                .Include(m => m.ToolKey).ThenInclude(k => k.Tool)
                .Include(m => m.ToolKey).ThenInclude(k => k.TypeRef)
                .Include(m => m.ToolKey).ThenInclude(k => k.ToolRef)
                .Include(m => m.ToolKey).ThenInclude(k => k.SizeRef)
                .Include(m => m.Machine)
                .Where(m => m.ToolKey != null)
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

            // 🧮 Group และรวม machine_no
            var grouped = data
                .GroupBy(x => new
                {
                    tool_type = x.ToolKey.Type?.type_name ?? "",
                    tool_name = x.ToolKey.Tool?.tool_name ?? "",
                    type_ref = x.ToolKey.TypeRef?.type_name ?? "",
                    tool_ref = x.ToolKey.ToolRef?.tool_name ?? "",
                    size_ref = x.ToolKey.SizeRef?.size_ref ?? ""
                })
                .Select(g =>
                {
                    var machineList = g
                        .Select(x => x.Machine?.machine_no ?? "null")
                        .Distinct()
                        .OrderBy(x => int.TryParse(x, out var n) ? n : int.MaxValue)
                        .ToList();

                    var machineJoined = string.Join(", ", machineList);

                    return new
                    {
                        g.Key.tool_type,
                        g.Key.tool_name,
                        g.Key.type_ref,
                        g.Key.tool_ref,
                        g.Key.size_ref,
                        machine_no = machineJoined,
                        machine_list = machineList
                    };
                });

            // 🧠 Filter exact match แบบ string เป๊ะๆ
            var result = grouped
                .Where(x =>
                    !targetMachineSet.Any() ||  // ถ้าไม่ได้ filter ก็ผ่านหมด
                    string.Join(", ", x.machine_list.OrderBy(x => x)) == string.Join(", ", targetMachineSet)
                )
                .Select(x => new
                {
                    x.tool_type,
                    x.tool_name,
                    x.type_ref,
                    x.tool_ref,
                    x.size_ref,
                    x.machine_no
                })
                .OrderBy(x => x.tool_type)
                .ThenBy(x => x.tool_name)
                .ToList();

            return Ok(result);
        }





        // POST: api/ToolMachineMap
        // [HttpPost]
        // public async Task<IActionResult> PostToolMachineMap([FromBody] ToolMachineMap newMap)
        // {
        //     if (newMap == null || newMap.tool_key_id == 0 || newMap.machine_id == 0)
        //         return BadRequest("Invalid input");

        //     // ป้องกันไม่ให้ duplicate
        //     bool exists = await _context.ToolMachineMaps
        //         .AnyAsync(m => m.tool_key_id == newMap.tool_key_id && m.machine_id == newMap.machine_id);

        //     if (exists)
        //         return Conflict("Mapping already exists");

        //     _context.ToolMachineMaps.Add(newMap);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Mapping inserted" });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPost]
        public async Task<IActionResult> PostToolMachineMap([FromBody] ToolMachineMap newMap)
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

            if (newMap == null || newMap.tool_key_id == 0 || newMap.machine_id == 0)
                return BadRequest("Invalid input");

            var toolKey = await _context.ToolKeyAlls
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.tool_key_id == newMap.tool_key_id);
            if (toolKey == null)
                return BadRequest("Invalid tool_key_id.");

            var machine = await _context.Machines
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(m => m.machine_id == newMap.machine_id);
            if (machine == null)
                return BadRequest("Invalid machine_id.");

            bool exists = await _context.ToolMachineMaps.AnyAsync(m =>
                m.tool_key_id == newMap.tool_key_id &&
                m.machine_id == newMap.machine_id);

            if (exists)
                return Conflict("Mapping already exists.");

            newMap.create_by = currentUserId;
            newMap.create_at = DateTime.Now;
            newMap.update_by = null;
            newMap.update_at = null;

            _context.ToolMachineMaps.Add(newMap);
            await _context.SaveChangesAsync();

            var result = new
            {
                newMap.map_id,
                tool_key_id = newMap.tool_key_id,
                machine_id = newMap.machine_id,
                machine_no = machine.machine_no
            };

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
                target_table = "toolMachineMap",
                target_id = newMap.map_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     map_id = newMap.map_id,
                //     tool_key_id = newMap.tool_key_id,
                //     machine_id = newMap.machine_id,
                //     machine_no = machine.machine_no,
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



        // ✅ DELETE: /api/ToolMachine/{map_id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteToolMachineMap(int id)
        // {
        //     var map = await _context.ToolMachineMaps.FindAsync(id);
        //     if (map == null)
        //     {
        //         return NotFound("Mapping not found.");
        //     }

        //     _context.ToolMachineMaps.Remove(map);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Mapping deleted successfully", id = id });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolMachineMap(int id)
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
            var map = await _context.ToolMachineMaps
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.map_id == id);

            if (map == null)
                return NotFound("Mapping not found.");

            var machine = await _context.Machines
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(m => m.machine_id == map.machine_id);

            var toolKey = await _context.ToolKeyAlls
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.tool_key_id == map.tool_key_id);

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
                target_table = "toolMachineMap",
                target_id = map.map_id.ToString(),
                // details = JsonConvert.SerializeObject(new
                // {
                //     map_id = map.map_id,
                //     tool_key_id = map.tool_key_id,
                //     machine_id = map.machine_id,
                //     machine_no = machine?.machine_no,
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
            var entity = await _context.ToolMachineMaps.FindAsync(id);
            if (entity != null)
            {
                _context.ToolMachineMaps.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Mapping deleted successfully",
                map_id = id
            });
        }


        // PUT: api/ToolMachineMap/{id}
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateMachineMap(int id, [FromBody] int new_machine_id)
        // {
        //     var existingMap = await _context.ToolMachineMaps.FindAsync(id);
        //     if (existingMap == null)
        //         return NotFound("Mapping not found.");

        //     // ป้องกันไม่ให้ duplicate
        //     bool exists = await _context.ToolMachineMaps
        //         .AnyAsync(m => m.tool_key_id == existingMap.tool_key_id && m.machine_id == new_machine_id && m.map_id != id);

        //     if (exists)
        //         return Conflict("Mapping with the same tool_key and machine already exists.");

        //     existingMap.machine_id = new_machine_id;

        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Machine updated successfully", map_id = id });
        // }

        [Authorize(Roles = "admin,editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachineMap(int id, [FromBody] int new_machine_id)
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

            // ===== Load existing mapping =====
            var existingMap = await _context.ToolMachineMaps
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.map_id == id);

            if (existingMap == null)
                return NotFound("Mapping not found.");

            // ตรวจว่า machine_id ใหม่มีจริงหรือไม่
            var newMachine = await _context.Machines
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(m => m.machine_id == new_machine_id);

            if (newMachine == null)
                return BadRequest("Invalid machine_id.");

            // ตรวจ duplicate
            bool exists = await _context.ToolMachineMaps.AnyAsync(m =>
                m.tool_key_id == existingMap.tool_key_id &&
                m.machine_id == new_machine_id &&
                m.map_id != id);

            if (exists)
                return Conflict("Mapping with the same tool_key and machine already exists.");

            // ===== Update =====
            var mapToUpdate = await _context.ToolMachineMaps.FindAsync(id);
            var oldMachine = await _context.Machines
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(m => m.machine_id == mapToUpdate.machine_id);

            mapToUpdate.machine_id = new_machine_id;
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
                    machine_id = existingMap.machine_id,
                    machine_no = oldMachine?.machine_no,
                    update_by = existingMap.update_by,
                    update_at = existingMap.update_at
                },
                @new = new
                {
                    map_id = mapToUpdate.map_id,
                    tool_key_id = mapToUpdate.tool_key_id,
                    machine_id = mapToUpdate.machine_id,
                    machine_no = newMachine?.machine_no,
                    update_by = mapToUpdate.update_by,
                    update_at = mapToUpdate.update_at
                }
            };

            var log = new Log
            {
                user_id = currentUserId,
                username_snapshot = username,
                action = "UPDATE",
                target_table = "toolMachineMap",
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
                message = "Machine updated successfully",
                map_id = mapToUpdate.map_id,
                tool_key_id = mapToUpdate.tool_key_id,
                machine_id = mapToUpdate.machine_id,
                machine_no = newMachine?.machine_no
            };

            return Ok(result);
        }


    }
}
