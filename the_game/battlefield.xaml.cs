using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace the_game
{
    public partial class battlefield : Window
    {
        public battlefield(string id)
        {
            InitializeComponent();
            this.id = id;
        }
        readonly string id;

        string nickname;
        string level;
        int wave = 1;

        string weapon;
        string protection;

        bool haveDD = false;
        bool haveHeal = false;

        string damageInfo;
        string protectionInfo;
        string healthInfo;

        List<Grid> enemy_field = new List<Grid>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")))
            {
                nickname = sr.ReadLine();
                level = sr.ReadLine();
                string zero = sr.ReadLine();
                zero = sr.ReadLine();
                weapon = sr.ReadLine();
                zero = sr.ReadLine();
                protection = sr.ReadLine();
            }
            nickname_info.Text = nickname;
            level_info.Text = "Уровень " + level;
            foto.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.iconPath, id + ".jpg")));
            wave_info.Text = "Волна " + wave;

            enemy_field.AddRange(new Grid[] { E0, E1, E2, E3, E4, E5, E6, E7, E8 });

            bots_render(2);
        }

        ListBox listBox1;
        private void bots_render(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Random rnd = new Random();
                int value = rnd.Next(0, 9);
                Console.WriteLine(value);

                //ListBox listBox1 = new ListBox();
                listBox1 = new ListBox();
                listBox1.SelectionMode = SelectionMode.Single;
                
                StackPanel stackPanel1 = new StackPanel();

                listBox1.BorderThickness = new Thickness(0, 0, 0, 0);
                listBox1.HorizontalAlignment = HorizontalAlignment.Center;

                TextBlock txt = new TextBlock();
                Image img = new Image();
                ProgressBar PB = new ProgressBar();

                txt.Text = enemy_field[value].Name;     // имя противника
                txt.TextAlignment = TextAlignment.Center;
                img.Height = 100;
                img.Width = 100;
                img.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.iconPath, id + ".jpg")));    // иконка противника
                PB.Maximum = 100 * wave;    // хп противника
                PB.Value = 100;     // при создании равное максимуму
                PB.Height = 7;
                PB.Width = 100;

                stackPanel1.Children.Add(txt);
                stackPanel1.Children.Add(img);
                stackPanel1.Children.Add(PB);

                //listBox1.Items.Add(txt);
                //listBox1.Items.Add(img);
                //listBox1.Items.Add(PB);
                listBox1.Items.Add(stackPanel1);

                enemy_field[value].Children.Add(listBox1);

                //listBox1.SelectionChanged += ListBox1_SelectionChanged;
                //listBox1.SelectedIndex += listBox1_SelectedIndexChanged;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("Click");
            
        }

        private void bots_clear()
        {
            for (int j = 0; j < enemy_field.Count; j++)
                enemy_field[j].Children.Clear();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            mode_selection mode = new mode_selection(id);
            mode.Show();
            this.Close();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            wave++;
            wave_info.Text = "Волна " + wave;

            bots_clear();

            int count_bots = wave < 10 ? (count_bots = wave) : (count_bots = 9);

            bots_render(count_bots);
        }
    }
}
