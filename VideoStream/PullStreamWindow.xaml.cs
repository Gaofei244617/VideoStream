using Microsoft.Win32;
using Serilog;
using System.Diagnostics;
using System.Windows;

namespace VideoStream
{
    public partial class PullStreamWindow : Window
    {
        public PullStreamWindow()
        {
            InitializeComponent();
            //this.Closing += Window_Closing;
        }

        // 播放视频流
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            string? url = this.url.Text;
            if (url != null && url.Length > 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("ffplay.exe");
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = "-i " + url;

                try
                {
                    ffplay = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };

                    ffplay.Exited += new EventHandler(FFplay_Exited);
                    ffplay.Start();
                    Log.Information("start ffplay {0}", url);

                    this.Close();
                }
                catch (Exception ex)
                {
                    Log.Error("start ffplay error: {0}", ex.ToString());
                }
            }
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
                string fileName = saveFileDialog.FileName;
                string? url = this.url.Text;

                ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe");
                //startInfo.CreateNoWindow = true;
                //startInfo.UseShellExecute = false;
                //startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = "-i " + url + " -hide_banner -c copy " + fileName;

                try
                {
                    ffmpeg = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };

                    ffmpeg.Start();

                    Log.Information("start saving stream {0} to {1}", url, fileName);
                    Log.Information("command: ", startInfo.Arguments);

                    this.Close(); // 关闭窗口
                }
                catch (Exception ex)
                {
                    Log.Error("start saving stream faild: {0}, {1}", url, ex.ToString());
                }
            }
        }

        private void FFplay_Exited(object? sender, EventArgs e)
        {
            Log.Information("ffplay exit");
        }

        //// 关闭窗口
        //private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    ffplay?.Kill();
        //    ffmpeg?.Kill();
        //}

        private Process? ffplay = null;
        private Process? ffmpeg = null;
    }
}