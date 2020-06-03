using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum CameraCommandType
    {
        /// <summary>
        /// 云台上仰
        /// </summary>
        UP = 21,
        /// <summary>
        /// 云台左转
        /// </summary>
        Left = 23,
        /// <summary>
        /// 云台右转
        /// </summary>
        Right = 24,
        /// <summary>
        /// 云台下俯
        /// </summary>
        Down = 22,
        /// <summary>
        /// 放大
        /// </summary>
        ZoomIn = 11,
        /// <summary>
        /// 缩小
        /// </summary>
        ZoomOut = 12,
        /// <summary>
        /// 光圈增大
        /// </summary>
        IrisOpen = 15,
        /// <summary>
        /// 光圈减少
        /// </summary>
        IrisClose = 16
    }
}
