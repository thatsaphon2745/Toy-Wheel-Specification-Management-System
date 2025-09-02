using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using api.DTOs;
using System.Reflection;

namespace api.Services
{
    public class OriginalSpecService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OriginalSpecService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> InsertOriginalSpecAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for OriginalSpec.");

            OriginalSpecDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<OriginalSpecDto>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize OriginalSpecDto: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            // ✅ NEW: Check duplicate ToolRefSpec first
            var duplicateSpec = await (
                from spec in _context.ToolRefSpecs
                join orig in _context.ToolKeyOriginals
                    on spec.tool_key_id equals orig.tool_key_id
                where
                    orig.tool_id == dto.tool_id &&
                    orig.type_id == dto.type_id &&
                    orig.size_ref_id == dto.size_ref_id &&
                    spec.axle_type_id == dto.axle_type_id
                select spec
            ).FirstOrDefaultAsync();

            if (duplicateSpec != null)
            {
                throw new Exception("Duplicate ToolRefSpec already exists for this Tool, Type, Size, and Axle.");
            }

            // ---------- STEP 1: ToolKeyOriginals ----------
            var existingOriginal = await _context.ToolKeyOriginals
                .FirstOrDefaultAsync(x =>
                    x.tool_id == dto.tool_id &&
                    x.type_id == dto.type_id &&
                    x.size_ref_id == dto.size_ref_id &&
                    x.knurling_type == dto.knurling_type
                );

            int toolKeyId;
            if (existingOriginal != null)
            {
                toolKeyId = existingOriginal.tool_key_id;
            }
            else
            {
                var newOriginal = new ToolKeyOriginal
                {
                    tool_id = dto.tool_id,
                    type_id = dto.type_id,
                    size_ref_id = dto.size_ref_id,
                    knurling_type = dto.knurling_type,
                    create_by = userId,
                    create_at = DateTime.Now
                };

                _context.ToolKeyOriginals.Add(newOriginal);
                await _context.SaveChangesAsync();

                toolKeyId = newOriginal.tool_key_id;

                // await InsertLog("INSERT", "ToolKeyOriginals", null, newOriginal, userId);

                var newOriginalJson = JsonConvert.SerializeObject(
                    newOriginal,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );
                await InsertLog("INSERT", "ToolKeyOriginals", null, newOriginalJson, userId);
            }

            // ---------- STEP 2: ToolRefSpecs ----------
            var newSpec = new ToolRefSpec
            {
                tool_key_id = toolKeyId,
                axle_type_id = dto.axle_type_id,
                overall_a = dto.overall_a,
                overall_b = dto.overall_b,
                overall_c = dto.overall_c,
                tolerance_a = dto.tolerance_a,
                tolerance_b = dto.tolerance_b,
                tolerance_c = dto.tolerance_c,
                f_shank_min = dto.f_shank_min,
                f_shank_max = dto.f_shank_max,
                chassis_span = dto.chassis_span,
                chassis_span1 = dto.chassis_span1,
                chassis_span2 = dto.chassis_span2,
                b2b_min = dto.b2b_min,
                b2b_max = dto.b2b_max,
                h2h_min = dto.h2h_min,
                h2h_max = dto.h2h_max,
                source = dto.source ?? "",
                description = dto.description ?? "",
                is_original_spec = dto.is_original_spec,
                knurling_type = dto.knurling_type,
                create_by = userId,
                create_at = DateTime.Now
            };

            _context.ToolRefSpecs.Add(newSpec);
            await _context.SaveChangesAsync();

            // await InsertLog("INSERT", "OriginalSpec", null, newSpec, userId);

            var newSpecJson = JsonConvert.SerializeObject(newSpec, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("INSERT", "OriginalSpec", null, newSpecJson, userId, newSpec.tool_ref_spec_id);

            // ---------- STEP 3: หา type_ref_id / tool_ref_id ----------

            var nullTypeId = await _context.TypeModels
                .Where(x => x.type_name == null)
                .Select(x => x.type_id)
                .FirstOrDefaultAsync();

            var nullToolId = await _context.Tools
                .Where(x => x.tool_name == null)
                .Select(x => x.tool_id)
                .FirstOrDefaultAsync();

            var existingAll = await _context.ToolKeyAlls.FirstOrDefaultAsync(x =>
                x.type_id == dto.type_id &&
                x.tool_id == dto.tool_id &&
                x.size_ref_id == dto.size_ref_id &&
                x.source_original_key_id == toolKeyId
            );

            if (existingAll == null)
            {
                var newAll = new ToolKeyAll
                {
                    type_id = dto.type_id,
                    tool_id = dto.tool_id,
                    type_ref_id = nullTypeId == 0 ? null : nullTypeId,
                    tool_ref_id = nullToolId == 0 ? null : nullToolId,
                    size_ref_id = dto.size_ref_id,
                    source_original_key_id = toolKeyId,
                    original_spec = dto.is_original_spec,
                    ref_spec = 0,
                    knurling = dto.knurling_type,
                    // create_by = userId,
                    // create_at = DateTime.Now
                };

                _context.ToolKeyAlls.Add(newAll);
                await _context.SaveChangesAsync();

                var newAllJson = JsonConvert.SerializeObject(
                    newAll,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }
                );
                await InsertLog("INSERT", "ToolKeyAlls", null, newAllJson, userId, newAll.tool_key_id);
            }



            return newSpec.tool_ref_spec_id;
        }



        public async Task<bool> UpdateOriginalSpecAsync(int pkId, JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for OriginalSpec update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            var entity = await _context.ToolRefSpecs
                .FirstOrDefaultAsync(x => x.tool_ref_spec_id == pkId);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

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

            // numeric fields (double)
            SetIfExists<double>("overall_a", v => entity.overall_a = v);
            SetIfExists<double>("overall_b", v => entity.overall_b = v);
            SetIfExists<double>("overall_c", v => entity.overall_c = v);
            SetIfExists<double>("tolerance_a", v => entity.tolerance_a = v);
            SetIfExists<double>("tolerance_b", v => entity.tolerance_b = v);
            SetIfExists<double>("tolerance_c", v => entity.tolerance_c = v);
            SetIfExists<double>("f_shank_min", v => entity.f_shank_min = v);
            SetIfExists<double>("f_shank_max", v => entity.f_shank_max = v);
            SetIfExists<double>("chassis_span1", v => entity.chassis_span1 = v);
            SetIfExists<double>("chassis_span2", v => entity.chassis_span2 = v);
            SetIfExists<double>("b2b_min", v => entity.b2b_min = v);
            SetIfExists<double>("b2b_max", v => entity.b2b_max = v);
            SetIfExists<double>("h2h_min", v => entity.h2h_min = v);
            SetIfExists<double>("h2h_max", v => entity.h2h_max = v);

            // string fields
            SetIfExistsStr("chassis_span", v => entity.chassis_span = v);
            SetIfExistsStr("source", v => entity.source = v);
            SetIfExistsStr("description", v => entity.description = v);

            // เพิ่ม fields อื่น ๆ ตาม schema ได้ เช่น
            // SetIfExists<int>("knurling_type", v => entity.knurling_type = v);
            // SetIfExistsStr("tool_type", v => entity.tool_type = v);

            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            // ---------- old_data → serialize ทั้งก้อน ----------
            var oldJson = JsonConvert.SerializeObject(
                oldSnapshot,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            // ---------- new_data → only changed fields ----------
            var changedData = new Dictionary<string, object?>();

            var entityType = typeof(ToolRefSpec);
            foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propName = prop.Name;

                if (!jObj.ContainsKey(propName))
                    continue;

                var newValue = jObj[propName]?.Type == JTokenType.Null
                    ? null
                    : jObj[propName]?.ToObject(prop.PropertyType);

                var oldDict = oldSnapshot as Dictionary<string, object?>;
                var oldValue = oldDict != null && oldDict.ContainsKey(propName)
                    ? oldDict[propName]
                    : null;

                var isSame =
                    (oldValue == null && newValue == null)
                    || (oldValue?.ToString() == newValue?.ToString());

                if (!isSame)
                {
                    changedData[propName] = newValue;
                }
            }

            var newJson = changedData.Any()
                ? JsonConvert.SerializeObject(changedData, Formatting.Indented)
                : null;

            await InsertLog("UPDATE", "OriginalSpec", oldJson, newJson, userId, pkId);

            return true;
        }


        public async Task<bool> DeleteOriginalSpecAsync(int pkId, int userId)
        {
            var entity = await _context.ToolRefSpecs
                .FirstOrDefaultAsync(x => x.tool_ref_spec_id == pkId);

            if (entity == null)
                return false;

            // ✅ check usage
            var hasUsage = await _context.ToolSpecs
                .AnyAsync(x => x.tool_ref_spec_id == pkId);

            if (hasUsage)
            {
                throw new Exception("Cannot delete OriginalSpec because it is still referenced in ToolSpecs.");
            }

            var oldSnapshot = CloneEntity(entity);

            _context.ToolRefSpecs.Remove(entity);
            await _context.SaveChangesAsync();

            // ✅ serialize oldSnapshot → JSON
            var oldJson = JsonConvert.SerializeObject(
                oldSnapshot,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            await InsertLog("DELETE", "OriginalSpec", oldJson, null, userId, pkId);


            var toolKeyId = entity.tool_key_id;

            var otherSpecs = await _context.ToolRefSpecs
                .Where(x => x.tool_key_id == toolKeyId && x.tool_ref_spec_id != pkId)
                .ToListAsync();

            if (!otherSpecs.Any())
            {
                var toolKeyAlls = await _context.ToolKeyAlls
                    .Where(x => x.source_original_key_id == toolKeyId)
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
                        }
                    );

                    await InsertLog("DELETE", "ToolKeyAlls", oldAllJson, null, userId);
                }

                var toolKeyOriginal = await _context.ToolKeyOriginals
                    .FirstOrDefaultAsync(x => x.tool_key_id == toolKeyId);

                if (toolKeyOriginal != null)
                {
                    var oldOriginal = CloneEntity(toolKeyOriginal);

                    _context.ToolKeyOriginals.Remove(toolKeyOriginal);
                    await _context.SaveChangesAsync();

                    var oldOriginalJson = JsonConvert.SerializeObject(
                        oldOriginal,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }
                    );

                    await InsertLog("DELETE", "ToolKeyOriginals", oldOriginalJson, null, userId);
                }
            }

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



        private Dictionary<string, object?> FilterPrimitiveProps(object entity)
        {
            return entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsPrimitive ||
                    p.PropertyType == typeof(string) ||
                    p.PropertyType == typeof(decimal) ||
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(DateTime?) ||
                    Nullable.GetUnderlyingType(p.PropertyType)?.IsPrimitive == true ||
                    Nullable.GetUnderlyingType(p.PropertyType) == typeof(decimal)
                )
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity, null)
                );
        }

        private object CloneEntity(object entity)
        {
            if (entity == null) return null;
            var dict = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsPrimitive ||
                    p.PropertyType == typeof(string) ||
                    p.PropertyType == typeof(decimal) ||
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(DateTime?) ||
                    Nullable.GetUnderlyingType(p.PropertyType)?.IsPrimitive == true ||
                    Nullable.GetUnderlyingType(p.PropertyType) == typeof(decimal)
                )
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity, null)
                );

            return dict;
        }

        private object? GetPrimaryKeyValue(object entity)
        {
            var prop = entity.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase));
            return prop?.GetValue(entity);
        }

        public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            OriginalSpecDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<OriginalSpecDto>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var duplicateSpec = await (
                from spec in _context.ToolRefSpecs
                join orig in _context.ToolKeyOriginals
                    on spec.tool_key_id equals orig.tool_key_id
                where
                    orig.tool_id == dto.tool_id &&
                    orig.type_id == dto.type_id &&
                    orig.size_ref_id == dto.size_ref_id &&
                    spec.axle_type_id == dto.axle_type_id
                select spec
            ).FirstOrDefaultAsync();

            return duplicateSpec == null;
        }
        public async Task<bool> CheckUpdateValidAsync(int pkId, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            OriginalSpecDto? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<OriginalSpecDto>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var duplicateSpec = await (
                from spec in _context.ToolRefSpecs
                join orig in _context.ToolKeyOriginals
                    on spec.tool_key_id equals orig.tool_key_id
                where
                    orig.tool_id == dto.tool_id &&
                    orig.type_id == dto.type_id &&
                    orig.size_ref_id == dto.size_ref_id &&
                    spec.axle_type_id == dto.axle_type_id &&
                    spec.tool_ref_spec_id != pkId
                select spec
            ).FirstOrDefaultAsync();

            return duplicateSpec == null;
        }

        public async Task<bool> CheckDeleteValidAsync(int pkId)
        {
            var hasUsage = await _context.ToolSpecs
                .AnyAsync(x => x.tool_ref_spec_id == pkId);

            return !hasUsage;
        }

        // public async Task<bool> CheckDeleteValidAsync(int toolRefSpecId)
        // {
        //     var toolRefSpec = await _context.ToolRefSpecs
        //         .FirstOrDefaultAsync(x => x.tool_ref_spec_id == toolRefSpecId);

        //     if (toolRefSpec == null)
        //         return false;

        //     var originalToolKeyId = toolRefSpec.tool_key_id;

        //     var linkedToolKeyAlls = await _context.ToolKeyAlls
        //         .Where(x => x.source_original_key_id == originalToolKeyId)
        //         .Select(x => x.tool_key_id)
        //         .ToListAsync();

        //     if (originalToolKeyId.HasValue)
        //     {
        //         linkedToolKeyAlls.Add(originalToolKeyId.Value);
        //     }

        //     var isUsedInMachineMap = await _context.ToolMachineMaps
        //         .AnyAsync(x => x.tool_key_id.HasValue &&
        //                        linkedToolKeyAlls.Contains(x.tool_key_id.Value));

        //     if (isUsedInMachineMap)
        //     {
        //         throw new Exception("Cannot delete OriginalSpec because it is still referenced in toolMachineMap. Please remove the machine mappings first.");
        //     }

        //     var isUsedInPadMap = await _context.ToolPadMaps
        //         .AnyAsync(x => x.tool_key_id.HasValue &&
        //                        linkedToolKeyAlls.Contains(x.tool_key_id.Value));

        //     if (isUsedInPadMap)
        //     {
        //         throw new Exception("Cannot delete OriginalSpec because it is still referenced in toolPadMap. Please remove the pad mappings first.");
        //     }

        //     var isUsedInSpecs = await _context.ToolSpecs
        //         .AnyAsync(x => x.tool_ref_spec_id == toolRefSpecId);

        //     if (isUsedInSpecs)
        //     {
        //         throw new Exception("Cannot delete OriginalSpec because it is still used in toolSpecs.");
        //     }

        //     return true;
        // }

    }
}
