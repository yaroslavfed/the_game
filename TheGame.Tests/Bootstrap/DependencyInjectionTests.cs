using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace the_game.Tests.Bootstrap;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddTheGameDesktop_RegistersMainWindowAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddTheGameDesktop();

        ServiceDescriptor registration = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(MainWindow));

        Assert.Equal(ServiceLifetime.Singleton, registration.Lifetime);
    }
}
