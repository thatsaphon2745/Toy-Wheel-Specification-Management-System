using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Tool
{
    public int tool_id { get; set; }

    public string? tool_name { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<ToolKeyAll> ToolKeyAllToolRefs { get; set; } = new List<ToolKeyAll>();

    public virtual ICollection<ToolKeyAll> ToolKeyAllTools { get; set; } = new List<ToolKeyAll>();

    public virtual ICollection<ToolKeyOriginal> ToolKeyOriginals { get; set; } = new List<ToolKeyOriginal>();

    public virtual ICollection<ToolKeyReference> ToolKeyReferences { get; set; } = new List<ToolKeyReference>();
}
