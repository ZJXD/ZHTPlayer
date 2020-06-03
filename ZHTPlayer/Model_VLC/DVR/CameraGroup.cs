using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Model
{
    [Serializable]
    public class CameraGroup : IPlay, ICloneable
    {
        public CameraGroup()
        {
            this.Items = new List<Camera>();
        }

        public string Name { get; set; }

        public List<Camera> Items { get; set; }

        /// <summary>
        /// 巡视时间间隔
        /// </summary>
        public int Interval { get; set; }

        DispatcherTimer timer = new DispatcherTimer();

        public void Pause() { timer.Stop(); }

        public void Continue() { timer.Start(); }

        public void Play(System.Windows.Forms.Control control)
        {
            try
            {
                Playing = true;
                if (this.Items == null || this.Items.Count < 1)
                {
                    return;
                }
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(Math.Max(5, this.Interval));
                int i = 0;
                int count = this.Items.Count;
                var list = this.Items.ToList();
                var playModel = list[i];
                foreach (var item in list)
                {
                    item.OnPlayError += Camera_OnPlayError;
                }
                Camera oldCamera = null;
                playModel.Play(control);
                i++;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    playModel.Stop();
                    playModel = list[i % count];
                    oldCamera = list[(i - 1) % count];
                    oldCamera.Stop();
                    playModel.Play(control);
                    i++;
                    timer.Start();
                };
                timer.Start();
            }
            catch (Exception)
            {
                if (this.OnPlayError != null)
                {
                    OnPlayError(this, new ErrorEventArgs
                    {
                        ErrorMessage = "播放错误",
                        Code = 601
                    });
                }
                Playing = false;
            }
        }

        void Camera_OnPlayError(object sender, ErrorEventArgs e)
        {
            if (this.OnPlayError != null)
            {
                OnPlayError(this, new ErrorEventArgs
                {
                    ErrorMessage = "播放错误",
                    Code = e.Code
                });
            }
        }

        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                foreach (var item in Items)
                {
                    if (item.Playing)
                    {
                        item.Stop();
                    }
                }
            }
            Playing = false;
        }

        public event ErrorDelegate OnPlayError;

        public bool Playing { get; private set; }

        public void BeginRecord(string name)
        {
            foreach (var item in this.Items)
            {
                item.BeginRecord(name);
            }
        }

        public void StopRecord()
        {
            foreach (var item in this.Items)
            {
                item.StopRecord();
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
