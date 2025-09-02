using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace api.Services
{
    public class ToolMachineService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToolMachineService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> InsertToolMachineAsync(JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for ToolMachineMap.");

            ToolMachineMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolMachineMap>(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize ToolMachineMap: " + ex.Message);
            }

            if (dto == null)
                throw new Exception("Invalid JSON payload.");

            var toolKey = await _context.ToolKeyAlls
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.tool_key_id == dto.tool_key_id);

            if (toolKey == null)
                throw new Exception($"tool_key_id {dto.tool_key_id} not found.");

            var machine = await _context.Machines
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.machine_id == dto.machine_id);

            if (machine == null)
                throw new Exception($"machine_id {dto.machine_id} not found.");

            var exists = await _context.ToolMachineMaps
                .AnyAsync(x =>
                    x.tool_key_id == dto.tool_key_id &&
                    x.machine_id == dto.machine_id
                );

            if (exists)
                throw new Exception("This tool-machine mapping already exists.");

            var newEntity = new ToolMachineMap
            {
                tool_key_id = dto.tool_key_id,
                machine_id = dto.machine_id,
                description = dto.description?.Trim(), // ✅ เพิ่มตรงนี้
                create_by = userId,
                create_at = DateTime.Now,
                update_by = null,
                update_at = null
            };

            _context.ToolMachineMaps.Add(newEntity);
            await _context.SaveChangesAsync();

            var newJson = JsonConvert.SerializeObject(new
            {
                newEntity.map_id,
                newEntity.tool_key_id,
                newEntity.machine_id,
                machine_no = machine?.machine_no,
                newEntity.description, // ✅ เพิ่มตรงนี้
                newEntity.create_by,
                newEntity.create_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("INSERT", "ToolMachineMap", null, newJson, userId, newEntity.map_id);

            return newEntity.map_id;
        }

        public async Task<bool> UpdateToolMachineAsync(int id, JToken? jsonData, int userId)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                throw new Exception("No data provided for ToolMachineMap update.");

            JObject jObj;
            try
            {
                jObj = JObject.Parse(jsonStr);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON payload: " + ex.Message);
            }

            var entity = await _context.ToolMachineMaps
                .FirstOrDefaultAsync(x => x.map_id == id);

            if (entity == null)
                return false;

            var oldSnapshot = CloneEntity(entity);

            if (jObj.ContainsKey("tool_key_id"))
                jObj.Remove("tool_key_id");

            if (jObj.ContainsKey("machine_id"))
            {
                var newMachineId = jObj["machine_id"]?.Value<int>();
                if (newMachineId != null && newMachineId != entity.machine_id)
                {
                    var machine = await _context.Machines
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.machine_id == newMachineId);

                    if (machine == null)
                        throw new Exception($"machine_id {newMachineId} not found.");

                    var exists = await _context.ToolMachineMaps
                        .AnyAsync(x =>
                            x.tool_key_id == entity.tool_key_id &&
                            x.machine_id == newMachineId &&
                            x.map_id != id
                        );

                    if (exists)
                        throw new Exception("This tool-machine mapping already exists.");

                    entity.machine_id = newMachineId.Value;
                }
            }

            if (jObj.ContainsKey("description"))
            {
                var newDesc = jObj["description"]?.Value<string>()?.Trim();
                if (newDesc != entity.description)
                {
                    entity.description = string.IsNullOrEmpty(newDesc) ? null : newDesc;
                }
            }


            entity.update_by = userId;
            entity.update_at = DateTime.Now;

            await _context.SaveChangesAsync();

            var machineNew = await _context.Machines
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.machine_id == entity.machine_id);

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var newJson = JsonConvert.SerializeObject(new
            {
                entity.map_id,
                entity.tool_key_id,
                entity.machine_id,
                machine_no = machineNew?.machine_no,
                entity.description, // ✅ เพิ่มตรงนี้
                entity.update_by,
                entity.update_at
            }, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("UPDATE", "ToolMachineMap", oldJson, newJson, userId, entity.map_id);

            return true;
        }

        // public async Task<bool> DeleteToolMachineAsync(int id, int userId)
        // {
        //     var entity = await _context.ToolMachineMaps
        //         .FirstOrDefaultAsync(x => x.map_id == id);

        //     if (entity == null)
        //         return false;

        //     var machine = await _context.Machines
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(m => m.machine_id == entity.machine_id);

        //     var oldSnapshot = new
        //     {
        //         entity.map_id,
        //         entity.tool_key_id,
        //         entity.machine_id,
        //         machine_no = machine?.machine_no,
        //         entity.create_by,
        //         entity.create_at,
        //         entity.update_by,
        //         entity.update_at
        //     };

        //     _context.ToolMachineMaps.Remove(entity);
        //     await _context.SaveChangesAsync();

        //     var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
        //     {
        //         NullValueHandling = NullValueHandling.Ignore
        //     });

        //     await InsertLog("DELETE", "ToolMachineMap", oldJson, null, userId, entity.map_id);

        //     return true;
        // }

        public async Task<bool> DeleteToolMachineAsync(int id, int userId)
        {
            var entity = await _context.ToolMachineMaps
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

            var machine = await _context.Machines
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.machine_id == entity.machine_id);

            var oldSnapshot = new
            {
                entity.map_id,
                entity.tool_key_id,
                tool_type = toolKey?.Type?.type_name,
                tool_name = toolKey?.Tool?.tool_name,
                type_ref = toolKey?.TypeRef?.type_name,
                tool_ref = toolKey?.ToolRef?.tool_name,
                size_ref = toolKey?.SizeRef?.size_ref,
                entity.machine_id,
                machine_no = machine?.machine_no,
                entity.description, // ✅ เพิ่มตรงนี้
                entity.create_by,
                entity.create_at,
                entity.update_by,
                entity.update_at
            };

            _context.ToolMachineMaps.Remove(entity);
            await _context.SaveChangesAsync();

            var oldJson = JsonConvert.SerializeObject(oldSnapshot, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await InsertLog("DELETE", "ToolMachineMap", oldJson, null, userId, entity.map_id);

            return true;
        }


        public async Task<bool> CheckInsertValidAsync(JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            ToolMachineMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolMachineMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.ToolMachineMaps
                .AnyAsync(x =>
                    x.tool_key_id == dto.tool_key_id &&
                    x.machine_id == dto.machine_id
                );

            return !exists;
        }

        public async Task<bool> CheckUpdateValidAsync(int id, JToken? jsonData)
        {
            var jsonStr = jsonData?.ToString(Formatting.None);
            if (string.IsNullOrEmpty(jsonStr))
                return false;

            ToolMachineMap? dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ToolMachineMap>(jsonStr);
            }
            catch
            {
                return false;
            }

            if (dto == null)
                return false;

            var exists = await _context.ToolMachineMaps
                .AnyAsync(x =>
                    x.tool_key_id == dto.tool_key_id &&
                    x.machine_id == dto.machine_id &&
                    x.map_id != id
                );

            return !exists;
        }

        public async Task<List<object>> GetAllAsync()
        {
            // ✅ Load entities first
            var list = await _context.ToolMachineMaps
                .Include(x => x.ToolKey).ThenInclude(t => t.Type)
                .Include(x => x.ToolKey).ThenInclude(t => t.Tool)
                .Include(x => x.ToolKey).ThenInclude(t => t.TypeRef)
                .Include(x => x.ToolKey).ThenInclude(t => t.ToolRef)
                .Include(x => x.ToolKey).ThenInclude(t => t.SizeRef)
                .Include(x => x.Machine)
                .ToListAsync();

            // ✅ Safe null propagation in pure C# (LINQ to Objects)
            var result = list.Select(x => new
            {
                x.map_id,
                x.tool_key_id,
                tool_type = x.ToolKey?.Type?.type_name,
                tool_name = x.ToolKey?.Tool?.tool_name,
                type_ref = x.ToolKey?.TypeRef?.type_name,
                tool_ref = x.ToolKey?.ToolRef?.tool_name,
                size_ref = x.ToolKey?.SizeRef?.size_ref,
                x.machine_id,
                machine_no = x.Machine?.machine_no,
                x.create_by,
                x.create_at,
                x.update_by,
                x.update_at
            }).Cast<object>().ToList();

            return result;
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
    }
}
