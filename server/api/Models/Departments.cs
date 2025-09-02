namespace api.Models
{
    public class Department
    {
        public int department_id { get; set; }
        public string department_name { get; set; }
        public ICollection<User> users { get; set; }
    }
}