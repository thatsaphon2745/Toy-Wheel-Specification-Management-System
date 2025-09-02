using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public UsersController(MbkBarbell9Context context)
        {
            _context = context;
        }

        // ✅ GET all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserStatus)
                .Include(u => u.Department)
                .Select(u => new UserDto
                {
                    user_id = u.user_id,
                    // username = u.username,
                    first_name = u.first_name,
                    last_name = u.last_name,
                    department_name = u.Department != null ? u.Department.department_name : null,
                    role_id = u.role_id,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    status_id = u.status_id,
                    status_name = u.UserStatus != null ? u.UserStatus.status_name : null,
                    employee_id = u.employee_id,
                    email = u.email
                })
                .ToListAsync();

            return Ok(users);
        }

        // ✅ GET user by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserStatus)
                .Include(u => u.Department)
                .Where(u => u.user_id == id)
                .Select(u => new UserDto
                {
                    user_id = u.user_id,
                    // username = u.username,
                    first_name = u.first_name,
                    last_name = u.last_name,
                    department_name = u.Department != null ? u.Department.department_name : null,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    status_name = u.UserStatus != null ? u.UserStatus.status_name : null,
                    employee_id = u.employee_id,
                    email = u.email
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // ✅ PUT user
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            if (id != updatedUser.user_id)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.first_name = updatedUser.first_name;
            user.last_name = updatedUser.last_name;
            user.department_id = updatedUser.department_id;
            user.role_id = updatedUser.role_id;
            user.status_id = updatedUser.status_id;
            user.employee_id = updatedUser.employee_id;
            user.email = updatedUser.email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "admin")]
        [HttpPatch("{id}/Role")]
        public async Task<IActionResult> PatchUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            // ✅ ดึง admin user_id จาก Claims ถ้าอยากใช้ (optional)
            var adminIdClaim = User.FindFirst("user_id");
            if (adminIdClaim == null)
                return Unauthorized();

            var adminId = int.Parse(adminIdClaim.Value);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // ✅ อัพเดต role
            user.role_id = dto.role_id;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User role updated successfully.",
                updated_by_admin_id = adminId
            });
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id}/ToggleStatus")]
        public async Task<IActionResult> PatchUserToggleStatus(int id)
        {
            var adminIdClaim = User.FindFirst("user_id");
            if (adminIdClaim == null)
                return Unauthorized();

            var adminId = int.Parse(adminIdClaim.Value);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // ✅ Toggle Active <-> Deactive
            if (user.status_id == 1)
            {
                user.status_id = 4; // Deactive
            }
            else
            {
                user.status_id = 1; // Active
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User status toggled successfully.",
                new_status_id = user.status_id,
                updated_by_admin_id = adminId
            });
        }


    }
}
