namespace api.DTOs
{
    public class OriginalSpecDto
    {
        public int tool_id { get; set; }
        public int type_id { get; set; }
        public int size_ref_id { get; set; }
        public int axle_type_id { get; set; }
        public int knurling_type { get; set; }
        public double? overall_a { get; set; }
        public double? overall_b { get; set; }
        public double? overall_c { get; set; }
        public double? tolerance_a { get; set; }
        public double? tolerance_b { get; set; }
        public double? tolerance_c { get; set; }
        public double? f_shank_min { get; set; }
        public double? f_shank_max { get; set; }
        public string? chassis_span { get; set; }
        public double? chassis_span1 { get; set; }
        public double? chassis_span2 { get; set; }
        public double? b2b_min { get; set; }
        public double? b2b_max { get; set; }
        public double? h2h_min { get; set; }
        public double? h2h_max { get; set; }
        public string? source { get; set; }
        public int is_original_spec { get; set; }

        public string? description { get; set; }
        
    }
}
