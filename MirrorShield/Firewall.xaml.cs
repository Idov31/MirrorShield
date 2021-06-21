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
using System.Xml;
using System.Xml.Linq;

using System.Threading;

using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Dns;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Gre;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Igmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace MirrorShield
{
    /// <summary>
    /// Interaction logic for Firewall.xaml
    /// </summary>
    public partial class Firewall : Window
    {
        private int times = 0;
        private int timem = 0;
        private int timeh = 0;
        private Dictionary<int, object[]> fd;

        public Firewall()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            fd = new Dictionary<int, object[]>();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
            ShowFW();
            SetFD();
        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            times++;
            if (times / 60 == 1)
            {
                timem++;
                times = 0;
            }
            if (timem / 60 == 1)
            {
                timeh++;
                timem = 0;
            }
            time.Text = timeh + ":" + timem + ":" + times;
        }

        private void SetFD()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            int index = 1;
            string[] arr = new string[8];
            foreach (XmlNode node in xdoc.DocumentElement)
            {
                arr[0] = node["SourceIP"].InnerText;
                arr[1] = node["SourcePort"].InnerText;
                arr[2] = node["DestinationPort"].InnerText;
                arr[3] = node["DestinationIP"].InnerText;
                arr[4] = node["Direction"].InnerText;
                arr[5] = node["Action"].InnerText;
                arr[6] = node["DeviceA"].InnerText;
                arr[7] = node["DeviceB"].InnerText;
                fd.Add(index, arr);
                index++;
            }
        }

        public void ShowFW()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            int index = 1;
            foreach (XmlNode node in xdoc.DocumentElement)
            {
                string ips = node["SourceIP"].InnerText;
                string ps = node["SourcePort"].InnerText;
                string pd = node["DestinationPort"].InnerText;
                string ipd = node["DestinationIP"].InnerText;
                string dr = node["Direction"].InnerText;
                string ac = node["Action"].InnerText;
                string adev = node["DeviceA"].InnerText;
                string bdev = node["DeviceB"].InnerText;
                string st = index + ":  SourceIP: " + ips + " , SourcePort: " + ps + " , DestinationIP: " + ipd + " , DestinationPort: " + pd + " , Direction: " + dr + " , Action: " + ac + " , DeviceA: " + adev + " , DeviceB: " + bdev;
                policylb.Items.Add(st);
                index++;
            }
        }

        private int GetIndex()
        {
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            string tempobj = xdoc.Element("Indexs").Element("PoliciesIndex").Value;
            int index = int.Parse(tempobj);
            int settedindex;
            settedindex = index + 1;
            xdoc.Element("Indexs").SetElementValue("PoliciesIndex", settedindex);
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            return settedindex;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            xdoc["Rules"].RemoveAll();
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            AddNewElement();
            AddToXML();
            policylb.Items.Clear();
            ShowFW();
        }

        private void AddNewElement()
        {
            int index = GetIndex();
            string[] arr = new string[8];
            arr[0] = sip.Text;
            arr[1] = sp.Text;
            arr[2] = dp.Text;
            arr[3] = dip.Text;
            arr[4] = direction.Text;
            arr[5] = action.Text;
            arr[6] = deva.Text;
            arr[7] = devb.Text;
            fd.Add(index, arr);
        }

        private void AddToXML()
        {
            XDocument xdoc = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            for (int i = 0; i < fd.Count; i++)
            {
                int temp = i + 1;
                if (temp <= fd.Count)
                {
                    object[] arrtemp = fd[temp];

                    if (temp <= fd.Count)
                    {
                        XElement root = new XElement("Rule");
                        XElement sip = new XElement("SourceIP");
                        XElement dip = new XElement("DestinationIP");
                        XElement sp = new XElement("SourcePort");
                        XElement dp = new XElement("DestinationPort");
                        XElement direction = new XElement("Direction");
                        XElement action = new XElement("Action");
                        XElement deva = new XElement("DeviceA");
                        XElement devb = new XElement("DeviceB");
                        root.SetAttributeValue("Index", temp);
                        sip.Value = arrtemp[0].ToString();
                        sp.Value = arrtemp[1].ToString();
                        dp.Value = arrtemp[2].ToString();
                        dip.Value = arrtemp[3].ToString();
                        direction.Value = arrtemp[4].ToString();
                        action.Value = arrtemp[5].ToString();
                        deva.Value = arrtemp[6].ToString();
                        devb.Value = arrtemp[7].ToString();
                        root.Add(sip);
                        root.Add(sp);
                        root.Add(dp);
                        root.Add(dip);
                        root.Add(direction);
                        root.Add(action);
                        root.Add(deva);
                        root.Add(devb);
                        xdoc.Element("Rules").Add(root);
                    }
                }
            }
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            xdoc["Rules"].RemoveAll();
            xdoc.Save("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            ChangePolicy();
            AddToXML();
            policylb.Items.Clear();
            ShowFW();
        }

        private void ChangePolicy()
        {
            int index = policylb.SelectedIndex + 1;
            string[] arr = new string[8];
            arr[0] = sip.Text;
            arr[1] = sp.Text;
            arr[2] = dp.Text;
            arr[3] = dip.Text;
            arr[4] = direction.Text;
            arr[5] = action.Text;
            arr[6] = deva.Text;
            arr[7] = devb.Text;
            fd[index] = arr;
        }

        private void Active_Click(object sender, RoutedEventArgs e)
        {
            Policy p;
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Policies.xml");
            XDocument xdoc2 = XDocument.Load("D:\\VS_Projects\\MirrorShield\\MirrorShield\\Indexs.xml");
            string index = policylb.SelectedIndex.ToString();
            string deva, devb;
            foreach (XmlNode node in xdoc.DocumentElement)
            {
                if (node.Attributes[0].Value == index)
                {
                    string ips = node["SourceIP"].InnerText;
                    string ps = node["SourcePort"].InnerText;
                    string pd = node["DestinationPort"].InnerText;
                    string ipd = node["DestinationIP"].InnerText;
                    string dr = node["Direction"].InnerText;
                    string ac = node["Action"].InnerText;
                    p = new Policy(dr, ips, ps, ipd, pd, ac);
                    deva = node["DeviceA"].InnerText;
                    devb = node["DeviceB"].InnerText;
                    showmebaby.Text = "You are now secured by MIRRORSHIELD";
                    Shielding(sender, e, p, deva, devb);
                }
            }
        }

        private void ActiveVPN(string sip, string dip, int sp, int dp)
        {
            Server sourceserver = new Server(sip, sp);
            Client sourceclient = new Client(sip, dp);
            Server destinationserver = new Server(dip, dp);
            Client destinationclient = new Client(dip, sp);
        }

        private void Shielding(object sender, RoutedEventArgs e, Policy p, string deva, string devb)
        {
            int result = 0;
            bool flag = false;
            PacketDevice selectedDeviceA = ChooseADevice(sender, e, deva);
            PacketDevice selectedDeviceB = ChooseADevice(sender, e, devb);
            Dictionary<string, Session> se = new Dictionary<string, Session>();
            if (p.VPN())
            {
                flag = true;
            }
            ConThread txrx = new ConThread(selectedDeviceA, selectedDeviceB, p, se);

            Thread tha = new Thread(new ThreadStart(txrx.RunA));
            Thread thb = new Thread(new ThreadStart(txrx.RunB));

            try
            {
                tha.Start();
                thb.Start();
            }
            catch (ThreadStateException e1)
            {
                Console.WriteLine(e1);  // Display text of exception
                result = 1;            // Result says there was an error
            }
            catch (ThreadInterruptedException e1)
            {
                Console.WriteLine(e1);  // This exception means that the thread
                                        // was interrupted during a Wait
                result = 1;            // Result says there was an error
            }
        }

        private PacketDevice ChooseADevice(object sender, RoutedEventArgs e, string reqDev)
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return null;
            }

            int deviceIndex = 0;
            do
            {
                string deviceIndexString = reqDev;
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

            return selectedDevice;
        }

        private void IP_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text == "Enter spesific IP/ Any")
                txtBox.Text = string.Empty;
        }

        private void Port_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text == "Enter spesific Port/Any")
                txtBox.Text = string.Empty;
        }

        private void Direction_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text == "Inbound/Outbound/Any")
                txtBox.Text = string.Empty;
        }

        private void Action_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text == "Allow / Deny / VPN")
                txtBox.Text = string.Empty;
        }

        private void Devices_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text == "1 / 2 / 3")
                txtBox.Text = string.Empty;
        }
    }
}