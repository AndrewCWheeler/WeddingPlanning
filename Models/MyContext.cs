using Microsoft.EntityFrameworkCore;
using System;

namespace WeddingPlanning.Models
{
    public class MyContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<RSVP> RSVPS { get; set; }
        public DbSet<Wedding> Weddings { get; set; }
    }
}
