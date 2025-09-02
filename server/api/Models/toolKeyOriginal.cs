using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ToolKeyOriginal
{
    public int tool_key_id { get; set; }

    public int? tool_id { get; set; }

    public int? type_id { get; set; }

    public int? size_ref_id { get; set; }

    public int? knurling_type { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual SizeRef? SizeRef { get; set; }

    public virtual Tool? Tool { get; set; }

    public virtual ICollection<ToolKeyAll> ToolKeyAlls { get; set; } = new List<ToolKeyAll>();

    public virtual ICollection<ToolKeyReference> ToolKeyReferences { get; set; } = new List<ToolKeyReference>();

    public virtual ICollection<ToolRefSpec> ToolRefSpecs { get; set; } = new List<ToolRefSpec>();

    public virtual TypeModel? Type { get; set; }
}
