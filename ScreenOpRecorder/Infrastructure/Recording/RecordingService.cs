using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;

using ScreenOpRecorder.Common.Events;
using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Core.Recording.Events;
using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Settings.Models;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Domain.ValueObjects;

using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public class RecordingService : IRecordingService, IDisposable
    {
        private enum RecordState
        {
            Idle,
            Prepared,
            Recording,
            Stopping,
            Disposed
        }

        private readonly ILogger<RecordingService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IMouseHookService _mouseHookService;
        private readonly IKeyboardHookService _keyboardHookService;
        private readonly IAudioCaptureService _audioCaptureService;
        private readonly IRecordingOutputCoordinator _outputCoordinator;
        private readonly IMediaFileMerger _mediaFileMerger;
        private readonly IEventBus _eventBus;

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
        private readonly RecordingOutputArtifacts _artifacts = new();
        private RecordState _state = RecordState.Idle;
        public string? LastOutputFolderPath => _outputCoordinator.LastOutputFolderPath;

        public RecordingService(
            ILogger<RecordingService> logger,
            IUserSettingsService settingsService,
            IMouseHookService mouseHookService,
            IKeyboardHookService keyboardHookService,
            IAudioCaptureService audioCaptureService,
            IRecordingOutputCoordinator outputCoordinator,
            IMediaFileMerger mediaFileMerger,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;
            _audioCaptureService = audioCaptureService;
            _outputCoordinator = outputCoordinator;
            _mediaFileMerger = mediaFileMerger;
            _eventBus = eventBus;
        }

        private void Setup(GraphicsCaptureItem item)
        {
            ThrowIfDisposed();

            if (_state is RecordState.Recording or RecordState.Stopping)
            {
                throw new InvalidOperationException($"Setup is not allowed while state is {_state}.");
            }

            ReleaseResources();

            _isStopRecord = false;
            _item = item;

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

        public bool TrySelectCaptureArea(ScreenRect captureArea)
        {
            ThrowIfDisposed();

            if (_state is RecordState.Recording or RecordState.Stopping)
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
            return true;
        }

        public async Task<bool> StartAsync()
        {
            ThrowIfDisposed();

            if (_item == null)
            {
                return false;
            }

            Setup(_item);
            await _outputCoordinator.PrepareAsync(_artifacts, _settingsService.Current);

            if (_state != RecordState.Prepared)
            {
                throw new InvalidOperationException($"StartAsync requires Prepared state. Current: {_state}.");
            }

            try
            {
                await StartVideoPipelineAsync();
                TransitionTo(RecordState.Recording);
            }
            catch
            {
                await ResetSessionAsync(RecordState.Idle);
                throw;
            }

            return true;
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
                await ResetSessionAsync(RecordState.Idle);
                return;
            }

            if (_state != RecordState.Recording)
            {
                return;
            }

            TransitionTo(RecordState.Stopping);
            _isStopRecord = true;

            await StopAudioIfNeededAsync();

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

            if (_artifacts.CaptureAudio)
            {
                await _mediaFileMerger.MergeIfNeededAsync(
                    _artifacts,
                    _settingsService.Current.QualityPreset,
                    _settingsService.Current.RecordingFps);
            }
            else
            {
                _artifacts.Reset();
            }
        }

        public void Dispose()
        {
            if (_state == RecordState.Disposed)
            {
                return;
            }

            _isStopRecord = true;
            _ = StopAudioIfNeededAsync();
            ReleaseResources();
            _artifacts.Reset();
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
            _eventBus.Publish(new ZoomAreaChangedEvent(new ScreenRect(rect.X, rect.Y, rect.Width, rect.Height)));
        }

        private void ThrowIfDisposed()
        {
            if (_state == RecordState.Disposed)
            {
                throw new ObjectDisposedException(nameof(RecordingService));
            }
        }

        private void TransitionTo(RecordState next)
        {
            if (_state == next)
            {
                return;
            }

            _logger.LogDebug("RecordingEngine state: {From} -> {To}", _state, next);
            _state = next;
        }

        private async Task StartVideoPipelineAsync()
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

            var videoOutputFile = _artifacts.VideoOutputFile
                ?? throw new InvalidOperationException("Video output file is not prepared.");
            var fileOp = await videoOutputFile.OpenAsync(FileAccessMode.ReadWrite);
            var prepareOp = await _transcoder!.PrepareMediaStreamSourceTranscodeAsync(_mediaStreamSource, fileOp, _profile);
            _recordingTask = prepareOp.TranscodeAsync().AsTask();
        }

        private async Task StopAudioIfNeededAsync()
        {
            if (_artifacts.CaptureAudio)
            {
                await _audioCaptureService.StopAsync();
            }
        }

        private async Task ResetSessionAsync(RecordState nextState)
        {
            await StopAudioIfNeededAsync();
            ReleaseResources();
            _artifacts.Reset();
            TransitionTo(nextState);
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
