using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ToolSpec
{
    public int tool_spec_id { get; set; }

    public int? ref_key_id { get; set; }

    public int? tool_ref_spec_id { get; set; }

    public int? position_type_id { get; set; }

    public int? axle_type_id { get; set; }

    public double? chassis_span_override { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    public string? description { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual AxleType? AxleType { get; set; }

    public virtual PositionType? PositionType { get; set; }

    public virtual ToolKeyReference? RefKey { get; set; }

    public virtual ToolRefSpec? ToolRefSpec { get; set; }
}
