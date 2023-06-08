using System;
using System.Collections.ObjectModel;
using StyexFleetManagement.Enums;

namespace StyexFleetManagement.ViewModel
{
    public interface ICountdownTimerViewModel
    {
        bool ShowStartButton { get; }
        bool ShowStopButton { get; }
        bool ShowTimePicker { get; }
        bool ShowCountdownProgressBar { get; }
        int CountdownProgress { get; }
        ObservableCollection<object> TimeSelection { get; }
        string RemainingTime { get; }
        TimeSpan SelectedTime { get; }
        long TimerEndTimeTicks { get; }
        void Initialize();
        void NewTimer();
        void HandleStartTimer();
        void HandleStopTimer();
    }
}