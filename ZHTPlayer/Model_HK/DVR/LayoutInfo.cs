using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 布局
    /// </summary>
    public class LayoutInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public int Cols { get; set; }

        /// <summary>
        /// 行
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 分隔列表
        /// </summary>
        public List<LayoutDetail> LayoutDetails { get; set; }
    }

    /// <summary>
    /// 布局分隔定义
    /// </summary>
    public class LayoutDetail
    {
        /// <summary>
        /// 合并单元格
        /// </summary>
        public string ChildCell { get; set; }

        /// <summary>
        /// 合并列数
        /// </summary>
        public int Cols { get; set; }

        /// <summary>
        /// 合并行数
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 是否是合并
        /// </summary>
        public int IsMerger { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Indexs { get; set; }
    }
}
