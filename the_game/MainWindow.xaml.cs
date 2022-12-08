using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace the_game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static readonly string media_back_path = System.IO.Directory.GetCurrentDirectory() + @"\background_video\";

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                start_page start = new start_page();
                this.Close();
                start.Show();
            }
        }

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            media_back.Source = new Uri(System.IO.Path.Combine(media_back_path, "backdrop.mp4"));
        }
    }
}
