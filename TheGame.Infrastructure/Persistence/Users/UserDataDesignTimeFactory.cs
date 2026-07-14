using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheGame.Infrastructure.Persistence.Users;

public sealed class UserDataDesignTimeFactory : IDesignTimeDbContextFactory<UserDataDbContext>
{
    public UserDataDbContext CreateDbContext(string[] args)
    {
        string path = args.FirstOrDefault() ?? Path.Combine(Path.GetTempPath(), "the-game-userdata-design.db");
        var options = new DbContextOptionsBuilder<UserDataDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        return new UserDataDbContext(options);
    }
}
