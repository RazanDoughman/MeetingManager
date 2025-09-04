namespace MeetingManager.Users.DTO
{
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }   // Admin, Employee, Guest

        // Always true to allow clearing FullName if desired
        public bool FullNameSet => true;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = "";
    }

    public class UserSummaryDto
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
    }

    public class UserDetailDto : UserSummaryDto
    {
        public List<string> Roles { get; set; } = new();
    }
}
