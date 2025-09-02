using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Pad
{
    public int pad_id { get; set; }

    public string? pad_name { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<PadBrassMap> PadBrassMaps { get; set; } = new List<PadBrassMap>();

    public virtual ICollection<PadHstMap> PadHstMaps { get; set; } = new List<PadHstMap>();

    public virtual ICollection<ToolPadMap> ToolPadMaps { get; set; } = new List<ToolPadMap>();
}
