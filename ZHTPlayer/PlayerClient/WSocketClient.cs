using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace PlayerClient
{
    public class WSocketClient : IDisposable
    {
        //日志管理
        public static NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        #region 向外传递数据事件
        public event Action<string> MessageReceived;
        #endregion

        WebSocket _webSocket;
        /// <summary>
        /// 检查重连线程
        /// </summary>
        Thread _thread;
        bool _isRunning = true;
        /// <summary>
        /// WebSocket连接地址
        /// </summary>
        public string ServerPath { get; set; }

        public WSocketClient(string url)
        {
            ServerPath = url;
            this._webSocket = new WebSocket(url);
            this._webSocket.Opened += WebSocket_Opened;
            this._webSocket.Error += WebSocket_Error;
            this._webSocket.Closed += WebSocket_Closed;
            this._webSocket.MessageReceived += WebSocket_MessageReceived;
        }

        #region "web socket "
        /// <summary>
        /// 连接方法
        /// <returns></returns>
        public bool Start()
        {
            bool result = true;
            try
            {
                this._webSocket.Open();

                this._isRunning = true;
                this._thread = new Thread(new ThreadStart(CheckConnection));
                this._thread.Start();
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 消息收到事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _Logger.Info(" Received:" + e.Message);
            MessageReceived?.Invoke(e.Message);
        }
        /// <summary>
        /// Socket关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Closed(object sender, EventArgs e)
        {
            _Logger.Info("websocket_Closed");
        }
        /// <summary>
        /// Socket报错事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Error(object sender, EventArgs e)
        {
            _Logger.Info("websocket_Error:" + e.ToString());
        }
        /// <summary>
        /// Socket打开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Opened(object sender, EventArgs e)
        {
            _Logger.Info(" websocket_Opened");
        }
        /// <summary>
        /// 检查重连线程
        /// </summary>
        private void CheckConnection()
        {
            do
            {
                try
                {
                    if (this._webSocket.State != WebSocketState.Open && this._webSocket.State != WebSocketState.Connecting)
                    {
                        _Logger.Info(" Reconnect websocket WebSocketState:" + this._webSocket.State);
                        this._webSocket.Close();
                        this._webSocket.Open();
                        Console.WriteLine("正在重连");
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error(ex.ToString());
                }
                Thread.Sleep(5000);
            } while (this._isRunning);
        }
        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Message"></param>
        public void SendMessage(string Message)
        {
            Task.Factory.StartNew(() =>
            {
                if (_webSocket != null && _webSocket.State == WebSocketState.Open)
                {
                    this._webSocket.Send(Message);
                }
            });
        }

        public void Dispose()
        {
            this._isRunning = false;
            try
            {
                _thread.Abort();
            }
            catch
            {

            }
            this._webSocket.Close();
            this._webSocket.Dispose();
            this._webSocket = null;
        }
    }
}
