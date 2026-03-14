using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;

using OpRec.Application.Input.Ports;
using OpRec.Application.Settings.Ports;
using OpRec.Common.Helpers;
using OpRec.Domain.ValueObjects;
using OpRec.Infrastructure.Recording.Models;
using OpRec.Infrastructure.Settings;

using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace OpRec.Infrastructure.Recording.Video
{
    public sealed class VideoCapture : IDisposable
    {
        private readonly ILogger<VideoCapture> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IMouseInputListener _mouseInputListener;

        private CompositionManager? _compositionManager;

        private CanvasDevice? _device;
        private CanvasRenderTarget? _renderTarget;
        private CanvasBitmap? _canvasBitmap;

        private GraphicsCaptureItem? _item;
        private Direct3D11CaptureFramePool? _framePool;
        private GraphicsCaptureSession? _session;

        private MediaStreamSource? _mediaStreamSource;
        private VideoStreamDescriptor? _videoDescriptor;
        private MediaEncodingProfile? _profile;
        private MediaTranscoder? _transcoder;

        private Rect _captureArea;

        private RecordingState _state = RecordingState.Waiting;

        private Task? _recordingTask;
        private DateTimeOffset _startTime;
        public bool HasSelectedCaptureArea => _item != null;

        public event Action<RecordingState>? RecordingStateChanged;
        public event Action<ScreenRect>? ZoomAreaChanged;

        public VideoCapture(
            ILogger<VideoCapture> logger,
            IUserSettingsService settingsService,
            IMouseInputListener mouseInputListener)
        {
            _logger = logger;
            _settingsService = settingsService;
            _mouseInputListener = mouseInputListener;
        }

        public async Task<bool> StartAsync(StorageFile filePath)
        {
            if (_state != RecordingState.Ready || _item == null)
            {
                if (_state != RecordingState.Ready)
                {
                    _logger.LogWarning("Recording state must be Ready.");
                }

                if (_item == null)
                {
                    _logger.LogWarning("No capture area selected.");
                }

                return false;
            }

            ChangeState(RecordingState.Starting);
            Setup();

            try
            {
                await StartCaptureAsync(filePath);
                ChangeState(RecordingState.Recording);
            }
            catch
            {
                Cleanup();
                ChangeState(RecordingState.Ready);
                throw;
            }

            return true;
        }

        public async Task StopAsync()
        {
            if (_state != RecordingState.Recording)
            {
                Cleanup();
                ChangeState(RecordingState.Waiting);
                return;
            }

            ChangeState(RecordingState.Stopping);
            try
            {
                if (_recordingTask != null)
                {
                    await _recordingTask;
                }
            }
            finally
            {
                Cleanup();
                ChangeState(RecordingState.Waiting);
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        public bool TrySelectCaptureArea(ScreenRect captureArea)
        {
            if (_state != RecordingState.Waiting && _state != RecordingState.Ready)
            {
                return false;
            }

            var captureItem = WindowHelper.CreateForMonitor(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);
            if (captureItem == null)
            {
                return false;
            }

            _item = captureItem;
            _captureArea = new Rect(captureArea.X, captureArea.Y, captureArea.Width, captureArea.Height);

            ChangeState(RecordingState.Ready);
            return true;
        }

        public bool TryToggleZoomAt(int screenX, int screenY)
        {
            if (_state != RecordingState.Recording)
            {
                return false;
            }

            var compositionManager = _compositionManager;
            if (compositionManager == null)
            {
                return false;
            }

            var captureLeft = _captureArea.X;
            var captureTop = _captureArea.Y;
            var captureRight = _captureArea.X + _captureArea.Width;
            var captureBottom = _captureArea.Y + _captureArea.Height;

            if (screenX < captureLeft || screenX > captureRight
                || screenY < captureTop || screenY > captureBottom)
            {
                return false;
            }

            var relativeX = (float)(screenX - captureLeft);
            var relativeY = (float)(screenY - captureTop);
            compositionManager.ToggleZoomAt(relativeX, relativeY);
            return true;
        }

        private void Setup()
        {
            var settings = _settingsService.Current;

            _compositionManager = new CompositionManager(_settingsService, _mouseInputListener, _captureArea);
            _compositionManager.ZoomChanged += OnZoomChanged;

            _device = new CanvasDevice();

            var videoProperties = VideoEncodingProperties.CreateUncompressed(
                MediaEncodingSubtypes.Bgra8,
                (uint)_captureArea.Width,
                (uint)_captureArea.Height);
            _videoDescriptor = new VideoStreamDescriptor(videoProperties);

            _mediaStreamSource = new MediaStreamSource(_videoDescriptor)
            {
                BufferTime = TimeSpan.FromSeconds(0)
            };
            _mediaStreamSource.SampleRequested += OnSampleRequested;

            _profile = MediaEncodingProfile.CreateMp4(VideoQualitySelector.FromSettings(settings));
            _profile.Video.FrameRate.Numerator = (uint)settings.RecordingFps;
            _profile.Video.FrameRate.Denominator = 1;
            _transcoder = new MediaTranscoder();
        }

        private async Task StartCaptureAsync(StorageFile filePath)
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

            var fileOp = await filePath.OpenAsync(FileAccessMode.ReadWrite);
            var prepareOp = await _transcoder!.PrepareMediaStreamSourceTranscodeAsync(_mediaStreamSource, fileOp, _profile);
            _recordingTask = prepareOp.TranscodeAsync().AsTask();
        }

        private void Cleanup()
        {
            _mediaStreamSource?.SampleRequested -= OnSampleRequested;
            _mediaStreamSource = null;
            _videoDescriptor = null;

            _compositionManager?.ZoomChanged -= OnZoomChanged;
            _compositionManager?.Dispose();
            _compositionManager = null;

            _framePool?.FrameArrived -= OnFrameArrived;
            _framePool?.Dispose();
            _framePool = null;

            _session?.Dispose();
            _session = null;

            _renderTarget?.Dispose();
            _renderTarget = null;

            _canvasBitmap?.Dispose();
            _canvasBitmap = null;

            _device = null;
            _profile = null;
            _transcoder = null;
            _recordingTask = null;
        }

        private void OnSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (_state == RecordingState.Stopping)
            {
                args.Request.Sample = null;
                return;
            }

            var compositionManager = _compositionManager;
            var device = _device;
            var canvasBitmap = _canvasBitmap;

            if (compositionManager == null || device == null || canvasBitmap == null)
            {
                args.Request.Sample = null;
                return;
            }

            var renderTarget = _renderTarget;
            if (renderTarget == null)
            {
                try
                {
                    renderTarget = new CanvasRenderTarget(device, (float)_captureArea.Width, (float)_captureArea.Height, 96);
                    _renderTarget = renderTarget;
                }
                catch (Exception ex)
                {
                    _logger.LogError("RenderTarget create error: {}", ex.Message);
                    args.Request.Sample = null;
                    return;
                }
            }

            try
            {
                compositionManager.ComposeFrame(renderTarget, canvasBitmap);
                var surface = (IDirect3DSurface)renderTarget;
                var timeStamp = DateTimeOffset.Now - _startTime;
                args.Request.Sample = MediaStreamSample.CreateFromDirect3D11Surface(surface, timeStamp);
            }
            catch (ObjectDisposedException)
            {
                args.Request.Sample = null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Sample Error: {}", ex.Message);
                args.Request.Sample = null;
            }
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            using var frame = sender.TryGetNextFrame();
            var device = _device;
            if (frame == null || device == null)
            {
                return;
            }

            try
            {
                _canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(device, frame.Surface);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void OnZoomChanged(Rect rect)
        {
            ZoomAreaChanged?.Invoke(new ScreenRect(rect.X, rect.Y, rect.Width, rect.Height));
        }

        private void ChangeState(RecordingState state)
        {
            if (_state == state)
            {
                return;
            }

            _logger.LogDebug("Display capture state changed: {} -> {}", _state, state);
            _state = state;
            RecordingStateChanged?.Invoke(_state);
        }

    }
}

