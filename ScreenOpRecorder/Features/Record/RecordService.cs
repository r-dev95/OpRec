using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Record.Events;
using ScreenOpRecorder.Features.Settings;
using ScreenOpRecorder.Shared.Events;

using Windows.Foundation;
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
        private enum RecordState
        {
            Idle,
            Prepared,
            Recording,
            Stopping,
            Disposed
        }

        private readonly ILogger<RecordService> _logger;
        private readonly IEventBus _eventBus;
        private readonly IUserSettingsService _settingsService;
        private readonly MouseHookService _mouseHookService;
        private readonly KeyboardHookService _keyboardHookService;

        private CompositionManager? _compositionManager;

        private CanvasDevice? _device;
        private GraphicsCaptureItem? _item;
        private Rect _captureArea;

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
        private bool _isStopRecord = true;
        private RecordState _state = RecordState.Idle;
        public string? LastOutputFolderPath { get; private set; }

        public RecordService(ILogger<RecordService> logger, IEventBus eventBus, IUserSettingsService settingsService, MouseHookService mouseHookService, KeyboardHookService keyboardHookService)
        {
            _logger = logger;
            _eventBus = eventBus;
            _settingsService = settingsService;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
        }

        public void Setup(GraphicsCaptureItem item, Rect captureArea)
        {
            ThrowIfDisposed();

            if (_state is RecordState.Recording or RecordState.Stopping)
            {
                throw new InvalidOperationException($"Setup is not allowed while state is {_state}.");
            }

            ReleaseResources();

            _isStopRecord = false;
            _item = item;
            _captureArea = captureArea;

            _compositionManager = new CompositionManager(_mouseHookService, _keyboardHookService, _captureArea, _settingsService.Current.ZoomFactor);
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

            _profile = MediaEncodingProfile.CreateMp4(ToVideoQuality(_settingsService.Current.QualityPreset));
            _profile.Video.FrameRate.Numerator = (uint)_settingsService.Current.RecordingFps;
            _profile.Video.FrameRate.Denominator = 1;
            _transcoder = new MediaTranscoder();

            TransitionTo(RecordState.Prepared);
        }

        public async Task StartAsync(GraphicsCaptureItem item, Rect captureArea)
        {
            ThrowIfDisposed();

            Setup(item, captureArea);

            string outputFolderPath = _settingsService.Current.OutputFolderPath;
            if (string.IsNullOrWhiteSpace(outputFolderPath))
            {
                outputFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            Directory.CreateDirectory(outputFolderPath);
            var localFolder = await StorageFolder.GetFolderFromPathAsync(outputFolderPath);
            var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            var file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            LastOutputFolderPath = localFolder.Path;

            if (_state != RecordState.Prepared)
            {
                throw new InvalidOperationException($"StartAsync requires Prepared state. Current: {_state}.");
            }

            try
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

                TransitionTo(RecordState.Recording);
            }
            catch
            {
                ReleaseResources();
                TransitionTo(RecordState.Idle);
                throw;
            }
        }

        public async Task StopAsync()
        {
            ThrowIfDisposed();

            if (_state == RecordState.Idle)
            {
                return;
            }

            if (_state == RecordState.Prepared)
            {
                ReleaseResources();
                TransitionTo(RecordState.Idle);
                return;
            }

            if (_state != RecordState.Recording)
            {
                return;
            }

            TransitionTo(RecordState.Stopping);
            _isStopRecord = true;

            try
            {
                if (_recordingTask != null)
                {
                    await _recordingTask;
                }
            }
            finally
            {
                ReleaseResources();
                TransitionTo(RecordState.Idle);
            }
        }

        public void Dispose()
        {
            if (_state == RecordState.Disposed)
            {
                return;
            }

            _isStopRecord = true;
            ReleaseResources();
            TransitionTo(RecordState.Disposed);
        }

        private void ReleaseResources()
        {
            _mediaStreamSource?.SampleRequested -= OnSampleRequested;
            _mediaStreamSource = null;
            _videoDescriptor = null;

            _compositionManager?.ZoomChanged -= OnZoomChanged;
            _compositionManager?.Dispose();
            _compositionManager = null;

            if (_framePool != null)
            {
                _framePool.FrameArrived -= OnFrameArrived;
                _framePool.Dispose();
                _framePool = null;
            }

            _session?.Dispose();
            _session = null;

            _renderTarget?.Dispose();
            _renderTarget = null;

            _canvasBitmap?.Dispose();
            _canvasBitmap = null;

            _transcoder = null;
            _profile = null;
            _device = null;
            _item = null;
            _recordingTask = null;
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
                _renderTarget = new CanvasRenderTarget(_device, (float)_captureArea.Width, (float)_captureArea.Height, 96);
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

        private void OnZoomChanged(Rect rect)
        {
            _eventBus.Publish(new ZoomAreaChangedEvent(rect));
        }

        private void ThrowIfDisposed()
        {
            if (_state == RecordState.Disposed)
            {
                throw new ObjectDisposedException(nameof(RecordService));
            }
        }

        private void TransitionTo(RecordState next)
        {
            if (_state == next)
            {
                return;
            }

            _logger.LogDebug("RecordService state: {From} -> {To}", _state, next);
            _state = next;
        }

        private static VideoEncodingQuality ToVideoQuality(QualityPreset preset)
        {
            return preset switch
            {
                QualityPreset.Low => VideoEncodingQuality.Wvga,
                QualityPreset.Medium => VideoEncodingQuality.HD720p,
                _ => VideoEncodingQuality.HD1080p
            };
        }
    }
}
