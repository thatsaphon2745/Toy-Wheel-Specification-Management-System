using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace api.Models;

public partial class PadBrassMap
{
    public int pad_brass_id { get; set; }

    public int? pad_id { get; set; }

    public int? brass_id { get; set; }

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

    public virtual Brass? Brass { get; set; }

    public virtual Pad? Pad { get; set; }
}
