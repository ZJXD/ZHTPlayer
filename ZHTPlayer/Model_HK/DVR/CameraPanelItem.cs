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

        private Camera _camera;

        public Camera Camera { set { this._camera = value; } get { return this._camera; } }

        private CameraGroup _cameraGroup;

        public CameraGroup CameraGroup { get { return this._cameraGroup; } set { this._cameraGroup = value; } }

        private PlayContentType? _playType;

        public PlayContentType? PlayType { get { return this._playType; } set { this._playType = value; } }

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
        /// 深度复制
        /// </summary>
        /// <returns></returns>
        private CameraPanelItem(Camera camera, CameraGroup cameraGroup,PlayContentType? playContent)
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
        }

        public CameraPanelItem()
        {
        }

        public object Clone()
        {
            return new CameraPanelItem(this.Camera, this.CameraGroup,this.PlayType);
        }
    }
}
