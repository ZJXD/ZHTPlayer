using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 电视墙布局以及视频源信息
    /// </summary>
    public class CameraPanel
    {
        public CameraPanel()
        {
            this.Cameras = new List<CameraPanelItem>();
        }

        public List<CameraPanelItem> Cameras { get; set; }

        public CameraPanelType Type { get; set; }

        /// <summary>
        /// 布局信息
        /// </summary>
        public LayoutInfo LayoutInfo { get; set; }
    }
}
