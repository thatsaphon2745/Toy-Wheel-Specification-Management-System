using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolKeyAllsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolKeyAllsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/ToolKeyAlls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetToolKeyAlls(
            [FromQuery] string? tool_type,
            [FromQuery] string? tool_name,
            [FromQuery] string? type_ref,
            [FromQuery] string? tool_ref,
            [FromQuery] string? size_ref,
            [FromQuery] string? original_spec)
        {
            var query = _context.ToolKeyAlls
                .Include(t => t.Type)
                .Include(t => t.Tool)
                .Include(t => t.TypeRef)
                .Include(t => t.ToolRef)
                .Include(t => t.SizeRef)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tool_type))
            {
                var values = tool_type.Split(',').ToHashSet();
                query = query.Where(t => t.Type != null && values.Contains(t.Type.type_name));
            }

            if (!string.IsNullOrEmpty(tool_name))
            {
                var values = tool_name.Split(',').ToHashSet();
                query = query.Where(t => t.Tool != null && values.Contains(t.Tool.tool_name));
            }

            if (!string.IsNullOrEmpty(type_ref))
            {
                var values = type_ref.Split(',').ToHashSet();
                query = query.Where(t => t.TypeRef != null && values.Contains(t.TypeRef.type_name));
            }

            if (!string.IsNullOrEmpty(tool_ref))
            {
                var values = tool_ref.Split(',').ToHashSet();
                query = query.Where(t => t.ToolRef != null && values.Contains(t.ToolRef.tool_name));
            }

            if (!string.IsNullOrEmpty(size_ref))
            {
                var values = size_ref.Split(',').ToHashSet();
                query = query.Where(t => t.SizeRef != null && values.Contains(t.SizeRef.size_ref));
            }

            if (!string.IsNullOrEmpty(original_spec))
            {
                var boolValues = original_spec.Split(',')
                    .Select(v => v.Trim().ToLower())
                    .Where(v => v == "true" || v == "false" || v == "1" || v == "0")
                    .Select(v => v == "true" || v == "1")
                    .ToHashSet();

                query = query.Where(t => t.original_spec != null && boolValues.Contains(t.original_spec != 0));
            }

            var result = await query
                .Select(t => new
                {
                    tool_key_id = t.tool_key_id,
                    tool_type = t.Type != null ? t.Type.type_name : null,
                    tool_name = t.Tool != null ? t.Tool.tool_name : null,
                    type_ref = t.TypeRef != null ? t.TypeRef.type_name : null,
                    tool_ref = t.ToolRef != null ? t.ToolRef.tool_name : null,
                    size_ref = t.SizeRef != null ? t.SizeRef.size_ref : null,
                    original_spec = t.original_spec,
                    ref_spec = t.ref_spec,  // ✅ เพิ่มตรงนี้
                    knurling = t.knurling != null ? t.knurling.ToString() : null,
                    pic_before_hst_file_name = t.pic_before_hst_file_name,
                    // ✅ เพิ่ม image_url
                    image_url = t.pic_before_hst != null
                        ? $"{Request.Scheme}://{Request.Host}/api/ToolKeyAlls/{t.tool_key_id}/before-hst"
                        : null
                    
                })
                .ToListAsync();

            return Ok(result);
        }
        // GET: api/ToolKeyAlls/distinct
        [HttpGet("distinct")]
        public async Task<ActionResult<object>> GetDistinctValues()
        {
            var distinctToolTypes = await _context.ToolKeyAlls
                .Include(t => t.Type)
                .Where(t => t.Type != null)
                .Select(t => t.Type.type_name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var distinctToolNames = await _context.ToolKeyAlls
                .Include(t => t.Tool)
                .Where(t => t.Tool != null)
                .Select(t => t.Tool.tool_name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var distinctTypeRefs = await _context.ToolKeyAlls
                .Include(t => t.TypeRef)
                .Where(t => t.TypeRef != null)
                .Select(t => t.TypeRef.type_name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var distinctToolRefs = await _context.ToolKeyAlls
                .Include(t => t.ToolRef)
                .Where(t => t.ToolRef != null)
                .Select(t => t.ToolRef.tool_name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var distinctSizeRefs = await _context.ToolKeyAlls
                .Include(t => t.SizeRef)
                .Where(t => t.SizeRef != null)
                .Select(t => t.SizeRef.size_ref)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var distinctOriginalSpecs = await _context.ToolKeyAlls
                .Where(t => t.original_spec != null)
                .Select(t => t.original_spec != 0)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return Ok(new
            {
                tool_type = distinctToolTypes,
                tool_name = distinctToolNames,
                type_ref = distinctTypeRefs,
                tool_ref = distinctToolRefs,
                size_ref = distinctSizeRefs,
                original_spec = distinctOriginalSpecs
            });
        }
        // POST: api/ToolKeyAlls
        [HttpPost]
        public async Task<ActionResult<ToolKeyAll>> PostToolKeyAll(ToolKeyAll toolKeyAll)
        {
            _context.ToolKeyAlls.Add(toolKeyAll);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostToolKeyAll), new { id = toolKeyAll.tool_key_id }, toolKeyAll);
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckDuplicate(
        int? type_id, int? tool_id, int? type_ref_id, int? tool_ref_id, int? size_ref_id)
        {
            var exists = await _context.ToolKeyAlls.FirstOrDefaultAsync(x =>
                (x.type_id ?? 0) == (type_id ?? 0) &&
                (x.tool_id ?? 0) == (tool_id ?? 0) &&
                (x.type_ref_id ?? 0) == (type_ref_id ?? 0) &&
                (x.tool_ref_id ?? 0) == (tool_ref_id ?? 0) &&
                (x.size_ref_id ?? 0) == (size_ref_id ?? 0)
            );

            return exists == null ? NotFound() : Ok(exists);
        }


        [HttpGet("exists")] //true or false
        // ตรวจสอบว่ามีข้อมูลที่ตรงตามเงื่อนไขหรือไม่ ใน toolKeyAlls หรือไม่
        // ถ้ามีจะ return true ถ้าไม่มีจะ return false
        public async Task<ActionResult<bool>> CheckExists(
        int? type_id, int? tool_id, int? type_ref_id, int? tool_ref_id, int? size_ref_id)
        {
            var exists = await _context.ToolKeyAlls.AnyAsync(x =>
                x.type_id == type_id &&
                x.tool_id == tool_id &&
                x.type_ref_id == type_ref_id &&
                x.tool_ref_id == tool_ref_id &&
                x.size_ref_id == size_ref_id
            );

            return Ok(exists); // 🔁 ✅ จะ return true หรือ false เสมอ
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolKeyAll(int id, ToolKeyAll toolKeyAll)
        {
            if (id != toolKeyAll.tool_key_id)
            {
                return BadRequest("tool_key_id mismatch");
            }

            _context.Entry(toolKeyAll).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ToolKeyAlls.Any(e => e.tool_key_id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolKeyAll(int id)
        {
            var item = await _context.ToolKeyAlls.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.ToolKeyAlls.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("byOriginalKey/{toolKeyId}")]
        public async Task<IActionResult> DeleteByOriginalKey(int toolKeyId)
        {
            var items = await _context.ToolKeyAlls
                .Where(t => t.source_original_key_id == toolKeyId)
                .ToListAsync();

            if (items.Count == 0)
            {
                return NotFound("No related toolKeyAll found for this original key");
            }

            _context.ToolKeyAlls.RemoveRange(items);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/ToolKeyAlls/check-original
        [HttpGet("check-original")]
        public async Task<IActionResult> CheckOriginalToolKeyAll(
            [FromQuery] int type_id,
            [FromQuery] int tool_id,
            [FromQuery] int type_ref_id,
            [FromQuery] int tool_ref_id,
            [FromQuery] int size_ref_id,
            [FromQuery] int source_original_key_id)
        {
            var exists = await _context.ToolKeyAlls.AnyAsync(k =>
                k.type_id == type_id &&
                k.tool_id == tool_id &&
                k.type_ref_id == type_ref_id &&
                k.tool_ref_id == tool_ref_id &&
                k.size_ref_id == size_ref_id &&
                k.source_original_key_id == source_original_key_id &&
                k.original_spec == 1
            );

            return Ok(new { exists });
        }


        // GET: api/ToolKeyAlls/{id}/ids
        [HttpGet("{id}/ids")]
        public async Task<ActionResult<object>> GetToolKeyAllIdsById(int id)
        {
            var t = await _context.ToolKeyAlls
                .FirstOrDefaultAsync(t => t.tool_key_id == id);

            if (t == null)
            {
                return NotFound();
            }

            var result = new
            {
                tool_key_id = t.tool_key_id,
                type_id = t.type_id,
                tool_id = t.tool_id,
                type_ref_id = t.type_ref_id,
                tool_ref_id = t.tool_ref_id,
                size_ref_id = t.size_ref_id
            };

            return Ok(result);
        }

        // GET: /api/ToolKeyAlls/{id}/before-hst
        [HttpGet("{id}/before-hst")]
        public async Task<IActionResult> GetBeforeHstImage(int id)
        {
            var row = await _context.ToolKeyAlls.FindAsync(id);
            if (row == null || row.pic_before_hst == null)
                return NotFound();

            var fileName = string.IsNullOrEmpty(row.pic_before_hst_file_name)
                ? "before_hst.png"
                : row.pic_before_hst_file_name;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";

            return File(row.pic_before_hst, contentType, fileName);
        }



        // POST: /api/ToolKeyAlls/{tool_key_id}/before-hst

        [HttpPost("{tool_key_id:int}/before-hst")]
        public async Task<IActionResult> UpsertBeforeHst(
            int tool_key_id,
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var row = await _context.ToolKeyAlls.FindAsync(tool_key_id);
            if (row == null) return NotFound("tool_key_id not found.");

            var allow = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allow.Contains(ext))
                return BadRequest("Only .jpg/.jpeg/.png/.webp allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            // ✅ อัปเดตเฉพาะรูปและชื่อไฟล์
            row.pic_before_hst = ms.ToArray();
            row.pic_before_hst_file_name = Path.GetFileName(file.FileName);

            // ❌ ไม่แตะ user_id/update_by/create_by
            // row.update_at = DateTime.Now; // ใส่ได้ถ้าต้องการ

            await _context.SaveChangesAsync();

            return Ok(new
            {
                tool_key_id,
                file_name = row.pic_before_hst_file_name,
                size = row.pic_before_hst?.Length ?? 0
            });
        }

        // DELETE: /api/ToolKeyAlls/{tool_key_id}/before-hst
        [HttpDelete("{tool_key_id:int}/before-hst")]
        public async Task<IActionResult> DeleteBeforeHst(int tool_key_id)
        {
            var row = await _context.ToolKeyAlls.FindAsync(tool_key_id);
            if (row == null) return NotFound("tool_key_id not found.");

            row.pic_before_hst = null;
            row.pic_before_hst_file_name = null;

            // ไม่แตะ user_id/update_by/create_by
            // row.update_at = DateTime.Now; // ใส่ได้ถ้าต้องเก็บ timestamp

            await _context.SaveChangesAsync();
            return NoContent();
        }



    }
}
