using System;
using System.Windows;
using LibVLCSharp.Shared;
using Model.Log;

namespace PlayerClient
{
    public partial class App : Application
    {
        private const string Tag = nameof(App);

        public App()
        {
            Core.Initialize();


            // 捕获全局异常
            DispatcherUnhandledException += App_DispatcherUnhandledException;       // UI线程未处理异常
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;     // 非UI线程为处理异常
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Default.Error(Tag + "UI线程出现异常", e.Exception);
            e.Handled = true;
        }
        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Default.Error(Tag + "出现异常", e.Exception);
            e.Handled = true;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var terminatingMessage = e.IsTerminating ? " The application is terminating." : string.Empty;
            var exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
            var message = string.Concat(exceptionMessage, terminatingMessage);
            Logger.Default.Error(Tag + "非UI线程出现异常", exception);
        }
    }
}