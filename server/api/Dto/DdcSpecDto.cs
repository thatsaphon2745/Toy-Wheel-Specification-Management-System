public class DdcSpecDto
{
    public int tool_spec_id { get; set; }
    public int? ref_key_id { get; set; }
    public int? tool_ref_spec_id { get; set; }
    public int? position_type_id { get; set; }
    public int? axle_type_id { get; set; }
    public double? chassis_span_override { get; set; }

    // เพิ่ม field จาก ToolKeyAlls
    public int? type_id { get; set; }
    public int? tool_id { get; set; }
    public int? type_ref_id { get; set; }
    public int? tool_ref_id { get; set; }
    public int? size_ref_id { get; set; }

    // Audit
    public int create_by { get; set; }
    public DateTime create_at { get; set; }
    public int? update_by { get; set; }
    public DateTime? update_at { get; set; }

    public string? description { get; set; }
}
