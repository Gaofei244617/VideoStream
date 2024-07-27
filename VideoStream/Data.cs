using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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

    // 推流状态
    public enum StateEnum
    {
        Init,
        Running,
        Stop
    }

    public class StreamItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _id = 0;                                   //
        private string? _video;                                //
        private string? _url;                                  //
        private string? _ip;                                   //
        private ProtoEnum _protocol = ProtoEnum.RTSP;          //
        private TransEnum _transProto = TransEnum.TCP;         //
        private StateEnum _state = StateEnum.Init;             //
        private VideoInfo? _info;                              //
        private Process? _ffmpeg = null;                       //
        public int RtspPort { set; get; }                      //
        public int RtmpPort { set; get; }                      //

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

        public StateEnum State
        {
            get => _state;
            set
            {
                _state = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
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
        protected void UpdateURL()
        {
            if (_protocol == ProtoEnum.RTSP)
            {
                URL = "rtsp://" + _ip + ":" + RtspPort.ToString() + "/" + Path.GetFileNameWithoutExtension(_video);
            }
            else if (_protocol == ProtoEnum.RTMP)
            {
                URL = "rtmp://" + _ip + ":" + RtmpPort.ToString() + "/" + Path.GetFileNameWithoutExtension(_video);
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
    }
}