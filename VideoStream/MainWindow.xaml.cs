﻿using Microsoft.Win32;
using Serilog;
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
            Log.Information("Init MainWindow");

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
                    Log.Information("本机IP: {0}", ip.ToString());
                }
            }
            ips.Add("127.0.0.1");
            Log.Information("本机IP: 127.0.0.1");

            // 解析mediamtx配置文件
            string content = File.ReadAllText("mediamtx.yml");
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yamlObject = deserializer.Deserialize<dynamic>(content);

            rtspPort = int.Parse(yamlObject["rtspAddress"].Split(':')[1]);
            rtmpPort = int.Parse(yamlObject["rtmpAddress"].Split(':')[1]);
            Log.Information("RTSP port: {0}, RTMP port: {1}", rtspPort, rtmpPort);

            // 启动推流服务器
            ProcessStartInfo startInfo = new ProcessStartInfo("mediamtx.exe");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            mediamtx = Process.Start(startInfo);
            if (mediamtx == null)
            {
                Log.Error("无法启动推流服务");
                MessageBox.Show("无法启动推流服务");
            }
            Log.Information("启动mediamtx服务");
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
                        StartStream(item);
                    }
                    else if (state == StateEnum.Running)
                    {
                        // Stop
                        StopStream(item);
                    }
                }
            }
        }

        // 推流
        private void StartStream(StreamItem item)
        {
            if (GetFFmpegParams(item) is string param)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe");
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = param;

                try
                {
                    item.FFmpeg = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };

                    item.FFmpeg.Exited += new EventHandler(FFmpeg_Exited);
                    item.FFmpeg.Start();
                    Log.Information("start ffmpeg {0}", param);
                }
                catch (Exception e)
                {
                    MessageBox.Show(Path.GetFileName(item.Video) + "\nException: " + e.ToString(), "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (item.FFmpeg == null)
                {
                    Log.Error("无法启动ffmpeg推流进程: {0}", item?.Info?.VideoPath);
                    MessageBox.Show("无法启动ffmpeg推流进程", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    item.State = StateEnum.Running;
                    item.NextState = "Stop";
                }
            }
        }

        private void StopStream(StreamItem item)
        {
            item.FFmpeg?.Kill();
            item.FFmpeg = null;
            item.State = StateEnum.Stop;
            item.NextState = "推流";
        }

        // 全部开始
        private void StartStreamAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].State == StateEnum.Init || items[i].State == StateEnum.Stop)
                {
                    StartStream(items[i]);
                }
            }
        }

        // 全部开始
        private void StopStreamAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].State == StateEnum.Running)
                {
                    StopStream(items[i]);
                }
            }
        }

        // 拉流
        private void PullStream_Click(object sender, RoutedEventArgs e)
        {
            var win = new PullStreamWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner, // 设置新窗口在父窗口中居中
                Owner = this // 设置父窗口
            };

            win.Show();
        }

        // 推流进程结束/异常退出
        private void FFmpeg_Exited(object? sender, EventArgs e)
        {
            if (sender is Process process)
            {
                foreach (var item in items)
                {
                    if (item.FFmpeg != null && item.FFmpeg.Id == process.Id)
                    {
                        StopStream(item);
                        MessageBox.Show("推流进程异常:\n" + item.Video, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    if (item.FFmpeg != null)
                    {
                        StopStream(item);
                    }
                    items.Remove(item);

                    // update index
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].ID = i + 1;
                    }
                }
            }
        }

        // 导入视频
        private void ImportVideo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string[] files = openFileDialog.FileNames;
                ImportVideos(files);
            }
        }

        // 导入本地视频
        private void File_Drop(object sender, DragEventArgs e)
        {
            // 获取拖拽进来的文件列表
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                ImportVideos(files);
            }
        }

        private void ImportVideos(string[] files)
        {
            foreach (var file in files)
            {
                if (file.Contains(' '))
                {
                    string msg = "文件名中含有空格: " + Path.GetFileName(file);
                    Log.Warning(msg);
                    MessageBox.Show(msg, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

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
                Log.Information("导入视频: {0}", file);
            }
        }

        // ffmpeg推流参数
        private string? GetFFmpegParams(StreamItem item)
        {
            string? param = null;
            if (item.Protocol == ProtoEnum.RTSP)
            {
                param = "-re -stream_loop -1 -i " + item?.Info?.VideoPath + " " + "-c copy -f rtsp -rtsp_transport tcp " + item?.GetStreamURL();
            }
            else if (item.Protocol == ProtoEnum.RTMP)
            {
                param = "-re -stream_loop -1 -i " + item?.Info?.VideoPath + " " + "-c copy -f flv " + item?.GetStreamURL();
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