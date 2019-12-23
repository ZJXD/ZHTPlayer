using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerClient
{
    /// <summary>
    /// 接收到的数据
    /// </summary>
    public class ResponseData
    {
        public string type { get; set; }

        public Data response { get; set; }

        public string msg { get; set; }
    }

    public class Data
    {
        public VideoData data { get; set; }

        public string moduleKey { get; set; }

        public string msg { get; set; }

        public string type { get; set; }

    }

    public class VideoData
    {
        public string rtsp { get; set; }

        public string phone { get; set; }

        /// <summary>
        /// 0：关闭，1：开始
        /// </summary>
        public string action { get; set; }

        public string actionStr { get; set; }
    }
}
