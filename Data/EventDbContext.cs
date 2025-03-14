using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Data
{
    public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
    {

        public DbSet<Models.Event> Events { get; set; }
        public DbSet<Models.EventArea> EventAreas { get; set; }
        public DbSet<Models.EventCategory> EventCategories { get; set; }
        public DbSet<Models.Registration> Registrations { get; set; }
        public DbSet<Models.RegistrationDetail> RegistrationDetails { get; set; }
        public DbSet<Models.Ticket> Tickets { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Role> Roles { get; set; }
        public DbSet<Models.Category> Categories { get; set; } 
        public DbSet<Models.Notification> Notifications { get; set; }
        public DbSet<Models.Feedback> Feedbacks { get; set; }
        public DbSet<Models.Comment> Comments { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Category>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Event>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Feedback>()
                .Property(f => f.CreatedAt)
                .HasDefaultValueSql("NOW()");


            modelBuilder.Entity<Role>().HasData( 
                new Role { RoleID = 1, RoleName = "Admin", RoleDescription = "Admin Role" },
                new Role { RoleID = 2, RoleName = "User", RoleDescription = "User Role" }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "Music", CategoryDescription = "Music Event" },
                new Category { CategoryID = 2, CategoryName = "Sport", CategoryDescription = "Sport Event" },
                new Category { CategoryID = 3, CategoryName = "Education", CategoryDescription = "Education Event" },
                new Category { CategoryID = 4, CategoryName = "Business", CategoryDescription = "Business Event" },
                new Category { CategoryID = 5, CategoryName = "Health", CategoryDescription = "Health Event" }
            );

        }






    }
}
