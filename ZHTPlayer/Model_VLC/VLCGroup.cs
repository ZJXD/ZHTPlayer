using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Vlc.DotNet.Forms;

namespace Model
{
    /// <summary>
    /// 视频组播放
    /// </summary>
    public class VLCGroup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public VLCGroup(int index, int interval)
        {
            this.Index = index;
            this.Interval = interval;
        }

        /// <summary>
        /// 组名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 巡视时间间隔
        /// </summary>
        public int Interval { get; set; }

        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// 在铺满下轮播
        /// </summary>
        /// <param name="vlcControl"></param>
        /// <param name="rtspStrs"></param>
        public void Play(VlcControl vlcControl, List<string> rtspStrs)
        {
            try
            {
                Playing = true;
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(Math.Max(5, this.Interval));
                int i = 0;
                int count = rtspStrs.Count;
                vlcControl.Play(new Uri(rtspStrs[i]));
                i++;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    string rtspStr = rtspStrs[i % count];
                    vlcControl.Play(new Uri(rtspStr));
                    i++;
                    timer.Start();
                };
                timer.Start();
            }
            catch (Exception)
            {
                Playing = false;
            }
        }

        /// <summary>
        /// 在自适应下轮播
        /// </summary>
        /// <param name="vlcControl"></param>
        /// <param name="rtspStrs"></param>
        public void PlayOnWPF(Vlc.DotNet.Wpf.VlcControl vlcControl, List<string> rtspStrs)
        {
            try
            {
                Playing = true;
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(Math.Max(5, this.Interval));
                int i = 0;
                int count = rtspStrs.Count;
                vlcControl.SourceProvider.MediaPlayer.Play(new Uri(rtspStrs[i]));
                i++;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    vlcControl.SourceProvider.MediaPlayer.Play(new Uri(rtspStrs[i % count]));
                    i++;
                    timer.Start();
                };
                timer.Start();
            }
            catch (Exception)
            {
                Playing = false;
            }
        }


        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            Playing = false;
        }
        private bool Playing { get; set; }
    }
}
