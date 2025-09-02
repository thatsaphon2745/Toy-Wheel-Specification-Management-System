using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolPadHstBrassMapController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ToolPadHstBrassMapController(MbkBarbell9Context context)
        {
            _context = context;
        }

        //     // GET: api/ToolPadHstBrassMap/pivot
        //     [HttpGet("pivot")]
        //     public async Task<ActionResult<IEnumerable<object>>> GetToolPadHstBrassMapPivot()
        //     {
        //         var toolPadMaps = await _context.ToolPadMaps
        //             .Include(t => t.ToolKey)
        //                 .ThenInclude(k => k.Type)
        //             .Include(t => t.ToolKey)
        //                 .ThenInclude(k => k.Tool)
        //             .Include(t => t.ToolKey)
        //                 .ThenInclude(k => k.TypeRef)
        //             .Include(t => t.ToolKey)
        //                 .ThenInclude(k => k.ToolRef)
        //             .Include(t => t.ToolKey)
        //                 .ThenInclude(k => k.SizeRef)
        //             .Include(t => t.Pad)
        //             .Include(t => t.HstType)
        //             .ToListAsync();

        //         var padBrassMap = await _context.PadBrassMaps
        //             .Include(pb => pb.Brass)
        //             .ToListAsync();

        //         var pivot = toolPadMaps
        // .GroupBy(x => new
        // {
        //     tool_type = x.ToolKey?.Type?.type_name,
        //     tool_name = x.ToolKey?.Tool?.tool_name,
        //     type_ref = x.ToolKey?.TypeRef?.type_name,
        //     tool_ref = x.ToolKey?.ToolRef?.tool_name,
        //     size_ref = x.ToolKey?.SizeRef?.size_ref
        // })
        // .Select(g =>
        // {
        //     var padsByType = g.ToDictionary(
        //         x => x.HstType?.hst_type ?? "",
        //         x => new
        //         {
        //             pad_name = x.Pad?.pad_name,
        //             brass_no = string.Join(", ",
        //                 padBrassMap
        //                     .Where(pb => pb.pad_id == x.pad_id)
        //                     .Select(pb => pb.Brass?.brass_no)
        //                     .Where(b => !string.IsNullOrEmpty(b))
        //             )
        //         }
        //     );

        //     return new
        //     {
        //         g.Key.tool_type,
        //         g.Key.tool_name,
        //         g.Key.type_ref,
        //         g.Key.tool_ref,
        //         g.Key.size_ref,
        //         HST_pad = padsByType.GetValueOrDefault("HST")?.pad_name,
        //         RIM_pad = padsByType.GetValueOrDefault("RIM")?.pad_name,
        //         INNER_pad = padsByType.GetValueOrDefault("INNER")?.pad_name,
        //         EXTRA_RIM_pad = padsByType.GetValueOrDefault("EXTRA_RIM")?.pad_name,
        //         HST_brass = padsByType.GetValueOrDefault("HST")?.brass_no,
        //         RIM_brass = padsByType.GetValueOrDefault("RIM")?.brass_no,
        //         INNER_brass = padsByType.GetValueOrDefault("INNER")?.brass_no,
        //         EXTRA_RIM_brass = padsByType.GetValueOrDefault("EXTRA_RIM")?.brass_no
        //     };
        // })
        // .ToList();


        //         return Ok(pivot);
        //     }

        // GET: api/ToolPadHstBrassMap/pivot
        [HttpGet("pivot")]
        public async Task<ActionResult<IEnumerable<object>>> GetToolPadHstBrassMapPivot()
        {
            var toolPadMaps = await _context.ToolPadMaps
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Type)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.Tool)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.TypeRef)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.ToolRef)
                .Include(t => t.ToolKey)
                    .ThenInclude(k => k.SizeRef)
                .Include(t => t.Pad)
                .Include(t => t.HstType)
                .ToListAsync();

            var padBrassMap = await _context.PadBrassMaps
                .Include(pb => pb.Brass)
                .ToListAsync();

            var pivot = toolPadMaps
                .GroupBy(x => new
                {
                    tool_type = x.ToolKey?.Type?.type_name,
                    tool_name = x.ToolKey?.Tool?.tool_name,
                    type_ref = x.ToolKey?.TypeRef?.type_name,
                    tool_ref = x.ToolKey?.ToolRef?.tool_name,
                    size_ref = x.ToolKey?.SizeRef?.size_ref
                })
                .Select(g =>
                {
                    // ✅ Group เพื่อจัดการกรณี HST type ซ้ำกัน
                    var padsByType = g
                        .GroupBy(x => x.HstType?.hst_type ?? "")
                        .ToDictionary(
                            gr => gr.Key,
                            gr =>
                            {
                                var first = gr.First();
                                return new
                                {
                                    pad_name = first.Pad?.pad_name,
                                    brass_no = string.Join(", ",
                                        padBrassMap
                                            .Where(pb => pb.pad_id == first.pad_id)
                                            .Select(pb => pb.Brass?.brass_no)
                                            .Where(b => !string.IsNullOrEmpty(b))
                                    )
                                };
                            });

                    return new
                    {
                        g.Key.tool_type,
                        g.Key.tool_name,
                        g.Key.type_ref,
                        g.Key.tool_ref,
                        g.Key.size_ref,
                        HST_pad = padsByType.GetValueOrDefault("HST")?.pad_name,
                        RIM_pad = padsByType.GetValueOrDefault("RIM")?.pad_name,
                        INNER_pad = padsByType.GetValueOrDefault("INNER")?.pad_name,
                        EXTRA_RIM_pad = padsByType.GetValueOrDefault("EXTRA_RIM")?.pad_name,
                        HST_brass = padsByType.GetValueOrDefault("HST")?.brass_no,
                        RIM_brass = padsByType.GetValueOrDefault("RIM")?.brass_no,
                        INNER_brass = padsByType.GetValueOrDefault("INNER")?.brass_no,
                        EXTRA_RIM_brass = padsByType.GetValueOrDefault("EXTRA_RIM")?.brass_no
                    };
                })
                .ToList();

            return Ok(pivot);
        }

    }
}
