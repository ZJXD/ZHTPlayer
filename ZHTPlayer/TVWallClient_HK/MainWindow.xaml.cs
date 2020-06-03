using Microsoft.Win32;
using Model;
using Model.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Threading;

namespace TVWallClient.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 初始化
        private string basePlayPath = AppDomain.CurrentDomain.BaseDirectory + "Play.config";
        private string sizeConfig = AppDomain.CurrentDomain.BaseDirectory + "Size.config";
        private int screenIndex = int.Parse(ConfigurationManager.AppSettings.Get("ScreenIndex"));
        private bool UseInputSize = bool.Parse(ConfigurationManager.AppSettings.Get("UseInputSize"));
        private Encoding encoding = Encoding.UTF8;
        private Dictionary<int, CameraPanelItem> cameras = new Dictionary<int, CameraPanelItem>();  // 当前视频墙视频信息
        //private DispatcherTimer stateChecktimer;            // 定时检查摄像头状态
        private int stateCheckNum = 10;                     // 每10次全部检查一次
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
            SystemSleepManagement.PreventSleep(true);

            //bool s = AllowsTransparency;
            //// 新增：提高性能，默认不开启，不用显示设置也可以
            //AllowsTransparency = false;

            TVWallController.PartEvent += OnStep;  // 将该类中的函数注册到Monitor静态类的PartEvent事件中。

            //stateChecktimer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromSeconds(60 * 2)
            //};
            //stateChecktimer.Tick += StateChecktimer_Tick;
            //stateChecktimer.Start();
        }

        /// <summary>
        /// 定时检查摄像机状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateChecktimer_Tick(object sender, EventArgs e)
        {
            foreach (var t in cameras.Values)
            {
                if (t.PlayType == PlayContentType.One && t.Camera.DVR.State == ConnectionState.Offline)
                {
                    t.Camera.DVR.Connection(false);
                    if (t.Camera.DVR.State == ConnectionState.Online)
                    {
                        this.gridVideo.SetValue(GridHelpers.CamearPanelProperty, new CameraPanel { Cameras = new List<CameraPanelItem> { t } });
                    }
                }
                else if (t.PlayType == PlayContentType.Group)
                {
                    t.CameraGroup.Items.ForEach(a =>
                    {
                        if (a.DVR.State == ConnectionState.Offline)
                        {
                            a.DVR.Connection(false);
                            if (a.DVR.State == ConnectionState.Online)
                            {
                                this.gridVideo.SetValue(GridHelpers.CamearPanelProperty, new CameraPanel { Cameras = new List<CameraPanelItem> { t } });
                            }
                        }
                    });
                }
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Default.Info("设备重启！！！");

            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            //Topmost = true;

            if (screenIndex > Screen.AllScreens.Length - 1)
            {
                this.Close();
                System.Windows.MessageBox.Show("配置投屏参数有误！", "提示");
                return;
            }
            // 投映到指定屏幕
            int leftPix = 0;
            for (int i = 0; i < screenIndex; i++)
            {
                leftPix += Screen.AllScreens[i].Bounds.Width;
            }
            this.Left = leftPix;
            this.Top = 0;
            Width = Screen.AllScreens[screenIndex].Bounds.Width;
            Height = Screen.AllScreens[screenIndex].Bounds.Height;

            InitPlay();
            //AutoStart(true);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var t in cameras.Values)
            {
                if (t.PlayType == PlayContentType.One)
                {
                    t.Camera.Dispose();
                }
                else
                {
                    t.CameraGroup.Items.ForEach(a => a.Dispose());
                }
            }

            // 彻底退出
            Environment.Exit(0);
        }

        /// <summary>
        /// 订阅 Monitor 的PartEvent事件，当触发PartEvent事件时（可能并不在类MainWindow对象中），被注册的函数就行做出相应的响应。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public ResponseData OnStep(Object sender, MessageArgs message)
        {
            ResponseData result = null;
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    result = DoCommand(message.ComMessage);
                }));

            return result;
        }

        private ResponseData DoCommand(string netStream)
        {

            string txt = string.Empty;
            string[] commands = null;
            string commandName = string.Empty;
            string paramStr = string.Empty;
            try
            {
                string mes = "操作成功！";
                txt = netStream;
                commands = txt.Split('|');
                commandName = commands[0];
                paramStr = commands[1];
                int index = -1;
                switch (commandName.ToUpper())
                {
                    case "CONNECTION":
                        var param = paramStr.Split(',');
                        break;
                    case "PLAYPANEL":
                        //this.stateChecktimer.Stop();
                        Logger.Default.Info("收到播放视频命令！CMD：" + paramStr);
                        TvConfig tvConfig = JsonConvert.DeserializeObject<TvConfig>(paramStr);
                        PlayOnTV(tvConfig);
                        File.WriteAllText(basePlayPath, txt, encoding);
                        //this.stateChecktimer.Start();
                        break;
                    case "SETSIZE":
                        var size = paramStr.Split(',').Select(m => int.Parse(m)).ToArray();
                        WindowState = WindowState.Normal;
                        WindowStyle = WindowStyle.None;
                        ResizeMode = ResizeMode.NoResize;
                        Topmost = true;
                        Top = 0;
                        Left = 0;
                        Width = size[0];
                        Height = size[1];
                        File.WriteAllText(sizeConfig, string.Format("width={0}\nheight={1}", Width, Height));

                        break;
                    case "SINGLEPLAY":
                        index = int.Parse(paramStr);
                        SetSingle(index);
                        break;
                    case "SINGLEBACK":
                        BackSingle();
                        break;
                    case "STOP":
                        Stop();
                        break;
                    case "EXIT":
                        this.Close();
                        break;
                    case "PAUSE":
                        index = int.Parse(paramStr);
                        Pause(index);
                        break;
                    case "CONTINUE":
                        index = int.Parse(paramStr);
                        Continue(index);
                        break;
                    case "TEST":
                        TvConfig testConfig = JsonConvert.DeserializeObject<TvConfig>(paramStr);
                        PlayOnTV(testConfig);
                        break;
                    case "PING":
                        mes = "连接成功！";
                        break;
                    default:
                        mes = "不存在此操作";
                        break;
                }

                return new ResponseData { Code = 0, MessageStr = mes };
            }
            catch (Exception e)
            {
                Logger.Default.Error($"执行：{commandName}命令错误", e);
                return new ResponseData { Code = 1, MessageStr = "操作失败！" };
            }
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        /// <param name="index"></param>
        private void Continue(int index)
        {
            Border border = GridHelpers.GetBorderByTag(this.gridVideo.Children, index);
            CameraPanelItem item = border.DataContext as CameraPanelItem;
            item.GetPlayView().Continue();
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <param name="index"></param>
        private void Pause(int index)
        {
            Border border = GridHelpers.GetBorderByTag(this.gridVideo.Children, index);
            CameraPanelItem item = border.DataContext as CameraPanelItem;
            item.GetPlayView().Pause();
        }

        /// <summary>
        /// 停止播放（全部）
        /// </summary>
        private void Stop()
        {
            foreach (Border item in this.gridVideo.Children)
            {
                if (item.DataContext is CameraPanelItem oldItem)
                {
                    oldItem.GetPlayView().Stop();
                }
                item.DataContext = null;
                item.Child = null;
            }
        }

        /// <summary>
        /// 初始化播放
        /// </summary>
        private void InitPlay()
        {
            try
            {
                // 初始化窗体大小样式等
                if (UseInputSize && File.Exists(sizeConfig))
                {
                    var txt = File.ReadAllText(sizeConfig);
                    var content = txt.Split('\n');
                    foreach (var item in content)
                    {
                        var command = item.Split('=');
                        var value = command[1].Trim();
                        switch (command[0].ToLower())
                        {
                            case "width":
                                Width = int.Parse(value);
                                break;
                            case "height":
                                Height = int.Parse(value);
                                break;
                            default:
                                break;
                        }
                    }
                }
                // 初始化布局、播放资源等
                if (File.Exists(basePlayPath))
                {
                    var txt = File.ReadAllText(basePlayPath);

                    var commands = txt.Split('|');
                    var commandName = commands[0];
                    var paramStr = commands[1];

                    switch (commandName.ToUpper())
                    {
                        case "PLAYPANEL":
                            TvConfig tvConfig = JsonConvert.DeserializeObject<TvConfig>(paramStr);
                            PlayOnTV(tvConfig);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Default.Error("初始化播放错误", e);
            }
        }

        /// <summary>
        /// 播放命令操作
        /// </summary>
        /// <param name="tvConfig"></param>
        public void PlayOnTV(TvConfig tvConfig)
        {
            // 当布局变化时视频信息会变，清楚全部的 CameraPanelItem 信息
            if (tvConfig.LayoutInfo != null)
            {
                this.cameras = new Dictionary<int, CameraPanelItem>();
            }

            // 处理数据对象，转换为封装的 Camera 等对象
            CameraPanel cameraPanel = new CameraPanel { LayoutInfo = tvConfig.LayoutInfo };

            if (tvConfig.MonitorInfo != null)
            {
                List<int> indexs = tvConfig.MonitorInfo.Select(t => t.Index).Distinct().ToList();
                foreach (int index in indexs)
                {
                    List<MonitorInfo> monitors = tvConfig.MonitorInfo.Where(t => t.Index == index).ToList();
                    CameraPanelItem panelItem = new CameraPanelItem { Index = index };
                    if (monitors.Count > 1)
                    {
                        CameraGroup cameraGroup = new CameraGroup
                        {
                            Interval = monitors[0].TimeInterval
                        };
                        foreach (var monitor in monitors)
                        {
                            DVR dvr = new DVR()
                            {
                                IP = monitor.Ip,
                                Port = monitor.AdminPort,
                                UserName = monitor.User,
                                Password = monitor.PassWord,
                                Id = monitor.Id
                            };
                            Camera camera = new Camera
                            {
                                StreamType = monitor.StreamType,
                                DVR = dvr,
                                Number = monitor.Channel
                            };
                            cameraGroup.Items.Add(camera);
                        }

                        panelItem.CameraGroup = cameraGroup;
                        panelItem.Flag = monitors[0].Flag;
                        panelItem.PlayType = PlayContentType.Group;
                    }
                    else
                    {
                        MonitorInfo monitor = monitors[0];
                        DVR dvr = new DVR()
                        {
                            IP = monitor.Ip,
                            Port = monitor.AdminPort,
                            UserName = monitor.User,
                            Password = monitor.PassWord,
                            Id = monitor.Id
                        };

                        Camera camera = new Camera()
                        {
                            DVR = dvr,
                            StreamType = monitor.StreamType,
                            Number = monitor.Channel
                        };
                        panelItem.Camera = camera;
                        panelItem.Flag = monitor.Flag;
                        panelItem.PlayType = PlayContentType.One;

                        if (monitor.Ip == null && monitor.User == null)
                        {
                            panelItem.Flag = 0;
                        }
                    }
                    cameraPanel.Cameras.Add(panelItem);

                    if (this.cameras.ContainsKey(index))
                    {
                        this.cameras.Remove(index);
                    }
                    this.cameras.Add(index, panelItem);
                }

                // 登录各个设备，为后面播放，
                // 这里再返回赋值，因为每次都需要登录信息，否则失败
                foreach (var item2 in cameraPanel.Cameras)
                {
                    if (item2.Camera != null)
                    {
                        item2.Camera.DVR.Connection(false);
                    }
                    else if (item2.CameraGroup != null)
                    {
                        foreach (var item in item2.CameraGroup.Items)
                        {
                            item.DVR.Connection(false);
                        }
                    }
                }
            }

            this.gridVideo.SetValue(GridHelpers.CamearPanelProperty, cameraPanel);
        }

        Border singleBorder;
        int singleRowSpan = 0;
        int singleRow = 0;
        int singleCol = 0;
        int singleColsSpan = 0;
        bool isSinglePlay = false;  // 是否已经全屏播放
        bool changeStream = false;  // 是否需要更改码流
        CameraPanelItem singleOldItem = null; //全屏播放的视频信息
        /// <summary>
        /// 双击全屏播放视频
        /// </summary>
        /// <param name="index">视频索引</param>
        private void SetSingle(int index)
        {
            if (!isSinglePlay)
            {
                var rows = this.gridVideo.RowDefinitions.Count;
                var cols = this.gridVideo.ColumnDefinitions.Count;
                foreach (UIElement item in this.gridVideo.Children)
                {
                    item.Visibility = Visibility.Hidden;
                }

                singleBorder = GridHelpers.GetBorderByTag(this.gridVideo.Children, index);
                singleBorder.Visibility = Visibility.Visible;
                singleRowSpan = (int)singleBorder.GetValue(Grid.RowSpanProperty);
                singleRow = (int)singleBorder.GetValue(Grid.RowProperty);
                singleColsSpan = (int)singleBorder.GetValue(Grid.ColumnSpanProperty);
                singleCol = (int)singleBorder.GetValue(Grid.ColumnProperty);

                // 视频源全部换为主码流
                singleOldItem = singleBorder.DataContext as CameraPanelItem;
                CameraPanelItem newItem = singleOldItem.Clone() as CameraPanelItem;
                if (newItem.PlayType == PlayContentType.One && newItem.Camera.StreamType == 1)
                {
                    singleOldItem.GetPlayView().Stop();
                    newItem.Camera.StreamType = 0;
                    changeStream = true;
                }
                if (newItem.PlayType == PlayContentType.Group)
                {
                    for (int i = 0; i < newItem.CameraGroup.Items.Count; i++)
                    {
                        if (newItem.CameraGroup.Items[i].StreamType == 1)
                        {
                            singleOldItem.GetPlayView().Stop();
                            newItem.CameraGroup.Items[i].StreamType = 0;
                            changeStream = true;
                        }
                    }
                }
                if (changeStream)
                {
                    GridHelpers.SetCameraOnBorder(singleBorder, newItem);
                }

                singleBorder.SetValue(Grid.RowProperty, 0);
                singleBorder.SetValue(Grid.ColumnProperty, 0);
                singleBorder.SetValue(Grid.ColumnSpanProperty, cols);
                singleBorder.SetValue(Grid.RowSpanProperty, rows);

                isSinglePlay = true;
            }
        }

        /// <summary>
        /// 双击返回全屏播放
        /// </summary>
        private void BackSingle()
        {
            if (singleBorder != null)
            {
                foreach (UIElement item in this.gridVideo.Children)
                {
                    item.Visibility = Visibility.Visible;
                }
                if (changeStream)
                {
                    GridHelpers.SetCameraOnBorder(singleBorder, singleOldItem);
                    changeStream = false;
                }
                singleBorder.SetValue(Grid.ColumnProperty, singleCol);
                singleBorder.SetValue(Grid.RowProperty, singleRow);
                singleBorder.SetValue(Grid.ColumnSpanProperty, singleColsSpan);
                singleBorder.SetValue(Grid.RowSpanProperty, singleRowSpan);

                singleBorder = null;
                isSinglePlay = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            alarmBorder.IsOpen = !alarmBorder.IsOpen;
        }

        /// <summary>  
        /// 修改程序在注册表中的键值  
        /// </summary>  
        /// <param name="isAuto">true:开机启动,false:不开机自启</param> 
        public static void AutoStart(bool isAuto)
        {
            try
            {
                RegistryKey R_local = Registry.LocalMachine;    //RegistryKey R_local = Registry.CurrentUser;
                RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                string exeName = "TVWallClient-" + ConfigurationManager.AppSettings.Get("APIPort");
                object registryObj = R_run.GetValue(exeName);
                if (registryObj != null)
                {
                    R_run.DeleteValue(exeName, false);
                }
                if (isAuto == true)
                {
                    R_run.SetValue(exeName, AppDomain.CurrentDomain.BaseDirectory + "TVWallClient.exe");
                }
                R_run.Close();
                R_local.Close();
            }
            catch (Exception e)
            {
                Logger.Default.Error("注册自启动有异常", e);
                System.Windows.MessageBox.Show("您需要管理员权限修改", "提示");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            if (screenIndex == 0)
            {
                this.Left = 0;
                this.Top = 0;
                Width = Screen.AllScreens[0].Bounds.Width;
                Height = Screen.AllScreens[0].Bounds.Height;
            }
            else
            {
                int leftPix = 0;
                for (int i = screenIndex - 1; i >= 0; i--)
                {
                    leftPix += Screen.AllScreens[i].Bounds.Width;
                }
                this.Left = leftPix;
                this.Top = 0;
                Width = Screen.AllScreens[screenIndex].Bounds.Width;
                Height = Screen.AllScreens[screenIndex].Bounds.Height;
            }
        }
    }

    class SystemSleepManagement
    {
        // 定义API函数
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continus = 0x80000000,
        }

        /// <summary>
        /// 阻止系统休眠，直到线程结束恢复休眠策略
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static void PreventSleep(bool includeDisplay = false)
        {
            if (includeDisplay)
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
            else
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Continus);
        }

        /// <summary>
        /// 恢复系统休眠策略
        /// </summary>
        public static void ResotreSleep()
        {
            SetThreadExecutionState(ExecutionFlag.Continus);
        }

        /// <summary>
        /// 重置系统休眠计时器
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static void ResetSleepTimer(bool includeDisplay = false)
        {
            if (includeDisplay)
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display);
            else
                SetThreadExecutionState(ExecutionFlag.System);
        }
    }
}
