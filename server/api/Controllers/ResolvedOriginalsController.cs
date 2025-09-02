using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;
using System.Runtime.CompilerServices;
namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolvedOriginalsController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ResolvedOriginalsController(MbkBarbell9Context context)
        {
            _context = context;
        }

        

        [HttpGet("spec")]
        public async Task<ActionResult> GetResolvedOriginal()
        {
            try
            {
                var filters = HttpContext.Request.Query;
                var whereClauses = new List<string>();


                string BuildClause(string column, string[] values)
                {
                    var trimmed = values
                        .Where(v => v != null)
                        .Select(v => v.Trim())
                        .ToArray();

                    var isNullSelected = trimmed.Any(v => v == "" || v.ToLower() == "null" || v == "(Blanks)");

                    var exactMatches = trimmed
                        .Where(v => v != "" && v.ToLower() != "null" && v != "(Blanks)")
                        .Select(v => $"'{v.Replace("'", "''")}'");

                    string clause = "";

                    if (exactMatches.Any())
                        clause = $"{column} IN ({string.Join(", ", exactMatches)})";

                    if (isNullSelected)
                        clause = clause != "" ? $"{clause} OR {column} IS NULL" : $"{column} IS NULL";

                    return $"({clause})";
                }

                if (filters.TryGetValue("tool_type", out var toolTypes))
                    whereClauses.Add(BuildClause("tool_type", toolTypes.ToArray()));

                if (filters.TryGetValue("tool_name", out var toolNames))
                    whereClauses.Add(BuildClause("tool_name", toolNames.ToArray()));

                if (filters.TryGetValue("size_ref", out var sizeRefs))
                    whereClauses.Add(BuildClause("size_ref", sizeRefs.ToArray()));

                if (filters.TryGetValue("axle_type", out var axleTypes))
                    whereClauses.Add(BuildClause("axle_type", axleTypes.ToArray()));

                if (filters.TryGetValue("machine_no", out var machineNos))
                    whereClauses.Add(BuildClause("machine_no", machineNos.ToArray()));


                if (filters.TryGetValue("pad_source_key", out var padSourceKeys))
                {
                    var valuesRaw = padSourceKeys.ToString();

                    var values = valuesRaw
                        .Split("),", StringSplitOptions.None)          // แยกที่ ),
                        .Select(v => v.Trim())
                        .Select(v => v.EndsWith(")") ? v : v + ")")    // ใส่ปิดวงเล็บให้ครบ
                        .ToArray();

                    whereClauses.Add(BuildClause(
                        "LTRIM(RTRIM(pad_source_key))",
                        values));
                }

                if (filters.TryGetValue("HST_pad", out var hstPads))
                    whereClauses.Add(BuildClause("HST_pad", hstPads.ToArray()));

                if (filters.TryGetValue("RIM_pad", out var rimPads))
                    whereClauses.Add(BuildClause("RIM_pad", rimPads.ToArray()));

                if (filters.TryGetValue("INNER_pad", out var innerPads))
                    whereClauses.Add(BuildClause("INNER_pad", innerPads.ToArray()));

                if (filters.TryGetValue("EXTRA_RIM_pad", out var extraRimPads))
                    whereClauses.Add(BuildClause("EXTRA_RIM_pad", extraRimPads.ToArray()));

                if (filters.TryGetValue("HST_brass", out var hstBrass))
                {
                    var values = hstBrass.ToArray(); // ได้ array ของแต่ละค่าที่ frontend append มา
                    whereClauses.Add(BuildClause("HST_brass", values));
                }

                if (filters.TryGetValue("RIM_brass", out var rimBrass))
                    whereClauses.Add(BuildClause("RIM_brass", rimBrass.ToArray()));

                if (filters.TryGetValue("INNER_brass", out var innerBrass))
                    whereClauses.Add(BuildClause("INNER_brass", innerBrass.ToArray()));

                if (filters.TryGetValue("EXTRA_RIM_brass", out var extraRimBrass))
                    whereClauses.Add(BuildClause("EXTRA_RIM_brass", extraRimBrass.ToArray()));


                if (filters.TryGetValue("machine_source_key", out var machineSourceKeys))
                {
                    var valuesRaw = machineSourceKeys.ToString();

                    var values = valuesRaw
                        .Split("),", StringSplitOptions.None)          // แยกที่ ),
                        .Select(v => v.Trim())
                        .Select(v => v.EndsWith(")") ? v : v + ")")    // ใส่ปิดวงเล็บให้ครบ
                        .ToArray();

                    whereClauses.Add(BuildClause(
                        "LTRIM(RTRIM(machine_source_key))",
                        values));
                }

                // numeric fields
                if (filters.TryGetValue("f_shank_min", out var fShankMin))
                    whereClauses.Add(BuildClause("f_shank_min", fShankMin.ToArray()));

                if (filters.TryGetValue("f_shank_max", out var fShankMax))
                    whereClauses.Add(BuildClause("f_shank_max", fShankMax.ToArray()));

                if (filters.TryGetValue("b2b_min", out var b2bMin))
                    whereClauses.Add(BuildClause("b2b_min", b2bMin.ToArray()));

                if (filters.TryGetValue("b2b_max", out var b2bMax))
                    whereClauses.Add(BuildClause("b2b_max", b2bMax.ToArray()));

                if (filters.TryGetValue("h2h_min", out var h2hMin))
                    whereClauses.Add(BuildClause("h2h_min", h2hMin.ToArray()));

                if (filters.TryGetValue("h2h_max", out var h2hMax))
                    whereClauses.Add(BuildClause("h2h_max", h2hMax.ToArray()));

                if (filters.TryGetValue("chassis_span1", out var chassisSpan1))
                    whereClauses.Add(BuildClause("chassis_span1", chassisSpan1.ToArray()));

                if (filters.TryGetValue("chassis_span2", out var chassisSpan2))
                    whereClauses.Add(BuildClause("chassis_span2", chassisSpan2.ToArray()));

                if (filters.TryGetValue("is_original_spec", out var isOriginalSpec))
                    whereClauses.Add(BuildClause("is_original_spec", isOriginalSpec.ToArray()));

                if (filters.TryGetValue("knurling_type", out var knurlingType))
                    whereClauses.Add(BuildClause("knurling_type", knurlingType.ToArray()));

                if (filters.TryGetValue("overall_a", out var overallA))
                    whereClauses.Add(BuildClause("overall_a", overallA.ToArray()));

                if (filters.TryGetValue("tolerance_a", out var toleranceA))
                    whereClauses.Add(BuildClause("tolerance_a", toleranceA.ToArray()));

                if (filters.TryGetValue("overall_b", out var overallB))
                    whereClauses.Add(BuildClause("overall_b", overallB.ToArray()));

                if (filters.TryGetValue("tolerance_b", out var toleranceB))
                    whereClauses.Add(BuildClause("tolerance_b", toleranceB.ToArray()));

                if (filters.TryGetValue("overall_c", out var overallC))
                    whereClauses.Add(BuildClause("overall_c", overallC.ToArray()));

                if (filters.TryGetValue("tolerance_c", out var toleranceC))
                    whereClauses.Add(BuildClause("tolerance_c", toleranceC.ToArray()));

                if (filters.TryGetValue("pending_request", out var pendingRequest))
                    whereClauses.Add(BuildClause("pending_request", pendingRequest.ToArray()));

                if (filters.TryGetValue("create_by", out var createBy))
                    whereClauses.Add(BuildClause("create_by", createBy.ToArray()));

                if (filters.TryGetValue("update_by", out var updateBy))
                    whereClauses.Add(BuildClause("update_by", updateBy.ToArray()));

                if (filters.TryGetValue("source", out var source))
                    whereClauses.Add(BuildClause("source", source.ToArray()));

                if (filters.TryGetValue("description", out var description))
                    whereClauses.Add(BuildClause("description", description.ToArray()));

                var query = HttpContext.Request.Query;

                // if (query.TryGetValue("created_at_start", out var createdAtStart) && !string.IsNullOrWhiteSpace(createdAtStart))
                // {
                //     whereClauses.Add($"create_at >= '{createdAtStart}'");
                // }

                // if (query.TryGetValue("created_at_end", out var createdAtEnd) && !string.IsNullOrWhiteSpace(createdAtEnd))
                // {
                //     // ✅ ใช้เวลาให้สุดวัน (End-of-day)
                //     whereClauses.Add($"create_at <= '{createdAtEnd} 23:59:59.997'");
                // }

                // if (query.TryGetValue("updated_at_start", out var updatedAtStart) && !string.IsNullOrWhiteSpace(updatedAtStart))
                // {
                //     whereClauses.Add($"update_at >= '{updatedAtStart}'");
                // }

                // if (query.TryGetValue("updated_at_end", out var updatedAtEnd) && !string.IsNullOrWhiteSpace(updatedAtEnd))
                // {
                //     whereClauses.Add($"update_at <= '{updatedAtEnd} 23:59:59.997'");
                // }

                // ✅ Convert ISO datetime (T) → SQL datetime (space)
                if (filters.TryGetValue("created_at_start", out var createdAtStartStr) &&
                    DateTime.TryParse(createdAtStartStr, out var createdAtStart))
                {
                    whereClauses.Add($"create_at >= '{createdAtStart:yyyy-MM-dd HH:mm:ss}'");
                }

                if (filters.TryGetValue("created_at_end", out var createdAtEndStr) &&
                    DateTime.TryParse(createdAtEndStr, out var createdAtEnd))
                {
                    whereClauses.Add($"create_at <= '{createdAtEnd:yyyy-MM-dd HH:mm:ss}'");
                }

                if (filters.TryGetValue("updated_at_start", out var updatedAtStartStr) &&
                    DateTime.TryParse(updatedAtStartStr, out var updatedAtStart))
                {
                    whereClauses.Add($"update_at >= '{updatedAtStart:yyyy-MM-dd HH:mm:ss}'");
                }

                if (filters.TryGetValue("updated_at_end", out var updatedAtEndStr) &&
                    DateTime.TryParse(updatedAtEndStr, out var updatedAtEnd))
                {
                    whereClauses.Add($"update_at <= '{updatedAtEnd:yyyy-MM-dd HH:mm:ss}'");
                }


                var whereSql = whereClauses.Any()
                    ? $"WHERE {string.Join(" AND ", whereClauses)}"
                    : "";

                var sql = @"
                    ;WITH df_original_spec AS (
                        SELECT
                        tko.type_id,
                        tko.tool_id,
                        tko.size_ref_id,
                        trs.axle_type_id,
                        trs.overall_a,
                        trs.overall_b,
                        trs.overall_c,
                        trs.tolerance_a,
                        trs.tolerance_b,
                        trs.tolerance_c,
                        trs.f_shank_min,
                        trs.f_shank_max,
                        trs.h2h_min,
                        trs.h2h_max,
                        trs.chassis_span,
                        trs.chassis_span1,
                        trs.chassis_span2,
                        trs.b2b_min,
                        trs.b2b_max,
                        tko.knurling_type,
                        trs.source,
                        trs.is_original_spec,

                		u1.employee_id AS create_by,
                		trs.create_at,
                		u2.employee_id AS update_by,
                		trs.update_at,
                        trs.description,

						tka.pic_before_hst,
						tka.pic_before_hst_file_name,

                        tka.tool_key_id,
                        trs.tool_ref_spec_id,
                        tko.tool_key_id as tool_key_id_original

                        FROM toolKeyOriginals tko
                        JOIN toolRefSpecs trs ON tko.tool_key_id = trs.tool_key_id
                        LEFT JOIN toolKeyAlls tka
                            ON	tka.type_id     =	tko.type_id
                            AND tka.tool_id     =	tko.tool_id
                            AND tka.size_ref_id =	tko.size_ref_id
                            AND tka.source_original_key_id = tko.tool_key_id
                        LEFT JOIN users u1 ON trs.create_by = u1.user_id         -- ✅ สำหรับ created_by
                		LEFT JOIN users u2 ON trs.update_by = u2.user_id         -- ✅ สำหรับ updated_by
                    )

                    , df_original_spec_named AS (
                        SELECT
                            tm.type_name     AS tool_type,
                            t.tool_name      AS tool_name,
                            sr.size_ref      AS size_ref,
                            a.axle_type		 AS axle_type,
                            dfs.overall_a,
                            dfs.overall_b,
                            dfs.overall_c,
                            dfs.tolerance_a,
                            dfs.tolerance_b,
                            dfs.tolerance_c,
                            dfs.f_shank_min,
                            dfs.f_shank_max,
                            dfs.h2h_min,
                            dfs.h2h_max,
                            dfs.b2b_min,
                            dfs.b2b_max,
                            dfs.chassis_span,
                            dfs.chassis_span1,
                            dfs.chassis_span2,
                            dfs.knurling_type,
                            dfs.source,
                            dfs.is_original_spec,
                            dfs.tool_key_id
                        FROM df_original_spec dfs
                        LEFT JOIN typeModels tm     ON dfs.type_id       = tm.type_id
                        LEFT JOIN tools t           ON dfs.tool_id       = t.tool_id
                        LEFT JOIN sizeRefs sr       ON dfs.size_ref_id   = sr.size_ref_id
                        LEFT JOIN axleTypes a		ON dfs.axle_type_id	 = a.axle_type_id
                    )
                    , df_pad_encode AS (
                	SELECT 
                		tka.type_id,
                		tka.tool_id,
                		tka.type_ref_id,
                		tka.tool_ref_id,
                		tka.size_ref_id,
                		tpm.hst_type_id,
                		tpm.pad_id,
                		p.pad_name,
                		ISNULL(pbagg.brass_no, '') AS brass_no
                	FROM toolPadMap tpm
                	JOIN toolKeyAlls tka ON tpm.tool_key_id = tka.tool_key_id
                	JOIN pads p ON tpm.pad_id = p.pad_id
                	LEFT JOIN (
                		SELECT 
                			pb.pad_id,
                			STUFF((
                				SELECT ',' + b2.brass_no
                				FROM padBrassMap pb2
                				JOIN brasses b2 ON b2.brass_id = pb2.brass_id
                				WHERE pb2.pad_id = pb.pad_id
                				FOR XML PATH('')
                			), 1, 1, '') AS brass_no
                		FROM padBrassMap pb
                		GROUP BY pb.pad_id
                	) pbagg ON pbagg.pad_id = p.pad_id
                	GROUP BY 
                		tka.type_id, 
                		tka.tool_id, 
                		tka.type_ref_id, 
                		tka.tool_ref_id, 
                		tka.size_ref_id,
                		tpm.hst_type_id, 
                		tpm.pad_id, 
                		p.pad_name,
                		pbagg.brass_no
                	)
                    , df_pad_encode_3_key AS (
                	SELECT 
                		tka.type_id,
                		tka.tool_id,
                		tka.size_ref_id,
                		tpm.hst_type_id,
                		tpm.pad_id,
                		p.pad_name,
                		ISNULL(pbagg.brass_no, '') AS brass_no
                	FROM toolPadMap tpm
                	JOIN toolKeyAlls tka ON tpm.tool_key_id = tka.tool_key_id
                	JOIN pads p ON tpm.pad_id = p.pad_id
                	LEFT JOIN (
                		SELECT 
                			pb.pad_id,
                			STUFF((
                				SELECT ',' + b2.brass_no
                				FROM padBrassMap pb2
                				JOIN brasses b2 ON b2.brass_id = pb2.brass_id
                				WHERE pb2.pad_id = pb.pad_id
                				FOR XML PATH('')
                			), 1, 1, '') AS brass_no
                		FROM padBrassMap pb
                		GROUP BY pb.pad_id
                	) pbagg ON pbagg.pad_id = p.pad_id
                	GROUP BY 
                		tka.type_id, 
                		tka.tool_id, 
                		tka.size_ref_id,
                		tpm.hst_type_id, 
                		tpm.pad_id, 
                		p.pad_name,
                		pbagg.brass_no
                	)
                    , ranked_fallback AS (
                    SELECT *,
                        ROW_NUMBER() OVER (
                        PARTITION BY type_id, tool_id, size_ref_id
                        ORDER BY pad_id DESC
                        ) AS rn
                    FROM df_pad_encode_3_key
                    )

                    , pad_fallback_named AS (
                    SELECT 
                        rf.type_id,
                        rf.tool_id,
                        rf.size_ref_id,
                        rf.hst_type_id,
                        rf.pad_id,
                        rf.pad_name,
                        ht.hst_type,
                        rf.brass_no
                    FROM ranked_fallback rf
                    LEFT JOIN hstTypes ht ON rf.hst_type_id = ht.hst_type_id
                    )

                    , df_machine_encode AS (
                    SELECT
                        tka.type_id,
                        tka.tool_id,
                        tka.size_ref_id,
                        tka.tool_key_id,
                        m.machine_no
                        FROM toolKeyAlls tka
                    JOIN toolMachineMap tmm ON tmm.tool_key_id = tka.tool_key_id
                    LEFT JOIN machines m ON tmm.machine_id = m.machine_id
                    )

                    , machine_map_fallback AS (
                		SELECT
                			m.type_id,
                			m.tool_id,
                			m.size_ref_id,
                			STUFF((
                				SELECT ', ' + m2.machine_no
                				FROM df_machine_encode m2
                				WHERE
                					m2.type_id = m.type_id
                					AND m2.tool_id = m.tool_id
                					AND m2.size_ref_id = m.size_ref_id
                				ORDER BY
                					CASE 
                						WHEN ISNUMERIC(m2.machine_no) = 1 
                							THEN CAST(m2.machine_no AS INT)
                						ELSE NULL
                					END
                				FOR XML PATH('')
                			), 1, 2, '') AS machine_no
                		FROM df_machine_encode m
                		GROUP BY
                			m.type_id, m.tool_id, m.size_ref_id
                	)

                    , resolved_final AS (
                    SELECT
                        -- 🔑 Key for debugging
                        tm.type_name     AS tool_type,
                        t.tool_name      AS tool_name,
                        sr.size_ref      AS size_ref,

                        d.type_id,
                        d.tool_id,
                        d.size_ref_id,

                        -- 📐 Spec fields
                        d.axle_type_id,
                        d.overall_a,
                        d.overall_b,
                        d.overall_c,
                        d.tolerance_a,
                        d.tolerance_b,
                        d.tolerance_c,
                        d.f_shank_min,
                        d.f_shank_max,
                        d.h2h_min,
                        d.h2h_max,
                        d.chassis_span,
                        d.chassis_span1,
                        d.chassis_span2,
                        d.b2b_min,
                        d.b2b_max,
                        d.knurling_type,

                        d.create_by,
                		d.create_at,
                		d.update_by,
                		d.update_at,
                        d.description,

						d.pic_before_hst,
						d.pic_before_hst_file_name,

                        d.tool_key_id,
                        d.tool_ref_spec_id,
                        d.tool_key_id_original,
                        -- 🪛 Pad resolve
                        pf.pad_id           AS pad_id_lookup,
                        pf.hst_type_id		AS hst_type_id_lookup,
                        pf.pad_name         AS pad_name,
                        pf.hst_type         AS hst_type,
                        pf.brass_no         AS brass_no,

                        CASE 
                        WHEN pf.pad_id IS NOT NULL THEN 
                            CONCAT('(-, -, ', COALESCE(tm.type_name, '-'), ', ',
                                            COALESCE(t.tool_name, '-'), ', ',
                                            COALESCE(sr.size_ref, '-') ,')')
                        ELSE '(-, -, -, -, -)'
                        END AS pad_source_key,

                        -- ⚙️ Machine resolve
                        fm.machine_no AS machine_no,

                        CASE
                        WHEN fm.machine_no IS NOT NULL THEN 
                            CONCAT('(-, -, ', COALESCE(tm.type_name, '-'), ', ',
                                            COALESCE(t.tool_name, '-'), ', ',
                                            COALESCE(sr.size_ref, '-') ,')')
                        ELSE '(-, -, -, -, -)'
                        END AS machine_source_key,
                        d.source,
                        d.is_original_spec

                    FROM df_original_spec d


                    LEFT JOIN pad_fallback_named pf
                        ON d.type_id = pf.type_id
                    AND d.tool_id = pf.tool_id
                    AND d.size_ref_id = pf.size_ref_id

                    LEFT JOIN machine_map_fallback fm
                        ON d.type_id = fm.type_id
                    AND d.tool_id = fm.tool_id
                    AND d.size_ref_id = fm.size_ref_id

                    -- ✅ Lookup
                    LEFT JOIN typeModels tm     ON d.type_id     = tm.type_id
                    LEFT JOIN tools t           ON d.tool_id     = t.tool_id
                    LEFT JOIN sizeRefs sr       ON d.size_ref_id = sr.size_ref_id
                    )


                    , pad_pivot AS (
                    SELECT
                        tool_key_id,

                        MAX(CASE WHEN hst_type = 'HST' THEN pad_name END)       AS [HST_pad],
                        MAX(CASE WHEN hst_type = 'RIM' THEN pad_name END)       AS [RIM_pad],
                        MAX(CASE WHEN hst_type = 'INNER' THEN pad_name END)     AS [INNER_pad],
                        MAX(CASE WHEN hst_type = 'EXTRA_RIM' THEN pad_name END) AS [EXTRA_RIM_pad]

                    FROM resolved_final
                    GROUP BY tool_key_id
                    )

                    , pad_brass_map AS (
                        SELECT
                        p.pad_name,
                        ht.hst_type,
                        STUFF((
                            SELECT ', ' + b2.brass_no
                            FROM padBrassMap pb2
                            JOIN brasses b2 ON b2.brass_id = pb2.brass_id
                            WHERE pb2.pad_id = p.pad_id
                            ORDER BY TRY_CAST(b2.brass_no AS INT)  -- ✅ sort ตามตัวเลขจริง
                            FOR XML PATH('')
                        ), 1, 2, '') AS brass_no
                        FROM pads p
                        JOIN padHstMap phm ON p.pad_id = phm.pad_id
                        JOIN hstTypes ht ON phm.hst_type_id = ht.hst_type_id
                    )
                    ,

                    -- ✅ lookup brass โดยใช้ pad_name + hst_type จาก resolved_final
                    brass_pivot AS (
                        SELECT
                        rf.tool_key_id,
                        MAX(CASE WHEN pbm.hst_type = 'HST' THEN pbm.brass_no END)       AS HST_brass,
                        MAX(CASE WHEN pbm.hst_type = 'RIM' THEN pbm.brass_no END)       AS RIM_brass,
                        MAX(CASE WHEN pbm.hst_type = 'INNER' THEN pbm.brass_no END)     AS INNER_brass,
                        MAX(CASE WHEN pbm.hst_type = 'EXTRA_RIM' THEN pbm.brass_no END) AS EXTRA_RIM_brass
                        FROM resolved_final rf
                        LEFT JOIN pad_brass_map pbm
                        ON pbm.pad_name = rf.pad_name
                        AND pbm.hst_type = rf.hst_type
                        GROUP BY rf.tool_key_id
                    )
                    , df_spec_original_final_ranked AS (
                        SELECT
                        rf.tool_type,
                        rf.tool_name,
                        rf.size_ref,
                        at.axle_type,

                        rf.overall_a,
                        rf.overall_b,
                        rf.overall_c,
                        rf.tolerance_a,
                        rf.tolerance_b,
                        rf.tolerance_c,
                        rf.f_shank_min,
                        rf.f_shank_max,
                        rf.chassis_span,
                        rf.chassis_span1,
                        rf.chassis_span2,
                        rf.b2b_min,
                        rf.b2b_max,
                        rf.h2h_min,
                        rf.h2h_max,
                        rf.knurling_type,
                        rf.tool_key_id,
                        rf.tool_ref_spec_id,
                        rf.tool_key_id_original,

                        pp.HST_pad as 'HST_pad',
                        pp.RIM_pad as 'RIM_pad',
                        pp.INNER_pad as 'INNER_pad',
                        pp.EXTRA_RIM_pad as 'EXTRA_RIM_pad',

                        bp.HST_brass as 'HST_brass',
                        bp.RIM_brass as 'RIM_brass',
                        bp.INNER_brass as 'INNER_brass',
                        bp.EXTRA_RIM_brass as 'EXTRA_RIM_brass',

                        rf.pad_source_key,
                        rf.machine_no,
                        rf.machine_source_key,
                        rf.source,
                        rf.is_original_spec,

                        rf.create_by,
                		rf.create_at,
                		rf.update_by,
                		rf.update_at,
                        rf.description,

						rf.pic_before_hst_file_name,
                        CAST(NULL AS NVARCHAR(2048)) AS image_url,  -- ⬅⬅⬅ เพิ่มคอลัมน์ placeholder


                        ROW_NUMBER() OVER (
                            PARTITION BY rf.tool_type, rf.tool_name, rf.size_ref, at.axle_type
                            ORDER BY rf.tool_key_id  -- ✅ จะเลือกแถวแรกสุดของแต่ละกลุ่ม
                        ) AS rn

                        FROM resolved_final rf
                        LEFT JOIN axleTypes at     ON rf.axle_type_id = at.axle_type_id
                        LEFT JOIN pad_pivot pp     ON pp.tool_key_id = rf.tool_key_id
                        LEFT JOIN brass_pivot bp   ON bp.tool_key_id = rf.tool_key_id
                    ),

                    df_spec_original_final AS (
                		SELECT 
                			s.*,
                			r.request_type AS pending_request
                		FROM df_spec_original_final_ranked s
                		LEFT JOIN requests r
                			ON r.target_table = 'OriginalSpec'
                			AND r.target_pk_id = s.tool_ref_spec_id  -- 👈 เปลี่ยนเป็น primary key ของข้อมูลใน spec
                			AND r.request_status = 'Pending'     -- ✅ ตรวจสอบว่า request ยังไม่อนุมัติ
                			AND r.request_type IN ('UPDATE', 'DELETE')
                		WHERE s.rn = 1
                	)

                    SELECT * from df_spec_original_final
                    ";
                // var sql = $@"
                //     SELECT *
                //     FROM ResolvedOriginalCache
                // ";

                // ✅ ต่อ WHERE แค่ตรงนี้เท่านั้น
                sql += "\n" + whereSql + @"
                ORDER BY 
                    tool_type, 
                    tool_name, 
                    size_ref, 
                    axle_type;";

                FormattableString formattable = FormattableStringFactory.Create($"{sql}", Array.Empty<object>());

                var result = await _context.Database
                    .SqlQuery<ResolvedOriginalDto>(formattable)
                    .ToListAsync();

                // เติม URL ให้แต่ละแถว (ถ้าไม่มีไฟล์ ชอบจะให้เป็น null ก็เช็คชื่อไฟล์ก่อน)
                foreach (var x in result)
                {
                    // ถ้าอยากให้มี URL เฉพาะตอนมีไฟล์:
                    // var hasFile = !string.IsNullOrWhiteSpace(x.pic_before_hst_file_name);

                    x.image_url = x.tool_key_id > 0
                        ? Url.Action(
                            "GetBeforeHstImage",   // ชื่อ action ใน ToolKeyAllsController
                            "ToolKeyAlls",         // ชื่อ controller ตาม [Route("api/[controller]")] => "ToolKeyAlls"
                            new { id = x.tool_key_id },
                            Request.Scheme)
                        : null;

                    // ถ้า DTO ของมึงยังมี byte[] pic_before_hst อยู่ และไม่อยากส่ง binary ออก API:
                    // x.pic_before_hst = null;
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}