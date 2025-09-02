using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ToolKeyReference
{
    public int ref_key_id { get; set; }

    public int? tool_id { get; set; }

    public int? type_id { get; set; }

    public int? position_type_id { get; set; }

    public int? knurling_type { get; set; }

    public int? tool_key_id { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual PositionType? PositionType { get; set; }

    public virtual Tool? Tool { get; set; }

    public virtual ToolKeyOriginal? ToolKey { get; set; }

    public virtual ICollection<ToolKeyAll> ToolKeyAlls { get; set; } = new List<ToolKeyAll>();

    public virtual ICollection<ToolSpec> ToolSpecs { get; set; } = new List<ToolSpec>();

    public virtual TypeModel? Type { get; set; }
}
