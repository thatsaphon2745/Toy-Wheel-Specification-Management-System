using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;   // namespace ของ DbContext
using api.DTOs;     // DTOs
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MbkBarbell9Context _db;

        public AuthController(IConfiguration configuration, MbkBarbell9Context db)
        {
            _configuration = configuration;
            _db = db;
        }

        // [HttpPost("signup")]
        // public async Task<IActionResult> SignUp([FromBody] RegisterDto dto)
        // {
        //     if (await _db.Users.AnyAsync(u => u.username == dto.username))
        //     {
        //         return BadRequest(new { message = "Username already exists." });
        //     }

        //     var passwordHash = HashPassword(dto.password);

        //     var user = new User
        //     {
        //         username = dto.username,
        //         password_hash = passwordHash,
        //         first_name = dto.first_name,
        //         last_name = dto.last_name,
        //         department_id = dto.department_id,
        //         role_id = dto.role_id,
        //         status_id = dto.status_id,
        //         employee_id = dto.employee_id,
        //         email = dto.email
        //     };

        //     _db.Users.Add(user);
        //     await _db.SaveChangesAsync();

        //     return Ok(new { message = "User registered successfully!" });
        // }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.employee_id == dto.employee_id))
            {
                return BadRequest(new { message = "Employee ID already exists." });
            }

            // if (await _db.Users.AnyAsync(u => u.username == dto.username))
            // {
            //     return BadRequest(new { message = "Username already exists." });
            // }

            var passwordHash = HashPassword(dto.password);

            var user = new User
            {
                // username = dto.username,
                password_hash = passwordHash,
                first_name = dto.first_name,
                last_name = dto.last_name,
                department_id = dto.department_id,
                role_id = dto.role_id,
                status_id = dto.status_id,
                employee_id = dto.employee_id,
                email = dto.email
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }


        // [HttpPost("login")]
        // public async Task<IActionResult> Login([FromBody] LoginDto model)
        // {
        //     var user = await _db.Users.FirstOrDefaultAsync(u => u.username == model.username);
        //     if (user == null)
        //         return Unauthorized(new { message = "Invalid username or password." });

        //     if (!BCrypt.Net.BCrypt.Verify(model.password, user.password_hash))
        //     {
        //         return Unauthorized(new { message = "Invalid username or password." });
        //     }

        //     // Load JWT Settings
        //     var jwtSettings = _configuration.GetSection("Jwt");
        //     var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key is missing in configuration.");
        //     var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT Issuer is missing in configuration.");
        //     var audience = jwtSettings["Audience"] ?? throw new Exception("JWT Audience is missing in configuration.");
        //     // ถ้าครบ 60 min token ที่ได้รับไปจะ expire
        //     var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");

        //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        //     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //     var roleName = await _db.Roles
        //         .Where(r => r.role_id == user.role_id)
        //         .Select(r => r.role_name)
        //         .FirstOrDefaultAsync() ?? "user";

        //     // var claims = new List<Claim>
        //     // {
        //     //     new Claim(JwtRegisteredClaimNames.Sub, model.username),
        //     //     new Claim("role", roleName),
        //     //     new Claim("user_id", user.user_id.ToString()),
        //     //     new Claim("employee_id", user.employee_id ?? ""),
        //     //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //     // };

        //     var claims = new List<Claim>
        //     {
        //         new Claim(JwtRegisteredClaimNames.Sub, model.username),
        //         new Claim("role", roleName),
        //         new Claim(ClaimTypes.Role, roleName), // ✅ เพิ่มบรรทัดนี้
        //         new Claim("user_id", user.user_id.ToString()),
        //         new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
        //         new Claim("employee_id", user.employee_id ?? ""),
        //         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //     };

        //     var token = new JwtSecurityToken(
        //         issuer: issuer,
        //         audience: audience,
        //         claims: claims,
        //         expires: DateTime.UtcNow.AddMinutes(expireMinutes),
        //         signingCredentials: creds
        //     );

        //     var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //     return Ok(new
        //     {
        //         token = tokenString,
        //         username = user.username,
        //         role = roleName,
        //         employee_id = user.employee_id
        //     });
        // }

        //     [HttpPost("login")]
        //     public async Task<IActionResult> Login([FromBody] LoginDto model)
        //     {
        //         // var user = await _db.Users.FirstOrDefaultAsync(u => u.username == model.username);
        //         var user = await _db.Users.FirstOrDefaultAsync(u => u.employee_id == model.employee_id);

        //         if (user == null)
        //         {
        //             Console.WriteLine("❌ User not found: " + model.employee_id);
        //             return Unauthorized(new { message = "Invalid username or password." });
        //         }

        //         Console.WriteLine("👉 DB hash: " + user.password_hash);
        //         Console.WriteLine("👉 Plain password: " + model.password);

        //         var result = BCrypt.Net.BCrypt.Verify(model.password, user.password_hash);

        //         Console.WriteLine("👉 Verify result: " + result);

        //         if (!result)
        //         {
        //             return Unauthorized(new { message = "Invalid username or password." });
        //         }

        //         // **ต่อจากนี้ code เดิมของคุณ**
        //         var jwtSettings = _configuration.GetSection("Jwt");
        //         var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key is missing in configuration.");
        //         var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT Issuer is missing in configuration.");
        //         var audience = jwtSettings["Audience"] ?? throw new Exception("JWT Audience is missing in configuration.");
        //         var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");

        //         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        //         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //         var roleName = await _db.Roles
        //             .Where(r => r.role_id == user.role_id)
        //             .Select(r => r.role_name)
        //             .FirstOrDefaultAsync() ?? "user";

        //         var claims = new List<Claim>
        // {
        //     // new Claim(JwtRegisteredClaimNames.Sub, model.username),
        //     new Claim(JwtRegisteredClaimNames.Sub, model.employee_id),
        //     new Claim("role", roleName),
        //     new Claim(ClaimTypes.Role, roleName),
        //     new Claim("user_id", user.user_id.ToString()),
        //     new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
        //     new Claim("employee_id", user.employee_id ?? ""),
        //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // };

        //         var token = new JwtSecurityToken(
        //             issuer: issuer,
        //             audience: audience,
        //             claims: claims,
        //             expires: DateTime.UtcNow.AddMinutes(expireMinutes),
        //             signingCredentials: creds
        //         );

        //         var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //         return Ok(new
        //         {
        //             token = tokenString,
        //             // username = user.username,
        //             role = roleName,
        //             employee_id = user.employee_id
        //         });
        //     }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // 🔍 Include UserStatus เพื่อใช้ status_name
            var user = await _db.Users
                .Include(u => u.UserStatus)
                .FirstOrDefaultAsync(u => u.employee_id == model.employee_id);

            if (user == null)
            {
                Console.WriteLine("❌ User not found: " + model.employee_id);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // 🚫 เช็คว่า status เป็น Deactive ไหม
            if (user.UserStatus?.status_name == "Deactive")
            {
                Console.WriteLine("🚫 User is deactivated: " + model.employee_id);
                return Unauthorized(new { message = "Your account is deactivated. Please contact the administrator." });
            }

            Console.WriteLine("👉 DB hash: " + user.password_hash);
            Console.WriteLine("👉 Plain password: " + model.password);

            var result = BCrypt.Net.BCrypt.Verify(model.password, user.password_hash);
            Console.WriteLine("👉 Verify result: " + result);

            if (!result)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // 🔐 Generate JWT
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key is missing in configuration.");
            var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT Issuer is missing in configuration.");
            var audience = jwtSettings["Audience"] ?? throw new Exception("JWT Audience is missing in configuration.");
            var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roleName = await _db.Roles
                .Where(r => r.role_id == user.role_id)
                .Select(r => r.role_name)
                .FirstOrDefaultAsync() ?? "user";

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, model.employee_id),
        new Claim("role", roleName),
        new Claim(ClaimTypes.Role, roleName),
        new Claim("user_id", user.user_id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
        new Claim("employee_id", user.employee_id ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                role = roleName,
                employee_id = user.employee_id
            });
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }


        // public class ForgotPasswordDto
        // {
        //     public string employee_id { get; set; } = string.Empty;
        //     public string email { get; set; } = string.Empty;
        // }


        // [HttpPost("forgot-password")]
        // public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        // {
        //     var user = await _db.Users
        //         .FirstOrDefaultAsync(u => u.employee_id == dto.employee_id && u.email == dto.email);

        //     if (user == null)
        //         return NotFound(new { message = "Employee ID and email do not match." });

        //     var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        //     user.ResetToken = token;
        //     user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        //     await _db.SaveChangesAsync();

        //     var resetLink = $"http://localhost:5173/reset-password?token={token}";
        //     Console.WriteLine($"🔗 Reset Link: {resetLink}");

        //     return Ok(new { message = "Reset link sent." });
        // }


    }
}
