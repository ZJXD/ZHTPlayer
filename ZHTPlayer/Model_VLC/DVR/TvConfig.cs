using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class TvConfig
    {
        /// <summary>
        /// 布局信息
        /// </summary>
        public LayoutInfo LayoutInfo { get; set; }

        /// <summary>
        /// 监控信息列表
        /// </summary>
        public List<MonitorInfo> MonitorInfo { get; set; }
    }
}
