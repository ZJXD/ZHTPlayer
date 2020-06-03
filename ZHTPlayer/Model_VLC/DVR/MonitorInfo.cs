using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class MonitorInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// RTSP 端口
        /// </summary>
        public int RTSPPort { get; set; }

        /// <summary>
        /// 管理端口
        /// </summary>
        public int AdminPort { get; set; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 轮询时间
        /// </summary>
        public int TimeInterval { get; set; }

        /// <summary>
        /// 电视墙标识
        /// </summary>
        public int TvConfigId { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public int OnlineStatus { get; set; }

        /// <summary>
        /// 在布局中的标识
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// RTSP 视频流
        /// </summary>
        public string Rtsp { get; set; }

        /// <summary>
        /// RTMP 视频流
        /// </summary>
        public string Rtmp { get; set; }

        /// <summary>
        /// 播放流类型（0：主码流；1：子码流）
        /// </summary>
        public int StreamType { get; set; }

        /// <summary>
        /// 是否是删除（0：删除；1：新增或者替换）
        /// </summary>
        public int Flag { get; set; }
    }
}
