using System.Text.Json.Serialization;

public class UserDto
{
    public int user_id { get; set; }
    // public string username { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string? department_name { get; set; }
    public int role_id { get; set; }
    public string? role_name { get; set; }
    public int status_id { get; set; }
    public string? status_name { get; set; }
    public string? employee_id { get; set; }
    public string? email { get; set; }
}
