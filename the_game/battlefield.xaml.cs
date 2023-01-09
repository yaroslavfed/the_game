using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        private BackgroundWorker heal_worker = null;
        private BackgroundWorker dd_worker = null;

        public battlefield(string id)
        {
            InitializeComponent();

            heal_worker = new BackgroundWorker();
            heal_worker.WorkerSupportsCancellation = true;
            heal_worker.WorkerReportsProgress = true;
            heal_worker.DoWork += heal_worker_DoWork;
            heal_worker.ProgressChanged += heal_worker_ProgressChanged;
            heal_worker.RunWorkerCompleted += heal_worker_RunWorkerCompleted;

            dd_worker = new BackgroundWorker();
            dd_worker.WorkerSupportsCancellation = true;
            dd_worker.WorkerReportsProgress = true;
            dd_worker.DoWork += dd_worker_DoWork;
            dd_worker.ProgressChanged += dd_worker_ProgressChanged;
            dd_worker.RunWorkerCompleted += dd_worker_RunWorkerCompleted;

            this.id = id;
        }

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

        readonly string id;

        bool hero_attack_point = true;
        bool timer_attack_point = true;

        string nickname;
        string level;
        int wave = 1;

        string weapon;
        string protection;

        bool haveDD;
        bool haveHeal;

        double health_hero;
        double max_health_hero;
        double damage_hero;
        double protection_hero;

        int money_now;
        int exp_now;

        int total_reward;
        int total_exp;
        int wave_record;

        double enemy_damage;
        double enemy_protection;

        string damageInfo;
        string protectionInfo;
        string healthInfo;

        int target = -1;
        int number_of_bots_alive = 0;
        int number_of_bots_killed = 0;

        int assaulter;

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
        List<int> enemy_killed = new();
        List<ProgressBar> progress_bar_enemies = new();
        List<TextBlock> health_text_enemies = new();

        System.Windows.Threading.DispatcherTimer targetInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer struggleInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer attackInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer healthInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer gameInTime = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer deathInTime = new System.Windows.Threading.DispatcherTimer();

        //BackgroundWorker time_worker = new BackgroundWorker();
        DoubleAnimation Animation;

        private BackgroundWorker time_worker = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")))
            {
                nickname = sr.ReadLine();
                level = sr.ReadLine();
                exp_now = int.Parse(sr.ReadLine());
                money_now = int.Parse(sr.ReadLine());
                weapon = sr.ReadLine();
                string zero = sr.ReadLine();
                protection = sr.ReadLine();
            }
            nickname_info.Text = nickname;
            level_info.Text = "Уровень " + level;
            foto.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.iconPath, id + ".jpg")));
            wave_info.Text = "Волна " + wave;

            if (int.Parse(level) >= 25)
            {
                haveHeal = true;
                heal_button.IsEnabled = true;
            }
            else
            {
                haveHeal = false;
                heal_button.IsEnabled = false;
            }

            if (int.Parse(level) >= 50)
            {
                haveDD = true;
                dd_button.IsEnabled = true;
            }
            else
            {
                haveDD = false;
                dd_button.IsEnabled = false;
            }

            health_hero = 100 + double.Parse(level) * 4;
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.inventoryPath, weapon + ".txt")))
            {
                damage_hero = double.Parse(sr.ReadLine());
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.inventoryPath, protection + ".txt")))
            {
                protection_hero = double.Parse(sr.ReadLine());
            }
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.user_record_Path, id + ".txt")))
            {
                wave_record = int.Parse(sr.ReadLine());
            }

            nickname_hero_0.Text = nickname;
            img_hero_0.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.hero_icon_Path, "shooting_person.png")));
            hero_hp_0.Text = Convert.ToString(health_hero);
            hero_health_bar_0.Value = health_hero;

            max_health_hero = health_hero;
            hero_health_bar_0.Maximum = max_health_hero;

            total_reward = 0;

            health_text_enemies.AddRange(new TextBlock[] { hp0, hp1, hp2, hp3, hp4, hp5, hp6, hp7, hp8 });
            progress_bar_enemies.AddRange(new ProgressBar[] { enemy_health_bar_0, enemy_health_bar_1, enemy_health_bar_2, enemy_health_bar_3, enemy_health_bar_4, enemy_health_bar_5, enemy_health_bar_6, enemy_health_bar_7, enemy_health_bar_8 });

            enemy_field.AddRange(new ListBox[] { enemy0, enemy1, enemy2, enemy3, enemy4, enemy5, enemy6, enemy7, enemy8 });
            enemies_on_screen_nums.AddRange(new ObservableCollection<Enemy_person>[] { enemies_on_screen0, enemies_on_screen1, enemies_on_screen2, enemies_on_screen3, enemies_on_screen4, enemies_on_screen5, enemies_on_screen6, enemies_on_screen7, enemies_on_screen8 });

            deathInTime.Tick += new EventHandler(deathInTime_Tick);
            deathInTime.Interval = new TimeSpan(1000);
            //deathInTime.Start();

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

            attackInTime.Tick += new EventHandler(attackInTime_Tick);
            attackInTime.Interval = new TimeSpan(1);
            attackInTime.Start();

            healthInTime.Tick += new EventHandler(healthInTime_Tick);
            healthInTime.Interval = new TimeSpan(1);
            healthInTime.Start();

            gameInTime.Tick += new EventHandler(gameInTime_Tick);
            gameInTime.Interval = new TimeSpan(1);
            gameInTime.Start();
        }

        private void Timers_stop()
        {
            targetInTime.Stop();
            struggleInTime.Stop();
            attackInTime.Stop();
            healthInTime.Stop();
            gameInTime.Stop();
        }

        private void targetInTime_Tick(object sender, EventArgs e)
        {
            string string_enemy_added = String.Concat<int>(enemy_added);
            string string_enemy_killed = String.Concat<int>(enemy_killed);
            target_info.Text = "Target: " + Convert.ToString(target) + "\nAlive: " + number_of_bots_alive + "\nKilled: " + number_of_bots_killed + "\nPoint: " + hero_attack_point + "\nEnemy_add:\n" + string_enemy_added + "\nEnemy_killed:\n" + string_enemy_killed + "\nReward: " + total_reward;
        }

        private void struggleInTime_Tick(object sender, EventArgs e)
        {
            if(number_of_bots_alive == number_of_bots_killed)
            {
                Timers_stop();
                Go_to_next_wave();
            }
        }
        
        private void attackInTime_Tick(object sender, EventArgs e)
        {
            if(target == -1)
            {
                attack_button.IsEnabled= false;
            }
            else
            {
                attack_button.IsEnabled= true;
            }
        }

        private void healthInTime_Tick(object sender, EventArgs e)
        {
            hero_hp_0.Text = Convert.ToString(health_hero);
            hero_health_bar_0.Value = health_hero;

            if (health_hero <= 0)
            {
                hero_hp_0.Text = "0";
                hero_health_bar_0.Value = 0;
                Timers_stop();
                deathInTime.Start();
            }
        }

        private void deathInTime_Tick(object sender, EventArgs e)
        {
            deathInTime.Stop();
            heal_worker.CancelAsync();
            dd_worker.CancelAsync();
            total_exp = wave * 10;

            int modified_total_exp;
            if (exp_now + total_exp >= 100)
            {
                modified_total_exp = total_exp - (100 - exp_now);
                Rewrite(int.Parse(level) + 1, 1);
                Rewrite(modified_total_exp, 2);
            }
            else
            {
                Rewrite(exp_now + total_exp, 2);
            }
            Rewrite(money_now + total_reward, 3);

            if(wave_record < wave)
            {
                Save_record(wave, 0);
            }

            Thread.Sleep(1000);
            results_game results = new results_game(id, wave, total_reward, total_exp, weapon, protection);
            results.Show();
            this.Close();
        }

        private void Save_record(int new_record, int lineIndex)
        {
            string newValue = Convert.ToString(new_record);

            string path = System.IO.Path.Combine(resource_paths.user_record_Path, id + ".txt");

            int i = 0;
            string tempPath = path + ".tmp";
            using (StreamReader sr = new StreamReader(path)) // читаем
            using (StreamWriter sw = new StreamWriter(tempPath)) // и сразу же пишем во временный файл
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (lineIndex == i)
                        sw.WriteLine(newValue);
                    else
                        sw.WriteLine(line);
                    i++;
                }
            }
            File.Delete(path); // удаляем старый файл
            File.Move(tempPath, path); // переименовываем временный файл
        }

        private void Rewrite(int moneyNow, int lineIndex)
        {
            string newValue = Convert.ToString(moneyNow);

            string path = System.IO.Path.Combine(resource_paths.userPath, id + ".txt");

            int i = 0;
            string tempPath = path + ".tmp";
            using (StreamReader sr = new StreamReader(path)) // читаем
            using (StreamWriter sw = new StreamWriter(tempPath)) // и сразу же пишем во временный файл
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (lineIndex == i)
                        sw.WriteLine(newValue);
                    else
                        sw.WriteLine(line);
                    i++;
                }
            }
            File.Delete(path); // удаляем старый файл
            File.Move(tempPath, path); // переименовываем временный файл
        }

        private void gameInTime_Tick(object sender, EventArgs e)
        {
            if (hero_attack_point)
            {
                //if (timer_attack_point)
                //{
                //    timer_attack_point = false;
                //    int time_to_text_timer = 59;
                //    step_bar_timer.Value = double.MaxValue;
                //    step_text_timer.Text = "00:" + time_to_text_timer;

                //    time_worker = new BackgroundWorker();
                //    time_worker.WorkerSupportsCancellation = true;
                //    time_worker.WorkerReportsProgress = true;
                //    time_worker.DoWork += time_worker_DoWork;
                //    time_worker.ProgressChanged += time_worker_ProgressChanged;
                //    time_worker.RunWorkerCompleted += time_worker_RunWorkerCompleted;

                //    time_worker.RunWorkerAsync(59);
                //}
            }
            else
            {
                enemy_attack();
            }
        }

        void time_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int max = (int)e.Argument;
            for (int i = max; i >= 0; i--)
            {
                if (time_worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                int progressPercentage = Convert.ToInt32(((double)i / (double)max) * 100.0);
                (sender as BackgroundWorker).ReportProgress(progressPercentage, i);
                Thread.Sleep(1000);
            }
        }

        void time_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            step_bar_timer.Value = e.ProgressPercentage;
            if (e.UserState != null)
                step_text_timer.Text = "00:" + e.UserState;
        }

        void time_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            hero_attack_point = false;
            timer_attack_point = true;
        }

        private double Change_damage(double damage, double protection)
        {
            double modified_damage = damage * ((100 - protection)/100);
            return modified_damage;
        }

        private void enemy_attack()
        {
            var still_alive = enemy_added.Except(enemy_killed);
            List<int> list_still_alive = new();
            foreach (int s in still_alive)
                list_still_alive.Add(s);
            if(list_still_alive.Count != 0)
            {
                BackgroundWorker worker = new BackgroundWorker();

                assaulter = list_still_alive[new Random().Next(0, list_still_alive.Count)];
                enemy_damage = double.Parse(enemies_on_screen_nums[assaulter][enemies_on_screen_nums[assaulter].Count - 1].Damage);

                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync(assaulter);

                //health_hero -= enemy_damage;
                health_hero -= Change_damage(enemy_damage, protection_hero);
                hero_attack_point = true;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int result = 0;
            Thread.Sleep(100);
            for (int i = 0; i < 20; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                result = i;
                Thread.Sleep(20);
            }
            e.Result = result;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int step = e.ProgressPercentage;
            if (step % 2 == 0)
            {
                progress_bar_enemies[assaulter].Foreground = Brushes.Yellow;
            }
            else
            {
                progress_bar_enemies[assaulter].Foreground = Brushes.DarkRed;
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress_bar_enemies[assaulter].Foreground = Brushes.DarkRed;
        }

        private void Animation_disappearing(ListBox myObject)
        {
            Animation = new DoubleAnimation();
            Animation.From = 1.0;
            Animation.To = 0.0;
            Animation.Duration = TimeSpan.FromSeconds(0.5);
            myObject.BeginAnimation(ListBox.OpacityProperty, Animation);
        }

        private void Listbox_update()
        {
            for(int i = 0; i < enemy_field.Count; i++)
            {
                Animation = new DoubleAnimation();
                Animation.From = 0.0;
                Animation.To = 1.0;
                Animation.Duration = TimeSpan.FromSeconds(0.5);
                enemy_field[i].BeginAnimation(ListBox.OpacityProperty, Animation);
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

                Random rnd = new((int)(DateTime.Now.Ticks));
                int value = rnd.Next(0, 9);

                if (!enemy_added.Any(str => str == value))
                {
                    enemy_added.Add(value);
                    enemies_on_screen_nums[value] = new ObservableCollection<Enemy_person>();
                    enemies_on_screen_nums[value].Add(new Enemy_person() { Rank = rank, Name = name, Health = double.Parse(health), Damage = damage, Protetion = protetion, Reward = reward, Image = System.IO.Path.Combine(resource_paths.enemy_icon_Path, rank + ".png") });
                    enemy_field[value].ItemsSource = enemies_on_screen_nums[value];

                    health_text_enemies[value].Visibility = Visibility.Visible;
                    health_text_enemies[value].Text = health;

                    progress_bar_enemies[value].Width = double.Parse(health) / 3;
                    progress_bar_enemies[value].Visibility = Visibility.Visible;
                    progress_bar_enemies[value].Maximum = double.Parse(health);
                    progress_bar_enemies[value].Value = double.Parse(health);
                }
                number_of_bots_alive = enemy_added.Count;
            }
        }

        private void Bots_clear()
        {
            enemy_added.Clear();
            enemy_killed.Clear();
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

        private void Wave_heal()
        {
            if(health_hero / max_health_hero <= 0.5 && health_hero > 0)
            {
                health_hero *= 1.9;
            }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            Wave_heal();
            hero_attack_point = true;
            Listbox_update();
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
            Wave_heal();
            hero_attack_point = true;
            Listbox_update();
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
                enemies_on_screen_nums[target][enemies_on_screen_nums[target].Count - 1].Health -= damage_hero;
                double enemy_health = enemies_on_screen_nums[target][enemies_on_screen_nums[target].Count - 1].Health;

                if (enemy_health <= 0)
                {
                    enemy_killed.Add(target);
                    total_reward += int.Parse(enemies_on_screen_nums[target][enemies_on_screen_nums[target].Count - 1].Reward);

                    health_text_enemies[target].Text = Convert.ToString(0);
                    health_text_enemies[target].Visibility= Visibility.Hidden;

                    progress_bar_enemies[target].Value = 0;
                    progress_bar_enemies[target].Visibility= Visibility.Hidden;

                    number_of_bots_killed = enemy_killed.Count;
                    //number_of_bots_killed += 1;
                    Animation_disappearing(enemy_field[target]);
                    target = -1;
                }
                else
                {
                    health_text_enemies[target].Text = Convert.ToString(enemy_health);
                    progress_bar_enemies[target].Value = enemy_health;
                }
                //time_worker.CancelAsync();
                target = -1;
                Deselect(target);
                hero_attack_point = false;
            }
            //else
                //MessageBox.Show("First select a goal");
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A: attack_button_Click(attack_button, null); break;
                case Key.Z: heal_button_Click(heal_button, null); break;
                case Key.X: dd_button_Click(dd_button, null); break;
            }
        }        

        private void heal_button_Click(object sender, RoutedEventArgs e)
        {
            if (health_hero <= max_health_hero * 0.5 && haveHeal && heal_button.IsEnabled)
            {
                health_hero *= 1.5;
                heal_button.IsEnabled = false;
                heal_worker.RunWorkerAsync();
            }
        }

        void heal_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++)
            {
                if (heal_worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                heal_worker.ReportProgress(i);
                System.Threading.Thread.Sleep(100);
            }
        }

        void heal_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            heal_button.Content = e.ProgressPercentage + " %";
        }

        void heal_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                heal_button.Foreground = Brushes.Red;
                heal_button.Content = "Недоступно";
            }
            else
            {
                heal_button.IsEnabled = true;
                heal_button.Content = "Аптечка";
            }
        }

        private void dd_button_Click(object sender, RoutedEventArgs e)
        {
            if (haveDD && dd_button.IsEnabled)
            {
                damage_hero *= 1.1;
                dd_button.IsEnabled = false;
                dd_worker.RunWorkerAsync();
            }
        }

        void dd_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++)
            {
                if (dd_worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                dd_worker.ReportProgress(i);
                System.Threading.Thread.Sleep(500);
            }
        }

        void dd_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            dd_button.Content = e.ProgressPercentage + " %";
        }

        void dd_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                dd_button.Foreground = Brushes.Red;
                dd_button.Content = "Недоступно";
            }
            else
            {
                dd_button.IsEnabled = true;
                dd_button.Content = "Усиление";
            }
        }
    }
}
