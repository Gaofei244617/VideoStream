using FFmpeg.AutoGen;
using System.Windows;

namespace VideoStream
{
    public unsafe class VideoProbe : IDisposable
    {
        private readonly AVFormatContext* _pFormatContext;

        private VideoInfo _info = new VideoInfo();

        public VideoProbe(string videoPath)
        {
            int ret = -1;

            // 打开视频文件
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;

            if ((ret = ffmpeg.avformat_open_input(&pFormatContext, videoPath, null, null)) != 0)
            {
                // 打开文件失败
                MessageBox.Show($"Error opening the video file: {videoPath}");
            }

            // 获取流信息
            if ((ret = ffmpeg.avformat_find_stream_info(pFormatContext, null)) < 0)
            {
                // 获取流信息失败
                MessageBox.Show($"Error finding stream info: {ret}");
            }

            // 找到视频流
            int videoStreamIndex = -1;
            for (int i = 0; i < pFormatContext->nb_streams; i++)
            {
                if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    videoStreamIndex = i;
                    break;
                }
            }

            if (videoStreamIndex == -1)
            {
                // 没有找到视频流
                MessageBox.Show("Not find a video stream");
            }

            // 获取视频
            AVCodecParameters* codecParameters = pFormatContext->streams[videoStreamIndex]->codecpar;
            _info.Width = codecParameters->width;
            _info.Height = codecParameters->height;
            _info.FrameRate = codecParameters->framerate.num / (double)codecParameters->framerate.den; // 帧率

            // 编码格式
            AVCodecID encoder = codecParameters->codec_id;
            switch (encoder)
            {
                case AVCodecID.AV_CODEC_ID_H264:
                    _info.Encoder = EncodeType.H264;
                    break;

                case AVCodecID.AV_CODEC_ID_HEVC:
                    _info.Encoder = EncodeType.H265;
                    break;

                default:
                    _info.Encoder = EncodeType.Other;
                    break;
            }

            // 视频时长(秒)
            long duration = pFormatContext->streams[videoStreamIndex]->duration;
            AVRational time_base = pFormatContext->streams[videoStreamIndex]->time_base;
            _info.Time = duration * time_base.num / (double)time_base.den;

            // 关闭输入
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public VideoInfo info()
        {
            return _info;
        }

        public void Dispose()
        { }
    }
}