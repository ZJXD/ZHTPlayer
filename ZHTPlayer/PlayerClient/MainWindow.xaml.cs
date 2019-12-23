using LibVLCSharp.Shared;
using Microsoft.Win32;
using Model.Log;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using WebSocket4Net;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace PlayerClient
{
    public partial class MainWindow : Window
    {
        LibVLC _libVLC;
        MediaPlayer _mediaPlayer;

        // 创建一个客户端套接字
        WebSocket _webSocket;
        // 检查重连线程
        Thread _thread;
        // 用于循环监测状态，以便进行重连
        bool _isCheckConnect = true;
        // WebSocket连接地址
        public string ServerPath;

        int clientWidth;
        int clientHeight;
        int right;
        int bottom;
        string guid;
        string tempPicPath;
        string UploadImageURL;
        string AppKey;
        string CurrentPhone;

        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            //this.pic_btn.Visibility = Visibility.Hidden;

            this.clientHeight = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("Height"));
            this.clientWidth = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("Width"));
            this.bottom = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("bottom"));
            this.right = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("right"));
            this.guid = System.Configuration.ConfigurationManager.AppSettings.Get("guid");
            this.tempPicPath = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings.Get("picPath");
            this.UploadImageURL = System.Configuration.ConfigurationManager.AppSettings.Get("UploadImageURL");
            this.AppKey = System.Configuration.ConfigurationManager.AppSettings.Get("appKey");
            this.CurrentPhone = "";

            // 初始化 socket 信息
            this.ServerPath = System.Configuration.ConfigurationManager.AppSettings.Get("ServerPath");

            // 开始监听并处理数据
            //WSocketClient();

            // 在最上层
            this.Topmost = true;

            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            Width = Screen.AllScreens[0].Bounds.Width;
            Height = Screen.AllScreens[0].Bounds.Height;
            this.Left = Width - this.clientWidth - this.right;
            this.Top = Height - this.clientHeight - this.bottom;
            Width = this.clientWidth;
            Height = this.clientHeight;

            // 设置在截图时不在屏幕中显示路径(这个应该是禁止了所以的提示显示)
            List<string> paramsStr = new List<string> { "--no-osd", "--no-snapshot-preview" };
            _libVLC = new LibVLC(paramsStr.ToArray());
            // 全屏
            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                Fullscreen = true,
                AspectRatio = this.Width + ":" + (this.Height - 30)
            };

            // 在使用 MediaPlayer 前需要全部加装 VideoView
            //VideoView.Loaded += (s, er) => VideoView.MediaPlayer = _mediaPlayer;
            VideoView.Loaded += LoadedMediaPlayer;

            //AutoStart();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            this._webSocket.Close();
            // 彻底退出
            Environment.Exit(0);
        }

        /// <summary>
        /// Loaded 后回调事件
        /// </summary>
        /// <param name="s"></param>
        /// <param name="er"></param>
        void LoadedMediaPlayer(object s, RoutedEventArgs er)
        {
            VideoView.MediaPlayer = _mediaPlayer;
            VideoView.MediaPlayer.Playing += PlayingEvent;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        void StopPlay()
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                this.VideoView.Dispatcher.Invoke(new Action(() =>
                {
                    if (VideoView.MediaPlayer.IsPlaying)
                    {
                        this.pic_btn.Visibility = Visibility.Hidden;
                        this.Hide();
                        VideoView.MediaPlayer.Stop();
                    }
                }));
            }
            else
            {
                if (VideoView.MediaPlayer.IsPlaying)
                {
                    this.pic_btn.Visibility = Visibility.Hidden;
                    this.Hide();
                    VideoView.MediaPlayer.Stop();
                }
            }

            Logger.Default.Info("停止播放播放命令后222222 Topmost：" + this.Topmost);
        }

        /// <summary>
        /// 开始播放给的流
        /// </summary>
        /// <param name="url"></param>
        void Play(string url)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                this.VideoView.Dispatcher.Invoke(new Action(() =>
                {
                    if (!VideoView.MediaPlayer.IsPlaying)
                    {
                        VideoView.MediaPlayer.Play(new Media(_libVLC, url, FromType.FromLocation));
                    }
                }));
            }
            else
            {
                if (!VideoView.MediaPlayer.IsPlaying)
                {
                    //VideoView.MediaPlayer.Play(new Media(_libVLC, url, FromType.FromLocation));
                    VideoView.MediaPlayer.Play(new Media(_libVLC,
                        "rtsp://admin:a12345678@192.168.1.201:554/h264/ch1/main/av_stream", FromType.FromLocation));
                    this.Show();
                    this.pic_btn.Visibility = Visibility.Visible;
                }
            }

            Logger.Default.Info("播放命令后111111 Topmost：" + this.Topmost);
        }

        /// <summary>
        /// 播放成功回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PlayingEvent(object sender, EventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                this.VideoView.Dispatcher.Invoke(new Action(() =>
                {
                    this.Show();
                    this.pic_btn.Visibility = Visibility.Visible;
                }));
            }
            else
            {
                this.Show();
                this.pic_btn.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 暂停/继续
        /// </summary>
        void PausePlay()
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Pause();
            }
            else
            {
                VideoView.MediaPlayer.Play();
            }
        }

        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pic_btn_Click(object sender, RoutedEventArgs e)
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                if (!Directory.Exists(this.tempPicPath))
                {
                    Directory.CreateDirectory(this.tempPicPath);
                }
                string filePath = Path.Combine(this.tempPicPath, Guid.NewGuid().ToString("N") + ".jpg");
                bool isTake = VideoView.MediaPlayer.TakeSnapshot(0, filePath, (uint)Width, (uint)Height);
                if (isTake)
                {
                    this.UploadImage(filePath);
                }
            }
            else
            {
                this.Play("");
            }
        }

        #region WebSocket

        public void WSocketClient()
        {
            this._webSocket = new WebSocket(this.ServerPath);
            this._webSocket.Opened += WebSocket_Opened;
            this._webSocket.Error += WebSocket_Error;
            this._webSocket.Closed += WebSocket_Closed;
            this._webSocket.MessageReceived += WebSocket_MessageReceived;

            this.StartWatch();
        }
        /// <summary>
        /// 连接方法
        /// <returns></returns>
        public bool StartWatch()
        {
            bool result = true;
            try
            {
                this._webSocket.Open();

                this._isCheckConnect = true;
                this._thread = new Thread(new ThreadStart(CheckConnection));
                this._thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Default.Error("websocket 连接出错，" + ex.ToString(), ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 消息收到事件
        /// </summary>
        void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                ResponseData responseData = JsonConvert.DeserializeObject<ResponseData>(e.Message);
                if (responseData.response != null && responseData.response.data != null)
                {
                    // 初始化数据不做处理
                    if (responseData.response.type == "1" || responseData.response.moduleKey != "cm_video_link") return;
                    //if (responseData.response.moduleKey != "cm_video_link") return;

                    Logger.Default.Info("接收到数据：" + e.Message);
                    string url = responseData.response.data.rtsp;
                    string phone = responseData.response.data.phone;
                    string action = responseData.response.data.action;
                    if (action == "1")
                    {
                        this.CurrentPhone = phone;
                        this.Play(url);
                    }
                    else if (action == "0" && this.CurrentPhone == phone)
                    {
                        this.StopPlay();
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Default.Info("接收到数据：" + e.Message);
                //Logger.Default.Error("websocket_Received：" + ex.ToString(), e);
            }
        }
        /// <summary>
        /// Socket关闭事件
        /// </summary>
        void WebSocket_Closed(object sender, EventArgs e)
        {
            Logger.Default.Info("websocket_Closed");
        }
        /// <summary>
        /// Socket报错事件
        /// </summary>
        void WebSocket_Error(object sender, EventArgs e)
        {
            Logger.Default.Error("websocket_Error：" + e.ToString(), e);
        }
        /// <summary>
        /// Socket打开事件
        /// </summary>
        void WebSocket_Opened(object sender, EventArgs e)
        {
            SendMessage message = new SendMessage
            {
                guid = this.guid,
                data = new List<string> { "cm_video_link" },
                type = "1"
            };
            this.SendMessage(JsonConvert.SerializeObject(message));

            Logger.Default.Info("websocket_Opened");
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
                        Logger.Default.Info("Reconnect websocket WebSocketState:" + this._webSocket.State);
                        this._webSocket.Close();
                        this._webSocket.Open();
                        Logger.Default.Info("正在重连");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Default.Error("WebSocket 重连失败，" + ex.ToString(), ex);
                }
                Thread.Sleep(5000);
            } while (this._isCheckConnect);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Message"></param>
        public void SendMessage(string Message)
        {
            Task.Factory.StartNew(() =>
            {
                if (_webSocket != null && _webSocket.State == WebSocket4Net.WebSocketState.Open)
                {
                    this._webSocket.Send(Message);
                }
            });
        }

        #endregion

        /// <summary>
        /// 关闭按钮重载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            Logger.Default.Info("关闭窗体时333333 Topmost：" + this.Topmost);

            // 在手动隐藏窗体的时候关闭视频播放，使在再次呼入播放时能正常播放
            this.StopPlay();

            //Thread.Sleep(10000);
            //this.Show();
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="filePath"></param>
        private void UploadImage(string filePath)
        {
            try
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                //1.HttpWebRequest
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.UploadImageURL);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = "POST";
                request.KeepAlive = true;
                request.Timeout = 30000;
                //request.Headers.Add("appKey", this.AppKey);
                request.Credentials = CredentialCache.DefaultCredentials;

                using (Stream stream = request.GetRequestStream())
                {
                    //1.2 file
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    stream.Write(boundarybytes, 0, boundarybytes.Length);
                    string header = string.Format(headerTemplate, "file", Path.GetFileName(filePath));
                    byte[] headerbytes = Encoding.ASCII.GetBytes(header);
                    stream.Write(headerbytes, 0, headerbytes.Length);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                        }
                    }

                    //1.3 form end
                    stream.Write(endbytes, 0, endbytes.Length);
                }
                //2.WebResponse
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    string responseStr = stream.ReadToEnd();

                    Logger.Default.Info("图片上传结果：" + responseStr);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Error("上传图片出错", ex);
            }
        }


        /// <summary>
        /// 开机自启动
        /// </summary>
        private void AutoStart()
        {
            try
            {
                RegistryKey Local = Registry.LocalMachine;
                RegistryKey runKey = Local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree);
                string keyName = Path.GetFileName(System.Windows.Forms.Application.ExecutablePath);
                string valueStr = "\"" + System.Windows.Forms.Application.ExecutablePath + "\" -autorun";
                object key = runKey.GetValue(keyName);
                if (key == null)
                {
                    runKey.SetValue(keyName, valueStr);
                }
                //if (key != null)
                //{
                //    runKey.DeleteValue(keyName);
                //}
                //else
                //{
                //    runKey.SetValue(keyName, valueStr);
                //}
                Local.Close();
            }
            catch (Exception e)
            {
                Logger.Default.Error("开机自启出现异常，" + e.ToString(), e);
            }
        }
    }
}