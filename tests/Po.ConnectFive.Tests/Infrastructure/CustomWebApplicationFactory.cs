using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PoConnectFive.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests that properly handles
/// the Blazor Web App + Aspire structure in .NET 10.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Add in-memory configuration for Azurite connection string
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var azuriteConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AzureTableStorage"] = azuriteConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove Aspire's service discovery which can interfere with testing
            var serviceDiscoveryDescriptor = services.FirstOrDefault(
                d => d.ServiceType.FullName?.Contains("ServiceDiscovery") == true);
            if (serviceDiscoveryDescriptor != null)
            {
                services.Remove(serviceDiscoveryDescriptor);
            }
        });
    }
}
