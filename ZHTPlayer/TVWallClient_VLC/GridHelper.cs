using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vlc.DotNet.Forms;

namespace TVWallClient.UI
{

    public class GridHelpers
    {
        private static readonly DirectoryInfo libDirectory;
        private static ImageSource imageSource;
        private static readonly bool showLogo;  // 是否显示 Logo
        private static readonly bool UseWPFControl; // 是否自适应窗体，否则铺满

        static GridHelpers()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;

            // Default installation path of VideoLAN.LibVLC.Windows
            libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            imageSource = new BitmapImage(new Uri(@"Images\Logo.png", UriKind.Relative));
            showLogo = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("ShowLogo"));
            UseWPFControl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("Adaptive"));
        }

        /// <summary>
        /// 创建视频容器
        /// </summary>
        /// <returns></returns>
        public static Border CreateBorder()
        {
            Border border = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.White,
                AllowDrop = true,
                Background = Brushes.Gray
            };

            if (showLogo)
            {
                border.Child = new Image
                {
                    Source = imageSource,
                    Width = 125,
                    Height = 161
                };
            }
            return border;
        }

        /// <summary>
        /// 给对应 Border 放置监控，查看是否已有，有就停止
        /// </summary>
        /// <param name="border"></param>
        /// <param name="panelItem"></param>
        public static void SetCameraOnBorder(Border border, CameraPanelItem panelItem)
        {
            if (border.DataContext is CameraPanelItem oldItem)
            {
                oldItem.GetPlayView().Stop();
                border.DataContext = null;
                border.Child = null;
            }

            var channel = panelItem.GetPlayView();
            border.DataContext = panelItem;

            var winFormHost = new WindowsFormsHost();
            var playPanle = new System.Windows.Forms.Panel();
            winFormHost.Child = playPanle;
            border.Background = Brushes.Gray;
            border.Child = winFormHost;
            playPanle.Controls.Clear();
            channel.OnPlayError += (s, e) =>
            {
                var log = NLog.LogManager.GetCurrentClassLogger();
                log.Error(e.ErrorMessage + e.Code);
            };
            if (channel.Playing)
            {
                channel.Stop();
            }
            channel.Play(winFormHost.Child);
        }

        /// <summary>
        /// 播放视频流等
        /// </summary>
        /// <param name="border"></param>
        /// <param name="panelItem"></param>
        public static void SetBorderChildByVLC(Border border, CameraPanelItem panelItem)
        {
            if (border.DataContext is CameraPanelItem oldItem)
            {
                border.DataContext = null;
                border.Child = null;
            }
            border.DataContext = panelItem;

            if (UseWPFControl)
            {
                Vlc.DotNet.Wpf.VlcControl vlcControl = new Vlc.DotNet.Wpf.VlcControl();
                vlcControl.SourceProvider.CreatePlayer(libDirectory);

                if (panelItem.RtspStrs.Count > 1)
                {
                    VLCGroup vlcGroup = new VLCGroup(1, 15);
                    vlcGroup.PlayOnWPF(vlcControl, panelItem.RtspStrs);
                }
                else
                {
                    vlcControl.SourceProvider.MediaPlayer.Play(new Uri(panelItem.RtspStrs[0]));
                }
                border.Child = vlcControl;
            }
            else
            {
                VlcControl vlcControl = new VlcControl();
                string ratio = border.ActualWidth + ":" + border.ActualHeight;
                vlcControl.BeginInit();
                vlcControl.VlcLibDirectory = libDirectory;
                vlcControl.EndInit();
                WindowsFormsHost windowsFormsHost = new WindowsFormsHost
                {
                    Child = vlcControl
                };
                border.Child = windowsFormsHost;
                vlcControl.Video.FullScreen = true;
                vlcControl.Video.AspectRatio = ratio;

                if (panelItem.RtspStrs.Count > 1)
                {
                    VLCGroup vlcGroup = new VLCGroup(1, 15);
                    vlcGroup.Play(vlcControl, panelItem.RtspStrs);
                }
                else
                {
                    vlcControl.Play(new Uri(panelItem.RtspStrs[0]));
                }
            }
        }

        public static readonly DependencyProperty CamearPanelProperty =
           DependencyProperty.RegisterAttached(
               "CamearPanel", typeof(CameraPanel), typeof(GridHelpers),
               new PropertyMetadata(new CameraPanel(), CamearPanelChaneg));
        public static void CamearPanelChaneg(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid))
            {
                return;
            }
            Grid grid = obj as Grid;
            CameraPanel oldTV = e.OldValue as CameraPanel;
            CameraPanel newTV = e.NewValue as CameraPanel;

            // 只有布局的，绘制布局
            // 两者都有的，判断布局是否有变化
            // 只有视频源信息的，替换视频源
            if (newTV.LayoutInfo != null && newTV.Cameras == null)
            {
                CreateGrid(grid, newTV.LayoutInfo);
            }
            else if (newTV.LayoutInfo != null && newTV.Cameras != null)
            {
                if (oldTV.LayoutInfo != null && oldTV.LayoutInfo.Id == newTV.LayoutInfo.Id)
                {
                    // 替换视频源
                    foreach (var panelItem in newTV.Cameras)
                    {
                        Border border = GetBorderByTag(grid.Children, panelItem.Index);
                        if (panelItem.Flag == 1)
                        {
                            SetBorderChildByVLC(border, panelItem);
                        }
                        else if (panelItem.Flag == 0)
                        {
                            if (border.Child is WindowsFormsHost windowsForms)
                            {
                                if (windowsForms.Child is VlcControl vlcControl)
                                {
                                    vlcControl.Stop();
                                }
                            }
                            border.Child = new Image
                            {
                                Source = imageSource,
                                Width = 125,
                                Height = 161
                            };
                        }
                    }
                }
                else
                {
                    CreateGrid(grid, newTV.LayoutInfo);
                    foreach (var panelItem in newTV.Cameras)
                    {
                        Border border = GetBorderByTag(grid.Children, panelItem.Index);
                        SetBorderChildByVLC(border, panelItem);
                    }
                }
            }
            else if (newTV.Cameras != null)
            {
                if (grid.Children.Count > 0)
                {
                    // 替换视频源
                    foreach (var panelItem in newTV.Cameras)
                    {
                        Border border = GetBorderByTag(grid.Children, panelItem.Index);
                        if (panelItem.Flag == 1)
                        {
                            SetBorderChildByVLC(border, panelItem);
                        }
                        else if (panelItem.Flag == 0)
                        {
                            if (border.Child is WindowsFormsHost windowsForms)
                            {
                                if (windowsForms.Child is VlcControl vlcControl)
                                {
                                    vlcControl.Stop();
                                }
                            }
                            border.Child = new Image
                            {
                                Source = imageSource,
                                Width = 125,
                                Height = 161
                            };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据布局生成网格布局
        /// </summary>
        /// <param name="grid">网格</param>
        /// <param name="layout"></param>
        public static void CreateGrid(Grid grid, LayoutInfo layout)
        {
            for (int i = 0; i < grid.Children.Count; i++)
            {
                var border = grid.Children[i] as Border;
                if (border.DataContext is CameraPanelItem oldItem)
                {
                    oldItem.GetPlayView().Stop();
                }
                border.DataContext = null;
                border.Child = null;
            }
            grid.Children.Clear();

            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            // 分隔，否则都是全屏
            for (int i = 0; i < layout.Rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < layout.Cols; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach (var item in layout.LayoutDetails)
            {
                var border = CreateBorder();
                border.Tag = item.Indexs;
                int startRow = (item.Indexs - 1) / layout.Cols;
                int startColumn = (item.Indexs - 1) % layout.Cols;
                border.SetValue(Grid.RowProperty, startRow);
                border.SetValue(Grid.ColumnProperty, startColumn);
                if (item.IsMerger == 1)
                {
                    border.SetValue(Grid.ColumnSpanProperty, item.Cols);
                    border.SetValue(Grid.RowSpanProperty, item.Rows);
                }
                grid.Children.Add(border);
            }
        }

        /// <summary>
        /// 根据 Index 获取Border
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Border GetBorderByTag(UIElementCollection elementCollection, int index)
        {
            foreach (Border border in elementCollection)
            {
                if (int.Parse(border.Tag.ToString()) == index)
                {
                    return border;
                }
            }
            return null;
        }
    }
}
