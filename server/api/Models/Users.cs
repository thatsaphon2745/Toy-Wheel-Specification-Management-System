namespace api.Models
{
    public class User
    {
        public int user_id { get; set; }
        // public string username { get; set; }
        public string password_hash { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int department_id { get; set; }
        public int role_id { get; set; }
        public int status_id { get; set; }
        public string employee_id { get; set; }

        public string? email { get; set; }


        public Department Department { get; set; }
        public Role Role { get; set; }
        public UserStatus UserStatus { get; set; }
    }
}
