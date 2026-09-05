using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LunktrionApi.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var dummyConnectionString = "Host=localhost;Database=lunktrion_dev;Username=postgres;Password=postgres";

            optionsBuilder.UseNpgsql(dummyConnectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
