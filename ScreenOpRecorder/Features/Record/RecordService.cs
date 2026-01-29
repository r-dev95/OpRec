using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Overlay;

using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace ScreenOpRecorder.Features.Record
{
    public class RecordService : IDisposable
    {
        private readonly ILogger<RecordService> _logger;

        private readonly OverlayViewModel _viewModel;

        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;

        private CompositionManager? _compositionManager;

        private CanvasDevice? _device;
        private GraphicsCaptureItem? _item;

        private Direct3D11CaptureFramePool? _framePool;
        private GraphicsCaptureSession? _session;

        private MediaStreamSource? _mediaStreamSource;
        private VideoStreamDescriptor? _videoDescriptor;
        private MediaTranscoder? _transcoder;
        private MediaEncodingProfile? _profile;

        private CanvasRenderTarget? _renderTarget;
        private CanvasBitmap? _canvasBitmap;

        private Task? _recordingTask;

        private DateTimeOffset _startTime;

        private bool _isStopRecord = false;

        public RecordService(ILogger<RecordService> logger, OverlayViewModel viewModel, MouseHookService mouseHookService, KeyboardHookService keyboardHookService)
        {
            _logger = logger;
            _viewModel = viewModel;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
        }

        public void Setup(GraphicsCaptureItem item)
        {
            _isStopRecord = false;

            _item = item;

            _compositionManager = new CompositionManager(_mouseHookService, _keyboardHookService, _item);

            _device = new CanvasDevice();

            var videoProperties = VideoEncodingProperties.CreateUncompressed(
                MediaEncodingSubtypes.Bgra8,
                (uint)item.Size.Width,
                (uint)item.Size.Height);
            _videoDescriptor = new VideoStreamDescriptor(videoProperties);

            _mediaStreamSource = new MediaStreamSource(_videoDescriptor);
            _mediaStreamSource.BufferTime = TimeSpan.FromSeconds(0);
            _mediaStreamSource.SampleRequested += OnSampleRequested;

            _profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

            _transcoder = new MediaTranscoder();
        }

        public async Task StartAsync(StorageFile file)
        {
            _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                _device,
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                _item!.Size);

            _framePool.FrameArrived += OnFrameArrived;

            _session = _framePool.CreateCaptureSession(_item);
            _session.StartCapture();

            _startTime = DateTimeOffset.Now;

            var fileOp = await file.OpenAsync(FileAccessMode.ReadWrite);
            var prepareOp = await _transcoder!.PrepareMediaStreamSourceTranscodeAsync(_mediaStreamSource, fileOp, _profile);
            _recordingTask = prepareOp.TranscodeAsync().AsTask();
        }

        public void Dispose()
        {
            _mediaStreamSource!.SampleRequested -= OnSampleRequested;
            _mediaStreamSource = null;

            _renderTarget?.Dispose();
            _renderTarget = null;

            _canvasBitmap?.Dispose();
            _canvasBitmap = null;

            _framePool?.Dispose();
            _framePool = null;

            _session?.Dispose();
            _session = null;
        }

        public async Task StopAsync()
        {
            _isStopRecord = true;

            if (_recordingTask != null)
            {
                await _recordingTask;
            }

            Dispose();
        }

        private void OnSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (_isStopRecord || _canvasBitmap == null)
            {
                args.Request.Sample = null;
                return;
            }

            if (_renderTarget == null)
            {
                _renderTarget = new CanvasRenderTarget(_device, (float)_canvasBitmap.Size.Width, (float)_canvasBitmap.Size.Height, 96);
            }

            _compositionManager!.ComposeFrame(_renderTarget, _canvasBitmap);

            try
            {
                var surface = (IDirect3DSurface)_renderTarget;
                var timeStamp = DateTimeOffset.Now - _startTime;
                args.Request.Sample = MediaStreamSample.CreateFromDirect3D11Surface(surface, timeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Sample Error: {}", ex.Message);
            }
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            using var frame = sender.TryGetNextFrame();
            if (frame == null)
            {
                return;
            }

            _canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_device, frame.Surface);
        }
    }
}
