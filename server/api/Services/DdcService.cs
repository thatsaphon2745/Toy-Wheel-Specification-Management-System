using api.Models;
using api.DTOs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;


namespace api.Services
{
    public class DdcService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DdcService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return false;

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch
        //     {
        //         return false;
        //     }

        //     if (dto == null)
        //         return false;

        //     var exists = await _context.ToolKeyAlls
        //         .AsNoTracking()
        //         .AnyAsync(x =>
        //             x.type_id == dto.type_id &&
        //             x.tool_id == dto.tool_id &&
        //             x.type_ref_id == dto.type_ref_id &&
        //             x.tool_ref_id == dto.tool_ref_id &&
        //             x.size_ref_id == dto.size_ref_id);
        //             // x.position_type_id = dto.position_type_id;

        //     return !exists;
        // }

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return false;

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch
        //     {
        //         return false;
        //     }

        //     if (dto == null)
        //         return false;

        //     var exists = await (
        //         from all in _context.ToolKeyAlls
        //         join refTool in _context.ToolKeyReferences
        //             on all.source_ref_key_id equals refTool.ref_key_id
        //                 join toolSpec in _context.ToolSpecs
        //                     on refTool.ref_key_id equals toolSpec.ref_key_id

        //         where
        //             all.type_id == dto.type_id &&
        //             all.tool_id == dto.tool_id &&
        //             all.type_ref_id == dto.type_ref_id &&
        //             all.tool_ref_id == dto.tool_ref_id &&
        //             all.size_ref_id == dto.size_ref_id &&
        //             refTool.position_type_id == dto.position_type_id &&
        //             toolSpec.axle_type_id == dto.axle_type_id
        //         select refTool
        //     ).AnyAsync();
        //     Console.WriteLine($"🔍 CheckInsertValidAsync: exists={exists}");

        //     return !exists;
        // }

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //     {
        //         Console.WriteLine("CheckInsertValidAsync: jsonStr is null or empty.");
        //         return false;
        //     }

        //     Console.WriteLine($"CheckInsertValidAsync: Received JSON string: {jsonStr}");

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"CheckInsertValidAsync: Failed to deserialize JSON. Exception: {ex.Message}");
        //         return false;
        //     }

        //     if (dto == null)
        //     {
        //         Console.WriteLine("CheckInsertValidAsync: Deserialized DTO is null.");
        //         return false;
        //     }

        //     Console.WriteLine($"CheckInsertValidAsync: DTO values:" +
        //         $" type_id={dto.type_id}," +
        //         $" tool_id={dto.tool_id}," +
        //         $" type_ref_id={dto.type_ref_id}," +
        //         $" tool_ref_id={dto.tool_ref_id}," +
        //         $" size_ref_id={dto.size_ref_id}," +
        //         $" position_type_id={dto.position_type_id}," +
        //         $" axle_type_id={dto.axle_type_id}," +
        //         $" chassis_span_override={dto.chassis_span_override}");

        //     var exists = await (
        //         from all in _context.ToolKeyAlls
        //         join refTool in _context.ToolKeyReferences
        //             on all.source_ref_key_id equals refTool.ref_key_id
        //         join toolSpec in _context.ToolSpecs
        //             on refTool.ref_key_id equals toolSpec.ref_key_id
        //         where
        //             all.type_id == dto.type_id &&
        //             all.tool_id == dto.tool_id &&
        //             all.type_ref_id == dto.type_ref_id &&
        //             all.tool_ref_id == dto.tool_ref_id &&
        //             all.size_ref_id == dto.size_ref_id &&
        //             toolSpec.position_type_id == dto.position_type_id &&
        //             toolSpec.axle_type_id == dto.axle_type_id
        //         select refTool
        //     ).AnyAsync();

        //     Console.WriteLine($"CheckInsertValidAsync: Query result exists = {exists}");

        //     return !exists;
        // }

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //     {
        //         Console.WriteLine("CheckInsertValidAsync: jsonStr is null or empty.");
        //         return false;
        //     }

        //     Console.WriteLine($"CheckInsertValidAsync: Received JSON string: {jsonStr}");

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"CheckInsertValidAsync: Failed to deserialize JSON. Exception: {ex.Message}");
        //         return false;
        //     }

        //     if (dto == null)
        //     {
        //         Console.WriteLine("CheckInsertValidAsync: Deserialized DTO is null.");
        //         return false;
        //     }

        //     Console.WriteLine($"CheckInsertValidAsync: DTO values:" +
        //         $" type_id={dto.type_id}," +
        //         $" tool_id={dto.tool_id}," +
        //         $" type_ref_id={dto.type_ref_id}," +
        //         $" tool_ref_id={dto.tool_ref_id}," +
        //         $" size_ref_id={dto.size_ref_id}," +
        //         $" position_type_id={dto.position_type_id}," +
        //         $" axle_type_id={dto.axle_type_id}," +
        //         $" chassis_span_override={dto.chassis_span_override}");

        //     // STEP 1: หา tool_key_id จาก ToolKeyOriginals
        //     var toolKeyId = await _context.ToolKeyOriginals
        //         .Where(x =>
        //             x.type_id == dto.type_ref_id &&
        //             x.tool_id == dto.tool_ref_id &&
        //             x.size_ref_id == dto.size_ref_id
        //         )
        //         .Select(x => x.tool_key_id)
        //         .FirstOrDefaultAsync();

        //     if (toolKeyId == 0)
        //     {
        //         Console.WriteLine("CheckInsertValidAsync: No tool_key_id found in ToolKeyOriginals → invalid reference. Cannot insert.");
        //         return false; // ห้าม insert เพราะ user อ้างถึง original spec ที่ไม่มีจริง
        //     }

        //     Console.WriteLine($"CheckInsertValidAsync: Found tool_key_id = {toolKeyId}");

        //     // STEP 2 & 3: หา ref_key_id ใน ToolKeyReferences และ check axle_type_id ใน ToolSpecs
        //     var exists = await (
        //         from tr in _context.ToolKeyReferences
        //         join ts in _context.ToolSpecs
        //             on tr.ref_key_id equals ts.ref_key_id
        //         where
        //             tr.type_id == dto.type_id &&
        //             tr.tool_id == dto.tool_id &&
        //             tr.tool_key_id == toolKeyId &&
        //             tr.position_type_id == dto.position_type_id &&
        //             ts.axle_type_id == dto.axle_type_id
        //         select tr
        //     ).AnyAsync();

        //     Console.WriteLine($"CheckInsertValidAsync: Query result exists = {exists}");

        //     return !exists;
        // }

        // public async Task<CheckResult> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //     {
        //         return new CheckResult
        //         {
        //             IsValid = false,
        //             Error = "Invalid request. JSON is empty."
        //         };
        //     }

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (Exception ex)
        //     {
        //         return new CheckResult
        //         {
        //             IsValid = false,
        //             Error = $"Invalid JSON format: {ex.Message}"
        //         };
        //     }

        //     if (dto == null)
        //     {
        //         return new CheckResult
        //         {
        //             IsValid = false,
        //             Error = "Invalid request. DTO is null."
        //         };
        //     }

        //     var toolKeyId = await _context.ToolKeyOriginals
        //         .Where(x =>
        //             x.type_id == dto.type_ref_id &&
        //             x.tool_id == dto.tool_ref_id &&
        //             x.size_ref_id == dto.size_ref_id
        //         )
        //         .Select(x => x.tool_key_id)
        //         .FirstOrDefaultAsync();

        //     if (toolKeyId == 0)
        //     {
        //         return new CheckResult
        //         {
        //             IsValid = false,
        //             Error = "Original Spec not found. Please check Type Ref, Tool Ref, and Size Ref."
        //         };
        //     }

        //     var exists = await (
        //         from tr in _context.ToolKeyReferences
        //         join ts in _context.ToolSpecs
        //             on tr.ref_key_id equals ts.ref_key_id
        //         where
        //             tr.type_id == dto.type_id &&
        //             tr.tool_id == dto.tool_id &&
        //             tr.tool_key_id == toolKeyId &&
        //             tr.position_type_id == dto.position_type_id &&
        //             ts.axle_type_id == dto.axle_type_id
        //         select tr
        //     ).AnyAsync();

        //     if (exists)
        //     {
        //         return new CheckResult
        //         {
        //             IsValid = false,
        //             Error = "Cannot insert because this DDC Spec already exists."
        //         };
        //     }

        //     return new CheckResult
        //     {
        //         IsValid = true
        //     };
        // }


        public async Task<CheckResult> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = "Invalid request. JSON is empty."
                };
            }

            DdcToolInsertDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
            }
            catch (Exception ex)
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = $"Invalid JSON format: {ex.Message}"
                };
            }

            if (dto == null)
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = "Invalid request. DTO is null."
                };
            }


            var toolKeyId = await _context.ToolKeyOriginals
                .Where(x =>
                    x.type_id == dto.type_ref_id &&
                    x.tool_id == dto.tool_ref_id &&
                    x.size_ref_id == dto.size_ref_id
                )
                .Select(x => x.tool_key_id)
                .FirstOrDefaultAsync();

            if (toolKeyId == 0)
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = "Original Spec not found. Please check Type Ref, Tool Ref, and Size Ref."
                };
            }

            // ✅ ดึงชื่อ axle_type
            var axleTypeName = await _context.AxleTypes
                .Where(x => x.axle_type_id == dto.axle_type_id)
                .Select(x => x.axle_type)
                .FirstOrDefaultAsync();

            // ✅ เช็คว่ามี axle_type_id ใน original spec หรือไม่
            var axleTypeIds = await _context.ToolRefSpecs
                .Where(spec => spec.tool_key_id == toolKeyId)
                .Select(spec => spec.axle_type_id)
                .Distinct()
                .ToListAsync();

            if (!axleTypeIds.Contains(dto.axle_type_id))
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = $"Axle Type {axleTypeName ?? "Unknown"} not found for this original spec."
                };
            }

            var exists = await (
                from tr in _context.ToolKeyReferences
                join ts in _context.ToolSpecs
                    on tr.ref_key_id equals ts.ref_key_id
                where
                    tr.type_id == dto.type_id &&
                    tr.tool_id == dto.tool_id &&
                    tr.tool_key_id == toolKeyId &&
                    tr.position_type_id == dto.position_type_id &&
                    ts.axle_type_id == dto.axle_type_id
                select tr
            ).AnyAsync();

            if (exists)
            {
                return new CheckResult
                {
                    IsValid = false,
                    Error = "Cannot insert because this DDC Spec already exists."
                };
            }

            return new CheckResult
            {
                IsValid = true
            };
        }

        public class CheckResult
        {
            public bool IsValid { get; set; }
            public string? Error { get; set; }
        }





        // public async Task<bool> InsertDdcToolAsync(JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool insert.");

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Failed to deserialize DdcToolInsertDto: " + ex.Message);
        //     }

        //     if (dto == null)
        //         throw new Exception("Invalid JSON payload.");

        //     using var trx = await _context.Database.BeginTransactionAsync();

        //     try
        //     {
        //         // ---------- STEP 1: ToolKeyOriginals ----------
        //         var originalKey = await _context.ToolKeyOriginals
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.type_id == dto.type_ref_id &&
        //                 x.tool_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id);

        //         if (originalKey == null)
        //         {
        //             throw new Exception("Original tool key not found for given Type Ref, Tool Ref, and Size Ref.");
        //         }

        //         // ---------- STEP 2: ToolRefSpecs ----------
        //         var refSpec = await _context.ToolRefSpecs
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.tool_key_id == originalKey.tool_key_id &&
        //                 x.axle_type_id == dto.axle_type_id);

        //         if (refSpec == null)
        //         {
        //             throw new Exception("ToolRefSpec not found for given axle_type.");
        //         }

        //         // ---------- STEP 3: ToolKeyReferences ----------
        //         var newRef = new ToolKeyReference
        //         {
        //             tool_id = dto.tool_id,
        //             type_id = dto.type_id,
        //             position_type_id = dto.position_type_id,
        //             // knurling_type = dto.knurling_type,
        //             tool_key_id = originalKey.tool_key_id,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolKeyReferences.Add(newRef);
        //         await _context.SaveChangesAsync();

        //         var newRefJson = JsonConvert.SerializeObject(newRef, new JsonSerializerSettings
        //         {
        //             NullValueHandling = NullValueHandling.Ignore
        //         });
        //         await InsertLog("INSERT", "ToolKeyReferences", null, newRefJson, userId, newRef.ref_key_id);

        //         // ---------- STEP 4: ToolSpecs ----------
        //         var newSpec = new ToolSpec
        //         {
        //             ref_key_id = newRef.ref_key_id,
        //             tool_ref_spec_id = refSpec.tool_ref_spec_id,
        //             position_type_id = dto.position_type_id,
        //             axle_type_id = dto.axle_type_id,
        //             chassis_span_override = dto.chassis_span_override,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolSpecs.Add(newSpec);
        //         await _context.SaveChangesAsync();

        //         var newSpecJson = JsonConvert.SerializeObject(newSpec, new JsonSerializerSettings
        //         {
        //             NullValueHandling = NullValueHandling.Ignore
        //         });
        //         await InsertLog("INSERT", "ToolSpecs", null, newSpecJson, userId, newSpec.tool_spec_id);

        //         // ---------- STEP 5: ToolKeyAlls ----------
        //         var exists = await _context.ToolKeyAlls
        //             .AsNoTracking()
        //             .AnyAsync(x =>
        //                 x.type_id == dto.type_id &&
        //                 x.tool_id == dto.tool_id &&
        //                 x.type_ref_id == dto.type_ref_id &&
        //                 x.tool_ref_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id
        //             );

        //         if (!exists)
        //         {
        //             var newAll = new ToolKeyAll
        //             {
        //                 type_id = dto.type_id,
        //                 tool_id = dto.tool_id,
        //                 type_ref_id = dto.type_ref_id,
        //                 tool_ref_id = dto.tool_ref_id,
        //                 size_ref_id = dto.size_ref_id,
        //                 source_ref_key_id = newRef.ref_key_id,
        //                 original_spec = 0
        //             };

        //             _context.ToolKeyAlls.Add(newAll);
        //             await _context.SaveChangesAsync();

        //             var newAllJson = JsonConvert.SerializeObject(newAll, new JsonSerializerSettings
        //             {
        //                 NullValueHandling = NullValueHandling.Ignore
        //             });
        //             await InsertLog("INSERT", "ToolKeyAlls", null, newAllJson, userId, newAll.tool_key_id);
        //         }
        //         else
        //         {
        //             Console.WriteLine("🔁 ToolKeyAll already exists → skipping insert.");
        //         }

        //         await trx.CommitAsync();
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         await trx.RollbackAsync();
        //         Console.WriteLine($"🔥 Error during InsertDdcToolAsync: {ex.Message}");
        //         return false;
        //     }
        // }

        // public async Task<bool> InsertDdcToolAsync(JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool insert.");

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Failed to deserialize DdcToolInsertDto: " + ex.Message);
        //     }

        //     if (dto == null)
        //         throw new Exception("Invalid JSON payload.");

        //     try
        //     {
        //         // STEP 1
        //         var originalKey = await _context.ToolKeyOriginals
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.type_id == dto.type_ref_id &&
        //                 x.tool_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id);

        //         if (originalKey == null)
        //             throw new Exception("Original tool key not found for given Type Ref, Tool Ref, and Size Ref.");

        //         // STEP 2
        //         var refSpec = await _context.ToolRefSpecs
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.tool_key_id == originalKey.tool_key_id &&
        //                 x.axle_type_id == dto.axle_type_id);

        //         if (refSpec == null)
        //             throw new Exception("ToolRefSpec not found for given axle_type.");

        //         // STEP 3
        //         var newRef = new ToolKeyReference
        //         {
        //             tool_id = dto.tool_id,
        //             type_id = dto.type_id,
        //             position_type_id = dto.position_type_id,
        //             tool_key_id = originalKey.tool_key_id,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolKeyReferences.Add(newRef);
        //         await _context.SaveChangesAsync();

        //         var newRefJson = JsonConvert.SerializeObject(newRef);
        //         await InsertLog("INSERT", "ToolKeyReferences", null, newRefJson, userId, newRef.ref_key_id);

        //         // STEP 4
        //         var newSpec = new ToolSpec
        //         {
        //             ref_key_id = newRef.ref_key_id,
        //             tool_ref_spec_id = refSpec.tool_ref_spec_id,
        //             position_type_id = dto.position_type_id,
        //             axle_type_id = dto.axle_type_id,
        //             chassis_span_override = dto.chassis_span_override,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolSpecs.Add(newSpec);
        //         await _context.SaveChangesAsync();

        //         var newSpecJson = JsonConvert.SerializeObject(newSpec);
        //         await InsertLog("INSERT", "ToolSpecs", null, newSpecJson, userId, newSpec.tool_spec_id);

        //         // STEP 5
        //         var exists = await _context.ToolKeyAlls
        //             .AsNoTracking()
        //             .AnyAsync(x =>
        //                 x.type_id == dto.type_id &&
        //                 x.tool_id == dto.tool_id &&
        //                 x.type_ref_id == dto.type_ref_id &&
        //                 x.tool_ref_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id
        //             );

        //         if (!exists)
        //         {
        //             var newAll = new ToolKeyAll
        //             {
        //                 type_id = dto.type_id,
        //                 tool_id = dto.tool_id,
        //                 type_ref_id = dto.type_ref_id,
        //                 tool_ref_id = dto.tool_ref_id,
        //                 size_ref_id = dto.size_ref_id,
        //                 source_ref_key_id = newRef.ref_key_id,
        //                 original_spec = 0
        //             };

        //             _context.ToolKeyAlls.Add(newAll);
        //             await _context.SaveChangesAsync();

        //             var newAllJson = JsonConvert.SerializeObject(newAll);
        //             await InsertLog("INSERT", "ToolKeyAlls", null, newAllJson, userId, newAll.tool_key_id);
        //         }
        //         else
        //         {
        //             Console.WriteLine("🔁 ToolKeyAll already exists → skipping insert.");
        //         }

        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"🔥 Error during InsertDdcToolAsync: {ex.Message}");
        //         return false;
        //     }
        // }

        // public async Task<bool> InsertDdcToolAsync(JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool insert.");

        //     DdcToolInsertDto? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Failed to deserialize DdcToolInsertDto: " + ex.Message);
        //     }

        //     if (dto == null)
        //         throw new Exception("Invalid JSON payload.");

        //     try
        //     {
        //         // ---------- STEP 1: ToolKeyOriginals ----------
        //         var originalKey = await _context.ToolKeyOriginals
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.type_id == dto.type_ref_id &&
        //                 x.tool_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id);

        //         if (originalKey == null)
        //         {
        //             Console.WriteLine("❌ STEP 1: Original tool key not found!");
        //             throw new Exception("Original tool key not found for given Type Ref, Tool Ref, and Size Ref.");
        //         }

        //         // ---------- STEP 2: ToolRefSpecs ----------
        //         var refSpec = await _context.ToolRefSpecs
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(x =>
        //                 x.tool_key_id == originalKey.tool_key_id &&
        //                 x.axle_type_id == dto.axle_type_id);

        //         if (refSpec == null)
        //         {
        //             Console.WriteLine("❌ STEP 2: ToolRefSpec not found!");
        //             throw new Exception("ToolRefSpec not found for given axle_type.");
        //         }

        //         // ---------- STEP 3: ToolKeyReferences ----------
        //         var newRef = new ToolKeyReference
        //         {
        //             tool_id = dto.tool_id,
        //             type_id = dto.type_id,
        //             position_type_id = dto.position_type_id,
        //             tool_key_id = originalKey.tool_key_id,
        //             knurling_type = dto.knurling_type,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolKeyReferences.Add(newRef);
        //         await _context.SaveChangesAsync();

        //         var newRefJson = JsonConvert.SerializeObject(
        //             newRef,
        //             new JsonSerializerSettings
        //             {
        //                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //                 NullValueHandling = NullValueHandling.Ignore
        //             });
        //         await InsertLog("INSERT", "ToolKeyReferences", null, newRefJson, userId, newRef.ref_key_id);

        //         // ---------- STEP 4: ToolSpecs ----------
        //         var newSpec = new ToolSpec
        //         {
        //             ref_key_id = newRef.ref_key_id,
        //             tool_ref_spec_id = refSpec.tool_ref_spec_id,
        //             position_type_id = dto.position_type_id,
        //             axle_type_id = dto.axle_type_id,
        //             chassis_span_override = dto.chassis_span_override,
        //             create_by = userId,
        //             create_at = DateTime.Now
        //         };

        //         _context.ToolSpecs.Add(newSpec);
        //         await _context.SaveChangesAsync();

        //         var newSpecJson = JsonConvert.SerializeObject(
        //             newSpec,
        //             new JsonSerializerSettings
        //             {
        //                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //                 NullValueHandling = NullValueHandling.Ignore
        //             });
        //         await InsertLog("INSERT", "ToolSpecs", null, newSpecJson, userId, newSpec.tool_spec_id);

        //         // ---------- STEP 5: ToolKeyAlls ----------
        //         var exists = await _context.ToolKeyAlls
        //             .AsNoTracking()
        //             .AnyAsync(x =>
        //                 x.type_id == dto.type_id &&
        //                 x.tool_id == dto.tool_id &&
        //                 x.type_ref_id == dto.type_ref_id &&
        //                 x.tool_ref_id == dto.tool_ref_id &&
        //                 x.size_ref_id == dto.size_ref_id);

        //         if (!exists)
        //         {
        //             var newAll = new ToolKeyAll
        //             {
        //                 type_id = dto.type_id,
        //                 tool_id = dto.tool_id,
        //                 type_ref_id = dto.type_ref_id,
        //                 tool_ref_id = dto.tool_ref_id,
        //                 size_ref_id = dto.size_ref_id,
        //                 source_ref_key_id = newRef.ref_key_id,
        //                 original_spec = 0,
        //                 ref_spec = 1,
        //                 knurling = dto.knurling_type,
        //             };

        //             _context.ToolKeyAlls.Add(newAll);
        //             await _context.SaveChangesAsync();

        //             var newAllJson = JsonConvert.SerializeObject(
        //                 newAll,
        //                 new JsonSerializerSettings
        //                 {
        //                     ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //                     NullValueHandling = NullValueHandling.Ignore
        //                 });
        //             await InsertLog("INSERT", "ToolKeyAlls", null, newAllJson, userId, newAll.tool_key_id);
        //         }
        //         else
        //         {
        //             Console.WriteLine("🔁 STEP 5: ToolKeyAll already exists → skipping insert.");
        //         }

        //         Console.WriteLine("✅ InsertDdcToolAsync completed successfully.");
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"🔥 Error during InsertDdcToolAsync: {ex.Message}");
        //         return false;
        //     }
        // }

        public async Task<bool> InsertDdcToolAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for DDC Tool insert.");

            DdcToolInsertDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize DdcToolInsertDto: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            double? chassisOverride = null;

            if (!string.IsNullOrWhiteSpace(dto.chassis_span_override?.ToString()))
            {
                chassisOverride = (double)Math.Round(dto.chassis_span_override.Value, 5);
            }

            try
            {
                // ---------- STEP 1: ToolKeyOriginals ----------
                var originalKey = await _context.ToolKeyOriginals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.type_id == dto.type_ref_id &&
                        x.tool_id == dto.tool_ref_id &&
                        x.size_ref_id == dto.size_ref_id);

                if (originalKey == null)
                {
                    Console.WriteLine("❌ STEP 1: Original tool key not found!");
                    throw new Exception("Original tool key not found for given Type Ref, Tool Ref, and Size Ref.");
                }

                Console.WriteLine($"✅ STEP 1: Found Original ToolKeyId = {originalKey.tool_key_id}");

                // ---------- STEP 2: ToolRefSpecs ----------
                var refSpec = await _context.ToolRefSpecs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.tool_key_id == originalKey.tool_key_id &&
                        x.axle_type_id == dto.axle_type_id);

                if (refSpec == null)
                {
                    Console.WriteLine("❌ STEP 2: ToolRefSpec not found!");
                    throw new Exception("ToolRefSpec not found for given axle_type.");
                }

                Console.WriteLine($"✅ STEP 2: Found ToolRefSpecId = {refSpec.tool_ref_spec_id}");

                // ---------- STEP 3: ToolKeyReferences ----------
                ToolKeyReference refRecord;

                var existingRef = await _context.ToolKeyReferences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.tool_id == dto.tool_id &&
                        x.type_id == dto.type_id &&
                        x.position_type_id == dto.position_type_id &&
                        x.knurling_type == dto.knurling_type &&
                        x.tool_key_id == originalKey.tool_key_id
                    );

                if (existingRef != null)
                {
                    Console.WriteLine($"🔁 STEP 3: Found existing ToolKeyReference → ref_key_id = {existingRef.ref_key_id}");
                    refRecord = existingRef;
                }
                else
                {
                    var newRef = new ToolKeyReference
                    {
                        tool_id = dto.tool_id,
                        type_id = dto.type_id,
                        position_type_id = dto.position_type_id,
                        tool_key_id = originalKey.tool_key_id,
                        knurling_type = dto.knurling_type,
                        create_by = userId,
                        create_at = DateTime.Now
                    };

                    _context.ToolKeyReferences.Add(newRef);
                    await _context.SaveChangesAsync();

                    var newRefJson = JsonConvert.SerializeObject(
                        newRef,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    await InsertLog("INSERT", "ToolKeyReferences", null, newRefJson, userId, newRef.ref_key_id);

                    Console.WriteLine($"✅ STEP 3: Inserted new ToolKeyReference → ref_key_id = {newRef.ref_key_id}");

                    refRecord = newRef;
                }

                // ---------- STEP 4: ToolSpecs ----------
                var existingSpec = await _context.ToolSpecs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ref_key_id == refRecord.ref_key_id &&
                        x.position_type_id == dto.position_type_id &&
                        x.axle_type_id == dto.axle_type_id &&
                        (
                            (x.chassis_span_override == null && dto.chassis_span_override == null) ||
                            (
                                x.chassis_span_override != null &&
                                dto.chassis_span_override != null &&
                                (
                                    x.chassis_span_override != null &&
                                    chassisOverride != null &&
                                    Math.Round(x.chassis_span_override.Value, 5) == chassisOverride.Value
                                )
                            )
                        )
                    );

                if (existingSpec != null)
                {
                    Console.WriteLine($"🔁 STEP 4: Found existing ToolSpec → tool_spec_id = {existingSpec.tool_spec_id}");
                }
                else
                {
                    var newSpec = new ToolSpec
                    {
                        ref_key_id = refRecord.ref_key_id,
                        tool_ref_spec_id = refSpec.tool_ref_spec_id,
                        position_type_id = dto.position_type_id,
                        axle_type_id = dto.axle_type_id,
                        chassis_span_override = chassisOverride, // ปัดแล้วจากข้างบน
                        description = dto.description, // ✅ ใหม่
                        create_by = userId,
                        create_at = DateTime.Now
                    };

                    _context.ToolSpecs.Add(newSpec);
                    await _context.SaveChangesAsync();

                    var newSpecJson = JsonConvert.SerializeObject(
                        newSpec,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    await InsertLog("INSERT", "ToolSpecs", null, newSpecJson, userId, newSpec.tool_spec_id);

                    Console.WriteLine($"✅ STEP 4: Inserted new ToolSpec → tool_spec_id = {newSpec.tool_spec_id}");
                }

                // ---------- STEP 5: ToolKeyAlls ----------
                var existsInAlls = await _context.ToolKeyAlls
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.type_id == dto.type_id &&
                        x.tool_id == dto.tool_id &&
                        x.type_ref_id == dto.type_ref_id &&
                        x.tool_ref_id == dto.tool_ref_id &&
                        x.size_ref_id == dto.size_ref_id);

                if (!existsInAlls)
                {
                    var newAll = new ToolKeyAll
                    {
                        type_id = dto.type_id,
                        tool_id = dto.tool_id,
                        type_ref_id = dto.type_ref_id,
                        tool_ref_id = dto.tool_ref_id,
                        size_ref_id = dto.size_ref_id,
                        source_ref_key_id = refRecord.ref_key_id,
                        original_spec = 0,
                        ref_spec = 1,
                        knurling = dto.knurling_type,
                    };

                    _context.ToolKeyAlls.Add(newAll);
                    await _context.SaveChangesAsync();

                    var newAllJson = JsonConvert.SerializeObject(
                        newAll,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    await InsertLog("INSERT", "ToolKeyAlls", null, newAllJson, userId, newAll.tool_key_id);

                    Console.WriteLine($"✅ STEP 5: Inserted new ToolKeyAll → tool_key_id = {newAll.tool_key_id}");
                }
                else
                {
                    Console.WriteLine("🔁 STEP 5: ToolKeyAll already exists → skipping insert.");
                }

                Console.WriteLine("✅ InsertDdcToolAsync completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Error during InsertDdcToolAsync: {ex.Message}");
                return false;
            }
        }



        // public async Task<bool> UpdateDdcToolAsync(int pkId, JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool update.");

        //     JObject jObj;
        //     try
        //     {
        //         jObj = JObject.Parse(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Invalid JSON payload: " + ex.Message);
        //     }

        //     var entity = await _context.ToolSpecs
        //         .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

        //     if (entity == null)
        //         return false;

        //     var oldSnapshot = CloneEntity(entity);

        //     void SetIfExists<T>(string propName, Action<T?> setter) where T : struct
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<T>());
        //         }
        //     }

        //     void SetIfExistsStr(string propName, Action<string?> setter)
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<string>());
        //         }
        //     }

        //     // Update fields
        //     SetIfExists<double>("chassis_span_override", v => entity.chassis_span_override = v);

        //     // เพิ่ม field อื่น ๆ ได้ตาม schema เช่น
        //     // SetIfExists<int>("position_type_id", v => entity.position_type_id = v);

        //     entity.update_by = userId;
        //     entity.update_at = DateTime.Now;

        //     await _context.SaveChangesAsync();

        //     var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
        //     {
        //         NullValueHandling = NullValueHandling.Ignore
        //     });

        //     var changedData = new Dictionary<string, object?>();

        //     foreach (var prop in typeof(ToolSpec).GetProperties())
        //     {
        //         if (!jObj.ContainsKey(prop.Name))
        //             continue;

        //         var newValue = jObj[prop.Name]?.Type == JTokenType.Null
        //             ? null
        //             : jObj[prop.Name]?.ToObject(prop.PropertyType);

        //         var oldDict = oldSnapshot as Dictionary<string, object?>;
        //         var oldValue = oldDict != null && oldDict.ContainsKey(prop.Name)
        //             ? oldDict[prop.Name]
        //             : null;

        //         var isSame =
        //             (oldValue == null && newValue == null)
        //             || (oldValue?.ToString() == newValue?.ToString());

        //         if (!isSame)
        //         {
        //             changedData[prop.Name] = newValue;
        //         }
        //     }

        //     var newJson = changedData.Any()
        //         ? JsonConvert.SerializeObject(changedData, Formatting.Indented)
        //         : null;

        //     await InsertLog("UPDATE", "ToolSpecs", oldJson, newJson, userId, pkId);

        //     Console.WriteLine("✅ UpdateDdcToolAsync success for ID " + pkId);
        //     return true;
        // }

        // public async Task<Dictionary<string, object?>> UpdateDdcToolAsync(int pkId, JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool update.");

        //     JObject jObj;
        //     try
        //     {
        //         jObj = JObject.Parse(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Invalid JSON payload: " + ex.Message);
        //     }

        //     var entity = await _context.ToolSpecs
        //         .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

        //     if (entity == null)
        //         return null;

        //     var oldSnapshot = CloneEntity(entity);

        //     void SetIfExists<T>(string propName, Action<T?> setter) where T : struct
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<T>());
        //         }
        //     }

        //     void SetIfExistsStr(string propName, Action<string?> setter)
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<string>());
        //         }
        //     }

        //     // Update fields
        //     SetIfExists<double>("chassis_span_override", v => entity.chassis_span_override = v);

        //     // เพิ่ม field อื่น ๆ ได้ตาม schema เช่น
        //     // SetIfExists<int>("position_type_id", v => entity.position_type_id = v);

        //     entity.update_by = userId;
        //     entity.update_at = DateTime.Now;

        //     await _context.SaveChangesAsync();

        //     var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
        //     {
        //         NullValueHandling = NullValueHandling.Ignore
        //     });

        //     var changedData = new Dictionary<string, object?>();

        //     foreach (var prop in typeof(ToolSpec).GetProperties())
        //     {
        //         if (!jObj.ContainsKey(prop.Name))
        //             continue;

        //         var newValue = jObj[prop.Name]?.Type == JTokenType.Null
        //             ? null
        //             : jObj[prop.Name]?.ToObject(prop.PropertyType);

        //         var oldDict = oldSnapshot as Dictionary<string, object?>;
        //         var oldValue = oldDict != null && oldDict.ContainsKey(prop.Name)
        //             ? oldDict[prop.Name]
        //             : null;

        //         var isSame =
        //             (oldValue == null && newValue == null)
        //             || (oldValue?.ToString() == newValue?.ToString());

        //         if (!isSame)
        //         {
        //             changedData[prop.Name] = newValue;
        //         }
        //     }

        //     var toolKeyAll = await _context.ToolKeyAlls
        //         .FirstOrDefaultAsync(x => x.source_ref_key_id == entity.ref_key_id);

        //     if (toolKeyAll != null)
        //     {
        //         changedData["type_id"] = toolKeyAll.type_id;
        //         changedData["tool_id"] = toolKeyAll.tool_id;
        //         changedData["type_ref_id"] = toolKeyAll.type_ref_id;
        //         changedData["tool_ref_id"] = toolKeyAll.tool_ref_id;
        //         changedData["size_ref_id"] = toolKeyAll.size_ref_id;
        //     }

        //     var newJson = changedData.Any()
        //         ? JsonConvert.SerializeObject(changedData, Formatting.Indented)
        //         : null;

        //     await InsertLog("UPDATE", "ToolSpecs", oldJson, newJson, userId, pkId);

        //     Console.WriteLine("✅ UpdateDdcToolAsync success for ID " + pkId);
        //     return changedData;
        // }

        // public async Task<Dictionary<string, object?>> UpdateDdcToolAsync(int pkId, JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool update.");

        //     JObject jObj;
        //     try
        //     {
        //         jObj = JObject.Parse(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Invalid JSON payload: " + ex.Message);
        //     }

        //     var entity = await _context.ToolSpecs
        //         .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

        //     if (entity == null)
        //         return null;

        //     var oldSnapshot = CloneEntity(entity);

        //     void SetIfExists<T>(string propName, Action<T?> setter) where T : struct
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<T>());
        //         }
        //     }

        //     void SetIfExistsStr(string propName, Action<string?> setter)
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<string>());
        //         }
        //     }

        //     // Update fields
        //     SetIfExists<double>("chassis_span_override", v => entity.chassis_span_override = v);

        //     // เพิ่ม field อื่น ๆ ได้ตาม schema เช่น
        //     // SetIfExists<int>("position_type_id", v => entity.position_type_id = v);

        //     entity.update_by = userId;
        //     entity.update_at = DateTime.Now;

        //     await _context.SaveChangesAsync();

        //     // แปลง oldSnapshot เป็น Dictionary
        //     var oldDict = oldSnapshot as Dictionary<string, object?>
        //                   ?? JsonConvert.DeserializeObject<Dictionary<string, object?>>(
        //                         JsonConvert.SerializeObject(oldSnapshot)
        //                      )
        //                   ?? new Dictionary<string, object?>();

        //     var changedData = new Dictionary<string, object?>();

        //     foreach (var prop in typeof(ToolSpec).GetProperties())
        //     {
        //         if (!jObj.ContainsKey(prop.Name))
        //             continue;

        //         var newValue = jObj[prop.Name]?.Type == JTokenType.Null
        //             ? null
        //             : jObj[prop.Name]?.ToObject(prop.PropertyType);

        //         var oldValue = oldDict.ContainsKey(prop.Name)
        //             ? oldDict[prop.Name]
        //             : null;

        //         var isSame =
        //             (oldValue == null && newValue == null)
        //             || (oldValue?.ToString() == newValue?.ToString());

        //         if (!isSame)
        //         {
        //             changedData[prop.Name] = newValue;
        //         }
        //     }

        //     var toolKeyAll = await _context.ToolKeyAlls
        //         .FirstOrDefaultAsync(x => x.source_ref_key_id == entity.ref_key_id);

        //     if (toolKeyAll != null)
        //     {
        //         oldDict["type_id"] = toolKeyAll.type_id;
        //         oldDict["tool_id"] = toolKeyAll.tool_id;
        //         oldDict["type_ref_id"] = toolKeyAll.type_ref_id;
        //         oldDict["tool_ref_id"] = toolKeyAll.tool_ref_id;
        //         oldDict["size_ref_id"] = toolKeyAll.size_ref_id;

        //         changedData["type_id"] = toolKeyAll.type_id;
        //         changedData["tool_id"] = toolKeyAll.tool_id;
        //         changedData["type_ref_id"] = toolKeyAll.type_ref_id;
        //         changedData["tool_ref_id"] = toolKeyAll.tool_ref_id;
        //         changedData["size_ref_id"] = toolKeyAll.size_ref_id;
        //     }

        //     // serialize oldDict ใหม่
        //     var oldJson = JsonConvert.SerializeObject(oldDict, Formatting.Indented);

        //     var newJson = changedData.Any()
        //         ? JsonConvert.SerializeObject(changedData, Formatting.Indented)
        //         : null;

        //     await InsertLog("UPDATE", "ToolSpecs", oldJson, newJson, userId, pkId);

        //     Console.WriteLine("✅ UpdateDdcToolAsync success for ID " + pkId);
        //     return changedData;
        // }

        // public async Task<Dictionary<string, object?>> UpdateDdcToolAsync(int pkId, JToken? jsonData, int userId)
        // {
        //     var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         throw new Exception("No data provided for DDC Tool update.");

        //     JObject jObj;
        //     try
        //     {
        //         jObj = JObject.Parse(jsonStr);
        //     }
        //     catch (JsonException ex)
        //     {
        //         throw new Exception("Invalid JSON payload: " + ex.Message);
        //     }

        //     // ✅ Include RefKey → ToolKeyAlls
        //     var entity = await _context.ToolSpecs
        //         .Include(x => x.RefKey)
        //             .ThenInclude(r => r.ToolKeyAlls)
        //         .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

        //     if (entity == null)
        //         return null;

        //     // ✅ สร้าง DTO จาก entity
        //     var dtoOld = new DdcSpecDto
        //     {
        //         tool_spec_id = entity.tool_spec_id,
        //         ref_key_id = entity.ref_key_id,
        //         tool_ref_spec_id = entity.tool_ref_spec_id,
        //         position_type_id = entity.position_type_id,
        //         axle_type_id = entity.axle_type_id,
        //         chassis_span_override = entity.chassis_span_override,
        //         create_by = entity.create_by,
        //         create_at = entity.create_at,
        //         update_by = entity.update_by,
        //         update_at = entity.update_at
        //     };

        //     var toolKeyAll = entity.RefKey?.ToolKeyAlls.FirstOrDefault();
        //     if (toolKeyAll != null)
        //     {
        //         dtoOld.type_id = toolKeyAll.type_id;
        //         dtoOld.tool_id = toolKeyAll.tool_id;
        //         dtoOld.type_ref_id = toolKeyAll.type_ref_id;
        //         dtoOld.tool_ref_id = toolKeyAll.tool_ref_id;
        //         dtoOld.size_ref_id = toolKeyAll.size_ref_id;
        //     }

        //     void SetIfExists<T>(string propName, Action<T?> setter) where T : struct
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<T>());
        //         }
        //     }

        //     void SetIfExistsStr(string propName, Action<string?> setter)
        //     {
        //         if (jObj.ContainsKey(propName))
        //         {
        //             var token = jObj[propName];
        //             setter(token?.Type == JTokenType.Null ? null : token?.Value<string>());
        //         }
        //     }

        //     // ✅ Update fields
        //     SetIfExists<double>("chassis_span_override", v => entity.chassis_span_override = v);
        //     // เพิ่ม field อื่น ๆ ได้ตาม schema เช่น
        //     // SetIfExists<int>("position_type_id", v => entity.position_type_id = v);

        //     entity.update_by = userId;
        //     entity.update_at = DateTime.Now;

        //     await _context.SaveChangesAsync();

        //     // ✅ สร้าง DTO หลัง update (new)
        //     var dtoNew = new DdcSpecDto
        //     {
        //         tool_spec_id = entity.tool_spec_id,
        //         ref_key_id = entity.ref_key_id,
        //         tool_ref_spec_id = entity.tool_ref_spec_id,
        //         position_type_id = entity.position_type_id,
        //         axle_type_id = entity.axle_type_id,
        //         chassis_span_override = entity.chassis_span_override,
        //         create_by = entity.create_by,
        //         create_at = entity.create_at,
        //         update_by = entity.update_by,
        //         update_at = entity.update_at
        //     };

        //     if (toolKeyAll != null)
        //     {
        //         dtoNew.type_id = toolKeyAll.type_id;
        //         dtoNew.tool_id = toolKeyAll.tool_id;
        //         dtoNew.type_ref_id = toolKeyAll.type_ref_id;
        //         dtoNew.tool_ref_id = toolKeyAll.tool_ref_id;
        //         dtoNew.size_ref_id = toolKeyAll.size_ref_id;
        //     }

        //     var oldDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(
        //         JsonConvert.SerializeObject(dtoOld)
        //     ) ?? new Dictionary<string, object?>();

        //     var newDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(
        //         JsonConvert.SerializeObject(dtoNew)
        //     ) ?? new Dictionary<string, object?>();

        //     var changedData = new Dictionary<string, object?>();

        //     foreach (var kvp in newDict)
        //     {
        //         var oldVal = oldDict.ContainsKey(kvp.Key) ? oldDict[kvp.Key] : null;
        //         var newVal = kvp.Value;

        //         var isSame = (oldVal == null && newVal == null)
        //                      || (oldVal?.ToString() == newVal?.ToString());

        //         if (!isSame)
        //         {
        //             changedData[kvp.Key] = newVal;
        //         }
        //     }

        //     var oldJson = JsonConvert.SerializeObject(dtoOld, Formatting.Indented);
        //     var newJson = changedData.Any()
        //         ? JsonConvert.SerializeObject(changedData, Formatting.Indented)
        //         : null;

        //     await InsertLog("UPDATE", "ToolSpecs", oldJson, newJson, userId, pkId);

        //     Console.WriteLine("✅ UpdateDdcToolAsync success for ID " + pkId);
        //     return changedData;
        // }

        public async Task<Dictionary<string, object?>> UpdateDdcToolAsync(
            int pkId,
            JToken? jsonData,
            int userId
        )
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for DDC Tool update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            // ✅ Include RefKey → ToolKeyAlls
            var entity = await _context.ToolSpecs
                .Include(x => x.RefKey)
                    .ThenInclude(r => r.ToolKeyAlls)
                .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

            if (entity == null)
                return null;

            // ✅ DTO ก่อน update
            var dtoOld = new DdcSpecDto
            {
                tool_spec_id = entity.tool_spec_id,
                ref_key_id = entity.ref_key_id,
                tool_ref_spec_id = entity.tool_ref_spec_id,
                position_type_id = entity.position_type_id,
                axle_type_id = entity.axle_type_id,
                chassis_span_override = entity.chassis_span_override,
                description = entity.description, // ✅ ใหม่
                create_by = entity.create_by,
                create_at = entity.create_at,
                update_by = entity.update_by,
                update_at = entity.update_at
            };

            var toolKeyAll = entity.RefKey?.ToolKeyAlls.FirstOrDefault();
            if (toolKeyAll != null)
            {
                dtoOld.type_id = toolKeyAll.type_id;
                dtoOld.tool_id = toolKeyAll.tool_id;
                dtoOld.type_ref_id = toolKeyAll.type_ref_id;
                dtoOld.tool_ref_id = toolKeyAll.tool_ref_id;
                dtoOld.size_ref_id = toolKeyAll.size_ref_id;
            }

            void SetIfExists<T>(string propName, Action<T?> setter) where T : struct
            {
                if (jObj.ContainsKey(propName))
                {
                    var token = jObj[propName];
                    setter(token?.Type == JTokenType.Null ? null : token?.Value<T>());
                }
            }

            void SetIfExistsStr(string propName, Action<string?> setter)
            {
                if (jObj.ContainsKey(propName))
                {
                    var token = jObj[propName];
                    setter(token?.Type == JTokenType.Null ? null : token?.Value<string>());
                }
            }

            // ✅ Update fields
            SetIfExists<double>("chassis_span_override", v => entity.chassis_span_override = v);
            SetIfExistsStr("description", v => entity.description = v); // ✅ FIXED

            // เพิ่ม field อื่น ๆ ได้ตาม schema เช่น:
            // SetIfExists<int>("position_type_id", v => entity.position_type_id = v);

            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ DTO หลัง update
            var dtoNew = new DdcSpecDto
            {
                tool_spec_id = entity.tool_spec_id,
                ref_key_id = entity.ref_key_id,
                tool_ref_spec_id = entity.tool_ref_spec_id,
                position_type_id = entity.position_type_id,
                axle_type_id = entity.axle_type_id,
                chassis_span_override = entity.chassis_span_override,
                description = entity.description, // ✅ ใหม่
                create_by = entity.create_by,
                create_at = entity.create_at,
                update_by = entity.update_by,
                update_at = entity.update_at
            };

            if (toolKeyAll != null)
            {
                dtoNew.type_id = toolKeyAll.type_id;
                dtoNew.tool_id = toolKeyAll.tool_id;
                dtoNew.type_ref_id = toolKeyAll.type_ref_id;
                dtoNew.tool_ref_id = toolKeyAll.tool_ref_id;
                dtoNew.size_ref_id = toolKeyAll.size_ref_id;
            }

            // ✅ Compare old vs new
            var oldDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(
                JsonConvert.SerializeObject(dtoOld)
            ) ?? new Dictionary<string, object?>();

            var newDictFull = JsonConvert.DeserializeObject<Dictionary<string, object?>>(
                JsonConvert.SerializeObject(dtoNew)
            ) ?? new Dictionary<string, object?>();

            var changedData = new Dictionary<string, object?>();

            foreach (var kvp in newDictFull)
            {
                var oldVal = oldDict.ContainsKey(kvp.Key) ? oldDict[kvp.Key] : null;
                var newVal = kvp.Value;

                var isSame = (oldVal == null && newVal == null)
                             || (oldVal?.ToString() == newVal?.ToString());

                if (!isSame)
                {
                    changedData[kvp.Key] = newVal;
                }
            }

            // ✅ ✅ ✅ ส่วนที่เพิ่ม สำหรับ request table
            var refKeyInfoDict = new Dictionary<string, object?>()
            {
                ["type_id"] = toolKeyAll?.type_id,
                ["tool_id"] = toolKeyAll?.tool_id,
                ["type_ref_id"] = toolKeyAll?.type_ref_id,
                ["tool_ref_id"] = toolKeyAll?.tool_ref_id,
                ["size_ref_id"] = toolKeyAll?.size_ref_id
            };

            // รวม refKeyInfo เข้า old_data
            foreach (var kv in refKeyInfoDict)
            {
                oldDict[kv.Key] = kv.Value;
            }

            // รวม refKeyInfo เข้า new_data
            Dictionary<string, object?>? newDict = null;
            if (changedData.Any())
            {
                newDict = new Dictionary<string, object?>(changedData);
                foreach (var kv in refKeyInfoDict)
                {
                    newDict[kv.Key] = kv.Value;
                }
            }

            var oldJson = JsonConvert.SerializeObject(oldDict, Formatting.Indented);
            var newJson = newDict != null
                ? JsonConvert.SerializeObject(newDict, Formatting.Indented)
                : null;

            await InsertLog("UPDATE", "ToolSpecs", oldJson, newJson, userId, pkId);

            Console.WriteLine("✅ UpdateDdcToolAsync success for ID " + pkId);
            return changedData;
        }

        public async Task<bool> DeleteDdcToolAsync(int pkId, int userId)
        {
            var entity = await _context.ToolSpecs
                .FirstOrDefaultAsync(x => x.tool_spec_id == pkId);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

            var refKeyId = entity.ref_key_id;

            // STEP 1 → ลบ ToolSpec
            _context.ToolSpecs.Remove(entity);
            await _context.SaveChangesAsync();

            var oldJson = JsonConvert.SerializeObject(
                oldSnapshot,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            await InsertLog("DELETE", "ToolSpecs", oldJson, null, userId, pkId);

            // STEP 2 → ลบ ToolKeyAlls ก่อน ToolKeyReference
            var toolKeyAlls = await _context.ToolKeyAlls
                .Where(x => x.source_ref_key_id == refKeyId)
                .ToListAsync();

            foreach (var all in toolKeyAlls)
            {
                var oldAll = CloneEntity(all);

                _context.ToolKeyAlls.Remove(all);
                await _context.SaveChangesAsync();

                var oldAllJson = JsonConvert.SerializeObject(
                    oldAll,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                await InsertLog("DELETE", "ToolKeyAlls", oldAllJson, null, userId, all.tool_key_id);
            }

            // STEP 3 → ลบ ToolKeyReference
            var refEntity = await _context.ToolKeyReferences
                .FirstOrDefaultAsync(x => x.ref_key_id == refKeyId);

            if (refEntity != null)
            {
                var oldRef = CloneEntity(refEntity);
                _context.ToolKeyReferences.Remove(refEntity);
                await _context.SaveChangesAsync();

                var oldRefJson = JsonConvert.SerializeObject(
                    oldRef,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                await InsertLog("DELETE", "ToolKeyReferences", oldRefJson, null, userId, refKeyId);
            }

            Console.WriteLine("✅ DeleteDdcToolAsync success for ID " + pkId);
            return true;
        }




        private async Task InsertLog(string action, string tableName, string? oldJson, string? newJson, int userId, object? targetId = null)
        {
            var log = new Log
            {
                user_id = userId,
                username_snapshot = _httpContextAccessor.HttpContext?.User.Identity?.Name,
                action = action,
                target_table = tableName.ToLower(),
                target_id = targetId?.ToString(),
                old_data = oldJson,
                new_data = newJson,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        private object CloneEntity(object entity)
        {
            if (entity == null) return null;
            var dict = entity.GetType()
                .GetProperties()
                .Where(p =>
                    p.PropertyType.IsPrimitive ||
                    p.PropertyType == typeof(string) ||
                    p.PropertyType == typeof(decimal) ||
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(DateTime?) ||
                    Nullable.GetUnderlyingType(p.PropertyType)?.IsPrimitive == true ||
                    Nullable.GetUnderlyingType(p.PropertyType) == typeof(decimal))
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity, null)
                );

            return dict;
        }

        public async Task<bool> CheckUpdateValidAsync(int pkId, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            DdcToolInsertDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DdcToolInsertDto>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var duplicate = await _context.ToolKeyAlls
                .Where(x =>
                    x.type_id == dto.type_id &&
                    x.tool_id == dto.tool_id &&
                    x.type_ref_id == dto.type_ref_id &&
                    x.tool_ref_id == dto.tool_ref_id &&
                    x.size_ref_id == dto.size_ref_id)
                .FirstOrDefaultAsync();

            return duplicate == null;
        }
        public async Task<bool> CheckDeleteValidAsync(int pkId)
        {
            // Check if this ToolSpec is used in other tables
            // (ตอนนี้ไม่มี table ไหน reference tool_spec_id โดยตรง แต่เช็คเผื่ออนาคต)

            var hasUsage = false;

            // ตัวอย่างถ้าในอนาคตมี table ชี้ tool_spec_id เช่น
            // hasUsage = await _context.ToolMachineMaps.AnyAsync(x => x.tool_spec_id == pkId);

            if (hasUsage)
            {
                return false;
            }

            return true;
        }



    }
}
