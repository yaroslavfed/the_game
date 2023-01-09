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
        public results_game(string id, int wave, int total_reward, int total_exp)
        {
            InitializeComponent();
            this.id = id;
            this.wave = wave;
            this.total_reward = total_reward;
            this.total_exp = total_exp;
        }
        readonly string id;
        readonly int wave;
        readonly int total_reward;
        readonly int total_exp;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            result_text.Text = string.Format("Боец ты продержался {0} волн и заработал {1} монет, прикупи снаряжение получше и возвращайся за новыми рекордами", wave, total_reward);
            result_wave.Text = wave.ToString();
            result_reward.Text = total_reward.ToString();
            result_experience.Text = total_exp.ToString();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            mode_selection mode = new mode_selection(id);
            mode.Show();
            this.Close();
        }
    }
}
