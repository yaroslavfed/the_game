using ReactiveUI;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
        Keyboard.Focus(this);
        string mediaPath = Path.Combine(
            _paths.ApplicationDirectory,
            "background_video",
            "backdrop.mp4");

        if (!File.Exists(mediaPath))
        {
            BackgroundMedia.Visibility = Visibility.Collapsed;
            return;
        }

        BackgroundMedia.Source = new Uri(mediaPath);
        BackgroundMedia.Play();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || ViewModel is null)
        {
            return;
        }

        e.Handled = true;
        ViewModel.ContinueCommand.Execute().Subscribe();
    }

    private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        BackgroundMedia.Stop();
        BackgroundMedia.Visibility = Visibility.Collapsed;
    }

    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        BackgroundMedia.Position = TimeSpan.Zero;
        BackgroundMedia.Play();
    }
}
