using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace api.Services
{
    public class ToolPadService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToolPadService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> InsertToolPadAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for ToolPadMap.");

            ToolPadMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize ToolPadMap: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            // FK checks

            var toolKey = await _context.ToolKeyAlls
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.tool_key_id == dto.tool_key_id);

            if (toolKey == null)
                throw new Exception($"tool_key_id {dto.tool_key_id} not found.");

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == dto.pad_id);

            if (pad == null)
                throw new Exception($"pad_id {dto.pad_id} not found.");

            var hstType = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == dto.hst_type_id);

            if (hstType == null)
                throw new Exception($"hst_type_id {dto.hst_type_id} not found.");

            // Duplicate check
            var exists = await _context.ToolPadMaps
                .AnyAsync(x =>
                    x.tool_key_id == dto.tool_key_id &&
                    x.pad_id == dto.pad_id &&
                    x.hst_type_id == dto.hst_type_id);

            if (exists)
                throw new Exception("This tool-pad mapping already exists.");

            // Check valid pad + hst pair
            var isValidPadHst = await _context.PadHstMaps
                .AnyAsync(x => x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

            if (!isValidPadHst)
                throw new Exception("This pad is not valid for the given HST type.");

            var newEntity = new ToolPadMap
            {
                tool_key_id = dto.tool_key_id,
                pad_id = dto.pad_id,
                hst_type_id = dto.hst_type_id,
                description = dto.description, // ✅ เพิ่มตรงนี้
                create_by = userId,
                create_at = DateTime.Now,
                update_by = null,
                update_at = null
            };

            _context.ToolPadMaps.Add(newEntity);
            await _context.SaveChangesAsync();

            var newJson = JsonConvert.SerializeObject(new
            {
                newEntity.map_id,
                newEntity.tool_key_id,
                tool_type = toolKey?.Type?.type_name,
                tool_name = toolKey?.Tool?.tool_name,
                type_ref = toolKey?.TypeRef?.type_name,
                tool_ref = toolKey?.ToolRef?.tool_name,
                size_ref = toolKey?.SizeRef?.size_ref,
                newEntity.pad_id,
                pad_name = pad?.pad_name,
                newEntity.hst_type_id,
                hst_type = hstType?.hst_type,
                description = newEntity.description, // ✅ เพิ่มตรงนี้
                newEntity.create_by,
                newEntity.create_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("INSERT", "ToolPadMap", null, newJson, userId, newEntity.map_id);

            return newEntity.map_id;
        }

        public async Task<bool> UpdateToolPadAsync(int id, JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for ToolPadMap update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            var entity = await _context.ToolPadMaps
                .FirstOrDefaultAsync(x => x.map_id == id);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

            // tool_key_id should not be changed
            if (jObj.ContainsKey("tool_key_id"))
                jObj.Remove("tool_key_id");

            if (jObj.ContainsKey("pad_id"))
            {
                var newPadId = jObj["pad_id"]?.Value<int>();
                if (newPadId != null && newPadId != entity.pad_id)
                {
                    var pad = await _context.Pads
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.pad_id == newPadId);
                    if (pad == null)
                        throw new Exception($"pad_id {newPadId} not found.");

                    entity.pad_id = newPadId.Value;
                }
            }

            if (jObj.ContainsKey("hst_type_id"))
            {
                var newHstTypeId = jObj["hst_type_id"]?.Value<int>();
                if (newHstTypeId != null && newHstTypeId != entity.hst_type_id)
                {
                    var hstType = await _context.HstTypes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.hst_type_id == newHstTypeId);
                    if (hstType == null)
                        throw new Exception($"hst_type_id {newHstTypeId} not found.");

                    entity.hst_type_id = newHstTypeId.Value;
                }
            }

            // check duplicate again
            var exists = await _context.ToolPadMaps
                .AnyAsync(x =>
                    x.tool_key_id == entity.tool_key_id &&
                    x.pad_id == entity.pad_id &&
                    x.hst_type_id == entity.hst_type_id &&
                    x.map_id != id);

            if (exists)
                throw new Exception("This tool-pad mapping already exists.");

            // Check pad-hst pair is valid
            var isValidPadHst = await _context.PadHstMaps
                .AnyAsync(x => x.pad_id == entity.pad_id && x.hst_type_id == entity.hst_type_id);

            if (!isValidPadHst)
                throw new Exception("This pad is not valid for the given HST type.");

            if (jObj.ContainsKey("description"))
            {
                var newDescription = jObj["description"]?.Value<string>()?.Trim();
                entity.description = string.IsNullOrEmpty(newDescription) ? null : newDescription;
            }


            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            var toolKey = await _context.ToolKeyAlls
                .Include(t => t.Type)
                .Include(t => t.Tool)
                .Include(t => t.TypeRef)
                .Include(t => t.ToolRef)
                .Include(t => t.SizeRef)
                .FirstOrDefaultAsync(t => t.tool_key_id == entity.tool_key_id);

            var padNew = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var hstTypeNew = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == entity.hst_type_id);

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var newJson = JsonConvert.SerializeObject(new
            {
                entity.map_id,
                entity.tool_key_id,
                tool_type = toolKey?.Type?.type_name,
                tool_name = toolKey?.Tool?.tool_name,
                type_ref = toolKey?.TypeRef?.type_name,
                tool_ref = toolKey?.ToolRef?.tool_name,
                size_ref = toolKey?.SizeRef?.size_ref,
                entity.pad_id,
                pad_name = padNew?.pad_name,
                entity.hst_type_id,
                hst_type = hstTypeNew?.hst_type,
                description = entity.description, // ✅ เพิ่มตรงนี้
                entity.update_by,
                entity.update_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("UPDATE", "ToolPadMap", oldJson, newJson, userId, entity.map_id);

            return true;
        }

        public async Task<bool> DeleteToolPadAsync(int id, int userId)
        {
            var entity = await _context.ToolPadMaps
                .FirstOrDefaultAsync(x => x.map_id == id);

            if (entity == null)
                return false;

            var toolKey = await _context.ToolKeyAlls
                .Include(t => t.Type)
                .Include(t => t.Tool)
                .Include(t => t.TypeRef)
                .Include(t => t.ToolRef)
                .Include(t => t.SizeRef)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.tool_key_id == entity.tool_key_id);

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var hstType = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == entity.hst_type_id);

            var oldSnapshot = new
            {
                entity.map_id,
                entity.tool_key_id,
                tool_type = toolKey?.Type?.type_name,
                tool_name = toolKey?.Tool?.tool_name,
                type_ref = toolKey?.TypeRef?.type_name,
                tool_ref = toolKey?.ToolRef?.tool_name,
                size_ref = toolKey?.SizeRef?.size_ref,
                entity.pad_id,
                pad_name = pad?.pad_name,
                entity.hst_type_id,
                hst_type = hstType?.hst_type,
                description = entity.description, // ✅ เพิ่มบรรทัดนี้
                entity.create_by,
                entity.create_at,
                entity.update_by,
                entity.update_at
            };

            _context.ToolPadMaps.Remove(entity);
            await _context.SaveChangesAsync();

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("DELETE", "ToolPadMap", oldJson, null, userId, entity.map_id);

            return true;
        }

        public async Task<List<object>> GetAllAsync()
        {
            var result = await _context.ToolPadMaps
                .Include(x => x.ToolKey).ThenInclude(t => t.Type)
                .Include(x => x.ToolKey).ThenInclude(t => t.Tool)
                .Include(x => x.ToolKey).ThenInclude(t => t.TypeRef)
                .Include(x => x.ToolKey).ThenInclude(t => t.ToolRef)
                .Include(x => x.ToolKey).ThenInclude(t => t.SizeRef)
                .Include(x => x.Pad)
                .Include(x => x.HstType)
                .Select(x => new
                {
                    x.map_id,
                    x.tool_key_id,
                    tool_type = x.ToolKey.Type.type_name,
                    tool_name = x.ToolKey.Tool.tool_name,
                    type_ref = x.ToolKey.TypeRef.type_name,
                    tool_ref = x.ToolKey.ToolRef.tool_name,
                    size_ref = x.ToolKey.SizeRef.size_ref,
                    x.pad_id,
                    pad_name = x.Pad.pad_name,
                    x.hst_type_id,
                    hst_type = x.HstType.hst_type,
                    x.description, // ✅ เพิ่มตรงนี้
                    x.create_by,
                    x.create_at,
                    x.update_by,
                    x.update_at
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        private object CloneEntity(object entity)
        {
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

        private async Task InsertLog(
            string action,
            string tableName,
            string? oldJson,
            string? newJson,
            int userId,
            object? targetId = null
        )
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

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return false;

        //     ToolPadMap? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
        //     }
        //     catch
        //     {
        //         return false;
        //     }

        //     if (dto == null)
        //         return false;

        //     // Duplicate check
        //     var exists = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == dto.tool_key_id &&
        //         x.pad_id == dto.pad_id &&
        //         x.hst_type_id == dto.hst_type_id);

        //     return !exists;
        // }

        // public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return false;

        //     ToolPadMap? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
        //     }
        //     catch
        //     {
        //         return false;
        //     }

        //     if (dto == null)
        //         return false;

        //     // ✅ Step 1: ตรวจสอบว่ามีอยู่ใน padHstMap หรือไม่
        //     var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
        //         x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

        //     if (!isValidPadHst)
        //         return false; // ❌ ไม่มี mapping pad-hst นี้ในระบบ

        //     // ✅ Step 2: ตรวจสอบว่าซ้ำหรือไม่
        //     var exists = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == dto.tool_key_id &&
        //         x.pad_id == dto.pad_id &&
        //         x.hst_type_id == dto.hst_type_id);

        //     return !exists;
        // }

        // public async Task<(bool isValid, string message)> CheckInsertValidAsync(JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return (false, "Input is empty or null.");

        //     ToolPadMap? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
        //     }
        //     catch
        //     {
        //         return (false, "Invalid JSON format.");
        //     }

        //     if (dto == null)
        //         return (false, "Deserialization failed.");

        //     // ✅ ตรวจสอบว่ามี mapping pad-hst อยู่จริงไหม
        //     var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
        //         x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

        //     if (!isValidPadHst)
        //         return (false, "Invalid pad name or hst type: not found in PadHstMap.");

        //     // ✅ ตรวจสอบว่าไม่ซ้ำ
        //     var exists = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == dto.tool_key_id &&
        //         x.pad_id == dto.pad_id &&
        //         x.hst_type_id == dto.hst_type_id);

        //     if (exists)
        //         return (false, "Duplicate ToolPadMap: this mapping already exists.");

        //     return (true, "Valid insert.");
        // }

        public async Task<(bool isValid, string message)> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return (false, "Input is empty or null.");

            ToolPadMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
            }
            catch
            {
                return (false, "Invalid JSON format.");
            }

            if (dto == null)
                return (false, "Deserialization failed.");

            // ✅ ตรวจสอบว่ามี mapping pad-hst อยู่จริงไหม
            var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
                x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

            if (!isValidPadHst)
                return (false, "Invalid pad name or hst type: not found in PadHstMap.");

            // ✅ ตรวจสอบว่าไม่ซ้ำ
            var exists = await _context.ToolPadMaps.AnyAsync(x =>
                x.tool_key_id == dto.tool_key_id &&
                x.pad_id == dto.pad_id &&
                x.hst_type_id == dto.hst_type_id);

            if (exists)
                return (false, "Cannot insert because this ToolPadMap mapping already exists or is invalid.");

            return (true, "Valid insert.");
        }




        // public async Task<bool> CheckUpdateValidAsync(int id, JToken? jsonData)
        // {
        //     var jsonStr = jsonData?.ToString(Formatting.None);
        //     if (string.IsNullOrEmpty(jsonStr))
        //         return false;

        //     ToolPadMap? dto;
        //     try
        //     {
        //         dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
        //     }
        //     catch
        //     {
        //         return false;
        //     }

        //     if (dto == null)
        //         return false;

        //     var exists = await _context.ToolPadMaps.AnyAsync(x =>
        //         x.tool_key_id == dto.tool_key_id &&
        //         x.pad_id == dto.pad_id &&
        //         x.hst_type_id == dto.hst_type_id &&
        //         x.map_id != id);

        //     return !exists;
        // }

        public async Task<(bool isValid, string message)> CheckUpdateValidAsync(int id, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return (false, "Input is empty or null.");

            ToolPadMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolPadMap>(jsonStr);
            }
            catch
            {
                return (false, "Invalid JSON format.");
            }

            if (dto == null)
                return (false, "Deserialization failed.");

            // ✅ ตรวจสอบว่ามี mapping pad-hst อยู่จริงไหม
            var isValidPadHst = await _context.PadHstMaps.AnyAsync(x =>
                x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

            if (!isValidPadHst)
                return (false, "Invalid pad name or hst type: not found in PadHstMap.");

            // ✅ ตรวจสอบว่าการ update จะไม่ทำให้เกิด duplicate
            var exists = await _context.ToolPadMaps.AnyAsync(x =>
                x.tool_key_id == dto.tool_key_id &&
                x.pad_id == dto.pad_id &&
                x.hst_type_id == dto.hst_type_id &&
                x.map_id != id);

            if (exists)
                return (false, "Cannot update because the new mapping would duplicate an existing ToolPadMap.");

            return (true, "Valid update.");
        }

        

    }
}
