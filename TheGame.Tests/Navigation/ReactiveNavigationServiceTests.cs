using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using the_game.Navigation;
using the_game.ViewModels;
using Xunit;

namespace the_game.Tests.Navigation;

public sealed class ReactiveNavigationServiceTests
{
    [Fact]
    public async Task NavigateToAsync_ResolvesAndPushesViewModel()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<TestPageViewModel>();
        services.AddSingleton<INavigationService, ReactiveNavigationService>();
        await using ServiceProvider provider = services.BuildServiceProvider();
        INavigationService navigation = provider.GetRequiredService<INavigationService>();

        await navigation.NavigateToAsync<TestPageViewModel>(TestContext.Current.CancellationToken);

        ShellViewModel shell = provider.GetRequiredService<ShellViewModel>();
        Assert.IsType<TestPageViewModel>(shell.Router.GetCurrentViewModel());
    }

    private sealed class TestPageViewModel(ShellViewModel hostScreen)
        : ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "test";

        public IScreen HostScreen { get; } = hostScreen;
    }
}
