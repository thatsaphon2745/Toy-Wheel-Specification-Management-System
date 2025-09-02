using Microsoft.AspNetCore.Mvc;
using api.Models;
using System.Linq;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly MbkBarbell9Context _db;

        public DepartmentController(MbkBarbell9Context db)
        {
            _db = db;
        }

        // GET: api/Department
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var departments = _db.Departments
                .Select(d => new
                {
                    department_id = d.department_id,
                    department_name = d.department_name
                })
                .ToList();

            return Ok(departments);
        }
    }
}
