using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class ResolvedSpecDto
    {
        [JsonPropertyName("tool_type")]
        public string? tool_type { get; set; }

        [JsonPropertyName("tool_name")]
        public string? tool_name { get; set; }

        [JsonPropertyName("position_type")]
        public string? position_type { get; set; }

        [JsonPropertyName("type_ref")]
        public string? type_ref { get; set; }

        [JsonPropertyName("tool_ref")]
        public string? tool_ref { get; set; }

        [JsonPropertyName("size_ref")]
        public string? size_ref { get; set; }

        [JsonPropertyName("axle_type")]
        public string? axle_type { get; set; }

        [JsonPropertyName("overall_a")]
        public double? overall_a { get; set; }

        [JsonPropertyName("overall_b")]
        public double? overall_b { get; set; }

        [JsonPropertyName("overall_c")]
        public double? overall_c { get; set; }

        [JsonPropertyName("tolerance_a")]
        public double? tolerance_a { get; set; }

        [JsonPropertyName("tolerance_b")]
        public double? tolerance_b { get; set; }

        [JsonPropertyName("tolerance_c")]
        public double? tolerance_c { get; set; }

        [JsonPropertyName("f_shank_min")]
        public double? f_shank_min { get; set; }

        [JsonPropertyName("f_shank_max")]
        public double? f_shank_max { get; set; }

        [JsonPropertyName("b2b_min")]
        public double? b2b_min { get; set; }

        [JsonPropertyName("b2b_max")]
        public double? b2b_max { get; set; }

        [JsonPropertyName("h2h_min")]
        public double? h2h_min { get; set; }

        [JsonPropertyName("h2h_max")]
        public double? h2h_max { get; set; }

        [JsonPropertyName("chassis_span")]
        public double? chassis_span { get; set; }

        [JsonPropertyName("knurling_type")]
        public int? knurling_type { get; set; }

        [JsonPropertyName("HST_pad")]
        public string? HST_pad { get; set; }

        [JsonPropertyName("RIM_pad")]
        public string? RIM_pad { get; set; }

        [JsonPropertyName("INNER_pad")]
        public string? INNER_pad { get; set; }

        [JsonPropertyName("EXTRA_RIM_pad")]
        public string? EXTRA_RIM_pad { get; set; }

        [JsonPropertyName("HST_brass")]
        public string? HST_brass { get; set; }

        [JsonPropertyName("RIM_brass")]
        public string? RIM_brass { get; set; }

        [JsonPropertyName("INNER_brass")]
        public string? INNER_brass { get; set; }

        [JsonPropertyName("EXTRA_RIM_brass")]
        public string? EXTRA_RIM_brass { get; set; }

        [JsonPropertyName("pad_source")]
        public string? pad_source { get; set; }

        [JsonPropertyName("pad_source_key")]
        public string? pad_source_key { get; set; }

        [JsonPropertyName("machine_no")]
        public string? machine_no { get; set; }

        [JsonPropertyName("machine_source")]
        public string? machine_source { get; set; }

        [JsonPropertyName("machine_source_key")]
        public string? machine_source_key { get; set; }

        [JsonPropertyName("pending_request")]
        public string? pending_request { get; set; }

        [JsonPropertyName("create_by")]
        public string create_by { get; set; }
        [JsonPropertyName("create_at")]
        public DateTime create_at { get; set; }
        [JsonPropertyName("update_by")]
        public string? update_by { get; set; }
        [JsonPropertyName("update_at")]
        public DateTime? update_at { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("ref_key_id")]
        public int? ref_key_id { get; set; }

        [JsonPropertyName("tool_key_id")]
        public int? tool_key_id { get; set; }

        [JsonPropertyName("tool_spec_id")]
        public int? tool_spec_id { get; set; }
    }
}
