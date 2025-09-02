// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using api.Models;

// namespace api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class ResolvedToolSpecsController : ControllerBase
//     {
//         private readonly MbkBarbell9Context _context;

//         public ResolvedToolSpecsController(MbkBarbell9Context context)
//         {
//             _context = context;
//         }

//         // GET: api/ResolvedToolSpecs
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<object>>> GetResolvedSpecs()
//         {
//             var data = await _context.toolSpecs
//                 .Include(ts => ts.ref_key)
//                     .ThenInclude(rk => rk.tool)
//                 .Include(ts => ts.ref_key)
//                     .ThenInclude(rk => rk.type)
//                 .Include(ts => ts.ref_key)
//                     .ThenInclude(rk => rk.position_type)
//                 .Include(ts => ts.tool_ref_spec)
//                     .ThenInclude(trs => trs.tool_key)
//                         .ThenInclude(tko => tko.tool)
//                 .Include(ts => ts.tool_ref_spec)
//                     .ThenInclude(trs => trs.tool_key)
//                         .ThenInclude(tko => tko.type)
//                 .Include(ts => ts.tool_ref_spec)
//                     .ThenInclude(trs => trs.tool_key)
//                         .ThenInclude(tko => tko.size_ref)
//                 .Include(ts => ts.tool_ref_spec)
//                     .ThenInclude(trs => trs.axle_type)
//                 .Include(ts => ts.axle_type)
//                 .Include(ts => ts.position_type)
//                 .ToListAsync();

//             var result = data.Select(t => new
//             {
//                 t.tool_spec_id,

//                 // 🔁 REF: toolKeyReference
//                 tool_type = t.ref_key?.type?.type_name,
//                 tool_name = t.ref_key?.tool?.tool_name,
//                 position_type = t.ref_key?.position_type?.position_type,

//                 // 🔁 ORIG: toolKeyOriginal (via toolRefSpec)
//                 type_ref = t.tool_ref_spec?.tool_key?.type?.type_name,
//                 tool_ref = t.tool_ref_spec?.tool_key?.tool?.tool_name,
//                 size_ref = t.tool_ref_spec?.tool_key?.size_ref?.size_ref,
//                 axle_type = t.tool_ref_spec?.axle_type?.axle_type,

//                 // 🔩 SPEC FIELDS
//                 overall_a = t.tool_ref_spec?.overall_a,
//                 overall_b = t.tool_ref_spec?.overall_b,
//                 overall_c = t.tool_ref_spec?.overall_c,
//                 tolerance_a = t.tool_ref_spec?.tolerance_a,
//                 tolerance_b = t.tool_ref_spec?.tolerance_b,
//                 tolerance_c = t.tool_ref_spec?.tolerance_c,
//                 f_shank_min = t.tool_ref_spec?.f_shank_min,
//                 f_shank_max = t.tool_ref_spec?.f_shank_max,
//                 b2b_min = t.tool_ref_spec?.b2b_min,
//                 b2b_max = t.tool_ref_spec?.b2b_max,
//                 h2h_min = t.tool_ref_spec?.h2h_min,
//                 h2h_max = t.tool_ref_spec?.h2h_max,

//                 // ✅ override chassis_span ถ้ามี
//                 chassis_span = t.chassis_span_override != null 
//                     ? t.chassis_span_override.ToString() 
//                     : t.tool_ref_spec?.chassis_span,

//                 // 🔩 จาก reference
//                 knurling_type = t.ref_key?.knurling_type
//             }).ToList();

//             return Ok(result);
//         }
//     }
// }
