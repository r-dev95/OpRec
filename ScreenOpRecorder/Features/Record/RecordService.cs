using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Overlay;

using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScreenOpRecorder.Features.Record
{
    public class RecordService : IDisposable
    {
        private readonly ILogger<RecordService> _logger;

        private readonly OverlayViewModel _viewModel;

        private readonly MouseHookService _mouseHookService;

        private CompositionController _compositionEngine;

        private CanvasDevice _device;
        private GraphicsCaptureItem _item;

        private Direct3D11CaptureFramePool _framePool;
        private GraphicsCaptureSession _session;

        private MediaStreamSource _mediaStreamSource;
        private VideoStreamDescriptor _videoDescriptor;
        private MediaTranscoder _transcoder;
        private MediaEncodingProfile _profile;

        private CanvasRenderTarget? _renderTarget;
        private CanvasRenderTarget? _currentFrame;

        private Task? _recordingTask;

        private DateTimeOffset _startTime;

        private bool _isStopRecord = false;

        public RecordService(ILogger<RecordService> logger, OverlayViewModel viewModel, MouseHookService mouseHookService)
        {
            _logger = logger;
            _viewModel = viewModel;
            _mouseHookService = mouseHookService;
        }

        public void Setup(GraphicsCaptureItem item)
        {
            int width = item.Size.Width;
            int height = item.Size.Height;

            _isStopRecord = false;

            _item = item;

            _compositionEngine = new CompositionController(_device, width, height);

            _device = new CanvasDevice();

            var videoProperties = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Bgra8, (uint)width, (uint)height);
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
                _item.Size);

            _framePool.FrameArrived += OnFrameArrived;

            _session = _framePool.CreateCaptureSession(_item);
            _session.StartCapture();

            _startTime = DateTimeOffset.Now;

            var fileOp = await file.OpenAsync(FileAccessMode.ReadWrite);
            var prepareOp = await _transcoder.PrepareMediaStreamSourceTranscodeAsync(_mediaStreamSource, fileOp, _profile);
            _recordingTask = prepareOp.TranscodeAsync().AsTask();
        }

        public void Dispose()
        {
            _currentFrame?.Dispose();
            _currentFrame = null;

            _mediaStreamSource.SampleRequested -= OnSampleRequested;
            _mediaStreamSource = null;

            _renderTarget?.Dispose();
            _renderTarget = null;

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
            if (_isStopRecord || _currentFrame == null)
            {
                args.Request.Sample = null;
                return;
            }

            try
            {
                var surface = (IDirect3DSurface)_currentFrame;
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

            using var canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_device, frame.Surface);

            if (_renderTarget == null)
            {
                _renderTarget = new CanvasRenderTarget(_device, (float)canvasBitmap.Size.Width, (float)canvasBitmap.Size.Height, 96);
            }

            using (var ds = _renderTarget.CreateDrawingSession())
            {
                //ds.DrawImage(canvasBitmap);
                float scaleX = (float)_renderTarget.Size.Width / _item.Size.Width;
                float scaleY = (float)_renderTarget.Size.Height / _item.Size.Height;

                int mouseX = (int)(_mouseHookService.CurrentX * scaleX);
                int mouseY = (int)(_mouseHookService.CurrentY * scaleY);

                _compositionEngine.ComposeFrame(ds, canvasBitmap, mouseX, mouseY);
            }

            _currentFrame = _renderTarget;
        }
    }
}
