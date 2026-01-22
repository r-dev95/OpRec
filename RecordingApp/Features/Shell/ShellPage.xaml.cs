using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RecordingApp.Features.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        private readonly ILogger<ShellPage> _logger;

        public ShellPage(ILogger<ShellPage> logger)
        {
            InitializeComponent();

            _logger = logger;
        }
    }
}
