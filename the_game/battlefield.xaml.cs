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
        int wave;

        string weapon;
        string protection;

        bool haveDD = false;
        bool haveHeal = false;

        string damageInfo;
        string protectionInfo;
        string healthInfo;


    }
}
