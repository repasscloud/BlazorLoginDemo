using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlazorLoginDemo.Web.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            // Keep this in sync with your appsettings connection string
            .UseSqlite("Data Source=Data/app.db")
            .Options;

        return new ApplicationDbContext(options);
    }
}
