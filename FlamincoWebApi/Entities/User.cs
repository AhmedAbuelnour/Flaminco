using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlamincoWebApi.Entities
{
    public class User
    {
        [Key] public string Id { get; set; }
        public string FullName { get; set; }
        public string[]? Attributes { get; set; }
    }

    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>().OwnsMany(a => a.Attributes, builder =>
            //{
            //    builder.ToJson();
            //});

            base.OnModelCreating(modelBuilder);
        }
    }
}
