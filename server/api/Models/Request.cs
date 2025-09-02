using System;
namespace api.Models;
public class Request
{
    public int request_id { get; set; }

    public string request_type { get; set; } = null!;

    public string request_status { get; set; } = null!;

    public string target_table { get; set; } = null!;

    public int? target_pk_id { get; set; }

    public string? old_data { get; set; }

    public string? new_data { get; set; }

    public int requested_by { get; set; }

    public DateTime requested_at { get; set; }

    public int? approved_by { get; set; }

    public DateTime? approved_at { get; set; }

    public string? note { get; set; }

    // ✅ Navigation Properties
    public User? RequestedByUser { get; set; }

    public User? ApprovedByUser { get; set; }
}
