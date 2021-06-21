using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace MirrorShield
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Register r = new Register();
            r.Show();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            DBManagement dbm = new DBManagement();
            if (dbm.Islogin(uname.Text, psw.Password))
            {
                MainWindow mw = new MainWindow();
                mw.SetUser(uname.Text);
                mw.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Username Or Password Is Incorrect");
            }
        }
    }
}
