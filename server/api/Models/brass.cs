using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Brass
{
    public int brass_id { get; set; }

    public string? brass_no { get; set; }

    public string? description { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<PadBrassMap> PadBrassMaps { get; set; } = new List<PadBrassMap>();
}
