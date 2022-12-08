using System.IO;
using System.Windows;

namespace the_game
{
    public partial class mode_selection : Window
    {
        public mode_selection(string id)
        {
            InitializeComponent();
            this.id = id;
        }
        readonly string id;

        string nickname;
        string record;

        public static readonly string userPath = System.IO.Directory.GetCurrentDirectory() + @"\users\";
        public static readonly string user_record_Path = System.IO.Directory.GetCurrentDirectory() + @"\records\";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(userPath, id + ".txt")))
            {
                nickname = sr.ReadLine();
            }
            nickname_player.Text = nickname;

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(user_record_Path, id + ".txt")))
            {
                record = sr.ReadLine();
            }
            record_player.Text = "Рекорд:\t" + record + " волн";
        }

        private void single_game_start_Click(object sender, RoutedEventArgs e)
        {

        }

        private void game_exit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
