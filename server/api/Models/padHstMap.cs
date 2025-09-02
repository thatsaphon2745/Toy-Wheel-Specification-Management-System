using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace api.Models;

public partial class PadHstMap
{
    public int pad_hst_id { get; set; }

    public int? pad_id { get; set; }

    public int? hst_type_id { get; set; }

    // 🔥 Audit Fields
    public int create_by { get; set; }

    public DateTime create_at { get; set; }

    public int? update_by { get; set; }

    public DateTime? update_at { get; set; }

    public string? description { get; set; }

    // pending field
    [NotMapped]
    public string? pending_request { get; set; }

    // 🔥 Optional Navigation Properties
    public virtual User? CreateByUser { get; set; }

    public virtual User? UpdateByUser { get; set; }

    public virtual HstType? HstType { get; set; }

    public virtual Pad? Pad { get; set; }
}
