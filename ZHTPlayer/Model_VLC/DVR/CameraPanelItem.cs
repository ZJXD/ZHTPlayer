using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 视频源信息
    /// </summary>
    [Serializable]
    public class CameraPanelItem : ICloneable
    {
        public int Id { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }


        public Camera Camera { get; set; }


        public CameraGroup CameraGroup { get; set; }


        public PlayContentType? PlayType { get; set; }

        public IPlay GetPlayView()
        {
            switch (PlayType)
            {
                case PlayContentType.One:
                    return Camera;
                case PlayContentType.Group:
                    return CameraGroup;
                default:
                    return Camera;
            }
        }

        /// <summary>
        /// 是否是删除（0：删除；1：新增或者替换）
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// 设备对应的 RTSP 流地址
        /// </summary>
        public List<string> RtspStrs { get; set; }

        /// <summary>
        /// 设备对应的 RTMP 流地址
        /// </summary>
        public List<string> RtmpStrs { get; set; }

        /// <summary>
        /// 深度复制
        /// </summary>
        /// <returns></returns>
        private CameraPanelItem(Camera camera, CameraGroup cameraGroup, PlayContentType? playContent, List<string> rtspStrs, List<string> rtmpStrs)
        {
            if (camera != null)
            {
                this.Camera = camera.Clone() as Camera;
            }
            if (cameraGroup != null)
            {
                this.CameraGroup = cameraGroup.Clone() as CameraGroup;
            }
            this.PlayType = playContent;
            if (rtspStrs != null)
            {
                this.RtspStrs = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(rtspStrs));
            }
            if (rtmpStrs != null)
            {
                this.RtmpStrs = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(rtmpStrs));
            }
        }

        public CameraPanelItem()
        {
        }

        public object Clone()
        {
            return new CameraPanelItem(this.Camera, this.CameraGroup, this.PlayType, this.RtspStrs, this.RtmpStrs);
        }
    }
}
