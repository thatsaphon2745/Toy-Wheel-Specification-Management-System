using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserStatusesController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public UserStatusesController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: api/UserStatuses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserStatus>>> GetUserStatuses()
        {
            return await _context.UserStatuses.ToListAsync();
        }

        // ✅ GET: api/UserStatuses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserStatus>> GetUserStatus(int id)
        {
            var status = await _context.UserStatuses.FindAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            return status;
        }

        // ✅ POST: api/UserStatuses
        [HttpPost]
        public async Task<ActionResult<UserStatus>> PostUserStatus(UserStatus status)
        {
            _context.UserStatuses.Add(status);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserStatus), new { id = status.status_id }, status);
        }

        // ✅ PUT: api/UserStatuses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserStatus(int id, UserStatus status)
        {
            if (id != status.status_id)
            {
                return BadRequest();
            }

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserStatusExists(id))
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

        // ✅ DELETE: api/UserStatuses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserStatus(int id)
        {
            var status = await _context.UserStatuses.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }

            _context.UserStatuses.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserStatusExists(int id)
        {
            return _context.UserStatuses.Any(e => e.status_id == id);
        }
    }
}
