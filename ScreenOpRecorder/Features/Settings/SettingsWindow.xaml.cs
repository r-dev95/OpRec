using System;

using Microsoft.UI.Xaml;

using Windows.Storage.Pickers;

using WinRT.Interop;

namespace ScreenOpRecorder.Features.Settings
{
    public sealed partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel ViewModel;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.CloseRequested += OnCloseRequested;
        }

        private void OnCloseRequested()
        {
            Close();
        }

        private async void OnClickBrowseFolder(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.SetOutputFolderPath(folder.Path);
            }
        }
    }
}
