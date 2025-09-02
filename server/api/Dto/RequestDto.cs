using Newtonsoft.Json.Linq;

namespace api.DTOs
{
    public class RequestDto
    {
        public string request_type { get; set; } = null!;
        public string target_table { get; set; } = null!;
        public int? target_pk_id { get; set; }
        public JToken? old_data { get; set; }      // ✅ แก้ตรงนี้
        public JToken? new_data { get; set; }      // ✅ แก้ตรงนี้
        public string? note { get; set; }
    }

    public class RequestResponseDto
    {
        public int request_id { get; set; }
        public string request_type { get; set; } = null!;
        public string request_status { get; set; } = null!;
        public string target_table { get; set; } = null!;
        public int? target_pk_id { get; set; }
        public string? old_data { get; set; }
        public string? new_data { get; set; }
        public string requested_by { get; set; }
        public DateTime requested_at { get; set; }
        public string? approved_by { get; set; }
        public DateTime? approved_at { get; set; }
        public string? note { get; set; }
    }
}
