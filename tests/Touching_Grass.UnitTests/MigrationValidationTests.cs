using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Touching_Grass.Data;

namespace Touching_Grass.UnitTests;

public sealed class MigrationValidationTests
{
    [Fact]
    public void MySqlModelContainsIdentityAndOwnedPlantTables()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySQL("Server=localhost;Port=3306;Database=validation;User ID=validation;Password=validation")
            .Options;

        using var context = new ApplicationDbContext(options);
        var tables = context.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("AspNetUsers", tables);
        Assert.Contains("UserPlants", tables);
        Assert.Contains("AspNetUserLogins", tables);
    }

    [Fact]
    public void MySqlMigrationAssemblyContainsOnlyTheGeneratedInitialMigration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySQL("Server=localhost;Port=3306;Database=validation;User ID=validation;Password=validation")
            .Options;

        using var context = new ApplicationDbContext(options);
        var migrations = context.Database.GetService<IMigrationsAssembly>()
            .Migrations
            .Keys
            .ToArray();

        Assert.Single(migrations);
        Assert.EndsWith("_MySqlInitial", migrations[0], StringComparison.Ordinal);
    }
}
