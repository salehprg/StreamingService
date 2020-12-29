using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.User;

namespace Models
{
    public class AppDbContext : IdentityDbContext<UserModel , IdentityRole<int> , int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<Meeting> Meetings {get; set;}
        public DbSet<MeetingServicesModel> Services {get; set;}


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Meeting>()
                .HasOne<UserModel>()
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}