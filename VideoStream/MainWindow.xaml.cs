using System.Collections.ObjectModel;
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
            this.VideoTable.ItemsSource = items;

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

            // 解析mediamtx配置文件
            string content = File.ReadAllText("mediamtx.yml");
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yamlObject = deserializer.Deserialize<dynamic>(content);

            rtspPort = int.Parse(yamlObject["rtspAddress"].Split(':')[1]);
            rtmpPort = int.Parse(yamlObject["rtmpAddress"].Split(':')[1]);
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
                    if (state == StateEnum.Init)
                    {
                        item.State = StateEnum.Running;
                        button.Content = "Stop";
                    }
                    else if (state == StateEnum.Running)
                    {
                        item.State = StateEnum.Stop;
                        button.Content = "推流";
                    }
                    else if (state == StateEnum.Stop)
                    {
                        item.State = StateEnum.Running;
                        button.Content = "Stop";
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
                    for (int i = 0; i < items.Count(); i++)
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

        private void File_Drop(object sender, DragEventArgs e)
        {
            // 获取拖拽进来的文件列表
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                foreach (var file in files)
                {
                    StreamItem item = new StreamItem();
                    item.Video = Path.GetFileName(file); ;
                    item.Info = (new VideoProbe(file)).info();
                    item.ID = items.Count() + 1;

                    items.Add(item);
                }
            }
        }

        // DataGrid item source
        private ObservableCollection<StreamItem> items = new ObservableCollection<StreamItem>();

        // 本机IP
        private ObservableCollection<string> ips = new ObservableCollection<string>();

        private int rtspPort = 0;  // RTSP流端口
        private int rtmpPort = 0;  // RTMP流端口
    }
}