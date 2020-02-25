using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RomeClockControlLibrary
{
    /// <summary>
    /// 罗马时钟
    /// </summary>
    public partial class RomeClockControl : UserControl, IDisposable
    {
        public RomeClockControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// x轴的中心位置
        /// </summary>
        private double CenterPixToX => this.ActualWidth / 2;

        /// <summary>
        /// y轴的中心位置
        /// </summary>
        private double CenterPixToY => this.ActualHeight / 2;

        /// <summary>
        /// 秒
        /// </summary>
        private Canvas CanvasHour = null;

        /// <summary>
        /// 分
        /// </summary>
        private Canvas CanvasMinute = null;

        /// <summary>
        /// 时
        /// </summary>
        private Canvas CanvasSecond = null;

        /// <summary>
        /// UI更新线程
        /// </summary>
        private Thread thread = null;

        /// <summary>
        /// 缓存时的显示控件
        /// </summary>
        private TextBlock BlockHour = null;

        /// <summary>
        /// 缓存分的显示控件
        /// </summary>
        private TextBlock BlockMinute = null;

        /// <summary>
        /// 缓存秒的显示控件
        /// </summary>
        private TextBlock BlockSecond = null;

        /// <summary>
        /// 添加控件
        /// </summary>
        private void Add(AddType type)
        {
            var offset = 0;//偏移量
            var count = 0;//总量
            var str = string.Empty;
            var time = 0;
            double AngleTime = 0;
            Canvas canvas = new Canvas();
            canvas.Tag = type;

            switch (type)
            {
                case AddType.Hour:
                    offset = 95;
                    count = 24;
                    str = "时";
                    CanvasHour = canvas;
                    time = DateTime.Now.Hour;
                    break;
                case AddType.Minute:
                    offset = 60;
                    count = 60;
                    str = "分";
                    CanvasMinute = canvas;
                    time = DateTime.Now.Minute;
                    break;
                case AddType.Second:
                    offset = 30;
                    count = 60;
                    str = "秒";
                    CanvasSecond = canvas;
                    time = DateTime.Now.Second;
                    break;
                default:
                    return;
            }

            var angle = 360 / count;//角度
            var x = CenterPixToX - offset;//起始位置
            var y = CenterPixToY - offset;

            for (int i = 0; i < count; i++)
            {
                TextBlock t = new TextBlock();
                if (i <= 9)
                {
                    t.Text = $"0{i}{str}";
                }
                else
                {
                    t.Text = $"{i}{str}";
                }
                t.Tag = i;
                t.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7d7d7d"));
                canvas.Children.Add(t);

                var sinv = Math.Sin((90 - angle * i) * (Math.PI / 180));
                var cosv = Math.Cos((90 - angle * i) * (Math.PI / 180));
                var a = CenterPixToY - y * sinv;
                var b = CenterPixToX + y * cosv;
                Canvas.SetLeft(t, b);
                Canvas.SetTop(t, a);

                //设置角度
                RotateTransform r = new RotateTransform();
                r.Angle = angle * i - 90;
                t.RenderTransform = r;

                if (i == time)
                {
                    AngleTime = 360 - r.Angle;
                    //更新样式
                    t.Foreground = new SolidColorBrush(Colors.White);
                    switch (type)
                    {
                        case AddType.Hour:
                            BlockHour = t;
                            break;
                        case AddType.Minute:
                            BlockMinute = t;
                            break;
                        case AddType.Second:
                            BlockSecond = t;
                            break;
                    }
                }
            }
            grid.Children.Add(canvas);

            //获取当前时间，旋转对应的角度
            RotateTransform rtf = new RotateTransform();
            rtf.CenterX = CenterPixToX;
            rtf.CenterY = CenterPixToY;
            rtf.Angle = AngleTime;
            canvas.RenderTransform = rtf;
        }

        /// <summary>
        /// 渲染时钟
        /// </summary>
        public void Show()
        {
            Dispose();
            //设置圆角
            bor.CornerRadius = new CornerRadius(this.ActualWidth / 2);

            Add(AddType.Hour);
            Add(AddType.Minute);
            Add(AddType.Second);

            AddName();

            thread = new Thread(new ThreadStart(ThreadMethod));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 生成名称
        /// </summary>
        private void AddName()
        {
            TextBlock tb = new TextBlock();
            tb.Text = "ZHT";
            tb.Foreground = new SolidColorBrush(Colors.White);
            tb.FontSize = 60;
            //tb.FontFamily = new FontFamily("华文琥珀");
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(tb);
        }

        /// <summary>
        /// UI更新线程
        /// </summary>
        private void ThreadMethod()
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    var s = DateTime.Now.Second;
                    var m = DateTime.Now.Minute;
                    var h = DateTime.Now.Hour;

                    //处理时
                    if (m == 0 && (int)BlockHour.Tag != h)
                    {
                        SetUI(CanvasHour, h);
                    }
                    //处理分
                    if (s == 0 && (int)BlockMinute.Tag != m)
                    {
                        SetUI(CanvasMinute, m);
                    }
                    //处理秒
                    SetUI(CanvasSecond, s);
                });
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="can"></param>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        private void SetUI(Canvas can, int tag)
        {
            var type = (AddType)can.Tag;
            foreach (TextBlock item in can.Children)
            {
                if ((int)item.Tag == tag)
                {
                    Debug.WriteLine(item.Text);

                    var fr = item.RenderTransform as RotateTransform;
                    var angle = 360 - fr.Angle;
                    var rtf = can.RenderTransform as RotateTransform;
                    DoubleAnimation db = null;
                    if (type == AddType.Minute)
                    {
                        angle -= 360;
                        db = new DoubleAnimation(angle, new Duration(TimeSpan.FromSeconds(1)));
                        db.Completed += DbMinute_Completed;
                        BlockMinute = item;
                    }
                    else if (type == AddType.Hour)
                    {
                        angle += 360;
                        db = new DoubleAnimation(angle, new Duration(TimeSpan.FromSeconds(1.5)));
                        db.Completed += DbHour_Completed;
                        BlockHour = item;
                    }
                    else
                    {
                        Console.WriteLine(angle);
                        db = new DoubleAnimation(angle, new Duration(TimeSpan.FromSeconds(0.25)));
                        db.Completed += DbSecond_Completed;
                        BlockSecond = item;
                    }
                    rtf.BeginAnimation(RotateTransform.AngleProperty, db);

                }
                else
                {
                    item.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7d7d7d"));
                }
            }
        }

        /// <summary>
        /// 秒 动画完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbSecond_Completed(object sender, EventArgs e)
        {
            BlockSecond.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// 时 动画完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbHour_Completed(object sender, EventArgs e)
        {
            var fr = CanvasHour.RenderTransform as RotateTransform;
            var angle = fr.Angle - 360;
            fr = null;
            RotateTransform rtf = new RotateTransform();
            rtf.CenterX = CenterPixToX;
            rtf.CenterY = CenterPixToY;
            rtf.Angle = angle;
            CanvasHour.RenderTransform = rtf;
            Debug.WriteLine(rtf.Angle);
            BlockHour.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// 分 动画完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbMinute_Completed(object sender, EventArgs e)
        {
            var fr = CanvasMinute.RenderTransform as RotateTransform;
            //var angle = fr.Angle + 360;
            var angle = fr.Angle;
            fr = null;
            RotateTransform rtf = new RotateTransform
            {
                CenterX = CenterPixToX,
                CenterY = CenterPixToY,
                Angle = angle
            };
            CanvasMinute.RenderTransform = rtf;
            Debug.WriteLine(rtf.Angle);
            BlockMinute.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            thread?.Abort();
            grid.Children.Clear();
        }
    }

    /// <summary>
    /// 添加类型
    /// </summary>
    public enum AddType
    {
        Hour,
        Minute,
        Second
    }
}
