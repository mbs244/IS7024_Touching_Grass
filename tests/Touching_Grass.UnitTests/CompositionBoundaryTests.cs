using Microsoft.AspNetCore.Mvc.Testing;

namespace Touching_Grass.UnitTests;

public sealed class CompositionBoundaryTests
{
    [Fact]
    public void AzureHostWithoutProtectedConnectionStringFailsBeforeBuilding()
    {
        var previousEnvironment = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
        try
        {
            Environment.SetEnvironmentVariable("MYSQLCONNSTR_localdb", null);
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseSetting("environment", "Production");
            });

            var exception = Assert.ThrowsAny<InvalidOperationException>(() => _ = factory.Server);
            Assert.Contains("MYSQLCONNSTR_localdb", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MYSQLCONNSTR_localdb", previousEnvironment);
        }
    }
}
