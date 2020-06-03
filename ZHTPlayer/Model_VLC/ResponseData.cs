using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 返回数据类型
    /// </summary>
    public class ResponseData
    {
        /// <summary>
        /// 返回类型编码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string MessageStr { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public object Data { get; set; }
    }
}
