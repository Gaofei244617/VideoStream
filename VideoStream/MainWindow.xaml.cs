using System.Collections.ObjectModel;
using System.IO;
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
    }
}