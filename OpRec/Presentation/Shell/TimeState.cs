using System;
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;

namespace OpRec.Presentation.Shell
{
    public partial class TimeState : ObservableObject
    {
        private const string DefaultTimeText = "00:00:00";
        private const string TimeFormat = @"hh\:mm\:ss";

        [ObservableProperty]
        public partial string Time { get; set; } = DefaultTimeText;

        private readonly Stopwatch _stopWatch;
        private readonly DispatcherTimer _timer;
        public TimeState()
        {
            _stopWatch = new();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => Update();
        }

        public void Start()
        {
            Time = DefaultTimeText;
            _stopWatch.Restart();
            _timer.Start();
        }

        public void Stop()
        {
            _stopWatch.Stop();
            _timer.Stop();
        }

        private void Update()
        {
            var ts = _stopWatch.Elapsed;
            Time = ts.ToString(TimeFormat);
        }
    }
}
