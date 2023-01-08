using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
using static System.Net.Mime.MediaTypeNames;

namespace the_game
{
    public partial class store : Window
    {
        public store(string id)
        {
            InitializeComponent();
            this.id = id;
        }
        readonly string id;
        int count;  // всего элементов инвентаря - оружие
        int countA;  // всего элементов инвентаря - оружие

        int nums;   // имеется элементов инвентаря - броня
        int numsA;   // имеется элементов инвентаря - броня

        bool ownership;
        bool ownershipA;

        string nickname;
        string levelNow;
        int money;
        int invPriceW;
        int invPriceA;

        string filenameW;
        string filenameA;
        int weaponNum = 0;
        int armorNum = 0;

        string weaponsOwn;
        string armorsOwn;

        string weaponDonned;
        string armorDonned;

        string[] w_subs;
        string[] a_subs;

        List<string> rarity_color = new List<string>() { "#97D6E8", "#3F48CB", "#9F46A1", "#FE00A5", "#FEC80D", "#EB1A23" };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer invInTime = new System.Windows.Threading.DispatcherTimer();
            invInTime.Tick += new EventHandler(invInTime_Tick);
            invInTime.Interval = new TimeSpan(1);
            invInTime.Start();

            nickname = File.ReadLines(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")).First();
            levelNow = File.ReadLines(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")).Skip(1).First();

            name.Text = nickname;
            level.Text = "УРОВЕНЬ " + levelNow;

            count = new DirectoryInfo(resource_paths.weaponPath).GetFiles().Length;
            countA = new DirectoryInfo(resource_paths.armorPath).GetFiles().Length;

            recalculation(ref weaponsOwn, ref weaponDonned, 5, 4, ref w_subs, ref nums);
            recalculation(ref armorsOwn, ref armorDonned, 7, 6, ref a_subs, ref numsA);
        }

        private void recalculation(ref string ItemsOwn, ref string ItemDonned, int nLine, int donnedLine, ref string[] subs, ref int num)
        {
            ItemDonned = File.ReadLines(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")).Skip(donnedLine).First();
            ItemsOwn = File.ReadLines(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")).Skip(nLine).First();

            subs = ItemsOwn.Split(' ');

            num = 0;
            foreach (var sub in subs)
            {
                num++;
            }
        }
        private void access(Button purchase, Button put_on, Button next, Button prev, string donned, int invNum, int counts, bool have, string fileName, string[] subs, int num)
        {
            if (fileName == donned)
            {
                put_on.IsEnabled = false;
                put_on.Content = "НАДЕТО";
            }
            else
            {
                put_on.IsEnabled = true;
                put_on.Content = "НАДЕТЬ";
            }

            have = false;
            for (int i = 0; i < num; i++)
            {
                if (subs[i] == fileName)
                {
                    purchase.IsEnabled = false;
                    purchase.Content = "ИМЕЕТСЯ";
                    have = true;
                    break;
                }
            }
            if (have == false)
            {
                purchase.IsEnabled = true;
                purchase.Content = "КУПИТЬ";
            }

            if (invNum == 0)
            {
                prev.Visibility = Visibility.Hidden;
                prev.IsEnabled = false;
            }
            else
            {
                prev.Visibility = Visibility.Visible;
                prev.IsEnabled = true;
            }
            if (invNum == counts - 1)
            {
                next.Visibility = Visibility.Hidden;
                next.IsEnabled = false;
            }
            else
            {
                next.Visibility = Visibility.Visible;
                next.IsEnabled = true;
            }
        }

        private void invInTime_Tick(object sender, EventArgs e)
        {
            money = Convert.ToInt32(File.ReadLines(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")).Skip(3).First());
            cash.Text = money + " $";

            filenameW = "1" + Convert.ToString(weaponNum);
            filenameA = "2" + Convert.ToString(armorNum);

            weaponNow.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.weaponPath, filenameW + ".png")));
            armorNow.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.armorPath, filenameA + ".jpg")));

            img_weapon_donned.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.weaponPath, weaponDonned + ".png")));
            img_armor_donned.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.armorPath, armorDonned + ".jpg")));

            access(buy, put, next, prev, weaponDonned, weaponNum, count, ownership, filenameW, w_subs, nums);
            access(buyA, putA, nextA, prevA, armorDonned, armorNum, countA, ownershipA, filenameA, a_subs, numsA);

            price(ref invPriceW, filenameW, wCost);
            price(ref invPriceA, filenameA, aCost);

            rare(filenameW, weapon_rarity_now);
            rare(filenameA, armor_rarity_now);

            rare(weaponDonned, weapon_rarity_donned);
            rare(armorDonned, armor_rarity_donned);
        }

        private void rare(string inventory_id, System.Windows.Controls.Label weapon_rarity)
        {
            int rarity = Convert.ToInt32(File.ReadLines(System.IO.Path.Combine(resource_paths.inventoryPath, inventory_id + ".txt")).Skip(2).First());
            weapon_rarity.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(rarity_color[rarity]);
        }

        private void price(ref int invPrice, string filename, TextBlock cost)
        {
            invPrice = Convert.ToInt32(File.ReadLines(System.IO.Path.Combine(resource_paths.inventoryPath, filename + ".txt")).Skip(1).First());
            cost.Text = invPrice + " $";
        }

        private void rewrite(bool opt, int moneyNow, string itemsOwn, string filename, int index)
        {
            string newValue = "";
            int lineIndex = index;

            if (opt == true)
            {
                newValue = itemsOwn + ' ' + filename;
            }
            else if (opt == false)
            {
                newValue = Convert.ToString(moneyNow);
            }

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

        private void put_on_inventory(string filename, int index)
        {
            string newValue = filename;
            int lineIndex = index;
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

        private void next_Click(object sender, EventArgs e)
        {
            if (weaponNum < count - 1)
                weaponNum++;
        }

        private void prev_Click(object sender, EventArgs e)
        {
            if (weaponNum > 0)
                weaponNum--;
        }

        private void nextA_Click(object sender, EventArgs e)
        {
            if (armorNum < countA - 1)
                armorNum++;
        }

        private void prevA_Click(object sender, EventArgs e)
        {
            if (armorNum > 0)
                armorNum--;
        }

        private void buy_Click(object sender, EventArgs e)
        {
            if (money - invPriceW >= 0)
            {
                rewrite(true, 0, weaponsOwn, filenameW, 5);
                recalculation(ref weaponsOwn, ref weaponDonned, 5, 4, ref w_subs, ref nums);
                rewrite(false, money -= invPriceW, "0", "0", 3);
            }
            else
            {
                MessageBox.Show("Insufficient funds");
            }
        }

        private void buyA_Click(object sender, EventArgs e)
        {
            if (money - invPriceA >= 0)
            {
                rewrite(true, 0, armorsOwn, filenameA, 7);
                recalculation(ref armorsOwn, ref armorDonned, 7, 6, ref a_subs, ref numsA);
                rewrite(false, money -= invPriceA, "0", "0", 3);
            }
            else
            {
                MessageBox.Show("Insufficient funds");
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            profile player = new profile(id);
            player.Show();
            this.Close();
        }

        private void put_Click(object sender, RoutedEventArgs e)
        {
            put_on_inventory(filenameW, 4);
            recalculation(ref weaponsOwn, ref weaponDonned, 5, 4, ref w_subs, ref nums);
        }

        private void putA_Click(object sender, RoutedEventArgs e)
        {
            put_on_inventory(filenameA, 6);
            recalculation(ref armorsOwn, ref armorDonned, 7, 6, ref a_subs, ref numsA);
        }
    }
}
