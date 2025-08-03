using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MeetingManager.Roles.Model;
using MeetingManager.Users.Model;


namespace MeetingManager.Roles.Model
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}