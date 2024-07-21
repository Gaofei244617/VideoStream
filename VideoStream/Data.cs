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

    public class TabItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _id;                       //
        private string? _video;                //
        private ProtoEnum _protocol;           //
        private TransEnum _transProto;         //
        private StateEnum _state;              //

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