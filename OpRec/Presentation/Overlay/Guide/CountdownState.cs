using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

namespace OpRec.Presentation.Overlay.Guide
{
    public partial class CountdownState : ObservableObject
    {
        private bool _isRunning;

        [ObservableProperty]
        public partial string Text { get; set; } = "3";

        [ObservableProperty]
        public partial Visibility Visibility { get; set; } = Visibility.Collapsed;

        public bool IsRunning => _isRunning;

        public async Task ShowAsync()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            try
            {
                var count = 3;
                for (var i = count; i >= 1; i--)
                {
                    Text = i.ToString();
                    Visibility = Visibility.Visible;
                    await Task.Delay(1000);
                }
            }
            finally
            {
                Visibility = Visibility.Collapsed;
                _isRunning = false;
            }
        }
    }
}
