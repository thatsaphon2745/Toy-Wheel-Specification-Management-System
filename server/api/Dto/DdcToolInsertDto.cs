namespace api.DTOs
{
    public class DdcToolInsertDto
    {
        public int type_id { get; set; }
        public int tool_id { get; set; }
        public int position_type_id { get; set; }
        // public int? knurling_type { get; set; }
        public int type_ref_id { get; set; }
        public int tool_ref_id { get; set; }
        public int size_ref_id { get; set; }
        public int axle_type_id { get; set; }
        public int knurling_type { get; set; }
        public double? chassis_span_override { get; set; }
        public string? description { get; set; }
    }
}
