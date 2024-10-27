using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using api.Model;
using Microsoft.AspNetCore.Identity;

namespace api.Model
{
    public class ApplicationDbContext : IdentityDbContext<Users, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Users>(entity =>
            {
                entity.ToTable("Users"); 

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); 
            });
        }
    }
}
