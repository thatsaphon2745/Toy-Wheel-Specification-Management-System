using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ToolRefSpec
{
    public int tool_ref_spec_id { get; set; }

    public int? tool_key_id { get; set; }

    public int? axle_type_id { get; set; }

    public double? overall_a { get; set; }

    public double? overall_b { get; set; }

    public double? overall_c { get; set; }

    public double? tolerance_a { get; set; }

    public double? tolerance_b { get; set; }

    public double? tolerance_c { get; set; }

    public double? f_shank_min { get; set; }

    public double? f_shank_max { get; set; }

    public string? chassis_span { get; set; }

    public double? b2b_min { get; set; }

    public double? b2b_max { get; set; }

    public double? h2h_min { get; set; }

    public double? h2h_max { get; set; }

    public string? source { get; set; }

    public int? is_original_spec { get; set; }

    public int? knurling_type { get; set; }

    public double? chassis_span1 { get; set; }

    public double? chassis_span2 { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // public string? source { get; set; }
    public string? description { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual AxleType? AxleType { get; set; }

    public virtual ToolKeyOriginal? ToolKey { get; set; }

    public virtual ICollection<ToolSpec> ToolSpecs { get; set; } = new List<ToolSpec>();
}
