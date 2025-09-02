using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq.Expressions;   // ✅ เพิ่มบรรทัดนี้

namespace api.Services
{
    public class DynamicService
    {
        private readonly MbkBarbell9Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // ✅ Table names (พหูพจน์) ที่อนุญาต
        private readonly HashSet<string> _allowedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "typeModels",
            "tools",
            "sizeRefs",
            "axleTypes",
            "positionTypes",
            "machines",
            "hstTypes",
            "pads",
            "brasses"
        };

        // ✅ Map table name → Entity name
        private readonly Dictionary<string, string> _entityMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "typeModels", "TypeModel" },
            { "tools", "Tool" },
            { "sizeRefs", "SizeRef" },
            { "axleTypes", "AxleType" },
            { "positionTypes", "PositionType" },
            { "machines", "Machine" },
            { "hstTypes", "HstType" },
            { "pads", "Pad" },
            { "brasses", "Brass" }
        };

        // เพิ่มเข้าไปใน DynamicService
        private readonly Dictionary<string, string[]> _uniqueFields = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "typeModels", new[] { "type_name" } },
            { "tools", new[] { "tool_name" } },
            { "sizeRefs", new[] { "size_ref" } },
            { "axleTypes", new[] { "axle_type" } },
            { "positionTypes", new[] { "position_type" } },
            { "machines", new[] { "machine_no" } },
            { "hstTypes", new[] { "hst_type" } },
            { "pads", new[] { "pad_name" } },
            { "brasses", new[] { "brass_no" } }
        };


        public DynamicService(MbkBarbell9Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<bool> InsertAsync(string tableName, JToken? jsonData, int userId)
        {
            if (!_allowedTables.Contains(tableName))
                return false;

            var json = jsonData?.ToString(Formatting.None);
            var entity = DeserializeEntity(tableName, json);
            if (entity == null) return false;

            // ✅ Check duplicate
            if (_uniqueFields.TryGetValue(tableName, out var uniqueFields))
            {
                var dbSet = GetDbSet(entity.GetType());
                IQueryable<object> query = (dbSet as IQueryable<object>)!;

                foreach (var field in uniqueFields)
                {
                    var propInfo = entity.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null) continue;

                    var value = propInfo.GetValue(entity);

                    if (value == null) continue;

                    var parameter = Expression.Parameter(entity.GetType(), "e");
                    var left = Expression.Property(parameter, propInfo.Name);
                    var right = Expression.Constant(value, propInfo.PropertyType);
                    var equal = Expression.Equal(left, right);
                    var lambda = Expression.Lambda(equal, parameter);

                    var whereMethod = typeof(Queryable).GetMethods()
                        .First(x => x.Name == "Where" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(entity.GetType());

                    query = (IQueryable<object>)whereMethod.Invoke(null, new object[] { query, lambda })!;
                }

                var exists = await query.AnyAsync();
                if (exists)
                {
                    // ✅ Duplicate → return false เงียบ ๆ
                    return false;
                }
            }

            SetAuditFields(entity, "create_by", userId);
            SetAuditFields(entity, "create_at", DateTime.Now);

            var dbSetToAdd = GetDbSet(entity.GetType());
            var addMethod = dbSetToAdd?.GetType().GetMethod("Add");
            addMethod?.Invoke(dbSetToAdd, new[] { entity });

            await _context.SaveChangesAsync();
            await InsertLog("INSERT", tableName, null, entity, userId);

            return true;
        }


        public async Task<bool> UpdateAsync(string tableName, int? pkId, JToken? jsonData, int userId)
        {
            if (!_allowedTables.Contains(tableName))
                return false;

            if (pkId == null || jsonData == null)
                return false;

            var entityType = FindEntityType(tableName);
            if (entityType == null) return false;

            var dbSet = GetDbSet(entityType);
            var findMethod = dbSet?.GetType().GetMethod("Find", new[] { typeof(object[]) });
            var entity = findMethod?.Invoke(dbSet, new object[] { new object[] { pkId } });
            if (entity == null) return false;

            // ✅ clone entity ก่อนแก้ → เก็บไว้เป็น old snapshot
            var oldEntityJson = JsonConvert.SerializeObject(entity, Formatting.None);
            var oldEntity = JsonConvert.DeserializeObject(oldEntityJson, entityType);

            // ✅ apply json changes ลง entity (new values)
            var json = jsonData.ToString(Formatting.None);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (dict == null) return false;

            // ✅ เช็ค duplicate ก่อน apply
            if (_uniqueFields.TryGetValue(tableName, out var uniqueFields))
            {
                foreach (var field in uniqueFields)
                {
                    if (!dict.ContainsKey(field))
                        continue;

                    var newValue = dict[field];
                    if (newValue == null) continue;

                    var propInfo = entityType.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null) continue;

                    var dbSetQuery = (dbSet as IQueryable<object>)!;

                    // สร้าง expression เช่น: e => e.field == newValue && e.PK != pkId
                    var parameter = Expression.Parameter(entityType, "e");
                    var left = Expression.Property(parameter, propInfo.Name);
                    var right = Expression.Constant(Convert.ChangeType(newValue, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType), propInfo.PropertyType);
                    var equal = Expression.Equal(left, right);

                    var pkProp = entityType.GetProperties().FirstOrDefault(x => x.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase));
                    if (pkProp == null) continue;

                    var pkLeft = Expression.Property(parameter, pkProp.Name);
                    var pkRight = Expression.Constant(pkId, pkProp.PropertyType);
                    var notEqual = Expression.NotEqual(pkLeft, pkRight);

                    var and = Expression.AndAlso(equal, notEqual);
                    var lambda = Expression.Lambda(and, parameter);

                    var whereMethod = typeof(Queryable).GetMethods()
                        .First(x => x.Name == "Where" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(entityType);

                    var query = (IQueryable<object>)whereMethod.Invoke(null, new object[] { dbSetQuery, lambda })!;

                    var exists = await query.AnyAsync();
                    if (exists)
                    {
                        // ✅ Duplicate → return false
                        return false;
                    }
                }
            }

            // ✅ ไม่มี duplicate → apply values
            foreach (var kv in dict)
            {
                var prop = entityType.GetProperty(kv.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && kv.Value != null)
                {
                    prop.SetValue(entity, Convert.ChangeType(kv.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                }
            }

            SetAuditFields(entity, "update_by", userId);
            SetAuditFields(entity, "update_at", DateTime.Now);

            await _context.SaveChangesAsync();

            await InsertLog("UPDATE", tableName, oldEntity, entity, userId);

            return true;
        }


        public async Task<bool> DeleteAsync(string tableName, int? pkId, int userId)
        {
            if (!_allowedTables.Contains(tableName))
                throw new InvalidOperationException($"Table '{tableName}' is not allowed for dynamic delete.");

            if (pkId == null)
                return false;

            var entityType = FindEntityType(tableName);
            if (entityType == null) return false;

            var dbSet = GetDbSet(entityType);
            var findMethod = dbSet?.GetType().GetMethod("Find", new[] { typeof(object[]) });
            var entity = findMethod?.Invoke(dbSet, new object[] { new object[] { pkId } });
            if (entity == null) return false;

            // ✅ clone entity ก่อนลบ → เก็บไว้เป็น old snapshot
            var oldEntityJson = JsonConvert.SerializeObject(entity, Formatting.None);
            var oldEntity = JsonConvert.DeserializeObject(oldEntityJson, entityType);

            var removeMethod = dbSet?.GetType().GetMethod("Remove");
            removeMethod?.Invoke(dbSet, new[] { entity });

            await _context.SaveChangesAsync();

            // ✅ save log → oldEntity มีค่า, newEntity = null
            await InsertLog("DELETE", tableName, oldEntity, null, userId);

            return true;
        }


        private object? DeserializeEntity(string tableName, string? jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return null;

            var entityType = FindEntityType(tableName);
            if (entityType == null) return null;

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            if (dict == null) return null;

            var entity = Activator.CreateInstance(entityType);

            foreach (var kv in dict)
            {
                if (kv.Value == null || kv.Value is DBNull)
                    continue;

                var prop = entityType.GetProperty(kv.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    prop.SetValue(entity, Convert.ChangeType(
                        kv.Value,
                        Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType
                    ));
                }
            }

            return entity;
        }


        private void SetAuditFields(object entity, string propName, object value)
        {
            var prop = entity.GetType()
                .GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                prop.SetValue(entity, value);
            }
        }

        private async Task InsertLog(string action, string tableName, object? oldEntity, object? newEntity, int userId)
        {
            var oldDict = oldEntity == null ? null : ExtractScalarProperties(oldEntity);
            var newDict = newEntity == null ? null : ExtractScalarProperties(newEntity);

            var oldJson = oldDict == null
                ? null
                : JsonConvert.SerializeObject(oldDict, Formatting.Indented);

            var newJson = newDict == null
                ? null
                : JsonConvert.SerializeObject(newDict, Formatting.Indented);

            var targetId = newEntity != null
                ? GetPrimaryKeyValue(newEntity)?.ToString()
                : GetPrimaryKeyValue(oldEntity)?.ToString();

            var log = new Log
            {
                user_id = userId,
                username_snapshot = _httpContextAccessor.HttpContext?.User.Identity?.Name,
                action = action,
                target_table = tableName.ToLower(),
                target_id = targetId,
                old_data = oldJson,
                new_data = newJson,
                created_at = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        private object? GetPrimaryKeyValue(object entity)
        {
            var prop = entity.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase));
            return prop?.GetValue(entity);
        }

        private Type? FindEntityType(string tableName)
        {
            if (!_entityMap.ContainsKey(tableName))
                return null;

            var entityName = _entityMap[tableName];
            var fullName = $"api.Models.{entityName}";
            return Type.GetType(fullName, false, true);
        }

        private object? GetDbSet(Type entityType)
        {
            var prop = _context.GetType()
                .GetProperties()
                .FirstOrDefault(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                    p.PropertyType.GetGenericArguments()[0] == entityType
                );

            return prop?.GetValue(_context);
        }

        private Dictionary<string, object?> ExtractScalarProperties(object entity)
        {
            return entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsPrimitive ||
                    p.PropertyType == typeof(string) ||
                    p.PropertyType == typeof(decimal) ||
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(DateTime?) ||
                    p.PropertyType == typeof(Guid) ||
                    Nullable.GetUnderlyingType(p.PropertyType)?.IsPrimitive == true ||
                    Nullable.GetUnderlyingType(p.PropertyType) == typeof(decimal) ||
                    Nullable.GetUnderlyingType(p.PropertyType) == typeof(Guid)
                )
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity, null)
                );
        }

        public async Task<bool> CheckInsertValidAsync(string tableName, JToken? jsonData)
        {
            if (!_allowedTables.Contains(tableName)) return false;

            var json = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
            var entity = DeserializeEntity(tableName, json);
            if (entity == null) return false;

            if (_uniqueFields.TryGetValue(tableName, out var uniqueFields))
            {
                var dbSet = GetDbSet(entity.GetType());
                IQueryable<object> query = (dbSet as IQueryable<object>)!;

                foreach (var field in uniqueFields)
                {
                    var propInfo = entity.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null) continue;

                    var value = propInfo.GetValue(entity);
                    if (value == null) continue;

                    var parameter = Expression.Parameter(entity.GetType(), "e");
                    var left = Expression.Property(parameter, propInfo.Name);
                    var right = Expression.Constant(value, propInfo.PropertyType);
                    var equal = Expression.Equal(left, right);
                    var lambda = Expression.Lambda(equal, parameter);

                    var whereMethod = typeof(Queryable).GetMethods()
                        .First(x => x.Name == "Where" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(entity.GetType());

                    query = (IQueryable<object>)whereMethod.Invoke(null, new object[] { query, lambda })!;
                }

                var exists = await query.AnyAsync();
                if (exists) return false;
            }

            return true;
        }


        public async Task<bool> CheckUpdateValidAsync(string tableName, int? pkId, JToken? jsonData)
        {
            if (!_allowedTables.Contains(tableName)) return false;

            if (pkId == null || jsonData == null) return false;

            var json = jsonData?.ToString(Formatting.None);
            var entity = DeserializeEntity(tableName, json);
            if (entity == null) return false;

            if (_uniqueFields.TryGetValue(tableName, out var uniqueFields))
            {
                var dbSet = GetDbSet(entity.GetType());
                IQueryable<object> query = (dbSet as IQueryable<object>)!;

                foreach (var field in uniqueFields)
                {
                    var propInfo = entity.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null) continue;

                    var value = propInfo.GetValue(entity);
                    if (value == null) continue;

                    var parameter = Expression.Parameter(entity.GetType(), "e");
                    var left = Expression.Property(parameter, propInfo.Name);
                    var right = Expression.Constant(value, propInfo.PropertyType);
                    var equal = Expression.Equal(left, right);

                    var pkProp = entity.GetType().GetProperties().FirstOrDefault(x => x.Name.EndsWith("_id", StringComparison.OrdinalIgnoreCase));
                    if (pkProp == null) continue;

                    var pkLeft = Expression.Property(parameter, pkProp.Name);
                    var pkRight = Expression.Constant(pkId, pkProp.PropertyType);
                    var notEqual = Expression.NotEqual(pkLeft, pkRight);

                    var and = Expression.AndAlso(equal, notEqual);
                    var lambda = Expression.Lambda(and, parameter);

                    var whereMethod = typeof(Queryable).GetMethods()
                        .First(x => x.Name == "Where" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(entity.GetType());

                    query = (IQueryable<object>)whereMethod.Invoke(null, new object[] { dbSet, lambda })!;
                }

                var exists = await query.AnyAsync();
                if (exists) return false;
            }

            return true;
        }

        public async Task<bool> CheckDeleteValidAsync(string tableName, int? pkId)
        {
            if (!_allowedTables.Contains(tableName)) return false;
            if (pkId == null) return false;

            var fkUsageCount = 0;

            if (tableName == "typeModels")
            {
                fkUsageCount += await _context.ToolKeyOriginals.CountAsync(x => x.type_id == pkId);
                fkUsageCount += await _context.ToolKeyReferences.CountAsync(x => x.type_id == pkId);
                fkUsageCount += await _context.ToolKeyAlls.CountAsync(x => x.type_id == pkId || x.type_ref_id == pkId);
            }
            else if (tableName == "tools")
            {
                fkUsageCount += await _context.ToolKeyOriginals.CountAsync(x => x.tool_id == pkId);
                fkUsageCount += await _context.ToolKeyReferences.CountAsync(x => x.tool_id == pkId);
                fkUsageCount += await _context.ToolKeyAlls.CountAsync(x => x.tool_id == pkId || x.tool_ref_id == pkId);
            }
            else if (tableName == "sizeRefs")
            {
                fkUsageCount += await _context.ToolKeyOriginals.CountAsync(x => x.size_ref_id == pkId);
                fkUsageCount += await _context.ToolKeyAlls.CountAsync(x => x.size_ref_id == pkId);
            }
            else if (tableName == "axleTypes")
            {
                fkUsageCount += await _context.ToolRefSpecs.CountAsync(x => x.axle_type_id == pkId);
                fkUsageCount += await _context.ToolSpecs.CountAsync(x => x.axle_type_id == pkId);
            }
            else if (tableName == "positionTypes")
            {
                fkUsageCount += await _context.ToolKeyReferences.CountAsync(x => x.position_type_id == pkId);
                fkUsageCount += await _context.ToolSpecs.CountAsync(x => x.position_type_id == pkId);
            }
            else if (tableName == "machines")
            {
                fkUsageCount += await _context.ToolMachineMaps.CountAsync(x => x.machine_id == pkId);
            }
            else if (tableName == "hstTypes")
            {
                fkUsageCount += await _context.PadHstMaps.CountAsync(x => x.hst_type_id == pkId);
                fkUsageCount += await _context.ToolPadMaps.CountAsync(x => x.hst_type_id == pkId);
            }
            else if (tableName == "pads")
            {
                fkUsageCount += await _context.PadHstMaps.CountAsync(x => x.pad_id == pkId);
                fkUsageCount += await _context.PadBrassMaps.CountAsync(x => x.pad_id == pkId);
                fkUsageCount += await _context.ToolPadMaps.CountAsync(x => x.pad_id == pkId);
            }
            else if (tableName == "brasses")
            {
                fkUsageCount += await _context.PadBrassMaps.CountAsync(x => x.brass_id == pkId);
            }

            return fkUsageCount == 0;
        }



    }
}
