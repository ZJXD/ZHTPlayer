using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerClient
{
    class SocketClient
    {
        readonly string IP;
        string port;
        public SocketClient(string ip, string port)
        {
            this.IP = ip;
            this.port = port;
        }

        // 创建一个客户端套接字
        Socket clientSocket = null;
        // 创建一个监听服务端的线程
        Thread threadServer = null;

        public byte[] MsgBuffer { get; private set; }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartWatch()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (string.IsNullOrEmpty(IP))
            {
                MessageBox.Show("监听ip地址不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(port))
            {
                MessageBox.Show("监听端口不能为空！");
                return;
            }
            IPAddress ip = IPAddress.Parse(IP);
            IPEndPoint endpoint = new IPEndPoint(ip, int.Parse(port));

            try
            {   //这里客户端套接字连接到网络节点(服务端)用的方法是Connect 而不是Bind
                clientSocket.Connect(endpoint);
            }
            catch
            {
            }

            // 创建一个线程监听服务端发来的消息
            threadServer = new Thread(RecMsg)
            {
                IsBackground = true
            };
            threadServer.Start();
        }

        /// <summary>
        ///  接收服务端发来的消息
        /// </summary>
        private void RecMsg()
        {
            while (true) //持续监听服务端发来的消息
            {
                //定义一个1M的内存缓冲区 用于临时性存储接收到的信息
                byte[] arrRecMsg = new byte[1024 * 1024];
                int length = 0;
                try
                {
                    // 将客户端套接字接收到的数据存入内存缓冲区, 并获取其长度
                    length = clientSocket.Receive(arrRecMsg);
                }
                catch
                {
                    return;
                }

                //将套接字获取到的字节数组转换为人可以看懂的字符串
                string strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
            }
        }

        /// <summary>
        /// 发送消息到服务端
        /// </summary>
        /// <param name="msg"></param>
        private void ClientSendMsg(string msg)
        {
            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(sendMsg);
        }

        /// <summary>
        /// 获取当前系统时间的方法
        /// </summary>
        /// <returns>当前时间</returns>
        private DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        #region 异步连接到服务端
        /// <summary>
        /// 客户端-异步连接到服务端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress ip, int port)
        {
            this.clientSocket.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), this.clientSocket);
        }

        /// <summary>
        /// 连接后的回调
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndConnect(ar);
            }
            catch (SocketException ex)
            { }
        }
        #endregion

        #region 异步发送消息
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }

        private void Send(byte[] byteData)
        {
            try
            {
                int length = byteData.Length;
                byte[] head = BitConverter.GetBytes(length);
                byte[] data = new byte[head.Length + byteData.Length];
                Array.Copy(head, data, head.Length);
                Array.Copy(byteData, 0, data, head.Length, byteData.Length);
                this.clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), this.clientSocket);
            }
            catch (SocketException ex)
            { }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndSend(ar);
            }
            catch (SocketException ex)
            { }
        }

        #endregion

        #region 异步接收消息

        public void ReceiveData()
        {
            clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int REnd = clientSocket.EndReceive(ar);
                if (REnd > 0)
                {
                    byte[] data = new byte[REnd];
                    Array.Copy(MsgBuffer, 0, data, 0, REnd);

                    //在此次可以对data进行按需处理

                    clientSocket.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                }
                else
                {
                    Dispose();
                }
            }
            catch (SocketException ex)
            { }
        }

        private void Dispose()
        {
            try
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
                this.clientSocket.Close();
            }
            catch (Exception ex)
            { }
        }

        #endregion
    }
}
