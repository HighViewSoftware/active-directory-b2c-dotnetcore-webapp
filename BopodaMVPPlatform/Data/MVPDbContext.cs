using BopodaMVPPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace BopodaMVPPlatform.Data
{
    public class MVPDbContext : DbContext
    {
        public MVPDbContext(DbContextOptions<MVPDbContext> options)
            : base(options)
        {

        }

        public DbSet<MVPUser> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserRelationship> UserRelationships { get; set; }
    }
}
