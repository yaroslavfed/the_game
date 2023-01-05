using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class start_page : Window
    {
        static int id = 0;
        static bool coincidences;
        bool Authorized;

        string line = "";
        string pass = "";
        string ID = "";

        bool save = false;

        public start_page()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer authorized = new System.Windows.Threading.DispatcherTimer();
            authorized.Tick += new EventHandler(authorized_Tick);
            authorized.Interval = new TimeSpan(1);
            authorized.Start();

            Authorized = false;
            panel1.Visibility = Visibility.Visible;
            panel2.Visibility = Visibility.Hidden;
            panel3.Visibility = Visibility.Hidden;

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.docPath, "logged.txt")))
            {
                while (!sr.EndOfStream)
                {
                    save = Convert.ToBoolean(sr.ReadLine());
                    if (save == true)
                    {
                        line = sr.ReadLine();
                        pass = sr.ReadLine();
                        ID = sr.ReadLine();

                        Authorized = true;

                        line = line.Substring(line.IndexOf(": ") + 2);
                        pass = pass.Substring(pass.IndexOf(": ") + 2);
                        ID = ID.Substring(ID.IndexOf(": ") + 2);
                    }
                }
            }
        }

        //person.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.hero_icon_Path, "start_page_person.png")));
        //person.Source = null;

        private void authorized_Tick(object sender, EventArgs e)
        {
            if (Authorized == true)
            {
                to_game.IsEnabled = true;
                //to_game.Opacity = 1.0;
                nickname.Content = line;

                panel3.Visibility = Visibility.Visible;
                panel1.Visibility = Visibility.Hidden;
                panel2.Visibility = Visibility.Hidden;
                person.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resource_paths.hero_icon_Path, "start_page_person.png")));
            }
            else
            {
                to_game.IsEnabled = false;
                to_game.Opacity = 0.4;
                nickname.Content = "";

                //panel1.Visibility = Visibility.Visible;
                //panel2.Visibility = Visibility.Hidden;
                panel3.Visibility = Visibility.Hidden;
                person.Source = null;
            }
        }

        private void MediaElement_Loaded(object sender, EventArgs e)
        {
            media_back.Source = new Uri(System.IO.Path.Combine(resource_paths.media_back_path, "back_start.mp4"));
        }

        private void to_game_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            profile player = new profile(ID);
            player.Show();
            this.Close();
        }

        private void to_settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Настройки");
        }

        private void to_info_about_us_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("О нас");
        }

        private void to_exit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void to_game_MouseEnter(object sender, MouseEventArgs e)
        {
            to_game.Opacity = 0.65;
        }

        private void to_game_MouseLeave(object sender, MouseEventArgs e)
        {
            to_game.Opacity = 1.0;
        }

        private void to_settings_MouseEnter(object sender, MouseEventArgs e)
        {
            to_settings.Opacity = 0.65;
        }

        private void to_settings_MouseLeave(object sender, MouseEventArgs e)
        {
            to_settings.Opacity = 1.0;
        }

        private void to_info_about_us_MouseEnter(object sender, MouseEventArgs e)
        {
            to_info_about_us.Opacity = 0.65;
        }

        private void to_info_about_us_MouseLeave(object sender, MouseEventArgs e)
        {
            to_info_about_us.Opacity = 1.0;
        }

        private void to_exit_MouseEnter(object sender, MouseEventArgs e)
        {
            to_exit.Opacity = 0.65;
        }

        private void to_exit_MouseLeave(object sender, MouseEventArgs e)
        {
            to_exit.Opacity = 1.0;
        }

        private void exitLogin_Click(object sender, EventArgs e)
        {
            Authorized = false;
            panel3.Visibility = Visibility.Hidden;
            panel1.Visibility = Visibility.Visible;

            string path = System.IO.Path.Combine(resource_paths.docPath, "logged.txt");
            string createText = "false" + Environment.NewLine;
            File.WriteAllText(path, createText);

            //startGame.IsEnabled = false;
            //nickname.Content = "";
        }

        #region Registering a new user
        private void sign_up_Click(object sender, EventArgs e)
        {
            coincidences = false;
            rLogin.Text = rLogin.Text.Trim();
            rPass.Password = rPass.Password.Trim();
            rPassD.Password = rPassD.Password.Trim();

            string s = File.ReadAllLines(System.IO.Path.Combine(resource_paths.docPath, "id.txt")).Last();
            id = int.Parse(s.Substring(s.IndexOf(": ") + 2)) + 1;

            //int found = s.IndexOf(": ");
            //s = s.Substring(found + 2);
            //id = Convert.ToInt32(s) + 1;
            //MessageBox.Show(Convert.ToString(id));

            string[] lines = { "Login: " + rLogin.Text, "Password: " + rPass.Password, "ID: " + Convert.ToString(id) };

            string[] search = File.ReadAllLines(System.IO.Path.Combine(resource_paths.docPath, "id.txt"));
            string slovo = rLogin.Text;
            if (search.Contains("Login: " + slovo))
            {
                coincidences = true;
            }

            if (rLogin.Text != String.Empty && rPass.Password != String.Empty && rPassD.Password != String.Empty)
            {
                if (rPass.Password == rPassD.Password)
                {
                    if (coincidences == false)
                    {
                        try
                        {
                            using (StreamWriter outputFile = File.AppendText(System.IO.Path.Combine(resource_paths.docPath, "id.txt")))
                            {
                                foreach (string line in lines)
                                    outputFile.WriteLine(line);
                            }
                            try
                            {
                                using (FileStream fs = File.Create(System.IO.Path.Combine(resource_paths.userPath, id + ".txt")))
                                {
                                    AddText(fs, rLogin.Text);   // логин                0 - номер строки в файле
                                    AddText(fs, "\r\n1");       // уровень              1 - номер строки в файле
                                    AddText(fs, "\r\n0");       // опыт                 2 - номер строки в файле
                                    AddText(fs, "\r\n0");       // деньги               3 - номер строки в файле
                                    AddText(fs, "\r\n10");      // выбранное оружие     4 - номер строки в файле
                                    AddText(fs, "\r\n10");      // склад оружия         5 - номер строки в файле
                                    AddText(fs, "\r\n20");      // выбранная броня      6 - номер строки в файле
                                    AddText(fs, "\r\n20");      // склад брони          7 - номер строки в файле
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            MessageBox.Show("Successful registration");

                            string fileName = System.IO.Path.Combine(resource_paths.docPath, "test.jpg");
                            string fileName1 = id + ".jpg";
                            string sourcePath = resource_paths.docPath;
                            string targetPath = System.IO.Path.Combine(resource_paths.docPath, @"img\usersIcon\");
                            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                            string destFile = System.IO.Path.Combine(targetPath, fileName1);
                            File.Copy(sourceFile, destFile, true);

                            panel2.Visibility = Visibility.Hidden;
                            panel1.Visibility = Visibility.Visible;
                            rLogin.Text = "";
                            rPass.Password = "";
                            rPassD.Password = "";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        id++;
                    }
                    else
                    {
                        MessageBox.Show("Login is busy");
                    }
                }
                else
                {
                    MessageBox.Show("Passwords don't match");
                }
            }
            else
            {
                MessageBox.Show("Empty fields");
            }
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }
        #endregion

        private void log_Click(object sender, RoutedEventArgs e)
        {
            panel2.Visibility = Visibility.Hidden;
            panel1.Visibility = Visibility.Visible;
        }

        private void remember_Checked(object sender, RoutedEventArgs e)
        {
            save = true;
        }

        private void remember_Unchecked(object sender, RoutedEventArgs e)
        {
            save = false;
        }

        private void sign_in_Click(object sender, RoutedEventArgs e)
        {
            bool retrieval = false;

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(resource_paths.docPath, "id.txt")))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line == "Login: " + lLogin.Text)
                    {
                        retrieval = true;
                        pass = sr.ReadLine();
                        ID = sr.ReadLine();
                        break;
                    }
                }
            }
            if (retrieval == true)
            {
                if ("Password: " + lPass.Password == pass)
                {
                    Authorized = true;

                    line = line.Substring(line.IndexOf(": ") + 2);
                    pass = pass.Substring(pass.IndexOf(": ") + 2);
                    ID = ID.Substring(ID.IndexOf(": ") + 2);
                    lLogin.Text = "";
                    lPass.Password = "";

                    string path = System.IO.Path.Combine(resource_paths.docPath, "logged.txt");
                    if (save == true)
                    {
                        string createText = "true" + Environment.NewLine + "Login: " + line + Environment.NewLine + "Password: " + pass + Environment.NewLine + "ID: " + ID + Environment.NewLine;
                        File.WriteAllText(path, createText);
                    }
                    else
                    {
                        string createText = "false" + Environment.NewLine;
                        File.WriteAllText(path, createText);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid password");
                }
            }
            else
            {
                MessageBox.Show("User not found");
            }
        }

        private void reg_Click(object sender, RoutedEventArgs e)
        {
            panel1.Visibility = Visibility.Hidden;
            panel2.Visibility = Visibility.Visible;
        }
    }
}
