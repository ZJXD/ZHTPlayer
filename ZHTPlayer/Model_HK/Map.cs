using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Map
    {
        public string Image { get; set; }

        public List<Marker> Marker { get; set; }

        public float Scale { get; set; }
    }

    public class Marker
    {
        /// <summary>
        /// 百分比横坐标
        /// </summary>
        public float PercentX { get; set; }

        /// <summary>
        /// 百分比纵坐标
        /// </summary>
        public float PercentY { get; set; }

        /// <summary>
        /// 标点类型
        /// </summary>
        public MarkerType Type { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public Camera Camera { get; set; }
    }

    public enum MarkerType
    {
        /// <summary>
        /// 报警设备
        /// </summary>
        Alarm,

        /// <summary>
        /// 摄像头设备
        /// </summary>
        Camera
    }
}
