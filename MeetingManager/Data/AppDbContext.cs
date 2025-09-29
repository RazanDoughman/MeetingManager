using MeetingManager.ActionItems.Model;
using MeetingManager.Attachments.Model;
using MeetingManager.Attendees.Model;
using MeetingManager.Features.Model;
using MeetingManager.Meetings.Model;
using MeetingManager.Notes.Model;
using MeetingManager.RoomFeatures.Model;
using MeetingManager.Rooms.Model;
using MeetingManager.Users.Model;
using MeetingManager.Roles.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetingManager
{
        public class AppDbContext : IdentityDbContext<ApplicationUser>
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> AppUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<RoomFeature> RoomFeatures { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<ActionItem> ActionItems { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.ClrType.Name);
            }


            modelBuilder.Entity<RoomFeature>()
                .HasOne(rf => rf.Room)
                .WithMany()
                .HasForeignKey(rf => rf.RoomId);

            modelBuilder.Entity<RoomFeature>()
                .HasOne(rf => rf.Feature)
                .WithMany()
                .HasForeignKey(rf => rf.FeatureId);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Meeting)
                .WithMany()
                .HasForeignKey(n => n.MeetingId);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.CreatedByUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedByUserId);

            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.Meeting)
                .WithMany()
                .HasForeignKey(a => a.MeetingId);

            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<ActionItem>()
                .HasOne(ai => ai.Meeting)
                .WithMany()
                .HasForeignKey(ai => ai.MeetingId);

            modelBuilder.Entity<ActionItem>()
                .HasOne(ai => ai.CreatedBy)
                .WithMany()
                .HasForeignKey(ai => ai.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActionItem>()
                .HasOne(ai => ai.AssignedTo)
                .WithMany()
                .HasForeignKey(ai => ai.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Meeting)
                .WithMany()
                .HasForeignKey(a => a.MeetingId);

            
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired(false);
            modelBuilder.Entity<User>().Property(u => u.PhoneNumber).IsRequired(false);

            modelBuilder.Entity<Meeting>()
                .HasIndex(m => new { m.RoomId, m.StartTime, m.EndTime });


        }

    }
}
