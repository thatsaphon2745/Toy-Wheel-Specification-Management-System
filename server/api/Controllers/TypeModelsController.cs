using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeModelsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public TypeModelsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // GET: api/TypeModels?search=MB
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeModel>>> GetTypeModels([FromQuery] string? search)
        {
            var query = _context.TypeModels.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    EF.Functions.Like(t.type_name.ToLower(), $"%{search.ToLower()}%")
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/TypeModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeModel>> GetTypeModel(int id)
        {
            var typeModel = await _context.TypeModels.FindAsync(id);

            if (typeModel == null)
            {
                return NotFound();
            }

            return typeModel;
        }

        // PUT: api/TypeModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeModel(int id, TypeModel typeModel)
        {
            if (id != typeModel.type_id)
            {
                return BadRequest();
            }

            _context.Entry(typeModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TypeModels
        [HttpPost]
        public async Task<ActionResult<TypeModel>> PostTypeModel(TypeModel typeModel)
        {
            _context.TypeModels.Add(typeModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeModel", new { id = typeModel.type_id }, typeModel);
        }

        // DELETE: api/TypeModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeModel(int id)
        {
            var typeModel = await _context.TypeModels.FindAsync(id);
            if (typeModel == null)
            {
                return NotFound();
            }

            _context.TypeModels.Remove(typeModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TypeModelExists(int id)
        {
            return _context.TypeModels.Any(e => e.type_id == id);
        }
    }
}
