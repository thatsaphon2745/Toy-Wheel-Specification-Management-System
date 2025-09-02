using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class ChassisSpanUpdateDto
    {
        [JsonPropertyName("chassis_span_override")]
        public double? chassis_span_override { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; } // ✅ แบบนี้ถูก
    }
}
