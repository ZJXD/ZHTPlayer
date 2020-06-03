using Microsoft.Win32;
using Model;
using Model.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using Vlc.DotNet.Forms;

namespace TVWallClient.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 初始化
        private string basePlayPath = AppDomain.CurrentDomain.BaseDirectory + "play.config";
        private string sizeConfig = AppDomain.CurrentDomain.BaseDirectory + "SizeConfig";
        private Encoding encoding = Encoding.UTF8;
        private int screenIndex = 0;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
            SystemSleepManagement.PreventSleep(true);

            TVWallController.PartEvent += OnStep;  // 将该类中的函数注册到Monitor静态类的PartEvent事件中。
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var primaryScreenHeight = SystemParameters.FullPrimaryScreenHeight;
            var primaryScreenWidth = SystemParameters.FullPrimaryScreenWidth;

            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            //Topmost = true;
            string appName = "TVWallClient";
            int appCount = Process.GetProcessesByName(appName).ToList().Count;
            int screenCount = Screen.AllScreens.Count();
            if (appCount > screenCount)
            {
                this.Close();
            }
            else if (appCount == 1)
            {
                this.Left = 0;
                this.Top = 0;
                Width = Screen.AllScreens[0].Bounds.Width;
                Height = Screen.AllScreens[0].Bounds.Height;
            }
            else
            {
                int leftPix = 0;
                for (int i = appCount - 2; i >= 0; i--)
                {
                    leftPix += Screen.AllScreens[i].Bounds.Width;
                }
                this.Left = leftPix;
                this.Top = 0;
                Width = Screen.AllScreens[appCount - 1].Bounds.Width;
                Height = Screen.AllScreens[appCount - 1].Bounds.Height;
            }
            this.screenIndex = appCount - 1;

            InitPlay();
            //AutoStart(true);      // 测试时注释，发布时不注释
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
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
                        TvConfig tvConfig = JsonConvert.DeserializeObject<TvConfig>(paramStr);
                        PlayOnTV(tvConfig);
                        File.WriteAllText(basePlayPath, txt, encoding);
                        break;
                    case "SETSIZE":
                        var size = paramStr.Split(',').Select(m => int.Parse(m)).ToArray();
                        WindowState = WindowState.Normal;
                        WindowStyle = WindowStyle.None;
                        ResizeMode = ResizeMode.NoResize;
                        Topmost = true;
                        //Top = 7;
                        //Left = 1;
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
                if (File.Exists(sizeConfig))
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
            // 处理数据对象，转换为封装的 Camera 等对象
            CameraPanel cameraPanel = new CameraPanel { LayoutInfo = tvConfig.LayoutInfo };

            if (tvConfig.MonitorInfo != null)
            {
                List<int> indexs = tvConfig.MonitorInfo.Select(t => t.Index).Distinct().ToList();
                foreach (int index in indexs)
                {
                    List<MonitorInfo> monitors = tvConfig.MonitorInfo.Where(t => t.Index == index).ToList();
                    CameraPanelItem panelItem = new CameraPanelItem { Index = index, RtspStrs = new List<string>(), RtmpStrs = new List<string>() };
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
                            panelItem.RtspStrs.Add(monitor.Rtsp);
                            panelItem.RtmpStrs.Add(monitor.Rtmp);
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
                        panelItem.RtspStrs.Add(monitor.Rtsp);
                        panelItem.RtmpStrs.Add(monitor.Rtmp);
                        panelItem.Camera = camera;
                        panelItem.Flag = monitor.Flag;
                        panelItem.PlayType = PlayContentType.One;
                    }
                    cameraPanel.Cameras.Add(panelItem);
                }
            }

            gridVideo.SetValue(GridHelpers.CamearPanelProperty, cameraPanel);
        }

        Border newBorder;
        bool isSinglePlay = false;  // 是否已经全屏播放
        bool changeStream = false;  // 是否需要更改码流
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

                newBorder = GridHelpers.CreateBorder();
                newBorder.Visibility = Visibility.Visible;
                Border singleBorder = GridHelpers.GetBorderByTag(this.gridVideo.Children, index);

                // 视频源全部换为主码流
                CameraPanelItem singleOldItem = singleBorder.DataContext as CameraPanelItem;
                CameraPanelItem newItem = singleOldItem.Clone() as CameraPanelItem;
                if (newItem.PlayType == PlayContentType.One && newItem.Camera.StreamType == 1)
                {
                    newItem.RtspStrs[0] = newItem.RtspStrs[0].Replace("/sub/", "/main/");
                    newItem.Camera.StreamType = 0;
                    changeStream = true;
                }
                if (newItem.PlayType == PlayContentType.Group)
                {
                    for (int i = 0; i < newItem.CameraGroup.Items.Count; i++)
                    {
                        if (newItem.CameraGroup.Items[i].StreamType == 1)
                        {
                            newItem.RtspStrs[i] = newItem.RtspStrs[i].Replace("/sub/", "/main/");
                            newItem.CameraGroup.Items[i].StreamType = 0;
                            changeStream = true;
                        }
                    }
                }
                newBorder.SetValue(Grid.RowProperty, 0);
                newBorder.SetValue(Grid.ColumnProperty, 0);
                newBorder.SetValue(Grid.ColumnSpanProperty, cols);
                newBorder.SetValue(Grid.RowSpanProperty, rows);
                this.gridVideo.Children.Add(newBorder);
                if (changeStream)
                {
                    GridHelpers.SetBorderChildByVLC(newBorder, newItem);
                }

                isSinglePlay = true;
            }
        }

        /// <summary>
        /// 双击返回全屏播放
        /// </summary>
        private void BackSingle()
        {
            if (newBorder != null)
            {
                foreach (UIElement item in this.gridVideo.Children)
                {
                    item.Visibility = Visibility.Visible;
                }
                if (newBorder.Child is WindowsFormsHost windowsForms)
                {
                    if (windowsForms.Child is VlcControl vlcControl)
                    {
                        vlcControl.Stop();
                    }
                }
                this.gridVideo.Children.Remove(newBorder);
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
                if (isAuto == true)
                {
                    RegistryKey R_local = Registry.LocalMachine;    //RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.SetValue("TVWallClient-" + System.Configuration.ConfigurationManager.AppSettings.Get("APIPort"), AppDomain.CurrentDomain.BaseDirectory + "TVWallClient.exe");
                    R_run.Close();
                    R_local.Close();
                }
                else
                {
                    RegistryKey R_local = Registry.LocalMachine;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.DeleteValue("应用名称", false);
                    R_run.Close();
                    R_local.Close();
                }

                //GlobalVariant.Instance.UserConfig.AutoStart = isAuto;
            }
            catch (Exception)
            {
                //MessageBoxDlg dlg = new MessageBoxDlg();
                //dlg.InitialData("您需要管理员权限修改", "提示", MessageBoxButtons.OK, MessageBoxDlgIcon.Error);
                //dlg.ShowDialog();
                //System.Windows.MessageBox.Show("您需要管理员权限修改", "提示");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            //Topmost = true;
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
