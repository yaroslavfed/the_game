using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace the_game
{
    public partial class battlefield : Window
    {
        public class Enemy_person
        {
            public string? Rank { get; set; }
            public string? Name { get; set; }
            public double Health { get; set; }
            public string? Damage { get; set; }
            public string? Protetion { get; set; }
            public string? Reward { get; set; }
            public string? Image { get; set; }
        }

        private Storyboard myStoryboard;

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

        int target = -1;
        int number_of_bots_alive = 0;
        int number_of_bots_killed = 0;

        //private ObservableCollection<Enemy_person> enemies_on_screen;
        private ObservableCollection<Enemy_person> enemies_on_screen0;
        private ObservableCollection<Enemy_person> enemies_on_screen1;
        private ObservableCollection<Enemy_person> enemies_on_screen2;
        private ObservableCollection<Enemy_person> enemies_on_screen3;
        private ObservableCollection<Enemy_person> enemies_on_screen4;
        private ObservableCollection<Enemy_person> enemies_on_screen5;
        private ObservableCollection<Enemy_person> enemies_on_screen6;
        private ObservableCollection<Enemy_person> enemies_on_screen7;
        private ObservableCollection<Enemy_person> enemies_on_screen8;

        //List<Grid> enemy_grid = new List<Grid>();
        List<ListBox> enemy_field = new();
        List<ObservableCollection<Enemy_person>> enemies_on_screen_nums = new();
        List<int> enemy_added = new();
        List<ProgressBar> progress_bar_enemies = new();

        System.Windows.Threading.DispatcherTimer targetInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer struggleInTime = new System.Windows.Threading.DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")))
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

            //enemy_grid.AddRange(new Grid[] { E0, E1, E2, E3, E4, E5, E6, E7, E8 });
            //progress_bar_enemies.AddRange(new ProgressBar[] { E0, E1, E2, E3, E4, E5, E6, E7, E8 });

            enemy_field.AddRange(new ListBox[] { enemy0, enemy1, enemy2, enemy3, enemy4, enemy5, enemy6, enemy7, enemy8 });
            enemies_on_screen_nums.AddRange(new ObservableCollection<Enemy_person>[] { enemies_on_screen0, enemies_on_screen1, enemies_on_screen2, enemies_on_screen3, enemies_on_screen4, enemies_on_screen5, enemies_on_screen6, enemies_on_screen7, enemies_on_screen8 });

            Bots_render(2);

            Timers_start();
        }

        private void Timers_start()
        {
            targetInTime.Tick += new EventHandler(targetInTime_Tick);
            targetInTime.Interval = new TimeSpan(1);
            targetInTime.Start();

            struggleInTime.Tick += new EventHandler(struggleInTime_Tick);
            struggleInTime.Interval = new TimeSpan(1);
            struggleInTime.Start();
        }

        private void Timers_stop()
        {
            targetInTime.Stop();
            struggleInTime.Stop();
        }

        private void targetInTime_Tick(object sender, EventArgs e)
        {
            //target_info.Text = Convert.ToString(target);
            target_info.Text = "Target:" + Convert.ToString(target) + "\nAlive: " + number_of_bots_alive + "\nKilled: " + number_of_bots_killed;
        }

        private void struggleInTime_Tick(object sender, EventArgs e)
        {
            if(number_of_bots_alive == number_of_bots_killed)
            {
                //MessageBox.Show("You win");
                Timers_stop();
                Go_to_next_wave();
            }
        }

        private void Animation_disappearing(ListBox myObject)
        {
            var myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));

            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, myObject.Name);
            //Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(myObject.Name.OpacityProperty));
        }

        private void Listbox_visibility()
        {
            for(int i = 0; i < enemy_field.Count; i++)
            {
                enemy_field[i].Visibility = Visibility.Visible;
            }
        }

        private void Bots_render(int n)
        {
            int max_rand;
            if (wave < 5)
                max_rand = wave - 1;
            else
                max_rand = 3;

            for (int i = 0; i < n; i++)
            {
                Random rand = new((int)(DateTime.Now.Ticks));
                string rank = Convert.ToString(rand.Next(0, max_rand+1));

                string name;
                string health;
                string damage;
                string protetion;
                string reward;

                using (StreamReader sr = new(System.IO.Path.Combine(resource_paths.enemyPath, rank + ".txt")))
                {
                    rank = sr.ReadLine();
                    name = sr.ReadLine();
                    health = sr.ReadLine();
                    damage = sr.ReadLine();
                    protetion = sr.ReadLine();
                    reward = sr.ReadLine();
                }

                //enemies_on_screen_nums[0] = new ObservableCollection<Enemy_person>();
                //enemies_on_screen_nums[0].Add(new Enemy_person() { Rank = rank, Name = name, Health = double.Parse(health), Damage = damage, Protetion = protetion, Reward = reward, Image = System.IO.Path.Combine(resource_paths.enemy_icon_Path, rank + ".png") });

                Random rnd = new((int)(DateTime.Now.Ticks));
                int value = rnd.Next(0, 9);

                //enemies_on_screen_nums[value] = new ObservableCollection<Enemy_person>();
                //enemies_on_screen_nums[value].Add(new Enemy_person() { Rank = rank, Name = name, Health = double.Parse(health), Damage = damage, Protetion = protetion, Reward = reward, Image = System.IO.Path.Combine(resource_paths.enemy_icon_Path, rank + ".png") });

                //enemy_added.Add(value);
                //enemy_field[value].ItemsSource = enemies_on_screen;

                if (!enemy_added.Any(str => str == value))
                {
                    enemy_added.Add(value);
                    enemies_on_screen_nums[value] = new ObservableCollection<Enemy_person>();
                    enemies_on_screen_nums[value].Add(new Enemy_person() { Rank = rank, Name = name, Health = double.Parse(health), Damage = damage, Protetion = protetion, Reward = reward, Image = System.IO.Path.Combine(resource_paths.enemy_icon_Path, rank + ".png") });
                    enemy_field[value].ItemsSource = enemies_on_screen_nums[value];
                }
                //enemy_added.Sort();
                number_of_bots_alive = enemy_added.Count;
            }
            char delim = ' ';
            string str = String.Join(delim, enemy_added);
            //MessageBox.Show(str);
        }

        private void Bots_clear()
        {
            enemy_added.Clear();
            for (int j = 0; j < enemy_field.Count; j++)
            {
                if (enemy_field[j].Items != null)
                {
                    enemy_field[j].ItemsSource = null;
                    enemy_field[j].Items.Clear();
                }
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Timers_stop();
            mode_selection mode = new mode_selection(id);
            mode.Show();
            this.Close();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            Listbox_visibility();
            Deselect(enemy_field.Count);

            wave++;
            wave_info.Text = "Волна " + wave;

            Bots_clear();
            number_of_bots_killed = 0;

            int count_bots = wave < 10 ? (count_bots = wave) : (count_bots = 9);

            Bots_render(count_bots);
            Timers_start();
        }

        private void Go_to_next_wave()
        {
            Listbox_visibility();
            Deselect(enemy_field.Count);

            wave++;
            wave_info.Text = "Волна " + wave;

            Bots_clear();
            number_of_bots_killed = 0;

            int count_bots = wave < 10 ? (count_bots = wave) : (count_bots = 9);

            Bots_render(count_bots);
            Timers_start();
        }

        private void Deselect(int num)
        {
            for(int i = 0; i < enemy_field.Count; i++)
            {
                enemy_field[i].SelectedIndex = -1;
                if (i != num)
                {
                    //enemy_field[i].SelectedIndex = -1;
                    enemy_field[i].BorderThickness = new Thickness(0, 0, 0, 0);
                }
            }
        }

        private void Information_output(int num)
        {
            target = int.Parse(enemy_field[num].Name[(enemy_field[num].Name.IndexOf("enemy") + 5)..]);

            var editItem = enemy_field[num].SelectedItem as Enemy_person;
            if (editItem == null)
                return;
            string message = string.Format("Field: {0},\nRank: {1},\nName: {2},\nHealth: {3},\nDamage: {4},\nProtection: {5},\nReward: {4}", target, editItem.Rank, editItem.Name, editItem.Health, editItem.Damage, editItem.Protetion, editItem.Reward);
            //MessageBox.Show(message);

            enemy_field[num].BorderThickness = new Thickness (1, 1, 1, 1);
            Deselect(num);
        }

        private void enemy0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 0;
            Information_output(num);
        }

        private void enemy1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 1;
            Information_output(num);
        }

        private void enemy2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 2;
            Information_output(num);
        }

        private void enemy3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 3;
            Information_output(num);
        }

        private void enemy4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 4;
            Information_output(num);
        }

        private void enemy5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 5;
            Information_output(num);
        }

        private void enemy6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 6;
            Information_output(num);
        }

        private void enemy7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 7;
            Information_output(num);
        }

        private void enemy8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = 8;
            Information_output(num);
        }

        private void attack_button_Click(object sender, RoutedEventArgs e)
        {
            if(target != -1)
            {
                //MessageBox.Show("The target on field " + target + " is attacked");
                enemies_on_screen_nums[target][enemies_on_screen_nums[target].Count - 1].Health -= 100;
                double enemy_health = enemies_on_screen_nums[target][enemies_on_screen_nums[target].Count - 1].Health;

                if (enemy_health <= 0)
                {
                    number_of_bots_killed += 1;
                    //MessageBox.Show("enemy_" + target + " is killed");
                    enemy_field[target].Visibility = Visibility.Hidden;
                    target = -1;
                }

                //MessageBox.Show(Convert.ToString(enemy_health));
                
            }
            else
            {
                MessageBox.Show("First select a goal");
            }
        }

        private void test_button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
