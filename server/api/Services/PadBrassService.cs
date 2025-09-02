using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace api.Services
{
    public class PadBrassService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PadBrassService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> InsertPadBrassAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for PadBrassMap.");

            PadBrassMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadBrassMap>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize PadBrassMap: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == dto.pad_id);

            if (pad == null)
                throw new Exception($"pad_id {dto.pad_id} not found.");

            var brass = await _context.Brasses
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.brass_id == dto.brass_id);

            if (brass == null)
                throw new Exception($"brass_id {dto.brass_id} not found.");

            var exists = await _context.PadBrassMaps
                .AnyAsync(x => x.pad_id == dto.pad_id && x.brass_id == dto.brass_id);

            if (exists)
                throw new Exception("This pad-brass mapping already exists.");

            var newEntity = new PadBrassMap
            {
                pad_id = dto.pad_id,
                brass_id = dto.brass_id,
                description = dto.description, // ✅ เพิ่มตรงนี้
                create_by = userId,
                create_at = DateTime.Now,
                update_by = null,
                update_at = null
            };

            _context.PadBrassMaps.Add(newEntity);
            await _context.SaveChangesAsync();

            var newJson = JsonConvert.SerializeObject(new
            {
                newEntity.pad_brass_id,
                newEntity.pad_id,
                pad_name = pad.pad_name,
                newEntity.brass_id,
                brass_no = brass.brass_no,
                newEntity.description, // ✅ เพิ่มตรงนี้
                newEntity.create_by,
                newEntity.create_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("INSERT", "PadBrassMap", null, newJson, userId, newEntity.pad_brass_id);

            return newEntity.pad_brass_id;
        }

        public async Task<bool> UpdatePadBrassAsync(int id, JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for PadBrass update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            var entity = await _context.PadBrassMaps
                .FirstOrDefaultAsync(x => x.pad_brass_id == id);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

            // ห้ามแก้ pad_id
            if (jObj.ContainsKey("pad_id"))
                jObj.Remove("pad_id");

            if (jObj.ContainsKey("brass_id"))
            {
                var newBrassId = jObj["brass_id"]?.Value<int>();
                if (newBrassId != null && newBrassId != entity.brass_id)
                {
                    var brass = await _context.Brasses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.brass_id == newBrassId);
                    if (brass == null)
                        throw new Exception($"brass_id {newBrassId} not found.");

                    var exists = await _context.PadBrassMaps
                        .AnyAsync(x =>
                            x.pad_id == entity.pad_id &&
                            x.brass_id == newBrassId &&
                            x.pad_brass_id != id
                        );

                    if (exists)
                        throw new Exception("This pad-brass mapping already exists.");

                    entity.brass_id = newBrassId.Value;
                }
            }

            if (jObj.ContainsKey("description"))
            {
                var newDescription = jObj["description"]?.Value<string>()?.Trim();
                entity.description = string.IsNullOrEmpty(newDescription) ? null : newDescription;
            }


            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var newBrass = await _context.Brasses
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.brass_id == entity.brass_id);

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var newJson = JsonConvert.SerializeObject(new
            {
                entity.pad_brass_id,
                entity.pad_id,
                pad_name = pad?.pad_name,
                entity.brass_id,
                brass_no = newBrass?.brass_no,
                entity.description, // ✅ เพิ่มตรงนี้
                entity.update_by,
                entity.update_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("UPDATE", "PadBrassMap", oldJson, newJson, userId, entity.pad_brass_id);

            return true;
        }

        public async Task<bool> DeletePadBrassAsync(int id, int userId)
        {
            var entity = await _context.PadBrassMaps
                .FirstOrDefaultAsync(x => x.pad_brass_id == id);

            if (entity == null)
                return false;

            var pad = await _context.Pads
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.pad_id == entity.pad_id);

            var brass = await _context.Brasses
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.brass_id == entity.brass_id);

            var oldSnapshot = new
            {
                entity.pad_brass_id,
                entity.pad_id,
                pad_name = pad?.pad_name,
                entity.brass_id,
                brass_no = brass?.brass_no,
                entity.description, // ✅ เพิ่มตรงนี้
                entity.create_by,
                entity.create_at,
                entity.update_by,
                entity.update_at
            };

            _context.PadBrassMaps.Remove(entity);
            await _context.SaveChangesAsync();

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("DELETE", "PadBrassMap", oldJson, null, userId, entity.pad_brass_id);

            return true;
        }

        public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            PadBrassMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadBrassMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.PadBrassMaps
                .AnyAsync(x => x.pad_id == dto.pad_id && x.brass_id == dto.brass_id);

            return !exists;
        }

        public async Task<bool> CheckUpdateValidAsync(int id, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            PadBrassMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PadBrassMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.PadBrassMaps
                .AnyAsync(x =>
                    x.pad_id == dto.pad_id &&
                    x.brass_id == dto.brass_id &&
                    x.pad_brass_id != id
                );

            return !exists;
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

        public async Task<List<object>> GetAllAsync()
        {
            var result = await _context.PadBrassMaps
                .Include(m => m.Pad)
                .Include(m => m.Brass)
                .Select(m => new
                {
                    m.pad_brass_id,
                    m.pad_id,
                    pad_name = m.Pad.pad_name,
                    m.brass_id,
                    brass_no = m.Brass.brass_no,
                    m.description, // ✅ เพิ่มตรงนี้
                    m.create_by,
                    m.create_at,
                    m.update_by,
                    m.update_at
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

    }
}
