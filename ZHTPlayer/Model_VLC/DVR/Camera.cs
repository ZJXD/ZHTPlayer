using Model.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Model
{
    [Serializable]
    public class Camera : CameraBase, IPlay, IDisposable,ICloneable
    {
        public event ErrorDelegate OnPlayError;

        public Control PlayPanel { get; set; }

        public bool HasRecord { get; set; }

        public bool IsHidePlay { get; set; }

        /// <summary>
        /// 读取流方式
        /// </summary>
        public int StreamType { get; set; }

        /// <summary>
        /// 开始录像
        /// </summary>
        /// <param name="name"></param>
        public void BeginRecord(string name)
        {
            name = name + "_.mp4";
            if (!Playing)
            {
                IsHidePlay = true;
                PlayPanel = new Panel();
                Play(PlayPanel);
            }
            CHCNetSDK.NET_DVR_MakeKeyFrame(UserId, Number);
            if (!CHCNetSDK.NET_DVR_SaveRealData(RealHandle, name))
            {
                var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                var str = "录像错误, error code= " + iLastErr;
                Logger.Default.Info(str);
                return;
            }
            else
            {
                HasRecord = true;
                Logger.Default.Info("开始录像");
            }
        }

        /// <summary>
        /// 结束录像
        /// </summary>
        public void StopRecord()
        {
            if (!CHCNetSDK.NET_DVR_StopSaveRealData(RealHandle))
            {
                var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                var str = "停止录像错误, error code= " + iLastErr;
                Logger.Default.Info(str);
            }
            else
            {
                this.Stop();
                IsHidePlay = false;
                HasRecord = false;
                Logger.Default.Info("录像结束");
            }
        }

        public Camera()
            : base()
        {
            RealHandle = -1;
            UserId = -1;
        }

        public int UserId { get; set; }

        public HKViewType ViewType { get; set; }

        private int RealHandle { get; set; }

        public override bool Playing
        {
            get { return RealHandle != -1; }
        }
        private Control control;

        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="playView"></param>
        /// <param name="streamType"></param>
        public override void Play(Control playView)
        {
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
            {
                hPlayWnd = playView.Handle,       //预览窗口 live view window
                lChannel = Number,                //预览的设备通道 the device Camera number
                dwStreamType = (uint)StreamType,  //码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                dwLinkMode = 0,                   //连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                bBlocked = true                  //0- 非阻塞取流，1- 阻塞取流
            };

            IntPtr pUser = IntPtr.Zero; //用户数据 user data 
            //int pUser = 0;
            control = playView;
            if (ViewType == HKViewType.Direct)
            {
                //打开预览 Start live view 
                RealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(DVR.UserId, ref lpPreviewInfo, null/*RealData*/, pUser);
            }
            else
            {
                lpPreviewInfo.hPlayWnd = IntPtr.Zero;//预览窗口 live view window
            }
            if (RealHandle == -1 && OnPlayError != null)
            {
                var code = (int)CHCNetSDK.NET_DVR_GetLastError();
                Logger.Default.Error("关闭实时预览失败，错误码：" + code);
                OnPlayError(this, new ErrorEventArgs
                {
                    ErrorMessage = "摄像机播放错误",
                    Code = code
                });
            }
        }

        public override void Stop()
        {
            if (RealHandle == -1)
            {
                return;
            }
            if (!CHCNetSDK.NET_DVR_StopRealPlay(RealHandle))
            {
                var code = CHCNetSDK.NET_DVR_GetLastError();

                Logger.Default.Error($"关闭摄像机实时预览失败，摄像机：{this.Name}，错误码：{code}");
            }
            RealHandle = -1;
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Continue()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this.HasRecord)
            {
                this.StopRecord();
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
