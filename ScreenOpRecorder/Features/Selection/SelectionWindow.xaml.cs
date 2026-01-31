using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

using ScreenOpRecorder.Features.Overlay;

using Windows.Foundation;

using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Selection
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectionWindow : Window
    {
        private readonly ILogger<SelectionWindow> _logger;
        private readonly SelectionViewModel ViewModel;

        private Point _startPoint;
        private bool _isSelecting = false;
        public Rect SelectedRect
        {
            get; private set;
        }
        public SelectionWindow(ILogger<SelectionWindow> logger, SelectionViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // タイトルバーを非表示にする（OverlappedPresenterを使用）
            var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            if (presenter != null)
            {
                presenter.SetBorderAndTitleBar(false, false); // 枠線とタイトルバーを無効化
            }
            OverlayHelper.SetAlwaysOnTop(this, true);
            OverlayHelper.MaximizeWindow(this);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isSelecting = true;
            _startPoint = e.GetCurrentPoint(SelectionCanvas).Position;
            ViewModel.IsSizeTagVisible = Visibility.Visible;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isSelecting)
            {
                return;
            }

            var currentPoint = e.GetCurrentPoint(SelectionCanvas).Position;
            ViewModel.SetCaptureRect(_startPoint, currentPoint);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isSelecting = false;
            // 選択終了後、少し待ってからウィンドウを閉じるか、決定ボタンを出す
            //Close();
        }
    }
}
