using FFmpeg.AutoGen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VideoStream
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;

            this.VideoTable.ItemsSource = items;
            this.LocalIP.ItemsSource = ips;

            // 获取本机IP
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in ipEntry.AddressList)
            {
                // IPv4地址
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
            ips.Add("127.0.0.1");

            // 解析mediamtx配置文件
            string content = File.ReadAllText("mediamtx.yml");
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yamlObject = deserializer.Deserialize<dynamic>(content);

            rtspPort = int.Parse(yamlObject["rtspAddress"].Split(':')[1]);
            rtmpPort = int.Parse(yamlObject["rtmpAddress"].Split(':')[1]);

            // 启动推流服务器
            ProcessStartInfo startInfo = new ProcessStartInfo("mediamtx.exe");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            mediamtx = Process.Start(startInfo);
            if (mediamtx == null)
            {
                MessageBox.Show("无法启动Mediamtx进程!");
            }
        }

        // 关闭窗口
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var item in items)
            {
                item?.FFmpeg?.Kill();
            }
            mediamtx?.Kill();
        }

        // 推流button
        private void VideoStream_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // 获取当前行数据
                if (button.DataContext is StreamItem item)
                {
                    StateEnum state = item.State;
                    if (state == StateEnum.Init || state == StateEnum.Stop)
                    {
                        // 推流
                        if (GetFFmpegParams(item) is string param)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe");
                            startInfo.CreateNoWindow = true;
                            startInfo.UseShellExecute = false;
                            startInfo.RedirectStandardOutput = true;
                            startInfo.Arguments = param;

                            item.FFmpeg = Process.Start(startInfo);
                            if (item.FFmpeg == null)
                            {
                                MessageBox.Show("无法启动ffmpeg推流");
                            }
                            else
                            {
                                item.State = StateEnum.Running;
                                button.Content = "Stop";
                            }
                        }
                    }
                    else if (state == StateEnum.Running)
                    {
                        // Stop
                        item.FFmpeg?.Kill();
                        item.FFmpeg = null;

                        item.State = StateEnum.Stop;
                        button.Content = "推流";
                    }
                }
            }
        }

        // 删除button
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // 获取当前行数据
                if (button.DataContext is StreamItem item)
                {
                    items.Remove(item);

                    // update index
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].ID = i + 1;
                    }
                }
            }
        }

        private void PushAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Push All");
        }

        // 导入本地视频
        private void File_Drop(object sender, DragEventArgs e)
        {
            // 获取拖拽进来的文件列表
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                foreach (var file in files)
                {
                    StreamItem item = new() 
                    {
                        RtspPort = rtspPort,
                        RtmpPort = rtmpPort
                    };

                    item.Video = Path.GetFileName(file); ;
                    item.Info = (new VideoProbe(file)).info();
                    item.ID = items.Count + 1;
                    item.IP = ips.Count > 0 ? ips[0] : null;

                    items.Add(item);
                }
            }
        }

        // ffmpeg推流参数
        private string? GetFFmpegParams(StreamItem item)
        {
            string? param = null;
            if (item.Protocol == ProtoEnum.RTSP)
            {
                param = "-re -stream_loop -1 -i " + item?.Info?.VideoPath + " " + "-c copy -f rtsp -rtsp_transport tcp rtsp://" + item?.IP + ":" + item?.RtspPort + "/" + Path.GetFileNameWithoutExtension(item?.Info?.VideoPath);
            }
            else if (item.Protocol == ProtoEnum.RTMP)
            {
                param = "-re -stream_loop -1 -i " + item?.Info?.VideoPath + " " + "-c copy -f flv rtmp://" + item?.IP + ":" + item?.RtmpPort + "/" + Path.GetFileNameWithoutExtension(item?.Info?.VideoPath);
            }

            return param;
        }

        // DataGrid item source
        private ObservableCollection<StreamItem> items = new ObservableCollection<StreamItem>();

        // 本机IP
        private ObservableCollection<string> ips = new ObservableCollection<string>();

        private int rtspPort = 0;  // RTSP流端口
        private int rtmpPort = 0;  // RTMP流端口

        private Process? mediamtx = null;
    }
}