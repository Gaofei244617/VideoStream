using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace VideoStream
{
    // 实时流协议
    public enum ProtoEnum
    {
        RTSP,
        RTMP
    }

    // 传输协议
    public enum TransEnum
    {
        TCP,
        UDP
    }

    public class StreamItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _id = 0;                                   //
        private string? _video;                                // 视频名(不含路径)
        private string? _url;                                  // 推流地址
        private string? _ip;                                   // 本机IP
        private ProtoEnum _protocol = ProtoEnum.RTSP;          // 流媒体协议(RTSP/RTMP)
        private TransEnum _transProto = TransEnum.TCP;         // 传输协议
        private string _state = "Stop";                        // 推流状态
        private string _nextState = "推流";                    // 下一操作状态
        private VideoInfo? _info;                              // 视频信息
        private Process? _ffmpeg = null;                       // ffmpeg推流句柄

        public StreamItem()
        {
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            Timer.Tick += Timer_Tick;
        }

        public int RtspPort { set; get; }                      // rtsp端口号
        public int RtmpPort { set; get; }                      // rtmp端口号

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
            }
        }

        public string? Video
        {
            get => _video;
            set
            {
                _video = value;
                UpdateURL();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Video)));
            }
        }

        public string? URL
        {
            get => _url;
            set
            {
                _url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(URL)));
            }
        }

        public string? IP
        {
            get => _ip;
            set
            {
                _ip = value;
                UpdateURL();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IP)));
            }
        }

        public ProtoEnum Protocol
        {
            get => _protocol;
            set
            {
                _protocol = value;
                UpdateURL();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Protocol)));
            }
        }

        public TransEnum TransProto
        {
            get => _transProto;
            set
            {
                _transProto = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TransEnum)));
            }
        }

        public string State
        {
            get => _state;
            set
            {
                _state = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }

        public string NextState
        {
            get => _nextState;
            set
            {
                _nextState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextState)));
            }
        }

        public VideoInfo? Info
        {
            get => _info;
            set
            {
                _info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info)));
            }
        }

        public Process? FFmpeg
        {
            get => _ffmpeg;
            set
            {
                _ffmpeg = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Process)));
            }
        }

        // 获取推流地址
        private string? GetStreamURL()
        {
            string validChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-"; // URL中允许出现的字符

            char[]? chars = Path.GetFileNameWithoutExtension(_video)?.ToCharArray();
            if (chars == null)
            {
                return null;
            }

            Random random = new();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!validChar.Contains(chars[i]))
                {
                    int n = random.Next(0, 27);
                    chars[i] = validChar[n];
                }
            }

            string path = new(chars);
            string? url = null;
            if (_protocol == ProtoEnum.RTSP)
            {
                url = "rtsp://" + _ip + ":" + RtspPort.ToString() + "/" + path;
            }
            else if (_protocol == ProtoEnum.RTMP)
            {
                url = "rtmp://" + _ip + ":" + RtmpPort.ToString() + "/" + path;
            }

            return url;
        }

        // 更新推流地址
        protected void UpdateURL()
        {
            URL = GetStreamURL();
        }

        private readonly DispatcherTimer Timer;             // 推流计时器
        private TimeSpan elapsedTime = TimeSpan.Zero;

        private void Timer_Tick(object? sender, EventArgs e)
        {
            elapsedTime += Timer.Interval;
            State = elapsedTime.ToString("hh\\:mm\\:ss");
        }

        // 启动推流
        public void StartStream()
        {
            if (GetFFmpegParams() is string param)
            {
                ProcessStartInfo startInfo = new("ffmpeg.exe")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = param
                };

                try
                {
                    FFmpeg = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };

                    FFmpeg.Exited += new EventHandler(FFmpeg_Exited);
                    FFmpeg.Start();
                    Timer.Start();
                    Log.Information("start ffmpeg {0}", param);
                }
                catch (Exception e)
                {
                    MessageBox.Show(Path.GetFileName(Video) + "\nException: " + e.ToString(), "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (FFmpeg == null)
                {
                    Log.Error("无法启动ffmpeg推流进程: {0}", Info?.VideoPath);
                    MessageBox.Show("无法启动ffmpeg推流进程", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    NextState = "Stop";
                }
            }
        }

        // 停止推流
        public void StopStream()
        {
            FFmpeg?.Kill();
            FFmpeg = null;

            Timer.Stop();
            elapsedTime = TimeSpan.Zero;
            State = "Stop";
            NextState = "推流";
            Log.Information("结束推流: {0}", Info?.VideoPath);
        }

        // ffmpeg推流参数
        private string? GetFFmpegParams()
        {
            string? param = null;
            if (Protocol == ProtoEnum.RTSP)
            {
                param = "-re -stream_loop -1 -i " + Info?.VideoPath + " " + "-c copy -f rtsp -rtsp_transport tcp " + GetStreamURL();
            }
            else if (Protocol == ProtoEnum.RTMP)
            {
                param = "-re -stream_loop -1 -i " + Info?.VideoPath + " " + "-c copy -f flv " + GetStreamURL();
            }

            return param;
        }

        private void FFmpeg_Exited(object? sender, EventArgs e)
        {
            if (sender is Process)
            {
                StopStream();
                //MessageBox.Show("推流进程异常:\n" + Video, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    // 视频编码格式
    public enum EncodeType
    {
        H264,
        H265,
        Other
    }

    public class VideoInfo
    {
        public string? VideoPath { get; set; }            // 文件名,含绝对路径
        public int Width { get; set; }                    // 视频宽度
        public int Height { get; set; }                   // 视频高度
        public double FrameRate { get; set; }             // 帧率
        public double Duration { get; set; }              // 视频时长
        public EncodeType Encoder { get; set; }           // 编码格式

        public override string ToString()
        {
            return $"文件: {VideoPath} \n画面: {Width}x{Height} \n帧率: {FrameRate} fps \n时长: {Duration}秒 \n编码格式: {Encoder}";
        }
    }
}