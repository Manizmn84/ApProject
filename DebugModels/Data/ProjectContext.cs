using Microsoft.EntityFrameworkCore;
using DebugModels.Models;

namespace DebugModels.Data
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teaches> Teaches { get; set; }
        public DbSet<Takes> Takes { get; set; }
        public DbSet<Sections> Sections { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
               .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<Instructor>()
               .HasOne(i => i.User)
               .WithMany(u => u.Instructors)
               .HasForeignKey(i => i.UserId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany(u => u.Students)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Teaches>()
                .HasOne(t => t.Instructor)
                .WithMany(i => i.Teaches)
                .HasForeignKey(t => t.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Takes>()
                .HasOne(t => t.Student)
                .WithMany(s => s.Takes)
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sections>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sections>()
                .HasOne(s => s.TimeSlot)
                .WithMany(t => t.Sections)
                .HasForeignKey(s => s.TimeSlotId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sections>()
                .HasOne(s => s.Teaches)
                .WithOne(t => t.Sections)
                .HasForeignKey<Sections>(s => s.TeachesId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sections>()
                .HasOne(s => s.Takes)
                .WithOne(t => t.Sections)
                .HasForeignKey<Sections>(s => s.TakesId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Sections)
                .WithOne(s => s.Course)
                .HasForeignKey<Course>(c => c.SectionsId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}