using Microsoft.Win32;
using System.Windows;

namespace VideoStream
{
    /// <summary>
    /// PullStreamWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PullStreamWindow : Window
    {
        public PullStreamWindow()
        {
            InitializeComponent();
        }

        // 播放视频流
        private void Play_Click(object sender, RoutedEventArgs e)
        {
        }

        // 保存视频流
        private void SaveVideo_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "保存视频流",
                Filter = "Video files (*.mp4)|*.mp4|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // 用户选择了文件并点击了保存
                string filename = saveFileDialog.FileName;
            }
        }
    }
}