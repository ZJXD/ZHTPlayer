using Model.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Model
{
    public class HKErrorEventArg : EventArgs
    {
        public string Message { get; set; }
        public int Code { get; set; }
    }

    public class DVR
    {
        public long Id { get; set; }

        #region 字段
        private string name = "";
        private string ip = "";
        private int port = 8000;
        private string userName = "admin";
        private string password = "12345";
        private bool online;
        private ConnectionState state = ConnectionState.None;
        #endregion

        #region 属性
        public bool Online
        {
            get { return online; }
            set
            {
                online = value;
                NotifyPropertyChanged("Online");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string IP
        {
            get { return ip; }
            set { ip = value; NotifyPropertyChanged("IP"); }
        }

        public int Port
        {
            get { return port; }
            set
            {
                port = value;
                NotifyPropertyChanged("Port");
            }
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; NotifyPropertyChanged("UserName"); }
        }

        public string Password
        {
            get { return password; }
            set { password = value; NotifyPropertyChanged("Password7"); }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 通道集合
        /// </summary>
        public virtual ObservableCollection<Camera> Cameras { get; set; }

        public int UserId { get; set; } = 0;

        public DVR()
        {
            this.Cameras = new ObservableCollection<Camera>();
        }

        public void Connection()
        {
            Connection(true);
        }

        CHCNetSDK.NET_DVR_DEVICEINFO_V30 deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
        public Action<object, HKErrorEventArg> ConnectionErrorAction { get; set; }

        public ConnectionState State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged("State");
                NotifyPropertyChanged("StateString");
            }
        }

        public void Connection(bool loadCamera)
        {
            int[] iCameraNum = new int[96];
            var loginResult = CHCNetSDK.NET_DVR_Login_V30(this.IP,
                this.Port,
                this.UserName,
                this.Password,
                ref deviceInfo);
            UserId = loginResult;
            if (loginResult == -1)
            {
                if (ConnectionErrorAction != null)
                {
                    var arg = new HKErrorEventArg
                    {
                        Code = (int)CHCNetSDK.NET_DVR_GetLastError()
                    };
                    ConnectionErrorAction(this, arg);
                }
                this.Online = false;
                this.State = ConnectionState.Offline;
                Logger.Default.Error($"[{this.Name}] 连接失败");
            }
            else
            {
                IntPtr outBuffer = new IntPtr();
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 outInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                uint outBufferReturn = 0;
                CHCNetSDK.NET_DVR_GetDVRConfig(UserId, CHCNetSDK.NET_DVR_GET_COMPRESSCFG_V30, deviceInfo.byStartChan, Marshal.ReadIntPtr(outInfo,0), 10000000, ref outBufferReturn);
                string outBu = Marshal.PtrToStringAnsi(outBuffer);
                this.State = ConnectionState.Online;
                this.Online = true;
                Logger.Default.Error($"[{this.Name}] 连接成功");
            }
        }

        public void Dispose()
        {
            CHCNetSDK.NET_DVR_Logout(UserId);
        }

        public void Restart()
        {
            CHCNetSDK.NET_DVR_RebootDVR(this.UserId);
        }
    }
}
