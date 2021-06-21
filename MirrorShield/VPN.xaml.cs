using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// Interaction logic for VPN.xaml
    /// </summary>
    public partial class VPN : Window
    {
        private string fips;
        private int fpi;
        private int spi;

        public VPN()
        {
            InitializeComponent();
            realip.Text = GetIP();
        }

        private string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        public void GetFriendIP(string ip)
        {
            friendip.Text = ip;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ActiveVPN()
        {
            friendip.Text = fips;
            Server sourceserver = new Server(GetIP(), spi);
            Client sourceclient = new Client(GetIP(), fpi);
            Server destinationserver = new Server(fips, fpi);
            Client destinationclient = new Client(fips, spi);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            fips = fip.Text;
            fpi = int.Parse(fp.Text);
            spi = int.Parse(sp.Text);
            ActiveVPN();
        }
    }
}
