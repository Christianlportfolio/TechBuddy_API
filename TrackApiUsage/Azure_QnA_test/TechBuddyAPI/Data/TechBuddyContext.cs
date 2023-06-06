using Microsoft.EntityFrameworkCore;
using TechBuddyAPI.Models;

namespace TechBuddyAPI.Data
{
    public class TechBuddyContext : DbContext
    {

        public TechBuddyContext(DbContextOptions<TechBuddyContext> options)
       : base(options)
        {
        }

        public DbSet<CustomerQuestion> CustomerQuestion { get; set; } 
        public DbSet<Users> Users { get; set; } 


    }
}
