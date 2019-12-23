using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerClient
{
    /// <summary>
    /// 发送消息体
    /// </summary>
    public class SendMessage
    {
        /// <summary>
        /// 客户端设置6-10位数字识别码作为唯一标识
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// modelus
        /// </summary>
        public List<string> data { get; set; }
    }
}
