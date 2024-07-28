using Serilog;
using System.Windows;

namespace VideoStream
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 初始化日志
            Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("stream.log", rollOnFileSizeLimit: true, fileSizeLimitBytes: 1024 * 1024 * 1024).CreateLogger();
        }
    }
}