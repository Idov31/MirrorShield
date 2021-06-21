using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace MirrorShield
{
    class ConThread
    {
        private PacketDevice DeviceA, DeviceB;
        private Dictionary<UInt48, string> macsTable;
        private Policy policy;
        private Dictionary<string, Session> se;
        private Object BridgeLocObject;
        private Object SessionPolicyLocObject;
        //////////////////////////////////////////////////////////////
        public ConThread(PacketDevice _DeviceA, PacketDevice _DeviceB, Policy _policy, Dictionary<string, Session> _se)
        {
            BridgeLocObject = new Object();
            SessionPolicyLocObject = new Object();
            se = _se;
            DeviceA = _DeviceA;
            DeviceB = _DeviceB;
            policy = _policy;
            macsTable = new Dictionary<UInt48, string>();
        }// end ConThread
        //////////////////////////////////////////////////////////////

        private bool OpenSession(Dictionary<string, Session> se, Packet packet)
        {
            if (packet.Ethernet.EtherType == EthernetType.Arp)
                return true;
            if (packet.Ethernet.EtherType != EthernetType.IpV4)
                return false;
            string key = packet.IpV4.Protocol.ToString() + packet.IpV4.Source.ToString() + packet.IpV4.Destination.ToString() + packet.IpV4.Tcp.SourcePort.ToString() + packet.IpV4.Tcp.DestinationPort.ToString();
            if (se.ContainsKey(key))
                return true;
            return false;
        }


        public void RunA()
        {
            using (PacketCommunicator communicatorARX =
                                     DeviceA.Open(1600,
                                     PacketDeviceOpenAttributes.Promiscuous,
                                     1000),

                                     communicatorBTX =
                                     DeviceB.Open(1600,
                                     PacketDeviceOpenAttributes.Promiscuous,
                                     1000))
            {
                Console.WriteLine("RunA: Listening on Device B - " + DeviceA.Description + "...");
                Packet packet;
                do
                {
                    PacketCommunicatorReceiveResult result = communicatorARX.ReceivePacket(out packet);
                    switch (result)
                    {
                        case PacketCommunicatorReceiveResult.Timeout:
                            continue;
                        case PacketCommunicatorReceiveResult.Ok:
                            //Console.WriteLine(packet.Timestamp.ToString("RunA: RX A <--- B : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);

                            lock (BridgeLocObject)
                            {
                                packet = SimpleBridgeARX(packet); // check if storming
                            }
                            // check if the access to the policy is possible (via using [lock and unlock])    
                            // PUT THE POLICY RIGHT HERE
                            policy.SetPacket(packet);
                            lock (SessionPolicyLocObject)
                            {
                                if (OpenSession(se, packet))
                                {
                                    if (packet != null)
                                    {
                                        LoggerARX(packet);
                                        communicatorBTX.SendPacket(packet);
                                        //Console.WriteLine(packet.Timestamp.ToString("RunA: TX A ---> B : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                                    }
                                }
                                else
                                {
                                    if (policy.Fine())
                                    {
                                        Session ts = new Session(packet.IpV4.Protocol.ToString(), packet.IpV4.Source.ToString(), packet.IpV4.Destination.ToString(), packet.IpV4.Tcp.SourcePort.ToString(), packet.IpV4.Tcp.DestinationPort.ToString());
                                        string tkey = packet.IpV4.Protocol.ToString() + packet.IpV4.Source.ToString() + packet.IpV4.Destination.ToString() + packet.IpV4.Tcp.SourcePort.ToString() + packet.IpV4.Tcp.DestinationPort.ToString();
                                        se.Add(tkey, ts);
                                        if (packet != null)
                                        {
                                            LoggerARX(packet);
                                            communicatorBTX.SendPacket(packet);
                                            //Console.WriteLine(packet.Timestamp.ToString("RunA: TX A ---> B : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                                        }
                                    }
                                }
                            }

                            break;
                        default:
                            throw new InvalidOperationException("RunA: The result " + result + " should never be reached here");
                    }
                } while (true);
            }// end using
        }// RunA

        //////////////////////////////////////////////////////////////

        public void RunB()
        {
            using (PacketCommunicator communicatorBRX =
                            DeviceB.Open(1600,
                            PacketDeviceOpenAttributes.Promiscuous,
                            1000),

                            communicatorATX =
                            DeviceA.Open(1600,
                            PacketDeviceOpenAttributes.Promiscuous,
                            1000))
            {
                Console.WriteLine("RunB: Listening on Device B - " + DeviceB.Description + "...");
                Packet packet;
                do
                {
                    PacketCommunicatorReceiveResult result = communicatorBRX.ReceivePacket(out packet);
                    switch (result)
                    {
                        case PacketCommunicatorReceiveResult.Timeout:
                            continue;
                        case PacketCommunicatorReceiveResult.Ok:
                            //Console.WriteLine(packet.Timestamp.ToString("RunB: RX B <--- A : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);

                            LoggerBRX(packet);
                            lock (SessionPolicyLocObject)
                            {
                                packet = SimpleBridgeBRX(packet); // check for storming
                            }

                            // check if the access to the policy is possible (via using [lock and unlock])
                            // PUT THE POLICY RIGHT HERE
                            policy.SetPacket(packet);
                            lock (SessionPolicyLocObject)
                            {
                                if (OpenSession(se, packet))
                                {
                                    Session ts = new Session(packet.IpV4.Protocol.ToString(), packet.IpV4.Source.ToString(), packet.IpV4.Destination.ToString(), packet.IpV4.Tcp.SourcePort.ToString(), packet.IpV4.Tcp.DestinationPort.ToString());
                                    string tkey = packet.IpV4.Protocol.ToString() + packet.IpV4.Source.ToString() + packet.IpV4.Destination.ToString() + packet.IpV4.Tcp.SourcePort.ToString() + packet.IpV4.Tcp.DestinationPort.ToString();
                                    se.Add(tkey, ts);
                                    if (packet != null)
                                    {
                                        LoggerBRX(packet);
                                        communicatorATX.SendPacket(packet);
                                        //Console.WriteLine(packet.Timestamp.ToString("RunA: TX A ---> B : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                                    }
                                }
                                else
                                {
                                    if (policy.Fine())
                                    {
                                        Session ts = new Session(packet.IpV4.Protocol.ToString(), packet.IpV4.Source.ToString(), packet.IpV4.Destination.ToString(), packet.IpV4.Tcp.SourcePort.ToString(), packet.IpV4.Tcp.DestinationPort.ToString());
                                        string tkey = packet.IpV4.Protocol.ToString() + packet.IpV4.Source.ToString() + packet.IpV4.Destination.ToString() + packet.IpV4.Tcp.SourcePort.ToString() + packet.IpV4.Tcp.DestinationPort.ToString();
                                        se.Add(tkey, ts);
                                        if (packet != null)
                                        {
                                            LoggerBRX(packet);
                                            communicatorATX.SendPacket(packet);
                                            //Console.WriteLine(packet.Timestamp.ToString("RunA: TX A ---> B : yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            throw new InvalidOperationException("RunB: The result " + result + " should never be reached here");
                    }
                } while (true);
            }// end using
        }// end RunB

        //////////////////////////////////////////////////////////////
        private Packet SimpleBridgeARX(Packet p)
        {
            string srcDevice;
            bool macFound;

            lock (macsTable)
            {
                macFound = macsTable.TryGetValue(p.Ethernet.Source.ToValue(), out srcDevice);
                if (macFound)
                {
                    if (srcDevice == "B")
                        p = null;
                }
                else
                    macsTable.Add(p.Ethernet.Source.ToValue(), "A");
            }// unlock

            return p;
        }// end SimpleBridgeARX

        //////////////////////////////////////////////////////////////
        private Packet SimpleBridgeBRX(Packet p)
        {
            string srcDevice;
            bool macFound;

            lock (macsTable)
            {
                macFound = macsTable.TryGetValue(p.Ethernet.Source.ToValue(), out srcDevice);
                if (macFound)
                {
                    if (srcDevice == "A")
                        p = null;
                }
                else
                    macsTable.Add(p.Ethernet.Source.ToValue(), "B");
            }// unlock

            return p;
        }// end SimpleBridgeBRX

        //////////////////////////////////////////////////////////////
        private void LoggerARX(Packet p)
        {
            string log = "LoggerARX:  ";

            switch (p.Ethernet.EtherType)
            {
                case EthernetType.Arp:
                    if (p.Ethernet.Arp.Operation == ArpOperation.Request)
                    {
                        log += "Arp Req - Who has? "; // +p.Ethernet.Arp.TargetProtocolAddress.ToString() + " Tell " + p.Ethernet.Arp.SenderProtocolAddress.ToString();
                        log += p.Ethernet.Arp.TargetProtocolAddress[0] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[1] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[2] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[3];
                        log += " Tell ";
                        log += p.Ethernet.Arp.SenderProtocolAddress[0] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[1] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[2] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[3];
                        Console.WriteLine(log);
                    }
                    break;
                case EthernetType.IpV4:
                    break;
            }//end switch
        }// end LoggerARX

        //////////////////////////////////////////////////////////////
        private void LoggerBRX(Packet p)
        {
            string log = "LoggerBRX:  ";

            switch (p.Ethernet.EtherType)
            {
                case EthernetType.Arp:
                    if (p.Ethernet.Arp.Operation == ArpOperation.Request)
                    {
                        log += "Arp Req - Who has? "; // +p.Ethernet.Arp.TargetProtocolAddress.ToString() + " Tell " + p.Ethernet.Arp.SenderProtocolAddress.ToString();
                        log += p.Ethernet.Arp.TargetProtocolAddress[0] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[1] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[2] + ".";
                        log += p.Ethernet.Arp.TargetProtocolAddress[3];
                        log += " Tell ";
                        log += p.Ethernet.Arp.SenderProtocolAddress[0] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[1] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[2] + ".";
                        log += p.Ethernet.Arp.SenderProtocolAddress[3];
                        Console.WriteLine(log);
                    }
                    break;
                case EthernetType.IpV4:
                    break;
            }//end switch
        }// end LoggerBRX
    }
}
