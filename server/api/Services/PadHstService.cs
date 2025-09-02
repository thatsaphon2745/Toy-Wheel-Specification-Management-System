using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace api.Services
{
    public class PadHstService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PadHstService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> InsertPadHstAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for PadHstMap.");

            PadHstMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadHstMap>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize PadHstMap: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == dto.pad_id);

            if (pad == null)
                throw new Exception($"pad_id {dto.pad_id} not found.");

            var hst = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == dto.hst_type_id);

            if (hst == null)
                throw new Exception($"hst_type_id {dto.hst_type_id} not found.");

            var exists = await _context.PadHstMaps
                .AnyAsync(x => x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

            if (exists)
                throw new Exception("This pad-hst mapping already exists.");

            var newEntity = new PadHstMap
            {
                pad_id = dto.pad_id,
                hst_type_id = dto.hst_type_id,
                description = string.IsNullOrWhiteSpace(dto.description) ? null : dto.description.Trim(), // ✅ เพิ่ม description
                create_by = userId,
                create_at = DateTime.Now,
                update_by = null,
                update_at = null
            };

            _context.PadHstMaps.Add(newEntity);
            await _context.SaveChangesAsync();

            var newJson = JsonConvert.SerializeObject(new
            {
                newEntity.pad_hst_id,
                newEntity.pad_id,
                pad_name = pad.pad_name,
                newEntity.hst_type_id,
                hst_type = hst.hst_type,
                newEntity.description, // ✅ เพิ่ม description ใน log
                newEntity.create_by,
                newEntity.create_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("INSERT", "PadHstMap", null, newJson, userId, newEntity.pad_hst_id);

            return newEntity.pad_hst_id;
        }

        public async Task<bool> UpdatePadHstAsync(int id, JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for PadHst update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            var entity = await _context.PadHstMaps
                .FirstOrDefaultAsync(x => x.pad_hst_id == id);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

            // ห้ามแก้ pad_id
            if (jObj.ContainsKey("pad_id"))
                jObj.Remove("pad_id");

            if (jObj.ContainsKey("hst_type_id"))
            {
                var newHstTypeId = jObj["hst_type_id"]?.Value<int>();
                if (newHstTypeId != null && newHstTypeId != entity.hst_type_id)
                {
                    var hst = await _context.HstTypes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.hst_type_id == newHstTypeId);
                    if (hst == null)
                        throw new Exception($"hst_type_id {newHstTypeId} not found.");

                    var exists = await _context.PadHstMaps
                        .AnyAsync(x =>
                            x.pad_id == entity.pad_id &&
                            x.hst_type_id == newHstTypeId &&
                            x.pad_hst_id != id
                        );

                    if (exists)
                        throw new Exception("This pad-hst mapping already exists.");

                    entity.hst_type_id = newHstTypeId.Value;
                }
            }

            // ✅ รองรับ description update
            if (jObj.ContainsKey("description"))
            {
                var newDesc = jObj["description"]?.Value<string>()?.Trim();
                entity.description = string.IsNullOrEmpty(newDesc) ? null : newDesc;
            }

            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var newHst = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == entity.hst_type_id);

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var newJson = JsonConvert.SerializeObject(new
            {
                entity.pad_hst_id,
                entity.pad_id,
                pad_name = pad?.pad_name,
                entity.hst_type_id,
                hst_type = newHst?.hst_type,
                entity.description, // ✅ ใส่ใน log ด้วย
                entity.update_by,
                entity.update_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("UPDATE", "PadHstMap", oldJson, newJson, userId, entity.pad_hst_id);

            return true;
        }

        // public async Task<bool> DeletePadHstAsync(int id, int userId)
        // {
        //     var entity = await _context.PadHstMaps
        //         .FirstOrDefaultAsync(x => x.pad_hst_id == id);

        //     if (entity == null)
        //         return false;

        //     var pad = await _context.Pads
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

        //     var hst = await _context.HstTypes
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(h => h.hst_type_id == entity.hst_type_id);

        //     var oldSnapshot = new
        //     {
        //         entity.pad_hst_id,
        //         entity.pad_id,
        //         pad_name = pad?.pad_name,
        //         entity.hst_type_id,
        //         hst_type = hst?.hst_type,
        //         entity.create_by,
        //         entity.create_at,
        //         entity.update_by,
        //         entity.update_at
        //     };

        //     _context.PadHstMaps.Remove(entity);
        //     await _context.SaveChangesAsync();

        //     var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
        //     {
        //         NullValueHandling = NullValueHandling.Ignore
        //     });

        //     await InsertLog("DELETE", "PadHstMap", oldJson, null, userId, entity.pad_hst_id);

        //     return true;
        // }

        public async Task<bool> DeletePadHstAsync(int id, int userId)
        {
            var entity = await _context.PadHstMaps
                .FirstOrDefaultAsync(x => x.pad_hst_id == id);

            if (entity == null)
                return false;

            // ✅ NEW: Check if any tool is still using this pad_id + hst_type_id
            var inUse = await _context.ToolPadMaps
                .AnyAsync(x =>
                    x.pad_id == entity.pad_id &&
                    x.hst_type_id == entity.hst_type_id);

            if (inUse)
            {
                // ไม่ให้ลบเพราะมี tool ใช้อยู่
                throw new Exception("Cannot delete PadHstMap because there are tools using this Pad and HST Type.");
                // return false;
            }

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var hst = await _context.HstTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.hst_type_id == entity.hst_type_id);

            var oldSnapshot = new
            {
                entity.pad_hst_id,
                entity.pad_id,
                pad_name = pad?.pad_name,
                entity.hst_type_id,
                hst_type = hst?.hst_type,
                entity.description, // ✅ เพิ่มตรงนี้เพื่อให้ log ครบ
                entity.create_by,
                entity.create_at,
                entity.update_by,
                entity.update_at
            };

            _context.PadHstMaps.Remove(entity);
            await _context.SaveChangesAsync();

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("DELETE", "PadHstMap", oldJson, null, userId, entity.pad_hst_id);

            return true;
        }


        public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            PadHstMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadHstMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.PadHstMaps
                .AnyAsync(x => x.pad_id == dto.pad_id && x.hst_type_id == dto.hst_type_id);

            return !exists;
        }

        public async Task<bool> CheckUpdateValidAsync(int id, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            PadHstMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadHstMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.PadHstMaps
                .AnyAsync(x =>
                    x.pad_id == dto.pad_id &&
                    x.hst_type_id == dto.hst_type_id &&
                    x.pad_hst_id != id
                );

            return !exists;
        }

        public async Task<bool> CheckDeleteValidAsync(int padHstId)
        {
            var entity = await _context.PadHstMaps
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.pad_hst_id == padHstId);

            if (entity == null)
                return false;

            var inUse = await _context.ToolPadMaps
                .AnyAsync(x =>
                    x.pad_id == entity.pad_id &&
                    x.hst_type_id == entity.hst_type_id);

            return !inUse;
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

        public async Task<bool> CanDeletePadHstAsync(int padHstId)
        {
            var entity = await _context.PadHstMaps
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.pad_hst_id == padHstId);

            if (entity == null)
                return false;

            var inUse = await _context.ToolPadMaps
                .AnyAsync(x =>
                    x.pad_id == entity.pad_id &&
                    x.hst_type_id == entity.hst_type_id);

            return !inUse;
        }

    }
}
