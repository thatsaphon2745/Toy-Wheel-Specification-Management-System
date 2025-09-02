using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("logs")]
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long log_id { get; set; }

        public int? user_id { get; set; }

        [MaxLength(255)]
        public string? username_snapshot { get; set; }

        [Required]
        [MaxLength(100)]
        public string action { get; set; }

        [MaxLength(100)]
        public string? target_table { get; set; }

        [MaxLength(255)]
        public string? target_id { get; set; }

        // public string? details { get; set; }

        public string? old_data { get; set; }
        public string? new_data { get; set; }

        // [MaxLength(50)]
        // public string? ip_address { get; set; }

        // [MaxLength(255)]
        // public string? device { get; set; }

        // [MaxLength(255)]
        // public string? os_info { get; set; }

        // [MaxLength(1024)]
        // public string? endpoint_url { get; set; }

        // [MaxLength(10)]
        // public string? http_method { get; set; }

        // public int? response_status { get; set; }

        [Required]
        public DateTime created_at { get; set; }
    }
}
