using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public LogsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET: /api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
        {
            var logs = await _context.Logs
                .OrderByDescending(l => l.created_at)
                .ToListAsync();

            return Ok(logs);
        }

        // ✅ GET: /api/Logs/table/tools
        [HttpGet("table/{tableName}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogsByTable(string tableName)
        {
            var logs = await _context.Logs
                .Where(l => l.target_table == tableName)
                .OrderByDescending(l => l.created_at)
                .ToListAsync();

            if (logs == null || logs.Count == 0)
                return NotFound($"No logs found for table {tableName}.");

            return Ok(logs);
        }

        // ✅ GET: /api/Logs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> GetLogById(long id)
        {
            var log = await _context.Logs.FindAsync(id);

            if (log == null)
                return NotFound();

            return Ok(log);
        }

        // ✅ DELETE log (Optional)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(long id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log == null)
                return NotFound();

            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
