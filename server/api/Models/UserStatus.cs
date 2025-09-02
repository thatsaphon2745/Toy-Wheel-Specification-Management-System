namespace api.Models
{
    public class UserStatus
    {
        public int status_id { get; set; }
        public string status_name { get; set; }
        public ICollection<User> users { get; set; }
    }
}