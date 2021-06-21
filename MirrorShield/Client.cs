using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MirrorShield
{
    class Client
    {
        IPEndPoint remoteEP;
        Socket clientSocket;
        Thread RxTxThread;
        //================================================================
        public Client(string ip, int port)
        {
            remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
            clientSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream, ProtocolType.Tcp);
            RxTxThread = new Thread(RunRxTx);
            RxTxThread.Start();
        }

        //================================================================
        private void RunRxTx()
        {
            byte[] bytes = new byte[1024];
            int byteContRx, tempCountRx, totalByteLen;
            clientSocket.Connect(remoteEP);
            Console.WriteLine("{0} Connected to {1}",
                  clientSocket.LocalEndPoint.ToString(),
                  clientSocket.RemoteEndPoint.ToString());

            while (true)
            {
                // get 10 bytes of message len in bytes, those 10 bytes
                // are not counted in the total length
                byteContRx = clientSocket.Receive(bytes, 10, SocketFlags.None);
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

                    byteContRx = clientSocket.Receive(bytes, bytesToRead, SocketFlags.None);
                    tempCountRx += byteContRx;

                    msg = Encoding.ASCII.GetString(bytes, 0, byteContRx);
                    sb.Append(msg);
                }// end while (tempCountRx < totalByteLen)
                ProcessMeggase(sb);
            }// end while true

            // Release the socket.
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        //==========================================================
        private void Tx(string key1, string key2, string[] arr) //sender
        {
            string lineMsg = MessageToLineProt(key1, key2, arr);

            byte[] bmsg = Encoding.ASCII.GetBytes(lineMsg);
            string strTotalByteCount = string.Format("{0:D10}", bmsg.Length);
            byte[] bmsgTotalCount = Encoding.ASCII.GetBytes(strTotalByteCount);

            clientSocket.Send(bmsgTotalCount);
            clientSocket.Send(bmsg);
        }
       
        //==========================================================
        private void ProcessMeggase(StringBuilder sb)
        {
            string key1, key2;
            string[] arr;
            LineProtToMessage(sb.ToString(), out key1, out key2, out arr);
        }
        //==========================================================
        static string MessageToLineProt(string key1, string key2, string[] arr)
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
