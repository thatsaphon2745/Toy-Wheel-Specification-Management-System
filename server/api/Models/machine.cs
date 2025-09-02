using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Machine
{
    public int machine_id { get; set; }

    public string? machine_no { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual ICollection<ToolMachineMap> ToolMachineMaps { get; set; } = new List<ToolMachineMap>();
}
