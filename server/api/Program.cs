using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using api.Models; // ✅ DbContext
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== DB & Logging =====
            builder.Services.AddDbContext<MbkBarbell9Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging()
                       .LogTo(Console.WriteLine, LogLevel.Information)
            );

            // ===== Controllers & JSON =====
            builder.Services
                .AddControllers()
                .AddJsonOptions(o => { o.JsonSerializerOptions.PropertyNamingPolicy = null; })
                .AddNewtonsoftJson(); // ถ้าใช้ Newtonsoft อยู่แล้ว

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ===== CORS (ถ้า FE/BE โดเมนเดียวกัน จะไม่ใช้ก็ได้) =====
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    policy
                        .AllowAnyOrigin()   // Prod จริงแนะนำ fix เป็นโดเมนของมึง
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ===== JWT =====
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            // ===== DI =====
            builder.Services.AddScoped<DynamicService>();
            builder.Services.AddScoped<OriginalSpecService>();
            builder.Services.AddScoped<DdcService>();
            builder.Services.AddScoped<PadBrassService>();
            builder.Services.AddScoped<PadHstService>();
            builder.Services.AddScoped<ToolPadService>();
            builder.Services.AddScoped<ToolMachineService>();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // ===== Swagger เฉพาะ Dev (จะเปิด Prod ก็ได้ถ้าต้องการ) =====
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ===== (แนะนำ) เปิด HSTS ใน Prod ถ้าใช้ HTTPS =====
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            // ===== Static Files + Default Document (เสิร์ฟ React ใน wwwroot) =====
            app.UseDefaultFiles();    // หา index.html
            app.UseStaticFiles();     // เสิร์ฟไฟล์ใน wwwroot

            // ===== Pipeline API =====
            // ถ้า IIS ยังไม่มี SSL/CERT ให้ปิด UseHttpsRedirection ชั่วคราวได้
            app.UseHttpsRedirection();

            app.UseCors("Default");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ===== SPA Fallback: route อื่นๆ ส่งกลับ index.html =====
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
