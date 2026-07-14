using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace the_game.Navigation;

public sealed class LegacyNavigationBridge(IServiceProvider services) : ILegacyNavigationBridge
{
    public void OpenMainMenu()
    {
        Window? shell = Application.Current.MainWindow;
        start_page mainMenu = services.GetRequiredService<start_page>();

        Application.Current.MainWindow = mainMenu;
        mainMenu.Show();
        shell?.Close();
    }
}
