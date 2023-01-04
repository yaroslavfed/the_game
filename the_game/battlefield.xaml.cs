﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        int target = -1;

        private ObservableCollection<Enemy_person> enemies_on_screen;
        private ObservableCollection<Enemy_person> enemies_null;

        //List<Grid> enemy_grid = new List<Grid>();
        List<ListBox> enemy_field = new();
        List<int> enemy_added = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer itemInTime = new System.Windows.Threading.DispatcherTimer();
            itemInTime.Tick += new EventHandler(targetInTime_Tick);
            itemInTime.Interval = new TimeSpan(1);
            itemInTime.Start();

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
            enemy_field.AddRange(new ListBox[] { enemy0, enemy1, enemy2, enemy3, enemy4, enemy5, enemy6, enemy7, enemy8 });

            Bots_render(2);
        }

        private void targetInTime_Tick(object sender, EventArgs e)
        {
            target_info.Text = Convert.ToString(target);
        }

        public class Enemy_person
        {
            public string? Rank { get; set; }
            public string? Name { get; set; }
            public double? Health { get; set; }
            public string? Damage { get; set; }
            public string? Protetion { get; set; }
            public string? Reward { get; set; }
            public string? Image { get; set; }
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

                enemies_on_screen = new ObservableCollection<Enemy_person>
                {
                    new Enemy_person() { Rank = rank, Name = name, Health = double.Parse(health), Damage = damage, Protetion = protetion, Reward = reward, Image = System.IO.Path.Combine(resource_paths.enemy_icon_Path, rank + ".png") }
                };

                Random rnd = new((int)(DateTime.Now.Ticks));
                int value = rnd.Next(0, 9);

                if (!enemy_added.Any(str => str == value))
                {
                    enemy_added.Add(value);
                    enemy_field[value].ItemsSource = enemies_on_screen;
                }
                enemy_added.Sort();

                //char delim = ' ';
                //string str = String.Join(delim, enemy_added);
                //MessageBox.Show(str);
            }       
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
            mode_selection mode = new mode_selection(id);
            mode.Show();
            this.Close();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            wave++;
            wave_info.Text = "Волна " + wave;

            Bots_clear();

            int count_bots = wave < 10 ? (count_bots = wave) : (count_bots = 9);

            Bots_render(count_bots);
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
                MessageBox.Show("The target on field " + target + " is attacked");
            }
            else
            {
                MessageBox.Show("First select a goal");
            }
        }
    }
}
