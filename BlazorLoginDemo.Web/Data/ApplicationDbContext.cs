using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.Group)
             .HasConversion<string>()           // store enum as string
             .HasMaxLength(32);

            b.Property(u => u.DisplayName).HasMaxLength(128);
            b.Property(u => u.Department).HasMaxLength(128);
        });
    }
}