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
    public partial class results_game : Window
    {
        public results_game(string id, int wave, int total_reward, int total_exp, string weapon, string protection)
        {
            InitializeComponent();
            this.id = id;
            this.wave = wave;
            this.total_reward = total_reward;
            this.total_exp = total_exp;
            this.weapon = weapon;
            this.protection = protection;
        }
        readonly string id;
        readonly int wave;
        readonly int total_reward;
        readonly int total_exp;
        readonly string weapon;
        readonly string protection;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            result_text.Text = string.Format("Боец, ты смог продержаться {0} волн, подмога уже на подходе.\nПолучай свою награду: {1} монет и {2} опыта.", wave, total_reward, total_exp);
            weapon_img.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.weaponPath, weapon + ".png")));
            armor_img.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.armorPath, protection + ".jpg")));
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            mode_selection mode = new mode_selection(id);
            mode.Show();
            this.Close();
        }
    }
}
