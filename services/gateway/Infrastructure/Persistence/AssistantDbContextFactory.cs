using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nolivra.Gateway.Infrastructure.Persistence;

public sealed class AssistantDbContextFactory : IDesignTimeDbContextFactory<AssistantDbContext>
{
    public AssistantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssistantDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=nolivra_gateway;Username=postgres;Password=postgres");

        return new AssistantDbContext(optionsBuilder.Options);
    }
}