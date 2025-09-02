using System;
using System.Collections.Generic;

namespace api.Models;

public partial class HstType
{
    public int hst_type_id { get; set; }

    public string hst_type { get; set; } = null!;

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<PadHstMap> PadHstMaps { get; set; } = new List<PadHstMap>();

    public virtual ICollection<ToolPadMap> ToolPadMaps { get; set; } = new List<ToolPadMap>();
}
