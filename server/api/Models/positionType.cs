using System;
using System.Collections.Generic;

namespace api.Models;

public partial class PositionType
{
    public int position_type_id { get; set; }

    public string position_type { get; set; } = null!;

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<ToolKeyReference> ToolKeyReferences { get; set; } = new List<ToolKeyReference>();

    public virtual ICollection<ToolSpec> ToolSpecs { get; set; } = new List<ToolSpec>();
}
