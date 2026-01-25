using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        private readonly ILogger<ShellPage> _logger;

        private readonly ShellViewModel ViewModel;

        public ShellPage(ILogger<ShellPage> logger, ShellViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;

            ViewModel = viewModel;
        }
    }
}
