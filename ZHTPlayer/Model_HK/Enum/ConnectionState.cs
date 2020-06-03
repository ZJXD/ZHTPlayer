using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum ConnectionState
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 1,
        /// <summary>
        /// 在线
        /// </summary>
        Online,
        /// <summary>
        /// 离线
        /// </summary>
        Offline,
    }
}
