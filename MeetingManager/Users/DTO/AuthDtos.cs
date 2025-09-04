namespace MeetingManager.Users.DTO
{
    public class LoginRequest { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Employee"; // Admin will set this
        public string? FullName { get; set; }             
    }
}
