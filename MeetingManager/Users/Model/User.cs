using MeetingManager.Roles.Model;
using System;

namespace MeetingManager.Users.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public Guid RoleId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Role? Role { get; set; }
    }
}
