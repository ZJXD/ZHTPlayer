using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Windows;

namespace TVWallClient.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            bool m_bInitSDK = Model.CHCNetSDK.NET_DVR_Init();
            if (!m_bInitSDK)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                Model.CHCNetSDK.NET_DVR_SetLogToFile(3, AppDomain.CurrentDomain.BaseDirectory, true);
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow w1 = new MainWindow();
            var index=1;
            if (File.Exists(Environment.CurrentDirectory + "/c"))
            {
                int.TryParse(File.ReadAllText(Environment.CurrentDirectory + "/c"), out index);
            }
            var screens = System.Windows.Forms.Screen.AllScreens;
            var s1 = screens[Math.Min(screens.Length-1,index)];
           // var portStr = System.Configuration.ConfigurationManager.AppSettings["screenIndex"];
            var r1 = s1.Bounds;
            w1.WindowStartupLocation = WindowStartupLocation.Manual;
            w1.Top = r1.Top;
            w1.Left = r1.Left;
            w1.Height = r1.Height;
            w1.Width = r1.Width;

            w1.Show();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }
    }
}
