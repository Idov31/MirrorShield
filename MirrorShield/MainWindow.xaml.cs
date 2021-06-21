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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace MirrorShield
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Firewall fw;
        private VPN vpn;

        public MainWindow()
        {
            InitializeComponent();
            uname.Text = "Guest!";
            AddLog();
            ShowLogs();
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            datefw.Text = xdoc.Element("Indexs").Element("FireWallDate").Value;
            datevpn.Text = xdoc.Element("Indexs").Element("VPNDate").Value;
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Login l = new Login();
            l.Show();
        }

        private void Login_In(object sender, MouseEventArgs e)
        {
            login.Foreground = Brushes.Red;
        }

        private void Login_Out(object sender, MouseEventArgs e)
        {
            login.Foreground = Brushes.Gold;
        }

        public void SetUser(string user)
        {
            uname.Text = user;
        }

        private int GetIndex()
        {
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            string tempobj = xdoc.Element("Indexs").Element("LogsIndex").Value;
            int index = int.Parse(tempobj);
            int settedindex;
            settedindex = index + 1;
            xdoc.Element("Indexs").SetElementValue("LogsIndex", settedindex);
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            return settedindex;
        }

        private void AddLog()
        {
            int index = GetIndex();
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Logs.xml");
            XElement root = new XElement("Log");
            XElement xip = new XElement("SourceIP");
            XElement xdate = new XElement("Date");
            xip.SetValue(GetIP());
            xdate.SetValue(DateTime.Now.ToString("dd/MM/yyyy"));
            root.Add(xip);
            root.Add(xdate);
            root.SetAttributeValue("Index", index);
            xdoc.Element("Logs").Add(root);
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Logs.xml");
        }

        private string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return (ip.ToString());
                }
            }
            return "";
        }

        private void ShowLogs()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Logs.xml");

            foreach (XmlNode node in xdoc.DocumentElement)
            {
                string ip = node["SourceIP"].InnerText;
                string date = node["Date"].InnerText;
                string st = ip + " , " + date;
                logslb.Items.Add(st);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Exit_In(object sender, RoutedEventArgs e)
        {
            exit.Foreground = Brushes.Red;
        }

        private void Exit_Out(object sender, RoutedEventArgs e)
        {
            exit.Foreground = Brushes.Gold;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Register r = new Register();
            r.Show();
        }

        private void Register_In(object sender, RoutedEventArgs e)
        {
            register.Foreground = Brushes.Red;
        }

        private void Register_Out(object sender, RoutedEventArgs e)
        {
            register.Foreground = Brushes.Gold;
        }

        private void Guild_In(object sender, RoutedEventArgs e)
        {
            guild.Foreground = Brushes.Red;
        }

        private void Guild_Out(object sender, RoutedEventArgs e)
        {
            guild.Foreground = Brushes.Gold;
        }

        private void ActiveFW_Click(object sender, RoutedEventArgs e)
        {
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            xdoc.Element("Indexs").Element("FireWallDate").Value = DateTime.Now.ToString("dd/MM/yyyy");
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            datefw.Text = DateTime.Now.ToString("dd/MM/yyyy");
            statusfw.Text = "Online";
            fw = new Firewall();
            fw.Show();
        }

        private void ActiveFW_In(object sender, RoutedEventArgs e)
        {
            activefw.Foreground = Brushes.Red;
        }

        private void ActiveFW_Out(object sender, RoutedEventArgs e)
        {
            activefw.Foreground = Brushes.Gold;
        }

        private void DisactiveFW_Click(object sender, RoutedEventArgs e)
        {
            statusfw.Text = "Offline";
            fw.Close();
        }

        private void DisactiveFW_In(object sender, RoutedEventArgs e)
        {
            disactivefw.Foreground = Brushes.Red;
        }

        private void DisactiveFW_Out(object sender, RoutedEventArgs e)
        {
            disactivefw.Foreground = Brushes.Gold;
        }

        private void ActiveVPN_In(object sender, RoutedEventArgs e)
        {
            activevpn.Foreground = Brushes.Red;
        }

        private void ActiveVPN_Out(object sender, RoutedEventArgs e)
        {
            activevpn.Foreground = Brushes.Gold;
        }

        private void DisactiveVPN_In(object sender, RoutedEventArgs e)
        {
            disactivevpn.Foreground = Brushes.Red;
        }

        private void DisactiveVPN_Out(object sender, RoutedEventArgs e)
        {
            disactivevpn.Foreground = Brushes.Gold;
        }

        private void DisactiveVPN_Click(object sender, RoutedEventArgs e)
        {
            statusvpn.Text = "Offline";
            vpn.Close();
        }

        private void ActiveVPN_Click(object sender, RoutedEventArgs e)
        {
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            xdoc.Element("Indexs").Element("VPNDate").Value = DateTime.Now.ToString("dd/MM/yyyy");
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            datevpn.Text = DateTime.Now.ToString("dd/MM/yyyy");
            statusvpn.Text = "Online";
            vpn = new VPN();
            vpn.Show();
        }

        private void Guild_Click(object sender, RoutedEventArgs e)
        {
            Guild guild = new Guild();
            guild.Show();
        }
    }
}
