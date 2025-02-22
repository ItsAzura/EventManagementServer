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
        public DbSet<Models.Comment> Comments { get; set; }
        public DbSet<Models.Category> Categories { get; set; } 
        public DbSet<Models.Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


        }






    }
}
