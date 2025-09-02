namespace api.Models
{
    public class Role
    {
        public int role_id { get; set; }
        public string role_name { get; set; }
        public ICollection<User> users { get; set; }
    }
}