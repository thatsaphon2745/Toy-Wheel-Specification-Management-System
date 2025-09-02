using System;
using System.Collections.Generic;

namespace api.Models;

public partial class AxleType
{
    public int axle_type_id { get; set; }

    public string? axle_type { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<ToolRefSpec> ToolRefSpecs { get; set; } = new List<ToolRefSpec>();

    public virtual ICollection<ToolSpec> ToolSpecs { get; set; } = new List<ToolSpec>();
}
