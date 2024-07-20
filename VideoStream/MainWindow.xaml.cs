using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace VideoStream
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.VideoTable.ItemsSource = items;

            items.Add(new TabItem() { ID = 1, Video = "John Doe", Protocol = ProtoEnum.RTSP, State = StateEnum.Init });
        }

        // 推流button
        private void VideoStream_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // 获取当前行数据
                if (button.DataContext is TabItem item)
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
                if (button.DataContext is TabItem item)
                {
                    items.Remove(item);
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
                    // 文件名
                    MessageBox.Show(file);
                }
            }
        }

        // DataGrid item source
        private ObservableCollection<TabItem> items = new ObservableCollection<TabItem>();
    }
}