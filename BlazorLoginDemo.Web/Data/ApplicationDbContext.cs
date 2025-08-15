using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupDomain> GroupDomains => Set<GroupDomain>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Group>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.HasMany(x => x.Domains)
             .WithOne(x => x.Group)
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GroupDomain>(e =>
        {
            e.HasIndex(x => x.Domain).IsUnique();
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Group)
             .WithMany()
             .HasForeignKey(u => u.GroupId);

            e.Property(u => u.DisplayName).HasMaxLength(128);
            e.Property(u => u.Department).HasMaxLength(128);
        });
    }
}
