using System;
using System.Collections.Generic;

namespace api.Models;

public partial class SizeRef
{
    public int size_ref_id { get; set; }

    public string? size_ref { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<ToolKeyAll> ToolKeyAlls { get; set; } = new List<ToolKeyAll>();

    public virtual ICollection<ToolKeyOriginal> ToolKeyOriginals { get; set; } = new List<ToolKeyOriginal>();
}
