using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace MirrorShield
{
    class Policy
    {
        private Packet p; // the packet will be recieved when the packet collector will be activated
        private Dictionary<string, string> dterms; // the dictionary that the terms are there with allow or deny
        
        public Policy(string ooi, string srcip, string srcp, string destip, string destp, string action)
        {
            dterms = new Dictionary<string, string>();
            dterms.Add("Source IP", srcip);
            dterms.Add("Source Port", srcp);
            dterms.Add("Destination IP", destip);
            dterms.Add("Destination Port", destp);
            dterms.Add("Direction", ooi);
            dterms.Add("Action", action);
        }

        public void SetPacket(Packet p)
        {
            this.p = p;
        }

        public string GetSIP()
        {
            return dterms["Source IP"];
        }

        public int GetSP()
        {
            return int.Parse(dterms["Source Port"]);
        }

        public string GetDIP()
        {
            return dterms["Destination IP"];
        }

        public int GetDP()
        {
            return int.Parse(dterms["Destination Port"]);
        }

        public void SetToTerm(string action, string term)
        {
            dterms[term] = action;
        }

        public bool VPN()
        {
            if (dterms["Action"] == "VPN")
                return true;
            return false;
        }

        public bool Fine()
        {
            if (dterms["Direction"] == "Outbound")
            {
                if (p.IpV4.Source.ToString() != dterms["Source IP"])
                    return false;
            }

            else if (dterms["Direction"] == "Inbound")
            {
                if (p.IpV4.Destination.ToString() != dterms["Source IP"])
                    return false;
            }

            if (dterms["Source Port"] != "Any")
            {
                if (p.IpV4.Tcp.SourcePort.ToString() != dterms["Source Port"])
                    return false;
            }

            if (dterms["Destination Port"] != "Any")
            {
                if (p.IpV4.Tcp.DestinationPort.ToString() != dterms["Destination Port"])
                    return false;
            }

            if (dterms["Source IP"] != "Any")
            {
                if (p.IpV4.Source.ToString() != dterms["Source IP"])
                    return false;
            }

            if (dterms["Destination IP"] != "Any")
            {
                if (p.IpV4.Destination.ToString() != dterms["Destination IP"])
                    return false;
            }

            if (dterms["Action"] == "Deny")
                return false;

            else if (dterms["Action"] == "Allow")
                return true;

            return true;
        }
    }
}
