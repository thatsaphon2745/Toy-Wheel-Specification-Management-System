using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;
using System.Runtime.CompilerServices;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolvedToolSpecsRawController : ControllerBase
    {
        private readonly MbkBarbell9Context _context;

        public ResolvedToolSpecsRawController(MbkBarbell9Context context)
        {
            _context = context;
        }

        [HttpGet("raw")]
        public async Task<ActionResult<IEnumerable<ResolvedSpecDto>>> GetResolvedSpecsRaw()
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


            // 🔍 Mapping field → column name
            if (filters.TryGetValue("tool_type", out var toolTypes))
                whereClauses.Add(BuildClause("tool_type", toolTypes.ToArray()));

            if (filters.TryGetValue("tool_name", out var toolNames))
                whereClauses.Add(BuildClause("tool_name", toolNames.ToArray()));

            if (filters.TryGetValue("position_type", out var posTypes))
                whereClauses.Add(BuildClause("position_type", posTypes.ToArray()));

            if (filters.TryGetValue("type_ref", out var typeRefs))
                whereClauses.Add(BuildClause("type_ref", typeRefs.ToArray()));

            if (filters.TryGetValue("tool_ref", out var toolRefs))
                whereClauses.Add(BuildClause("tool_ref", toolRefs.ToArray()));

            if (filters.TryGetValue("size_ref", out var sizeRefs))
                whereClauses.Add(BuildClause("size_ref", sizeRefs.ToArray()));

            if (filters.TryGetValue("axle_type", out var axleTypes))
                whereClauses.Add(BuildClause("axle_type", axleTypes.ToArray()));

            if (filters.TryGetValue("machine_no", out var machineNos))
                whereClauses.Add(BuildClause("machine_no", machineNos.ToArray()));


            // ✅ Pad columns
            if (filters.TryGetValue("HST_pad", out var hstPads))
                whereClauses.Add(BuildClause("HST_pad", hstPads.ToArray()));

            if (filters.TryGetValue("RIM_pad", out var rimPads))
                whereClauses.Add(BuildClause("RIM_pad", rimPads.ToArray()));

            if (filters.TryGetValue("INNER_pad", out var innerPads))
                whereClauses.Add(BuildClause("INNER_pad", innerPads.ToArray()));

            if (filters.TryGetValue("EXTRA_RIM_pad", out var extraRimPads))
                whereClauses.Add(BuildClause("EXTRA_RIM_pad", extraRimPads.ToArray()));

            // ✅ Brass columns
            if (filters.TryGetValue("HST_brass", out var hstBrass))
                whereClauses.Add(BuildClause("HST_brass", hstBrass.ToArray()));

            if (filters.TryGetValue("RIM_brass", out var rimBrass))
                whereClauses.Add(BuildClause("RIM_brass", rimBrass.ToArray()));

            if (filters.TryGetValue("INNER_brass", out var innerBrass))
                whereClauses.Add(BuildClause("INNER_brass", innerBrass.ToArray()));

            if (filters.TryGetValue("EXTRA_RIM_brass", out var extraRimBrass))
                whereClauses.Add(BuildClause("EXTRA_RIM_brass", extraRimBrass.ToArray()));


            // pad_source
            // if (filters.TryGetValue("pad_source", out var padSource))
            //     whereClauses.Add(BuildClause("pad_source", padSource.ToString().Split(',')));

            // pad_source
            if (filters.TryGetValue("pad_source", out var padSource))
                whereClauses.Add(BuildClause("pad_source", padSource.ToArray()));

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

            // machine_source
            // if (filters.TryGetValue("machine_source", out var machineSource))
            //     whereClauses.Add(BuildClause("machine_source", machineSource.ToString().Split(',')));

            // machine_source
            if (filters.TryGetValue("machine_source", out var machineSource))
                whereClauses.Add(BuildClause("machine_source", machineSource.ToArray()));

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
            // numeric field
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

            if (filters.TryGetValue("chassis_span", out var chassisSpan))
                whereClauses.Add(BuildClause("chassis_span", chassisSpan.ToArray()));

            if (filters.TryGetValue("knurling_type", out var knurlingType))
                whereClauses.Add(BuildClause("knurling_type", knurlingType.ToArray()));

            // over all dimension + tolerance
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

            // pending request
            if (filters.TryGetValue("pending_request", out var pendingRequest))
                whereClauses.Add(BuildClause("pending_request", pendingRequest.ToArray()));

            if (filters.TryGetValue("create_by", out var createBy))
                whereClauses.Add(BuildClause("create_by", createBy.ToArray()));
            // if (filters.TryGetValue("create_at", out var createAt))
            //     whereClauses.Add(BuildClause("create_at", createAt.ToArray()));

            if (filters.TryGetValue("update_by", out var updateBy))
                whereClauses.Add(BuildClause("update_by", updateBy.ToArray()));

            if (filters.TryGetValue("description", out var description))
                whereClauses.Add(BuildClause("description", description.ToArray()));
            // if (filters.TryGetValue("update_at", out var updateAt))
            //     whereClauses.Add(BuildClause("update_at", updateAt.ToArray()));

            var query = HttpContext.Request.Query;

            if (query.TryGetValue("created_at_start", out var createdAtStart) && !string.IsNullOrWhiteSpace(createdAtStart))
            {
                whereClauses.Add($"create_at >= '{createdAtStart}'");
            }

            if (query.TryGetValue("created_at_end", out var createdAtEnd) && !string.IsNullOrWhiteSpace(createdAtEnd))
            {
                // ✅ ใช้เวลาให้สุดวัน (End-of-day)
                whereClauses.Add($"create_at <= '{createdAtEnd} 23:59:59.997'");
            }

            if (query.TryGetValue("updated_at_start", out var updatedAtStart) && !string.IsNullOrWhiteSpace(updatedAtStart))
            {
                whereClauses.Add($"update_at >= '{updatedAtStart}'");
            }

            if (query.TryGetValue("updated_at_end", out var updatedAtEnd) && !string.IsNullOrWhiteSpace(updatedAtEnd))
            {
                whereClauses.Add($"update_at <= '{updatedAtEnd} 23:59:59.997'");
            }


            var whereSql = whereClauses.Any() ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";

            var sql = @"
            ;WITH df_spec_final AS (
            SELECT
                tkr.type_id,
                tkr.tool_id,
                ts.position_type_id,
                tko.type_id AS type_ref_id,
                tko.tool_id AS tool_ref_id,
                tko.size_ref_id,
                ts.axle_type_id,
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
                COALESCE(ts.chassis_span_override, trs.chassis_span) AS chassis_span,
                trs.b2b_min,
                trs.b2b_max,
                tkr.knurling_type,
                tka.tool_key_id,
                ts.tool_spec_id,
                tkr.ref_key_id,

                u1.employee_id AS create_by,
				ts.create_at,
				u2.employee_id AS update_by,
				ts.update_at,
                ts.description

            FROM toolSpecs ts
            JOIN toolKeyReferences tkr ON ts.ref_key_id = tkr.ref_key_id
            JOIN toolKeyOriginals tko ON tkr.tool_key_id = tko.tool_key_id
            JOIN toolRefSpecs trs ON ts.tool_ref_spec_id = trs.tool_ref_spec_id
            LEFT JOIN toolKeyAlls tka
                ON tka.type_id      = tkr.type_id
                AND tka.tool_id     = tkr.tool_id 
                AND tka.type_ref_id = tko.type_id
                AND tka.tool_ref_id =  tko.tool_id
                AND tka.size_ref_id = tko.size_ref_id
            LEFT JOIN users u1 ON ts.create_by = u1.user_id         -- ✅ สำหรับ created_by
		    LEFT JOIN users u2 ON ts.update_by = u2.user_id         -- ✅ สำหรับ updated_by
            ),

            df_spec_named AS (
            SELECT
                tm.type_name     AS tool_type,
                t.tool_name      AS tool_name,
                tm_ref.type_name AS type_ref,
                t_ref.tool_name  AS tool_ref,
                sr.size_ref      AS size_ref,

                b.position_type_id,
                b.axle_type_id,
                b.overall_a,
                b.overall_b,
                b.overall_c,
                b.tolerance_a,
                b.tolerance_b,
                b.tolerance_c,
                b.f_shank_min,
                b.f_shank_max,
                b.h2h_min,
                b.h2h_max,
                b.chassis_span,
                b.b2b_min,
                b.b2b_max,
                b.knurling_type,
                tka.tool_key_id
            FROM df_spec_final b
            LEFT JOIN typeModels tm     ON b.type_id       = tm.type_id
            LEFT JOIN tools t           ON b.tool_id       = t.tool_id
            LEFT JOIN typeModels tm_ref ON b.type_ref_id   = tm_ref.type_id
            LEFT JOIN tools t_ref       ON b.tool_ref_id   = t_ref.tool_id
            LEFT JOIN sizeRefs sr       ON b.size_ref_id   = sr.size_ref_id
            LEFT JOIN toolKeyAlls tka
                ON tka.type_id      = tm.type_id
                AND tka.tool_id     = t.tool_id 
                AND tka.type_ref_id = tm_ref.type_id
                AND tka.tool_ref_id =  t_ref.tool_id
                AND tka.size_ref_id = sr.size_ref_id
            ),

            df_pad_encode AS (
				SELECT 
					tka.type_id,
					tka.tool_id,
					tka.type_ref_id,
					tka.tool_ref_id,
					tka.size_ref_id,
					tpm.hst_type_id,
					tpm.pad_id,
					p.pad_name,
					ht.hst_type,
					STUFF((
						SELECT ', ' + b2.brass_no
						FROM padBrassMap pb2
						JOIN brasses b2 ON b2.brass_id = pb2.brass_id
						WHERE pb2.pad_id = tpm.pad_id
						ORDER BY TRY_CAST(b2.brass_no AS INT)
						FOR XML PATH('')
					), 1, 2, '') AS brass_no
				FROM toolPadMap tpm
				LEFT JOIN toolKeyAlls tka ON tpm.tool_key_id = tka.tool_key_id
				JOIN pads p ON tpm.pad_id = p.pad_id
				JOIN hstTypes ht ON tpm.hst_type_id = ht.hst_type_id
				LEFT JOIN padBrassMap pb ON pb.pad_id = p.pad_id
				LEFT JOIN brasses b ON pb.brass_id = b.brass_id
				GROUP BY 
					tka.type_id, tka.tool_id, tka.type_ref_id, tka.tool_ref_id, tka.size_ref_id,
					tpm.hst_type_id, ht.hst_type, tpm.pad_id, p.pad_name
			)
			,
            ranked_direct AS (
            SELECT *,
                ROW_NUMBER() OVER (
                PARTITION BY type_id, tool_id, type_ref_id, tool_ref_id, size_ref_id
                ORDER BY pad_id ASC
                ) AS rn
            FROM df_pad_encode
            WHERE type_ref_id IS NOT NULL AND tool_ref_id IS NOT NULL AND size_ref_id IS NOT NULL
            ),
            pad_direct_named AS (
            SELECT 
                rd.type_id,
                rd.tool_id,
                rd.type_ref_id,
                rd.tool_ref_id,
                rd.size_ref_id,
                rd.pad_id,
                rd.hst_type_id,
                rd.pad_name,
                ht.hst_type,
                rd.brass_no
            FROM ranked_direct rd
            LEFT JOIN hstTypes ht ON rd.hst_type_id = ht.hst_type_id
            --WHERE rd.rn = 1
            ),

            df_pad_encode_3_key AS (
				SELECT 
					tka.type_id,
					tka.tool_id,
					tka.size_ref_id,
					tpm.hst_type_id,
					tpm.pad_id,
					p.pad_name,
					STUFF((
						SELECT ', ' + b2.brass_no
						FROM padBrassMap pb2
						JOIN brasses b2 ON b2.brass_id = pb2.brass_id
						WHERE pb2.pad_id = tpm.pad_id
						ORDER BY TRY_CAST(b2.brass_no AS INT)
						FOR XML PATH('')
					), 1, 2, '') AS brass_no
				FROM toolPadMap tpm
				JOIN toolKeyAlls tka ON tpm.tool_key_id = tka.tool_key_id
				JOIN pads p ON tpm.pad_id = p.pad_id
				LEFT JOIN padBrassMap pb ON pb.pad_id = p.pad_id
				LEFT JOIN brasses b ON pb.brass_id = b.brass_id
				GROUP BY 
					tka.type_id, tka.tool_id, tka.size_ref_id,
					tpm.hst_type_id, tpm.pad_id, p.pad_name
			)
			,
            ranked_fallback AS (
            SELECT *,
                ROW_NUMBER() OVER (
                PARTITION BY type_id, tool_id, size_ref_id
                ORDER BY pad_id DESC
                ) AS rn
            FROM df_pad_encode_3_key
            ),
            pad_fallback_named AS (
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
            ),

            resolved_pad AS (
            SELECT
                tm.type_name     AS tool_type,
                t.tool_name      AS tool_name,
                tm_ref.type_name AS type_ref,
                t_ref.tool_name  AS tool_ref,
                sr.size_ref      AS size_ref,

                d.position_type_id,
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
                d.b2b_min,
                d.b2b_max,
                d.knurling_type,
                d.tool_key_id,

                -- ใช้ COALESCE เลือกค่าจาก direct → fallback → null
                COALESCE(pd.pad_id, pf.pad_id)             AS pad_id_lookup,
                COALESCE(pd.hst_type_id, pf.hst_type_id)   AS hst_type_id_lookup,
                COALESCE(pd.pad_name, pf.pad_name)         AS pad_name,
                COALESCE(pd.hst_type, pf.hst_type)         AS hst_type,
                COALESCE(pd.brass_no, pf.brass_no)         AS brass_no,

                -- ระบุ source ว่ามาจากไหน
                CASE 
                WHEN pd.pad_id IS NOT NULL THEN 'direct'
                WHEN pf.pad_id IS NOT NULL THEN 'fallback'
                ELSE 'missing'
                END AS pad_source,

                -- แสดง key ที่ใช้สำหรับ trace
                CASE 
                WHEN pd.pad_id IS NOT NULL THEN 
                    CONCAT('(',
                    COALESCE(tm.type_name, '-'), ', ',
                    COALESCE(t.tool_name, '-'), ', ',
                    COALESCE(tm_ref.type_name, '-'), ', ',
                    COALESCE(t_ref.tool_name, '-'), ', ',
                    COALESCE(sr.size_ref, '-'), ')')

                WHEN pf.pad_id IS NOT NULL THEN 
                    CONCAT('(',
                    '-', ', ',
                    '-', ', ',
                    COALESCE(tm_ref.type_name, '-'), ', ',
                    COALESCE(t_ref.tool_name, '-'), ', ',
                    COALESCE(sr.size_ref, '-'), ')')

                ELSE 
                    '(-, -, -, -, -)'
                END AS pad_source_key

            FROM df_spec_final d

            LEFT JOIN pad_direct_named pd
                ON d.type_id       = pd.type_id
            AND d.tool_id       = pd.tool_id
            -- จาก code ไม่รู้ว่า tool_ref_id สลับ กับ type_ref_id จากใน df_spec_final สลับกันได้ยังไง งง?
            AND d.type_ref_id   = pd.type_ref_id
            AND d.tool_ref_id   = pd.tool_ref_id
            AND d.size_ref_id   = pd.size_ref_id

            LEFT JOIN pad_fallback_named pf
                ON d.type_ref_id = pf.type_id
            AND d.tool_ref_id = pf.tool_id
            AND d.size_ref_id = pf.size_ref_id

            LEFT JOIN typeModels tm     ON d.type_id       = tm.type_id
            LEFT JOIN tools t           ON d.tool_id       = t.tool_id
            LEFT JOIN typeModels tm_ref ON d.type_ref_id   = tm_ref.type_id
            LEFT JOIN tools t_ref       ON d.tool_ref_id   = t_ref.tool_id
            LEFT JOIN sizeRefs sr       ON d.size_ref_id   = sr.size_ref_id
            ),

            df_machine_encode AS (
            SELECT
                tka.type_id,
                tka.tool_id,
                tka.type_ref_id,
                tka.tool_ref_id,
                tka.size_ref_id,
                tka.tool_key_id,
                m.machine_no
                FROM toolKeyAlls tka
            JOIN toolMachineMap tmm ON tmm.tool_key_id = tka.tool_key_id
            LEFT JOIN machines m ON tmm.machine_id = m.machine_id
            WHERE tka.type_id IS NOT NULL 
                AND tka.tool_id IS NOT NULL
                AND tka.type_ref_id IS NOT NULL 
                AND tka.tool_ref_id IS NOT NULL
                AND tka.size_ref_id IS NOT NULL
            ),
            machine_map_direct AS (
				SELECT
					m.type_id,
					m.tool_id,
					m.type_ref_id,
					m.tool_ref_id,
					m.size_ref_id,
					STUFF((
						SELECT ', ' + m2.machine_no
						FROM df_machine_encode m2
						WHERE
							m2.type_id = m.type_id
							AND m2.tool_id = m.tool_id
							AND m2.type_ref_id = m.type_ref_id
							AND m2.tool_ref_id = m.tool_ref_id
							AND m2.size_ref_id = m.size_ref_id
						ORDER BY TRY_CAST(m2.machine_no AS INT)
						FOR XML PATH('')
					), 1, 2, '') AS machine_no
				FROM df_machine_encode m
				GROUP BY
					m.type_id, m.tool_id, m.type_ref_id, m.tool_ref_id, m.size_ref_id
			)
			,

            machine_map_fallback AS (
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
						ORDER BY TRY_CAST(m2.machine_no AS INT)
						FOR XML PATH('')
					), 1, 2, '') AS machine_no
				FROM df_machine_encode m
				GROUP BY
					m.type_id, m.tool_id, m.size_ref_id
			)
			,
            resolved_machine AS (
            SELECT
                tm.type_name     AS tool_type,
                t.tool_name      AS tool_name,
                tm_ref.type_name AS type_ref,
                t_ref.tool_name  AS tool_ref,
                sr.size_ref      AS size_ref,

                d.position_type_id,
                d.axle_type_id,

                -- ใช้ COALESCE เลือก machine_no จาก direct → fallback → null
                COALESCE(dm.machine_no, fm.machine_no) AS machine_no,

                -- ระบุ source ของ machine
                CASE
                WHEN dm.machine_no IS NOT NULL THEN 'direct'
                WHEN fm.machine_no IS NOT NULL THEN 'fallback'
                ELSE 'missing'
                END AS machine_source,

                -- แสดง key ที่ใช้สำหรับ trace
                CASE
                WHEN dm.machine_no IS NOT NULL THEN 
                    CONCAT('(',
                    COALESCE(tm.type_name, '-'), ', ',
                    COALESCE(t.tool_name, '-'), ', ',
                    COALESCE(tm_ref.type_name, '-'), ', ',
                    COALESCE(t_ref.tool_name, '-'), ', ',
                    COALESCE(sr.size_ref, '-') ,')')
                WHEN fm.machine_no IS NOT NULL THEN 
                    CONCAT('(-, -, ',
                    COALESCE(tm_ref.type_name, '-'), ', ',
                    COALESCE(t_ref.tool_name, '-'), ', ',
                    COALESCE(sr.size_ref, '-') ,')')
                ELSE
                    '(-, -, -, -, -)'
                END AS machine_source_key

            FROM df_spec_final d

            LEFT JOIN machine_map_direct dm
                ON d.type_id       = dm.type_id
            AND d.tool_id       = dm.tool_id
            AND d.type_ref_id   = dm.type_ref_id
            AND d.tool_ref_id   = dm.tool_ref_id
            AND d.size_ref_id   = dm.size_ref_id

            LEFT JOIN machine_map_fallback fm
                ON d.type_ref_id = fm.type_id
            AND d.tool_ref_id = fm.tool_id
            AND d.size_ref_id = fm.size_ref_id

            LEFT JOIN typeModels tm     ON d.type_id       = tm.type_id
            LEFT JOIN tools t           ON d.tool_id       = t.tool_id
            LEFT JOIN typeModels tm_ref ON d.type_ref_id   = tm_ref.type_id
            LEFT JOIN tools t_ref       ON d.tool_ref_id   = t_ref.tool_id
            LEFT JOIN sizeRefs sr       ON d.size_ref_id   = sr.size_ref_id
            ),
            resolved_final AS (
            SELECT
                -- 🔑 Key for debugging
                tm.type_name     AS tool_type,
                t.tool_name      AS tool_name,
                tm_ref.type_name AS type_ref,
                t_ref.tool_name  AS tool_ref,
                sr.size_ref      AS size_ref,

                d.type_id,
                d.tool_id,
                d.type_ref_id,
                d.tool_ref_id,
                d.size_ref_id,

                -- 📐 Spec fields
                d.position_type_id,
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
                d.b2b_min,
                d.b2b_max,
                d.knurling_type,
                d.create_by,
				d.create_at,
				d.update_by,
				d.update_at,
                d.description,

                d.tool_key_id,
                d.tool_spec_id,
                d.ref_key_id,
                -- 🪛 Pad resolve
                COALESCE(pd.pad_id, pf.pad_id)             AS pad_id_lookup,
                COALESCE(pd.hst_type_id, pf.hst_type_id)   AS hst_type_id_lookup,
                COALESCE(pd.pad_name, pf.pad_name)         AS pad_name,
                COALESCE(pd.hst_type, pf.hst_type)         AS hst_type,
                COALESCE(pd.brass_no, pf.brass_no)         AS brass_no,

                CASE 
                WHEN pd.pad_id IS NOT NULL THEN 'direct'
                WHEN pf.pad_id IS NOT NULL THEN 'fallback'
                ELSE 'missing'
                END AS pad_source,

                CASE 
                WHEN pd.pad_id IS NOT NULL THEN 
                    CONCAT('(', COALESCE(tm.type_name, '-'), ', ',
                                COALESCE(t.tool_name, '-'), ', ',
                                COALESCE(tm_ref.type_name, '-'), ', ',
                                COALESCE(t_ref.tool_name, '-'), ', ',
                                COALESCE(sr.size_ref, '-') ,')')
                WHEN pf.pad_id IS NOT NULL THEN 
                    CONCAT('(-, -, ', COALESCE(tm_ref.type_name, '-'), ', ',
                                    COALESCE(t_ref.tool_name, '-'), ', ',
                                    COALESCE(sr.size_ref, '-') ,')')
                ELSE '(-, -, -, -, -)'
                END AS pad_source_key,

                -- ⚙️ Machine resolve
                COALESCE(dm.machine_no, fm.machine_no) AS machine_no,

                CASE 
                WHEN dm.machine_no IS NOT NULL THEN 'direct'
                WHEN fm.machine_no IS NOT NULL THEN 'fallback'
                ELSE 'missing'
                END AS machine_source,

                CASE
                WHEN dm.machine_no IS NOT NULL THEN 
                    CONCAT('(', COALESCE(tm.type_name, '-'), ', ',
                                COALESCE(t.tool_name, '-'), ', ',
                                COALESCE(tm_ref.type_name, '-'), ', ',
                                COALESCE(t_ref.tool_name, '-'), ', ',
                                COALESCE(sr.size_ref, '-') ,')')
                WHEN fm.machine_no IS NOT NULL THEN 
                    CONCAT('(-, -, ', COALESCE(tm_ref.type_name, '-'), ', ',
                                    COALESCE(t_ref.tool_name, '-'), ', ',
                                    COALESCE(sr.size_ref, '-') ,')')
                ELSE '(-, -, -, -, -)'
                END AS machine_source_key

            FROM df_spec_final d

            -- ✅ Pad joins
            LEFT JOIN pad_direct_named pd
                ON d.type_id       = pd.type_id
            AND d.tool_id       = pd.tool_id
            AND d.type_ref_id   = pd.type_ref_id
            AND d.tool_ref_id   = pd.tool_ref_id
            AND d.size_ref_id   = pd.size_ref_id

            LEFT JOIN pad_fallback_named pf
                ON d.type_ref_id = pf.type_id
            AND d.tool_ref_id = pf.tool_id
            AND d.size_ref_id = pf.size_ref_id

            -- ✅ Machine joins
            LEFT JOIN machine_map_direct dm
                ON d.type_id       = dm.type_id
            AND d.tool_id       = dm.tool_id
            AND d.type_ref_id   = dm.type_ref_id
            AND d.tool_ref_id   = dm.tool_ref_id
            AND d.size_ref_id   = dm.size_ref_id

            LEFT JOIN machine_map_fallback fm
                ON d.type_ref_id = fm.type_id
            AND d.tool_ref_id = fm.tool_id
            AND d.size_ref_id = fm.size_ref_id

            -- ✅ Lookup
            LEFT JOIN typeModels tm     ON d.type_id     = tm.type_id
            LEFT JOIN tools t           ON d.tool_id     = t.tool_id
            LEFT JOIN typeModels tm_ref ON d.type_ref_id = tm_ref.type_id
            LEFT JOIN tools t_ref       ON d.tool_ref_id = t_ref.tool_id
            LEFT JOIN sizeRefs sr       ON d.size_ref_id = sr.size_ref_id
            )

            -- ✅ Pivot pad_name by hst_type
            , pad_pivot AS (
            SELECT
                tool_key_id,

                MAX(CASE WHEN hst_type = 'HST' THEN pad_name END)       AS HST_pad,
                MAX(CASE WHEN hst_type = 'RIM' THEN pad_name END)       AS RIM_pad,
                MAX(CASE WHEN hst_type = 'INNER' THEN pad_name END)     AS INNER_pad,
                MAX(CASE WHEN hst_type = 'EXTRA_RIM' THEN pad_name END) AS EXTRA_RIM_pad

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
            , df_spec_final_clean_ranked AS (
                SELECT
                rf.tool_type,
                rf.tool_name,
                pt.position_type,
                rf.type_ref,
                rf.tool_ref,
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
                rf.b2b_min,
                rf.b2b_max,
                rf.h2h_min,
                rf.h2h_max,
                rf.knurling_type,
                rf.tool_key_id,
                rf.tool_spec_id,
                rf.ref_key_id,

                pp.HST_pad,
                pp.RIM_pad,
                pp.INNER_pad,
                pp.EXTRA_RIM_pad,

                bp.HST_brass,
                bp.RIM_brass,
                bp.INNER_brass,
                bp.EXTRA_RIM_brass,

                rf.pad_source,
                rf.pad_source_key,
                rf.machine_no,
                rf.machine_source,
                rf.machine_source_key,

                rf.create_by,
				rf.create_at,
				rf.update_by,
				rf.update_at,
                rf.description,

                ROW_NUMBER() OVER (
                    PARTITION BY 
                    rf.tool_type, rf.tool_name, pt.position_type,
                    rf.type_ref, rf.tool_ref, rf.size_ref, at.axle_type
                    ORDER BY rf.tool_key_id  -- หรือจะใช้ created_date, updated_date ก็ได้
                ) AS rn

                FROM resolved_final rf
                LEFT JOIN positionTypes pt ON rf.position_type_id = pt.position_type_id
                LEFT JOIN axleTypes at     ON rf.axle_type_id = at.axle_type_id
                LEFT JOIN pad_pivot pp     ON pp.tool_key_id = rf.tool_key_id
                LEFT JOIN brass_pivot bp   ON bp.tool_key_id = rf.tool_key_id
            )

            -- ✅ สุดท้าย: ดึงแค่ unique 1 แถวต่อ group
            , df_spec_final_clean AS (
			  SELECT 
				s.*,
				r.request_type AS pending_request
			  FROM df_spec_final_clean_ranked s
			  LEFT JOIN requests r
				ON r.target_table = 'DdcSpec'
				AND r.target_pk_id = s.tool_spec_id   -- ✅ primary key ของ DdcSpec
				AND r.request_status = 'Pending'
				AND r.request_type IN ('UPDATE', 'DELETE')
			  WHERE s.rn = 1
			)
            SELECT * FROM df_spec_final_clean
            ";

            // ✅ ต่อ WHERE แค่ตรงนี้เท่านั้น
            sql += "\n" + whereSql + @"
            ORDER BY 
                tool_type, 
                tool_name, 
                position_type, 
                type_ref, 
                tool_ref, 
                size_ref, 
                axle_type;";

            FormattableString formattable = FormattableStringFactory.Create($"{sql}", Array.Empty<object>());

            var result = await _context.Database
                .SqlQuery<ResolvedSpecDto>(formattable)
                .ToListAsync();

            return Ok(result);
        }
    }
}
