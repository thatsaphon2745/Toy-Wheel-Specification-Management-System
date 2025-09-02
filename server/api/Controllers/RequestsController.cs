using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using api.Models;
using api.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;


namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;
        private readonly DynamicService _dynamicService;
        private readonly OriginalSpecService _originalSpecService;
        private readonly DdcService _ddcService;
        private readonly PadBrassService _padBrassService;
        private readonly PadHstService _padHstService;
        private readonly ToolPadService _toolPadService;

        private readonly ToolMachineService _toolMachineService;

        public RequestsController(
            MbkBarbell9Context context,
            DynamicService dynamicService,
            OriginalSpecService originalSpecService,
            DdcService ddcService,
            PadBrassService padBrassService,
            PadHstService padHstService,
            ToolPadService toolPadService,
            ToolMachineService toolMachineService
        )
        {
            _context = context;
            _dynamicService = dynamicService;
            _originalSpecService = originalSpecService;
            _ddcService = ddcService;
            _padBrassService = padBrassService;
            _padHstService = padHstService;
            _toolPadService = toolPadService;
            _toolMachineService = toolMachineService;
        }


        // [HttpPost]
        // [Authorize(Roles = "admin,editor")]
        // public IActionResult CreateRequest([FromBody] RequestDto dto)
        // {
        //     var userIdClaim = User.FindFirst("user_id");
        //     if (userIdClaim == null)
        //         return Unauthorized();

        //     var requestedBy = int.Parse(userIdClaim.Value);

        //     var jsonOldData = dto.old_data?.ToString(Newtonsoft.Json.Formatting.None);
        //     var jsonNewData = dto.new_data?.ToString(Newtonsoft.Json.Formatting.None);

        //     var request = new Request
        //     {
        //         request_type = dto.request_type.ToUpper(),
        //         request_status = "Pending",
        //         target_table = dto.target_table,
        //         target_pk_id = dto.target_pk_id,
        //         old_data = jsonOldData,
        //         new_data = jsonNewData,
        //         requested_by = requestedBy,
        //         requested_at = DateTime.Now,
        //         note = dto.note
        //     };

        //     _context.Requests.Add(request);
        //     _context.SaveChanges();

        //     return Ok(new
        //     {
        //         message = "Request submitted successfully.",
        //         request_id = request.request_id
        //     });
        // }

        // [HttpPost]
        // [Authorize(Roles = "admin,editor")]
        // public async Task<IActionResult> CreateRequest([FromBody] RequestDto dto)
        // {
        //     var userIdClaim = User.FindFirst("user_id");
        //     if (userIdClaim == null)
        //         return Unauthorized();

        //     var requestedBy = int.Parse(userIdClaim.Value);

        //     var jsonNewData = dto.new_data?.ToString(Newtonsoft.Json.Formatting.None);
        //     JToken? newDataToken = null;

        //     if (!string.IsNullOrEmpty(jsonNewData))
        //     {
        //         newDataToken = JToken.Parse(jsonNewData);
        //     }

        //     // ✅ validate before saving request
        //     bool valid = true;
        //     string? errorMessage = null;

        //     if (dto.target_table == "OriginalSpec")
        //     {
        //         switch (dto.request_type.ToUpper())
        //         {
        //             case "INSERT":
        //                 valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
        //                 if (!valid)
        //                     errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
        //                 break;

        //             case "UPDATE":
        //                 valid = await _originalSpecService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
        //                 if (!valid)
        //                     errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
        //                 break;

        //             case "DELETE":
        //                 valid = await _originalSpecService.CheckDeleteValidAsync(dto.target_pk_id.Value);
        //                 if (!valid)
        //                     errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
        //                 break;

        //             default:
        //                 valid = false;
        //                 errorMessage = $"Unsupported request type: {dto.request_type}";
        //                 break;
        //         }
        //     }

        //     else
        //     {
        //         switch (dto.request_type.ToUpper())
        //         {
        //             case "INSERT":
        //                 valid = await _dynamicService.CheckInsertValidAsync(dto.target_table, newDataToken);
        //                 if (!valid)
        //                     errorMessage = $"Duplicate record exists in table {dto.target_table}.";
        //                 break;

        //             case "UPDATE":
        //                 valid = await _dynamicService.CheckUpdateValidAsync(dto.target_table, dto.target_pk_id, newDataToken);
        //                 if (!valid)
        //                     errorMessage = $"Cannot update because the new value would duplicate an existing record in table {dto.target_table}.";
        //                 break;

        //             case "DELETE":
        //                 valid = await _dynamicService.CheckDeleteValidAsync(dto.target_table, dto.target_pk_id);
        //                 if (!valid)
        //                     errorMessage = $"Cannot delete record in table {dto.target_table} because it is referenced in other tables.";
        //                 break;
        //         }
        //     }

        //     if (!valid)
        //     {
        //         return BadRequest(new
        //         {
        //             message = errorMessage
        //         });
        //     }

        //     var jsonOldData = dto.old_data?.ToString(Newtonsoft.Json.Formatting.None);

        //     var request = new Request
        //     {
        //         request_type = dto.request_type.ToUpper(),
        //         request_status = "Pending",
        //         target_table = dto.target_table,
        //         target_pk_id = dto.target_pk_id,
        //         old_data = jsonOldData,
        //         new_data = jsonNewData,
        //         requested_by = requestedBy,
        //         requested_at = DateTime.Now,
        //         note = dto.note
        //     };

        //     _context.Requests.Add(request);
        //     await _context.SaveChangesAsync();

        //     return Ok(new
        //     {
        //         message = "Request submitted successfully.",
        //         request_id = request.request_id
        //     });
        // }

        [HttpPost]
        [Authorize(Roles = "admin,editor")]
        public async Task<IActionResult> CreateRequest([FromBody] RequestDto dto)
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null)
                return Unauthorized();

            var requestedBy = int.Parse(userIdClaim.Value);

            var jsonNewData = dto.new_data?.ToString(Newtonsoft.Json.Formatting.None);
            JToken? newDataToken = null;

            if (!string.IsNullOrEmpty(jsonNewData))
            {
                newDataToken = JToken.Parse(jsonNewData);
            }

            // ✅ NEW: Check duplicate pending request
            if (dto.request_type.ToUpper() == "UPDATE" || dto.request_type.ToUpper() == "DELETE")
            {
                if (dto.target_pk_id == null)
                {
                    return BadRequest(new
                    {
                        message = "target_pk_id must be provided for UPDATE or DELETE requests."
                    });
                }

                var pendingRequest = await _context.Requests
                    .FirstOrDefaultAsync(r =>
                        r.target_table == dto.target_table &&
                        r.target_pk_id == dto.target_pk_id &&
                        r.request_status == "Pending" &&
                        (r.request_type == "UPDATE" || r.request_type == "DELETE")
                    );

                if (pendingRequest != null)
                {
                    return BadRequest(new
                    {
                        message = $"Cannot create new {dto.request_type} request because request_id {pendingRequest.request_id} is already pending for {dto.target_table}."
                    });
                }
            }


            // ✅ Validation before saving request
            bool valid = true;
            string? errorMessage = null;

            if (dto.target_table == "OriginalSpec")
            {
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
                        break;

                    case "UPDATE":
                        valid = await _originalSpecService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
                        break;

                    case "DELETE":
                        valid = await _originalSpecService.CheckDeleteValidAsync(dto.target_pk_id.Value);
                        if (!valid)
                            errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type: {dto.request_type}";
                        break;
                }
            }
            else if (dto.target_table == "DdcSpec")
            {
                // ✅ NEW: validation for DdcSpec
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        {
                            var checkResult = await _ddcService.CheckInsertValidAsync(newDataToken);
                            valid = checkResult.IsValid;

                            if (!valid)
                                errorMessage = checkResult.Error;

                            break;
                        }

                    case "UPDATE":
                        valid = await _ddcService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new DDCSpec would duplicate existing data.";
                        break;

                    case "DELETE":
                        valid = await _ddcService.CheckDeleteValidAsync(dto.target_pk_id.Value);
                        if (!valid)
                            errorMessage = $"Cannot delete DDCSpec because it is referenced in other tables.";
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type for DDC Tool: {dto.request_type}";
                        break;
                }
            }

            else if (dto.target_table == "PadBrassMap")
            {
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        valid = await _padBrassService.CheckInsertValidAsync(newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot insert because this pad-brass mapping already exists.";
                        break;

                    case "UPDATE":
                        valid = await _padBrassService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new mapping would duplicate an existing pad-brass mapping.";
                        break;

                    case "DELETE":
                        // ✅ PadBrassMap deletion generally safe, no foreign key reference
                        valid = true;
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type for PadBrassMap: {dto.request_type}";
                        break;
                }
            }

            else if (dto.target_table == "PadHstMap")
            {
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        valid = await _padHstService.CheckInsertValidAsync(newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot insert because this pad-hst mapping already exists.";
                        break;

                    case "UPDATE":
                        valid = await _padHstService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new mapping would duplicate an existing pad-hst mapping.";
                        break;

                    case "DELETE":
                        valid = true;
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type for PadHstMap: {dto.request_type}";
                        break;
                }
            }

            else if (dto.target_table == "ToolPadMap")
            {
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        var (isValid, message) = await _toolPadService.CheckInsertValidAsync(newDataToken);
                        valid = isValid;
                        if (!valid)
                            errorMessage = message;
                        break;

                    case "UPDATE":
                        var (isValidUpdate, updateMessage) = await _toolPadService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        valid = isValidUpdate;
                        if (!valid)
                            errorMessage = updateMessage;
                        break;


                    case "DELETE":
                        // ✅ ToolPadMap deletion generally safe, no FK ref from other tables
                        valid = true;
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type for ToolPadMap: {dto.request_type}";
                        break;
                }
            }

            else if (dto.target_table == "ToolMachineMap")
            {
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        valid = await _toolMachineService.CheckInsertValidAsync(newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot insert because this ToolMachineMap mapping already exists or is invalid.";
                        break;

                    case "UPDATE":
                        valid = await _toolMachineService.CheckUpdateValidAsync(dto.target_pk_id.Value, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new mapping would duplicate an existing ToolMachineMap.";
                        break;

                    case "DELETE":
                        // ToolMachineMap deletion is generally safe
                        valid = true;
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type for ToolMachineMap: {dto.request_type}";
                        break;
                }
            }


            else
            {
                // ✅ For other master tables (DynamicService)
                switch (dto.request_type.ToUpper())
                {
                    case "INSERT":
                        valid = await _dynamicService.CheckInsertValidAsync(dto.target_table, newDataToken);
                        if (!valid)
                            errorMessage = $"Duplicate record exists in table {dto.target_table}.";
                        break;

                    case "UPDATE":
                        valid = await _dynamicService.CheckUpdateValidAsync(dto.target_table, dto.target_pk_id, newDataToken);
                        if (!valid)
                            errorMessage = $"Cannot update because the new value would duplicate an existing record in table {dto.target_table}.";
                        break;

                    case "DELETE":
                        valid = await _dynamicService.CheckDeleteValidAsync(dto.target_table, dto.target_pk_id);
                        if (!valid)
                            errorMessage = $"Cannot delete record in table {dto.target_table} because it is referenced in other tables.";
                        break;

                    default:
                        valid = false;
                        errorMessage = $"Unsupported request type: {dto.request_type}";
                        break;
                }
            }

            if (!valid)
            {
                return BadRequest(new
                {
                    message = errorMessage
                });
            }

            var jsonOldData = dto.old_data?.ToString(Newtonsoft.Json.Formatting.None);

            var request = new Request
            {
                request_type = dto.request_type.ToUpper(),
                request_status = "Pending",
                target_table = dto.target_table,
                target_pk_id = dto.target_pk_id,
                old_data = jsonOldData,
                new_data = jsonNewData,
                requested_by = requestedBy,
                requested_at = DateTime.Now,
                note = dto.note
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();
            // ✅ Refresh Cache Manual แทน Trigger
            // await _context.Database.ExecuteSqlRawAsync("EXEC usp_refresh_resolved_original_cache");

            return Ok(new
            {
                message = "Request submitted successfully.",
                request_id = request.request_id
            });
        }


        // [HttpGet]
        // public IActionResult GetAllRequests()
        // {
        //     var list = _context.Requests
        //         .OrderByDescending(r => r.requested_at)
        //         .Select(r => new RequestResponseDto
        //         {
        //             request_id = r.request_id,
        //             request_type = r.request_type,
        //             request_status = r.request_status,
        //             target_table = r.target_table,
        //             target_pk_id = r.target_pk_id,
        //             old_data = r.old_data,
        //             new_data = r.new_data,
        //             requested_by = r.requested_by,
        //             requested_at = r.requested_at,
        //             approved_by = r.approved_by,
        //             approved_at = r.approved_at,
        //             note = r.note
        //         })
        //         .ToList();

        //     return Ok(list);
        // }

        //         [HttpGet]
        //         public IActionResult GetAllRequests(
        //     [FromQuery] string? request_type,
        //     [FromQuery] string? request_status,
        //     [FromQuery] string? target_table,
        //     [FromQuery] string? requested_by,   // ← ใช้ employee_id
        //     [FromQuery] string? approved_by     // ← ใช้ employee_id
        // )
        //         {
        //             var query = _context.Requests
        //                 .AsQueryable();

        //             if (!string.IsNullOrWhiteSpace(request_type))
        //             {
        //                 var requestTypes = request_type.Split(',').Select(x => x.Trim()).ToList();
        //                 query = query.Where(r => requestTypes.Contains(r.request_type));
        //             }

        //             if (!string.IsNullOrWhiteSpace(request_status))
        //             {
        //                 var requestStatuses = request_status.Split(',').Select(x => x.Trim()).ToList();
        //                 query = query.Where(r => requestStatuses.Contains(r.request_status));
        //             }

        //             if (!string.IsNullOrWhiteSpace(target_table))
        //             {
        //                 var targetTables = target_table.Split(',').Select(x => x.Trim()).ToList();
        //                 query = query.Where(r => targetTables.Contains(r.target_table));
        //             }

        //             if (!string.IsNullOrWhiteSpace(requested_by))
        //             {
        //                 var requestedEmployeeIds = requested_by.Split(',').Select(x => x.Trim()).ToList();

        //                 var matchingUserIds = _context.Users
        //                     .Where(u => requestedEmployeeIds.Contains(u.employee_id))
        //                     .Select(u => u.user_id)
        //                     .ToList();

        //                 query = query.Where(r => matchingUserIds.Contains(r.requested_by));
        //             }

        //             if (!string.IsNullOrWhiteSpace(approved_by))
        //             {
        //                 var approvedEmployeeIds = approved_by.Split(',').Select(x => x.Trim()).ToList();

        //                 var matchingUserIds = _context.Users
        //                     .Where(u => approvedEmployeeIds.Contains(u.employee_id))
        //                     .Select(u => u.user_id)
        //                     .ToList();

        //                 query = query.Where(r => r.approved_by != null && matchingUserIds.Contains(r.approved_by.Value));
        //             }

        //             // 💡 Join user เพื่อดึง employee_id กลับมาแสดงด้วยก็ได้
        //             var users = _context.Users.ToDictionary(u => u.user_id, u => u.employee_id);

        //             var list = query
        //                 .OrderByDescending(r => r.requested_at)
        //                 .ToList()
        //                 .Select(r => new RequestResponseDto
        //                 {
        //                     request_id = r.request_id,
        //                     request_type = r.request_type,
        //                     request_status = r.request_status,
        //                     target_table = r.target_table,
        //                     target_pk_id = r.target_pk_id,
        //                     old_data = r.old_data,
        //                     new_data = r.new_data,
        //                     requested_by = users.ContainsKey(r.requested_by) ? users[r.requested_by] : null,
        //                     requested_at = r.requested_at,
        //                     approved_by = r.approved_by != null && users.ContainsKey(r.approved_by.Value)
        //                         ? users[r.approved_by.Value]
        //                         : null,
        //                     approved_at = r.approved_at,
        //                     note = r.note
        //                 });

        //             return Ok(list);
        //         }

        //         [HttpGet]
        //         public async Task<IActionResult> GetAllRequests(
        //     [FromQuery] string? request_type,
        //     [FromQuery] string? request_status,
        //     [FromQuery] string? target_table,
        //     [FromQuery] string? requested_by,   // employee_id
        //     [FromQuery] string? approved_by,   // employee_id
        //     [FromQuery] string? request_id     // ID
        // )
        //         {
        //             var requestTypes = string.IsNullOrWhiteSpace(request_type)
        //                 ? new List<string>()
        //                 : request_type.Split(',').Select(x => x.Trim()).ToList();

        //             var requestStatuses = string.IsNullOrWhiteSpace(request_status)
        //                 ? new List<string>()
        //                 : request_status.Split(',').Select(x => x.Trim()).ToList();

        //             var targetTables = string.IsNullOrWhiteSpace(target_table)
        //                 ? new List<string>()
        //                 : target_table.Split(',').Select(x => x.Trim()).ToList();

        //             var requestedByEmployees = string.IsNullOrWhiteSpace(requested_by)
        //                 ? new List<string>()
        //                 : requested_by.Split(',').Select(x => x.Trim()).ToList();

        //             var approvedByEmployees = string.IsNullOrWhiteSpace(approved_by)
        //                 ? new List<string>()
        //                 : approved_by.Split(',').Select(x => x.Trim()).ToList();

        //             var requestIds = string.IsNullOrWhiteSpace(request_id)
        //                 ? new List<int>()
        //                 : request_id
        //                     .Split(',')
        //                     .Select(x => int.TryParse(x.Trim(), out var id) ? (int?)id : null)
        //                     .Where(id => id != null)
        //                     .Select(id => id.Value)
        //                     .ToList();

        //             // 🔍 ดึง employee_id จาก user_id
        //             var userDict = await _context.Users
        //                 .Select(u => new { u.user_id, u.employee_id })
        //                 .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

        //             // 🔍 ดึง user_id ที่ตรงกับ employee_id จาก query string
        //             var requestedByUserIds = userDict
        //                 .Where(x => requestedByEmployees.Contains(x.Value))
        //                 .Select(x => x.Key)
        //                 .ToList();

        //             var approvedByUserIds = userDict
        //                 .Where(x => approvedByEmployees.Contains(x.Value))
        //                 .Select(x => x.Key)
        //                 .ToList();

        //             // 🔍 ดึงข้อมูลทั้งหมดก่อน
        //             var raw = await _context.Requests
        //                 .OrderByDescending(r => r.requested_at)
        //                 .Select(r => new
        //                 {
        //                     r.request_id,
        //                     r.request_type,
        //                     r.request_status,
        //                     r.target_table,
        //                     r.target_pk_id,
        //                     r.old_data,
        //                     r.new_data,
        //                     r.requested_by,
        //                     r.requested_at,
        //                     r.approved_by,
        //                     r.approved_at,
        //                     r.note,
        //                     requested_emp = userDict.ContainsKey(r.requested_by) ? userDict[r.requested_by] : null,
        //                     approved_emp = r.approved_by != null && userDict.ContainsKey(r.approved_by.Value)
        //                         ? userDict[r.approved_by.Value]
        //                         : null
        //                 })
        //                 .ToListAsync();

        //             // ✅ เติม pending_request ถ้ามี (ถ้าอยากใส่ในอนาคต)
        //             var result = raw.Select(r => new RequestResponseDto
        //             {
        //                 request_id = r.request_id,
        //                 request_type = r.request_type,
        //                 request_status = r.request_status,
        //                 target_table = r.target_table,
        //                 target_pk_id = r.target_pk_id,
        //                 old_data = r.old_data,
        //                 new_data = r.new_data,
        //                 requested_by = r.requested_emp,
        //                 requested_at = r.requested_at,
        //                 approved_by = r.approved_emp,
        //                 approved_at = r.approved_at,
        //                 note = r.note
        //             });

        //             // ✅ Apply filters
        //             if (requestTypes.Any())
        //                 result = result.Where(x => requestTypes.Contains(x.request_type));

        //             if (requestStatuses.Any())
        //                 result = result.Where(x => requestStatuses.Contains(x.request_status));

        //             if (targetTables.Any())
        //                 result = result.Where(x => targetTables.Contains(x.target_table));

        //             if (requestIds.Any())
        //                 result = result.Where(x => requestIds.Contains(x.request_id));

        //             if (requestedByUserIds.Any())
        //                 result = result.Where(x => x.requested_by != null && requestedByEmployees.Contains(x.requested_by));

        //             if (approvedByUserIds.Any())
        //                 result = result.Where(x => x.approved_by != null && approvedByEmployees.Contains(x.approved_by));

        //             return Ok(result.ToList());
        //         }

        // [HttpGet]
        // public async Task<IActionResult> GetAllRequests(
        //     [FromQuery] string? request_type,
        //     [FromQuery] string? request_status,
        //     [FromQuery] string? target_table,
        //     [FromQuery] string? requested_by,
        //     [FromQuery] string? approved_by,
        //     [FromQuery] string? request_id
        // )
        // {
        //     var requestTypes = string.IsNullOrWhiteSpace(request_type)
        //         ? new List<string>()
        //         : request_type.Split(',').Select(x => x.Trim()).ToList();

        //     var requestStatuses = string.IsNullOrWhiteSpace(request_status)
        //         ? new List<string>()
        //         : request_status.Split(',').Select(x => x.Trim()).ToList();

        //     var targetTables = string.IsNullOrWhiteSpace(target_table)
        //         ? new List<string>()
        //         : target_table.Split(',').Select(x => x.Trim()).ToList();

        //     var requestedByEmployees = string.IsNullOrWhiteSpace(requested_by)
        //         ? new List<string>()
        //         : requested_by.Split(',').Select(x => x.Trim()).ToList();

        //     var approvedByEmployees = string.IsNullOrWhiteSpace(approved_by)
        //         ? new List<string>()
        //         : approved_by.Split(',').Select(x => x.Trim()).ToList();

        //     var requestIds = string.IsNullOrWhiteSpace(request_id)
        //         ? new List<int>()
        //         : request_id
        //             .Split(',')
        //             .Select(x => int.TryParse(x.Trim(), out var id) ? (int?)id : null)
        //             .Where(id => id != null)
        //             .Select(id => id.Value)
        //             .ToList();

        //     var userDict = await _context.Users
        //         .Select(u => new { u.user_id, u.employee_id })
        //         .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

        //     var requestedByUserIds = userDict
        //         .Where(x => requestedByEmployees.Contains(x.Value))
        //         .Select(x => x.Key)
        //         .ToList();

        //     var approvedByUserIds = userDict
        //         .Where(x => approvedByEmployees.Contains(x.Value))
        //         .Select(x => x.Key)
        //         .ToList();

        //     var raw = await _context.Requests
        //         .OrderByDescending(r => r.requested_at)
        //         .Select(r => new
        //         {
        //             r.request_id,
        //             r.request_type,
        //             r.request_status,
        //             r.target_table,
        //             r.target_pk_id,
        //             r.old_data,
        //             r.new_data,
        //             r.requested_by,
        //             r.requested_at,
        //             r.approved_by,
        //             r.approved_at,
        //             r.note,
        //             requested_emp = userDict.ContainsKey(r.requested_by) ? userDict[r.requested_by] : null,
        //             approved_emp = r.approved_by != null && userDict.ContainsKey(r.approved_by.Value)
        //                 ? userDict[r.approved_by.Value]
        //                 : null
        //         })
        //         .ToListAsync();

        //     var result = raw.Select(r => new RequestResponseDto
        //     {
        //         request_id = r.request_id,
        //         request_type = r.request_type,
        //         request_status = r.request_status,
        //         target_table = r.target_table,
        //         target_pk_id = r.target_pk_id,
        //         old_data = r.old_data,
        //         new_data = r.new_data,
        //         requested_by = r.requested_emp,
        //         requested_at = r.requested_at,
        //         approved_by = r.approved_emp,
        //         approved_at = r.approved_at,
        //         note = r.note
        //     });

        //     if (requestTypes.Any())
        //         result = result.Where(x => requestTypes.Contains(x.request_type));

        //     if (requestStatuses.Any())
        //         result = result.Where(x => requestStatuses.Contains(x.request_status));

        //     if (targetTables.Any())
        //         result = result.Where(x => targetTables.Contains(x.target_table));

        //     if (requestIds.Any())
        //         result = result.Where(x => requestIds.Contains(x.request_id));

        //     // ✅ รองรับ "" == IS NULL
        //     if (requestedByEmployees.Any())
        //     {
        //         bool hasNull = requestedByEmployees.Contains("");
        //         result = result.Where(x =>
        //             (x.requested_by == null && hasNull) ||
        //             (x.requested_by != null && requestedByEmployees.Contains(x.requested_by)));
        //     }

        //     if (approvedByEmployees.Any())
        //     {
        //         bool hasNull = approvedByEmployees.Contains("");
        //         result = result.Where(x =>
        //             (x.approved_by == null && hasNull) ||
        //             (x.approved_by != null && approvedByEmployees.Contains(x.approved_by)));
        //     }

        //     return Ok(result.ToList());
        // }

        [HttpGet]
        public async Task<IActionResult> GetAllRequests(
            [FromQuery] string? request_type,
            [FromQuery] string? request_status,
            [FromQuery] string? target_table,
            [FromQuery] string? requested_by,
            [FromQuery] string? approved_by,
            [FromQuery] string? request_id,
            [FromQuery] string? requested_at_start,
            [FromQuery] string? requested_at_end,
            [FromQuery] string? approved_at_start,
            [FromQuery] string? approved_at_end
        )
        {
            var requestTypes = string.IsNullOrWhiteSpace(request_type)
                ? new List<string>()
                : request_type.Split(',').Select(x => x.Trim()).ToList();

            var requestStatuses = string.IsNullOrWhiteSpace(request_status)
                ? new List<string>()
                : request_status.Split(',').Select(x => x.Trim()).ToList();

            var targetTables = string.IsNullOrWhiteSpace(target_table)
                ? new List<string>()
                : target_table.Split(',').Select(x => x.Trim()).ToList();

            var requestedByEmployees = string.IsNullOrWhiteSpace(requested_by)
                ? new List<string>()
                : requested_by.Split(',').Select(x => x.Trim()).ToList();

            var approvedByEmployees = string.IsNullOrWhiteSpace(approved_by)
                ? new List<string>()
                : approved_by.Split(',').Select(x => x.Trim()).ToList();

            var requestIds = string.IsNullOrWhiteSpace(request_id)
                ? new List<int>()
                : request_id
                    .Split(',')
                    .Select(x => int.TryParse(x.Trim(), out var id) ? (int?)id : null)
                    .Where(id => id != null)
                    .Select(id => id.Value)
                    .ToList();

            // ✅ แปลง string → DateTime?
            DateTime? requestedStart = DateTime.TryParse(requested_at_start, out var rs) ? rs : null;
            DateTime? requestedEnd = DateTime.TryParse(requested_at_end, out var re) ? re : null;
            DateTime? approvedStart = DateTime.TryParse(approved_at_start, out var aps) ? aps : null;
            DateTime? approvedEnd = DateTime.TryParse(approved_at_end, out var ape) ? ape : null;

            // ⬇️ ใส่ตรงนี้เลย
            if (requestedEnd != null)
                requestedEnd = requestedEnd.Value.Date.AddDays(1).AddTicks(-1);

            if (approvedEnd != null)
                approvedEnd = approvedEnd.Value.Date.AddDays(1).AddTicks(-1);

            var userDict = await _context.Users
                .Select(u => new { u.user_id, u.employee_id })
                .ToDictionaryAsync(u => u.user_id, u => u.employee_id);

            var requestedByUserIds = userDict
                .Where(x => requestedByEmployees.Contains(x.Value))
                .Select(x => x.Key)
                .ToList();

            var approvedByUserIds = userDict
                .Where(x => approvedByEmployees.Contains(x.Value))
                .Select(x => x.Key)
                .ToList();

            var raw = await _context.Requests
                .OrderByDescending(r => r.requested_at)
                .Select(r => new
                {
                    r.request_id,
                    r.request_type,
                    r.request_status,
                    r.target_table,
                    r.target_pk_id,
                    r.old_data,
                    r.new_data,
                    r.requested_by,
                    r.requested_at,
                    r.approved_by,
                    r.approved_at,
                    r.note,
                    requested_emp = userDict.ContainsKey(r.requested_by) ? userDict[r.requested_by] : null,
                    approved_emp = r.approved_by != null && userDict.ContainsKey(r.approved_by.Value)
                        ? userDict[r.approved_by.Value]
                        : null
                })
                .ToListAsync();

            var result = raw.Select(r => new RequestResponseDto
            {
                request_id = r.request_id,
                request_type = r.request_type,
                request_status = r.request_status,
                target_table = r.target_table,
                target_pk_id = r.target_pk_id,
                old_data = r.old_data,
                new_data = r.new_data,
                requested_by = r.requested_emp,
                requested_at = r.requested_at,
                approved_by = r.approved_emp,
                approved_at = r.approved_at,
                note = r.note
            });

            if (requestTypes.Any())
                result = result.Where(x => requestTypes.Contains(x.request_type));

            if (requestStatuses.Any())
                result = result.Where(x => requestStatuses.Contains(x.request_status));

            if (targetTables.Any())
                result = result.Where(x => targetTables.Contains(x.target_table));

            if (requestIds.Any())
                result = result.Where(x => requestIds.Contains(x.request_id));

            // ✅ รองรับ "" == IS NULL
            if (requestedByEmployees.Any())
            {
                bool hasNull = requestedByEmployees.Contains("");
                result = result.Where(x =>
                    (x.requested_by == null && hasNull) ||
                    (x.requested_by != null && requestedByEmployees.Contains(x.requested_by)));
            }

            if (approvedByEmployees.Any())
            {
                bool hasNull = approvedByEmployees.Contains("");
                result = result.Where(x =>
                    (x.approved_by == null && hasNull) ||
                    (x.approved_by != null && approvedByEmployees.Contains(x.approved_by)));
            }

            // ✅ Filter วันที่ตาม EnhancedDateTimeFilter
            if (requestedStart != null)
                result = result.Where(x => x.requested_at >= requestedStart.Value);

            if (requestedEnd != null)
                result = result.Where(x => x.requested_at <= requestedEnd.Value);

            if (approvedStart != null)
                result = result.Where(x => x.approved_at != null && x.approved_at >= approvedStart.Value);

            if (approvedEnd != null)
                result = result.Where(x => x.approved_at != null && x.approved_at <= approvedEnd.Value);

            return Ok(result.ToList());
        }

        // [HttpPost("approve/{id}")]
        // [Authorize(Roles = "admin")]
        // public async Task<IActionResult> ApproveRequest(int id)
        // {
        //     var adminIdClaim = User.FindFirst("user_id");
        //     if (adminIdClaim == null)
        //         return Unauthorized();

        //     var adminId = int.Parse(adminIdClaim.Value);

        //     var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
        //     if (req == null)
        //         return NotFound();

        //     if (req.request_status != "Pending")
        //         return BadRequest("Request already processed.");

        //     using var transaction = _context.Database.BeginTransaction();
        //     try
        //     {
        //         JToken? newDataToken = null;

        //         if (!string.IsNullOrEmpty(req.new_data))
        //         {
        //             var token = JToken.Parse(req.new_data);

        //             if (token.Type == JTokenType.String)
        //             {
        //                 newDataToken = JToken.Parse(token.Value<string>() ?? "{}");
        //             }
        //             else
        //             {
        //                 newDataToken = token;
        //             }
        //         }

        //         bool result;
        //         int? newPkId = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => (await _originalSpecService.InsertOriginalSpecAsync(newDataToken, adminId)) != null,
        //                 "UPDATE" => await _originalSpecService.UpdateOriginalSpecAsync(req.target_pk_id.Value, newDataToken, adminId),
        //                 "DELETE" => await _originalSpecService.DeleteOriginalSpecAsync(req.target_pk_id.Value, adminId),
        //                 _ => false
        //             };
        //         }
        //         else
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => await _dynamicService.InsertAsync(req.target_table, newDataToken, adminId),
        //                 "UPDATE" => await _dynamicService.UpdateAsync(req.target_table, req.target_pk_id, newDataToken, adminId),
        //                 "DELETE" => await _dynamicService.DeleteAsync(req.target_table, req.target_pk_id, adminId),
        //                 _ => false
        //             };
        //         }

        //         if (!result)
        //             return BadRequest($"Failed to execute {req.request_type} for table {req.target_table}. It might be duplicate or invalid data.");

        //         req.request_status = "Completed";
        //         req.approved_by = adminId;
        //         req.approved_at = DateTime.Now;

        //         if (newPkId != null)
        //         {
        //             req.target_pk_id = newPkId;
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return Ok(new { message = "Request approved and executed successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, $"Error during execution: {ex.Message}");
        //     }
        // }

        // [HttpPost("approve/{id}")]
        // [Authorize(Roles = "admin")]
        // public async Task<IActionResult> ApproveRequest(int id)
        // {
        //     var adminIdClaim = User.FindFirst("user_id");
        //     if (adminIdClaim == null)
        //         return Unauthorized();

        //     var adminId = int.Parse(adminIdClaim.Value);

        //     var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
        //     if (req == null)
        //         return NotFound();

        //     if (req.request_status != "Pending")
        //         return BadRequest("Request already processed.");

        //     if ((req.request_type == "UPDATE" || req.request_type == "DELETE")
        //         && !req.target_pk_id.HasValue)
        //     {
        //         return BadRequest("Request missing primary key id for update or delete.");
        //     }

        //     using var transaction = _context.Database.BeginTransaction();
        //     try
        //     {
        //         JToken? newDataToken = null;

        //         if (!string.IsNullOrEmpty(req.new_data))
        //         {
        //             var token = JToken.Parse(req.new_data);

        //             if (token.Type == JTokenType.String)
        //             {
        //                 newDataToken = JToken.Parse(token.Value<string>() ?? "{}");
        //             }
        //             else
        //             {
        //                 newDataToken = token;
        //             }
        //         }

        //         // ✅ validate again before executing
        //         bool valid = true;
        //         string? errorMessage = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _originalSpecService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _originalSpecService.CheckDeleteValidAsync(req.target_pk_id.Value);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type: {req.request_type}";
        //                     break;
        //             }
        //         }
        //         else
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _dynamicService.CheckInsertValidAsync(req.target_table, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Duplicate record exists in table {req.target_table}.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _dynamicService.CheckUpdateValidAsync(req.target_table, req.target_pk_id, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new value would duplicate an existing record in table {req.target_table}.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _dynamicService.CheckDeleteValidAsync(req.target_table, req.target_pk_id);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete record in table {req.target_table} because it is referenced in other tables.";
        //                     break;
        //             }
        //         }

        //         if (!valid)
        //         {
        //             return BadRequest(new
        //             {
        //                 message = $"Validation failed during approval: {errorMessage}"
        //             });
        //         }

        //         bool result;
        //         int? newPkId = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => (await _originalSpecService.InsertOriginalSpecAsync(newDataToken, adminId)) != null,
        //                 "UPDATE" => await _originalSpecService.UpdateOriginalSpecAsync(req.target_pk_id.Value, newDataToken, adminId),
        //                 "DELETE" => await _originalSpecService.DeleteOriginalSpecAsync(req.target_pk_id.Value, adminId),
        //                 _ => false
        //             };
        //         }
        //         else
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => await _dynamicService.InsertAsync(req.target_table, newDataToken, adminId),
        //                 "UPDATE" => await _dynamicService.UpdateAsync(req.target_table, req.target_pk_id, newDataToken, adminId),
        //                 "DELETE" => await _dynamicService.DeleteAsync(req.target_table, req.target_pk_id, adminId),
        //                 _ => false
        //             };
        //         }

        //         if (!result)
        //             return BadRequest(new
        //             {
        //                 message = $"Failed to execute {req.request_type} for table {req.target_table}. It might be duplicate or invalid data."
        //             });

        //         req.request_status = "Completed";
        //         req.approved_by = adminId;
        //         req.approved_at = DateTime.Now;

        //         if (newPkId != null)
        //         {
        //             req.target_pk_id = newPkId;
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return Ok(new { message = "Request approved and executed successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, $"Error during execution: {ex.Message}");
        //     }
        // }

        // [HttpPost("approve/{id}")]
        // [Authorize(Roles = "admin")]
        // public async Task<IActionResult> ApproveRequest(int id)
        // {
        //     var adminIdClaim = User.FindFirst("user_id");
        //     if (adminIdClaim == null)
        //         return Unauthorized();

        //     var adminId = int.Parse(adminIdClaim.Value);

        //     var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
        //     if (req == null)
        //         return NotFound();

        //     if (req.request_status != "Pending")
        //         return BadRequest("Request already processed.");

        //     if ((req.request_type == "UPDATE" || req.request_type == "DELETE")
        //         && !req.target_pk_id.HasValue)
        //     {
        //         return BadRequest("Request missing primary key id for update or delete.");
        //     }

        //     using var transaction = _context.Database.BeginTransaction();
        //     try
        //     {
        //         JToken? newDataToken = null;

        //         if (!string.IsNullOrEmpty(req.new_data))
        //         {
        //             var token = JToken.Parse(req.new_data);

        //             if (token.Type == JTokenType.String)
        //             {
        //                 newDataToken = JToken.Parse(token.Value<string>() ?? "{}");
        //             }
        //             else
        //             {
        //                 newDataToken = token;
        //             }
        //         }

        //         // ✅ validate again before executing
        //         bool valid = true;
        //         string? errorMessage = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _originalSpecService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _originalSpecService.CheckDeleteValidAsync(req.target_pk_id.Value);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type: {req.request_type}";
        //                     break;
        //             }
        //         }
        //         else if (req.target_table == "DdcSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _ddcService.CheckInsertValidAsync(newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot insert DDC tool because it already exists.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _ddcService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new DDCSpec would duplicate existing data.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _ddcService.CheckDeleteValidAsync(req.target_pk_id.Value);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete DDCSpec because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type for DDC Tool: {req.request_type}";
        //                     break;
        //             }
        //         }

        //         else
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _dynamicService.CheckInsertValidAsync(req.target_table, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Duplicate record exists in table {req.target_table}.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _dynamicService.CheckUpdateValidAsync(req.target_table, req.target_pk_id, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new value would duplicate an existing record in table {req.target_table}.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _dynamicService.CheckDeleteValidAsync(req.target_table, req.target_pk_id);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete record in table {req.target_table} because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type: {req.request_type}";
        //                     break;
        //             }
        //         }

        //         if (!valid)
        //         {
        //             return BadRequest(new
        //             {
        //                 message = $"Validation failed during approval: {errorMessage}"
        //             });
        //         }

        //         bool result;
        //         int? newPkId = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => (await _originalSpecService.InsertOriginalSpecAsync(newDataToken, adminId)) != null,
        //                 "UPDATE" => await _originalSpecService.UpdateOriginalSpecAsync(req.target_pk_id.Value, newDataToken, adminId),
        //                 "DELETE" => await _originalSpecService.DeleteOriginalSpecAsync(req.target_pk_id.Value, adminId),
        //                 _ => false
        //             };
        //         }
        //         else if (req.target_table == "DdcSpec")
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => await _ddcService.InsertDdcToolAsync(newDataToken, adminId),
        //                 "UPDATE" => await _ddcService.UpdateDdcToolAsync(req.target_pk_id.Value, newDataToken, adminId),
        //                 "DELETE" => await _ddcService.DeleteDdcToolAsync(req.target_pk_id.Value, adminId),
        //                 _ => false
        //             };
        //         }
        //         else
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => await _dynamicService.InsertAsync(req.target_table, newDataToken, adminId),
        //                 "UPDATE" => await _dynamicService.UpdateAsync(req.target_table, req.target_pk_id, newDataToken, adminId),
        //                 "DELETE" => await _dynamicService.DeleteAsync(req.target_table, req.target_pk_id, adminId),
        //                 _ => false
        //             };
        //         }

        //         if (!result)
        //             return BadRequest(new
        //             {
        //                 message = $"Failed to execute {req.request_type} for table {req.target_table}. It might be duplicate or invalid data."
        //             });

        //         req.request_status = "Completed";
        //         req.approved_by = adminId;
        //         req.approved_at = DateTime.Now;

        //         if (newPkId != null)
        //         {
        //             req.target_pk_id = newPkId;
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return Ok(new { message = "Request approved and executed successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, $"Error during execution: {ex.Message}");
        //     }
        // }

        // [HttpPost("approve/{id}")]
        // [Authorize(Roles = "admin")]
        // public async Task<IActionResult> ApproveRequest(int id)
        // {
        //     var adminIdClaim = User.FindFirst("user_id");
        //     if (adminIdClaim == null)
        //         return Unauthorized();

        //     var adminId = int.Parse(adminIdClaim.Value);

        //     var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
        //     if (req == null)
        //         return NotFound();

        //     if (req.request_status != "Pending")
        //         return BadRequest("Request already processed.");

        //     if ((req.request_type == "UPDATE" || req.request_type == "DELETE")
        //         && !req.target_pk_id.HasValue)
        //     {
        //         return BadRequest("Request missing primary key id for update or delete.");
        //     }

        //     using var transaction = _context.Database.BeginTransaction();
        //     try
        //     {
        //         JToken? newDataToken = null;

        //         if (!string.IsNullOrEmpty(req.new_data))
        //         {
        //             var token = JToken.Parse(req.new_data);

        //             if (token.Type == JTokenType.String)
        //             {
        //                 newDataToken = JToken.Parse(token.Value<string>() ?? "{}");
        //             }
        //             else
        //             {
        //                 newDataToken = token;
        //             }
        //         }

        //         bool valid = true;
        //         string? errorMessage = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _originalSpecService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _originalSpecService.CheckDeleteValidAsync(req.target_pk_id.Value);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type: {req.request_type}";
        //                     break;
        //             }
        //         }
        //         else if (req.target_table == "DdcSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _ddcService.CheckInsertValidAsync(newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot insert DDC tool because it already exists.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _ddcService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new DDCSpec would duplicate existing data.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _ddcService.CheckDeleteValidAsync(req.target_pk_id.Value);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete DDCSpec because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type for DDC Tool: {req.request_type}";
        //                     break;
        //             }
        //         }
        //         else
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     valid = await _dynamicService.CheckInsertValidAsync(req.target_table, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Duplicate record exists in table {req.target_table}.";
        //                     break;

        //                 case "UPDATE":
        //                     valid = await _dynamicService.CheckUpdateValidAsync(req.target_table, req.target_pk_id, newDataToken);
        //                     if (!valid)
        //                         errorMessage = $"Cannot update because the new value would duplicate an existing record in table {req.target_table}.";
        //                     break;

        //                 case "DELETE":
        //                     valid = await _dynamicService.CheckDeleteValidAsync(req.target_table, req.target_pk_id);
        //                     if (!valid)
        //                         errorMessage = $"Cannot delete record in table {req.target_table} because it is referenced in other tables.";
        //                     break;

        //                 default:
        //                     valid = false;
        //                     errorMessage = $"Unsupported request type: {req.request_type}";
        //                     break;
        //             }
        //         }

        //         if (!valid)
        //         {
        //             return BadRequest(new
        //             {
        //                 message = $"Validation failed during approval: {errorMessage}"
        //             });
        //         }

        //         object result;
        //         int? newPkId = null;

        //         if (req.target_table == "OriginalSpec")
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => (await _originalSpecService.InsertOriginalSpecAsync(newDataToken, adminId)) != null,
        //                 "UPDATE" => await _originalSpecService.UpdateOriginalSpecAsync(req.target_pk_id.Value, newDataToken, adminId),
        //                 "DELETE" => await _originalSpecService.DeleteOriginalSpecAsync(req.target_pk_id.Value, adminId),
        //                 _ => false
        //             };
        //         }
        //         else if (req.target_table == "DdcSpec")
        //         {
        //             switch (req.request_type.ToUpper())
        //             {
        //                 case "INSERT":
        //                     result = await _ddcService.InsertDdcToolAsync(newDataToken, adminId);
        //                     break;

        //                 case "UPDATE":
        //                     result = await _ddcService.UpdateDdcToolAsync(req.target_pk_id.Value, newDataToken, adminId);
        //                     break;

        //                 case "DELETE":
        //                     result = await _ddcService.DeleteDdcToolAsync(req.target_pk_id.Value, adminId);
        //                     break;

        //                 default:
        //                     result = false;
        //                     break;
        //             }
        //         }
        //         else
        //         {
        //             result = req.request_type.ToUpper() switch
        //             {
        //                 "INSERT" => await _dynamicService.InsertAsync(req.target_table, newDataToken, adminId),
        //                 "UPDATE" => await _dynamicService.UpdateAsync(req.target_table, req.target_pk_id, newDataToken, adminId),
        //                 "DELETE" => await _dynamicService.DeleteAsync(req.target_table, req.target_pk_id, adminId),
        //                 _ => false
        //             };
        //         }

        //         bool success = result switch
        //         {
        //             bool b => b,
        //             Dictionary<string, object?> dict => dict.Count > 0,
        //             _ => false
        //         };

        //         if (!success)
        //         {
        //             return BadRequest(new
        //             {
        //                 message = $"Failed to execute {req.request_type} for table {req.target_table}. It might be duplicate or invalid data."
        //             });
        //         }

        //         // ✅ save old_data เฉพาะ UPDATE ของ DdcSpec
        //         if (req.target_table == "DdcSpec" && req.request_type.ToUpper() == "UPDATE")
        //         {
        //             req.old_data = JsonConvert.SerializeObject(result, Formatting.Indented);
        //         }

        //         req.request_status = "Completed";
        //         req.approved_by = adminId;
        //         req.approved_at = DateTime.Now;

        //         if (newPkId != null)
        //         {
        //             req.target_pk_id = newPkId;
        //         }

        //         await _context.SaveChangesAsync();
        //         await transaction.CommitAsync();

        //         return Ok(new { message = "Request approved and executed successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         await transaction.RollbackAsync();
        //         return StatusCode(500, $"Error during execution: {ex.Message}");
        //     }
        // }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var adminIdClaim = User.FindFirst("user_id");
            if (adminIdClaim == null)
                return Unauthorized();

            var adminId = int.Parse(adminIdClaim.Value);

            var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
            if (req == null)
                return NotFound();

            if (req.request_status != "Pending")
                return BadRequest("Request already processed.");

            if ((req.request_type == "UPDATE" || req.request_type == "DELETE")
                && !req.target_pk_id.HasValue)
            {
                return BadRequest("Request missing primary key id for update or delete.");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                JToken? newDataToken = null;

                if (!string.IsNullOrEmpty(req.new_data))
                {
                    var token = JToken.Parse(req.new_data);

                    if (token.Type == JTokenType.String)
                    {
                        newDataToken = JToken.Parse(token.Value<string>() ?? "{}");
                    }
                    else
                    {
                        newDataToken = token;
                    }
                }

                bool valid = true;
                string? errorMessage = null;

                if (req.target_table == "OriginalSpec")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            valid = await _originalSpecService.CheckInsertValidAsync(newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot insert because OriginalSpec already exists with the same Tool, Type, Size, and Axle.";
                            break;

                        case "UPDATE":
                            valid = await _originalSpecService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because the new OriginalSpec would duplicate existing data.";
                            break;

                        case "DELETE":
                            valid = await _originalSpecService.CheckDeleteValidAsync(req.target_pk_id.Value);
                            if (!valid)
                                errorMessage = $"Cannot delete OriginalSpec because it is referenced in other tables.";
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type: {req.request_type}";
                            break;
                    }
                }
                else if (req.target_table == "DdcSpec")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            {
                                var checkResult = await _ddcService.CheckInsertValidAsync(newDataToken);
                                valid = checkResult.IsValid;

                                if (!valid)
                                    errorMessage = checkResult.Error;

                                break;
                            }

                        case "UPDATE":
                            valid = await _ddcService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because the new DDCSpec would duplicate existing data.";
                            break;

                        case "DELETE":
                            valid = await _ddcService.CheckDeleteValidAsync(req.target_pk_id.Value);
                            if (!valid)
                                errorMessage = $"Cannot delete DDCSpec because it is referenced in other tables.";
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type for DDC Tool: {req.request_type}";
                            break;
                    }
                }
                else if (req.target_table == "PadBrassMap")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            valid = await _padBrassService.CheckInsertValidAsync(newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot insert because this pad-brass mapping already exists.";
                            break;

                        case "UPDATE":
                            valid = await _padBrassService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because it would duplicate an existing pad-brass mapping.";
                            break;

                        case "DELETE":
                            valid = true; // safe to delete PadBrassMap
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type for PadBrassMap: {req.request_type}";
                            break;
                    }
                }

                else if (req.target_table == "PadHstMap")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            valid = await _padHstService.CheckInsertValidAsync(newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot insert because this pad-hst mapping already exists.";
                            break;

                        case "UPDATE":
                            valid = await _padHstService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because it would duplicate an existing pad-hst mapping.";
                            break;

                        case "DELETE":
                            valid = await _padHstService.CheckDeleteValidAsync(req.target_pk_id.Value);
                            if (!valid)
                                errorMessage = $"Cannot delete PadHstMap because it is still used by tools.";
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type for PadHstMap: {req.request_type}";
                            break;
                    }
                }

                else if (req.target_table == "ToolPadMap")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            var (isValid, message) = await _toolPadService.CheckInsertValidAsync(newDataToken);
                            valid = isValid;
                            if (!valid)
                                errorMessage = message;
                            break;

                        case "UPDATE":
                            (bool isValidUpdate, string updateMessage) = await _toolPadService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            valid = isValidUpdate;
                            if (!valid)
                                errorMessage = updateMessage;
                            break;

                        case "DELETE":
                            valid = true; // ToolPadMap generally safe to delete
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type for ToolPadMap: {req.request_type}";
                            break;
                    }
                }

                else if (req.target_table == "ToolMachineMap")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            valid = await _toolMachineService.CheckInsertValidAsync(newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot insert because this ToolMachineMap mapping already exists or is invalid.";
                            break;

                        case "UPDATE":
                            valid = await _toolMachineService.CheckUpdateValidAsync(req.target_pk_id.Value, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because it would duplicate an existing ToolMachineMap.";
                            break;

                        case "DELETE":
                            valid = true; // ToolMachineMap generally safe to delete
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type for ToolMachineMap: {req.request_type}";
                            break;
                    }
                }


                else
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            valid = await _dynamicService.CheckInsertValidAsync(req.target_table, newDataToken);
                            if (!valid)
                                errorMessage = $"Duplicate record exists in table {req.target_table}.";
                            break;

                        case "UPDATE":
                            valid = await _dynamicService.CheckUpdateValidAsync(req.target_table, req.target_pk_id, newDataToken);
                            if (!valid)
                                errorMessage = $"Cannot update because the new value would duplicate an existing record in table {req.target_table}.";
                            break;

                        case "DELETE":
                            valid = await _dynamicService.CheckDeleteValidAsync(req.target_table, req.target_pk_id);
                            if (!valid)
                                errorMessage = $"Cannot delete record in table {req.target_table} because it is referenced in other tables.";
                            break;

                        default:
                            valid = false;
                            errorMessage = $"Unsupported request type: {req.request_type}";
                            break;
                    }
                }

                if (!valid)
                {
                    return BadRequest(new
                    {
                        message = $"Validation failed during approval: {errorMessage}"
                    });
                }

                object result;
                int? newPkId = null;

                if (req.target_table == "OriginalSpec")
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => (await _originalSpecService.InsertOriginalSpecAsync(newDataToken, adminId)) != null,
                        "UPDATE" => await _originalSpecService.UpdateOriginalSpecAsync(req.target_pk_id.Value, newDataToken, adminId),
                        "DELETE" => await _originalSpecService.DeleteOriginalSpecAsync(req.target_pk_id.Value, adminId),
                        _ => false
                    };
                }
                else if (req.target_table == "DdcSpec")
                {
                    switch (req.request_type.ToUpper())
                    {
                        case "INSERT":
                            result = await _ddcService.InsertDdcToolAsync(newDataToken, adminId);
                            break;

                        case "UPDATE":
                            result = await _ddcService.UpdateDdcToolAsync(req.target_pk_id.Value, newDataToken, adminId);
                            break;

                        case "DELETE":
                            result = await _ddcService.DeleteDdcToolAsync(req.target_pk_id.Value, adminId);
                            break;

                        default:
                            result = false;
                            break;
                    }
                }
                else if (req.target_table == "PadBrassMap")
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => (await _padBrassService.InsertPadBrassAsync(newDataToken, adminId)) != null,
                        "UPDATE" => await _padBrassService.UpdatePadBrassAsync(req.target_pk_id.Value, newDataToken, adminId),
                        "DELETE" => await _padBrassService.DeletePadBrassAsync(req.target_pk_id.Value, adminId),
                        _ => false
                    };
                }

                else if (req.target_table == "PadHstMap")
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => (await _padHstService.InsertPadHstAsync(newDataToken, adminId)) != null,
                        "UPDATE" => await _padHstService.UpdatePadHstAsync(req.target_pk_id.Value, newDataToken, adminId),
                        "DELETE" => await _padHstService.DeletePadHstAsync(req.target_pk_id.Value, adminId),
                        _ => false
                    };
                }

                else if (req.target_table == "ToolPadMap")
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => (await _toolPadService.InsertToolPadAsync(newDataToken, adminId)) != null,
                        "UPDATE" => await _toolPadService.UpdateToolPadAsync(req.target_pk_id.Value, newDataToken, adminId),
                        "DELETE" => await _toolPadService.DeleteToolPadAsync(req.target_pk_id.Value, adminId),
                        _ => false
                    };
                }

                else if (req.target_table == "ToolMachineMap")
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => (await _toolMachineService.InsertToolMachineAsync(newDataToken, adminId)) != null,
                        "UPDATE" => await _toolMachineService.UpdateToolMachineAsync(req.target_pk_id.Value, newDataToken, adminId),
                        "DELETE" => await _toolMachineService.DeleteToolMachineAsync(req.target_pk_id.Value, adminId),
                        _ => false
                    };
                }


                else
                {
                    result = req.request_type.ToUpper() switch
                    {
                        "INSERT" => await _dynamicService.InsertAsync(req.target_table, newDataToken, adminId),
                        "UPDATE" => await _dynamicService.UpdateAsync(req.target_table, req.target_pk_id, newDataToken, adminId),
                        "DELETE" => await _dynamicService.DeleteAsync(req.target_table, req.target_pk_id, adminId),
                        _ => false
                    };
                }

                bool success = result switch
                {
                    bool b => b,
                    Dictionary<string, object?> dict => dict.Count > 0,
                    _ => false
                };

                if (!success)
                {
                    return BadRequest(new
                    {
                        message = $"Failed to execute {req.request_type} for table {req.target_table}. It might be duplicate or invalid data."
                    });
                }

                if (req.target_table == "DdcSpec" && req.request_type.ToUpper() == "UPDATE")
                {
                    // req.old_data = JsonConvert.SerializeObject(result, Formatting.Indented);
                }

                req.request_status = "Completed";
                req.approved_by = adminId;
                req.approved_at = DateTime.Now;

                if (newPkId != null)
                {
                    req.target_pk_id = newPkId;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                // ✅ Refresh Cache หลังอนุมัติ
                // await _context.Database.ExecuteSqlRawAsync("EXEC usp_refresh_resolved_original_cache");

                return Ok(new { message = "Request approved and executed successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error during execution: {ex.Message}");
            }
        }



        // [HttpPost("reject/{id}")]
        // [Authorize(Roles = "admin")]
        // public IActionResult RejectRequest(int id)
        // {
        //     var adminIdClaim = User.FindFirst("user_id");
        //     if (adminIdClaim == null)
        //         return Unauthorized();

        //     var adminId = int.Parse(adminIdClaim.Value);

        //     var req = _context.Requests.FirstOrDefault(r => r.request_id == id);
        //     if (req == null)
        //         return NotFound();

        //     if (req.request_status != "Pending")
        //         return BadRequest("Request already processed.");

        //     req.request_status = "Rejected";
        //     req.approved_by = adminId;
        //     req.approved_at = DateTime.Now;
        //     req.note = "Rejected by admin.";

        //     _context.SaveChanges();
        //     _context.Database.ExecuteSqlRawAsync("EXEC usp_refresh_resolved_original_cache");

        //     return Ok(new { message = "Request rejected successfully." });
        // }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var adminIdClaim = User.FindFirst("user_id");
            if (adminIdClaim == null)
                return Unauthorized();

            var adminId = int.Parse(adminIdClaim.Value);

            var req = await _context.Requests.FirstOrDefaultAsync(r => r.request_id == id);
            if (req == null)
                return NotFound();

            if (req.request_status != "Pending")
                return BadRequest("Request already processed.");

            req.request_status = "Rejected";
            req.approved_by = adminId;
            req.approved_at = DateTime.Now;
            req.note = "Rejected by admin.";

            await _context.SaveChangesAsync();
            // await _context.Database.ExecuteSqlRawAsync("EXEC usp_refresh_resolved_original_cache");

            return Ok(new { message = "Request rejected successfully." });
        }



        [HttpGet("pending")]
        public IActionResult GetPendingRequests([FromQuery] string target_table)
        {
            var pending = _context.Requests
                .Where(r => r.request_status == "PENDING"
                            && r.target_table == target_table)
                .Select(r => new
                {
                    request_id = r.request_id,
                    request_type = r.request_type,
                    target_pk_id = r.target_pk_id
                })
                .ToList();

            return Ok(pending);
        }

    }
}
