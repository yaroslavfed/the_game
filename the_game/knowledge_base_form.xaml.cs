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
using System.Windows.Shapes;

namespace the_game
{
    public partial class knowledge_base_form : Window
    {
        List<Page> Pages;

        public knowledge_base_form(string id)
        {
            InitializeComponent();

            this.Pages = new List<Page>();
            Pages.Add(new weapons_database_page());
            Pages.Add(new armors_database_page());
            //Pages.Add(new battle_database_page());
            this.id = id;
        }
        readonly string id;

        private void Page1_Click(object sender, RoutedEventArgs e)
        {
            log.Visibility = Visibility.Hidden;
            MyFrame.Content = Pages[0];
        }

        private void Page2_Click(object sender, RoutedEventArgs e)
        {
            log.Visibility = Visibility.Hidden;
            MyFrame.Content = Pages[1];
        }

        //private void Page3_Click(object sender, RoutedEventArgs e)
        //{
        //    log.Visibility = Visibility.Hidden;
        //    MyFrame.Content = Pages[2];
        //}

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            profile player = new profile(id);
            player.Show();
            this.Close();
        }
    }
}
