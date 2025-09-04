using Microsoft.AspNetCore.Identity;

namespace MeetingManager.Users.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; } 
    }
}
