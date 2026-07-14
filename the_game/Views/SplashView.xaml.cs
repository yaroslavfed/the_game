using ReactiveUI;
using System.IO;
using System.Windows;
using TheGame.Core.Storage;
using the_game.ViewModels;

namespace the_game.Views;

public partial class SplashView : ReactiveUserControl<SplashViewModel>
{
    private readonly IAppPaths _paths;

    public SplashView(IAppPaths paths)
    {
        InitializeComponent();
        _paths = paths;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();
        BackgroundMedia.Source = new Uri(Path.Combine(
            _paths.ApplicationDirectory,
            "background_video",
            "backdrop.mp4"));
        BackgroundMedia.Play();
    }

    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        BackgroundMedia.Position = TimeSpan.Zero;
        BackgroundMedia.Play();
    }
}
