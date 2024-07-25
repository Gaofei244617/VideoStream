using System.ComponentModel;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IP)));
            }
        }

        public ProtoEnum Protocol
        {
            get => _protocol;
            set
            {
                _protocol = value;
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
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public double Time { get; set; }

        public EncodeType Encoder { get; set; }
    }
}