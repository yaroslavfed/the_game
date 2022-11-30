﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace the_game
{
    public partial class profile : Window
    {
        public profile(string id)
        {
            InitializeComponent();
            this.id = id;
        }
        readonly string id;

        string nickname;
        string levelNow;
        string exp;
        string money;
        string weapon;
        string protection;
        bool selection = false;
        bool selection1 = false;

        bool haveDD = false;
        bool haveHeal = false;

        string damageInfo;
        string protectionInfo;
        string healthInfo;

        public static readonly string userPath = System.IO.Directory.GetCurrentDirectory() + @"\users\";
        public static readonly string iconPath = System.IO.Directory.GetCurrentDirectory() + @"\img\usersIcon\";
        public static readonly string hero_icon_Path = System.IO.Directory.GetCurrentDirectory() + @"\img\heroIcon\";
        public static readonly string weaponPath = System.IO.Directory.GetCurrentDirectory() + @"\img\wIcon\";
        public static readonly string armorPath = System.IO.Directory.GetCurrentDirectory() + @"\img\aIcon\";
        public static readonly string spellsPath = System.IO.Directory.GetCurrentDirectory() + @"\img\spells\";
        public static readonly string inventoryPath = System.IO.Directory.GetCurrentDirectory() + @"\inventory\";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer itemInTime = new System.Windows.Threading.DispatcherTimer();
            itemInTime.Tick += new EventHandler(itemInTime_Tick);
            itemInTime.Interval = new TimeSpan(1);
            itemInTime.Start();
            
            System.Windows.Threading.DispatcherTimer invInfo = new System.Windows.Threading.DispatcherTimer();
            invInfo.Tick += new EventHandler(invInfo_Tick);
            invInfo.Interval = new TimeSpan(1);
            invInfo.Start();

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(userPath, id + ".txt")))
            {
                nickname = sr.ReadLine();
                levelNow = sr.ReadLine();
                exp = sr.ReadLine();
                money = sr.ReadLine();
                weapon = sr.ReadLine();
                string zero = sr.ReadLine();
                protection = sr.ReadLine();
            }
            name.Content = nickname;
            level.Content = "УРОВЕНЬ " + levelNow;
            //lvlprogress.Value = Convert.ToInt32(exp);

            foto.Source = new BitmapImage(new Uri(System.IO.Path.Combine(iconPath, id + ".jpg")));
            person.Source = new BitmapImage(new Uri(System.IO.Path.Combine(hero_icon_Path, "person_test_3.png")));
            cash.Content = money + "$";

            damageInfo = "0";
            protectionInfo = "0";
            healthInfo = Convert.ToString(100 + Convert.ToInt32(levelNow) * 2);
            health.Text = healthInfo;

            listView.ItemsSource = null;
            listView.Items.Clear();

            if (Convert.ToInt32(levelNow) >= 25)
            {
                ddSpell.Source = new BitmapImage(new Uri(System.IO.Path.Combine(spellsPath, "dd.jpg")));
                haveDD = true;
            }
            else
            {
                ddSpell.Source = new BitmapImage(new Uri(System.IO.Path.Combine(spellsPath, "ddF.jpg")));
            }

            if (Convert.ToInt32(levelNow) >= 50)
            {
                healSpell.Source = new BitmapImage(new Uri(System.IO.Path.Combine(spellsPath, "heal.jpg")));
                haveHeal = true;
            }
            else
            {
                healSpell.Source = new BitmapImage(new Uri(System.IO.Path.Combine(spellsPath, "healF.jpg")));
            }
        }

        private void itemInTime_Tick(object sender, EventArgs e)
        {
            weapon = File.ReadLines(System.IO.Path.Combine(userPath, id + ".txt")).Skip(4).First();
            protection = File.ReadLines(System.IO.Path.Combine(userPath, id + ".txt")).Skip(6).First();

            weaponIcon.Source = new BitmapImage(new Uri(System.IO.Path.Combine(weaponPath, weapon + ".jpg")));
            armorIcon.Source = new BitmapImage(new Uri(System.IO.Path.Combine(armorPath, protection + ".jpg")));

            if (selection == true)
            {
                weaponButton.IsEnabled = false;
                armorButton.IsEnabled = false;
            }
            else
            {
                weaponButton.IsEnabled = true;
                armorButton.IsEnabled = true;
            }
        }

        private void invInfo_Tick(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(inventoryPath, weapon + ".txt")))
            {
                damageInfo = sr.ReadLine();
            }

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(inventoryPath, protection + ".txt")))
            {
                protectionInfo = sr.ReadLine();
            }

            damage.Text = damageInfo;
            armor.Text = protectionInfo;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            start_page menu = new start_page();
            menu.Show();
            this.Close();
        }

        private void to_map_Click(object sender, RoutedEventArgs e)
        {
            //Form3 map = new Form3(id, haveDD, haveHeal, damageInfo, protectionInfo)
            //{
            //    StartPosition = FormStartPosition.Manual,
            //    Location = Location,
            //    Size = Size
            //};
            //map.Show();
            //this.Close();
        }

        private void store_Click(object sender, RoutedEventArgs e)
        {
            store shop = new store(id);
            shop.Show();
            this.Close();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ImageButton_Click");
        }

        public class Item
        {
            public string number { get; set; }
            public string itemImg { get; set; }
        }

        int count = 0;
        private void weaponIcon_Click(object sender, RoutedEventArgs e)
        {
            View.Header = "Оружие";
            View.Width = listView.Width;
            listView.Visibility = Visibility.Visible;
            string weaponsOwn = File.ReadLines(System.IO.Path.Combine(userPath, id + ".txt")).Skip(5).First();
            string[] subs = weaponsOwn.Split(' ');

            foreach (var sub in subs)
            {
                int str = Convert.ToInt32(sub.Remove(0, 1));
                string index = "1" + str;

                Item dataItem = new()
                {
                    number = index,
                    itemImg = System.IO.Path.Combine(weaponPath, index + ".jpg")
                };

                listView.Items.Add(dataItem);
                count++;
            }
            selection = true;
        }

        int count1 = 0;
        private void armorIcon_Click(object sender, RoutedEventArgs e)
        {
            View.Header = "Броня";
            View.Width = invList.Width;
            listView.Visibility = Visibility.Visible;
            string armorOwn = File.ReadLines(System.IO.Path.Combine(userPath, id + ".txt")).Skip(7).First();
            string[] subs = armorOwn.Split(' ');

            foreach (var sub in subs)
            {
                int str = Convert.ToInt32(sub.Remove(0, 1));
                string index = "2" + str;

                Item dataItem = new Item()
                {
                    number = index,
                    itemImg = System.IO.Path.Combine(armorPath, index + ".jpg")
                };

                listView.Items.Add(dataItem);
                count1++;
            }
            selection = true;
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedItem != null)
            {
                string line = (listView.SelectedItem as Item).number;

                if (line[0] == '1')
                {
                    string newValue = "";
                    int lineIndex = 4;

                    newValue = line;

                    string path = System.IO.Path.Combine(userPath, id + ".txt");

                    int i = 0;
                    string tempPath = path + ".tmp";
                    using (StreamReader sr = new StreamReader(path)) // читаем
                    using (StreamWriter sw = new StreamWriter(tempPath)) // и сразу же пишем во временный файл
                    {
                        while (!sr.EndOfStream)
                        {
                            string srt = sr.ReadLine();
                            if (lineIndex == i)
                                sw.WriteLine(newValue);
                            else
                                sw.WriteLine(srt);
                            i++;
                        }
                    }
                    File.Delete(path); // удаляем старый файл
                    File.Move(tempPath, path); // переименовываем временный файл
                }
                else if (line[0] == '2')
                {
                    string newValue1 = "";
                    int lineIndex1 = 6;

                    newValue1 = line;

                    string path1 = System.IO.Path.Combine(userPath, id + ".txt");

                    int t = 0;
                    string tempPath1 = path1 + ".tmp";
                    using (StreamReader sr = new StreamReader(path1)) // читаем
                    using (StreamWriter sw = new StreamWriter(tempPath1)) // и сразу же пишем во временный файл
                    {
                        while (!sr.EndOfStream)
                        {
                            string line1 = sr.ReadLine();
                            if (lineIndex1 == t)
                                sw.WriteLine(newValue1);
                            else
                                sw.WriteLine(line1);
                            t++;
                        }
                    }
                    File.Delete(path1); // удаляем старый файл
                    File.Move(tempPath1, path1); // переименовываем временный файл
                }

                listView.Visibility = Visibility.Hidden;
                listView.ItemsSource = null;
                listView.Items.Clear();

                selection = false;
            }
        }

        private void invInfoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("infInfo");
        }
    }
}