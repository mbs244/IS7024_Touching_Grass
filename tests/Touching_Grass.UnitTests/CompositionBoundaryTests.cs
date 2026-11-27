using Microsoft.AspNetCore.Mvc.Testing;

namespace Touching_Grass.UnitTests;

public sealed class CompositionBoundaryTests
{
    [Fact]
    public void AzureHostWithoutProtectedConnectionStringFailsBeforeBuilding()
    {
        var previousEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseSetting("environment", "Production");
            });

            var exception = Assert.ThrowsAny<InvalidOperationException>(() => _ = factory.Server);
            Assert.Contains("ConnectionStrings:DefaultConnection", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previousEnvironment);
        }
    }
}
