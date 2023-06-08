using Acr.UserDialogs;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using StyexFleetManagement.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using EventType = StyexFleetManagement.Salus.Enums.EventType;

namespace StyexFleetManagement.ViewModel
{
    public class CountdownTimerViewModel : ViewModelBase, ICountdownTimerViewModel
    {
        private const long TimerDelay = TimeSpan.TicksPerSecond / 2;

        private bool _showStartButton;
        private bool _showStopButton;
        private bool _showTimePicker;
        private bool _showCountdownProgressBar;
        private int _countdownProgress;
        private ObservableCollection<object> _timeSelection;
        private string _remainingTime = "00:00:00";

        public long TimerEndTimeTicks { get; set; }
        public TimeSpan SelectedTime { get; set; }
        public Timer Timer { get; set; }
        public TimerType TimerType { get; set; }

        public void Initialize()
        {
            if (CountdownProgress > 0)
            {
                ShowTimePicker = false;
                ShowStartButton = false;
                ShowCountdownProgressBar = true;
                ShowStopButton = true;
            }
            else
            {
                TimeSelection = new ObservableCollection<object> { "00", "00", "00" };
                ShowTimePicker = true;
                ShowStartButton = true;
                ShowCountdownProgressBar = false;
                ShowStopButton = false;
            }
        }

        public bool ShowStartButton
        {
            get => _showStartButton;
            set
            {
                _showStartButton = value;
                RaisePropertyChanged(() => ShowStartButton);
            }
        }

        public bool ShowStopButton
        {
            get => _showStopButton;
            set
            {
                _showStopButton = value;
                RaisePropertyChanged(() => ShowStopButton);
            }
        }

        public bool ShowTimePicker
        {
            get => _showTimePicker;
            set
            {
                _showTimePicker = value;
                RaisePropertyChanged(() => ShowTimePicker);
            }
        }

        public bool ShowCountdownProgressBar
        {
            get => _showCountdownProgressBar;
            set
            {
                _showCountdownProgressBar = value;
                RaisePropertyChanged(() => ShowCountdownProgressBar);
            }
        }

        public int CountdownProgress
        {
            get => _countdownProgress;
            set
            {
                _countdownProgress = value;
                RaisePropertyChanged(() => CountdownProgress);
            }
        }

        public ObservableCollection<object> TimeSelection
        {
            get => _timeSelection;
            set
            {
                _timeSelection = value;
                RaisePropertyChanged(() => TimeSelection);
            }
        }

        public string RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                RaisePropertyChanged(() => RemainingTime);
            }
        }

        public void NewTimer()
        {
            Timer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };
            Timer.Elapsed += TimerOnElapsed;
        }

        public void HandleStartTimer()
        {
            NewTimer();

            var selectedTimeAsString = $"{TimeSelection[0]}:{TimeSelection[1]}:{TimeSelection[2]}";
            SelectedTime = TimeSpan.Parse(selectedTimeAsString);
            RemainingTime = selectedTimeAsString;
            TimerEndTimeTicks = DateTime.UtcNow.Ticks + SelectedTime.Ticks;
            CountdownProgress = GetCountdownProgress(SelectedTime.Ticks - TimerDelay, SelectedTime.Ticks); ;

            ShowStartButton = false;
            ShowStopButton = true;
            ShowCountdownProgressBar = true;
            ShowTimePicker = false;
        }

        public void HandleStopTimer()
        {
            var remainingTicks = TimerEndTimeTicks - DateTime.UtcNow.Ticks;
            var remainingTimeAsParts = TimeSpan.FromTicks(remainingTicks).ToString(@"hh\:mm\:ss").Split(':');
            TimeSelection = new ObservableCollection<object>
                {remainingTimeAsParts[0], remainingTimeAsParts[1], remainingTimeAsParts[2]};

            Timer.Stop();
            Timer.Dispose();

            ShowStartButton = true;
            ShowStopButton = false;
            ShowCountdownProgressBar = false;
            CountdownProgress = 0;
            ShowTimePicker = true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var remainingTicks = TimerEndTimeTicks - DateTime.UtcNow.Ticks;

            if (remainingTicks > 0)
            {
                CountdownProgress = GetCountdownProgress(remainingTicks - TimerDelay, SelectedTime.Ticks);
                RemainingTime = TimeSpan.FromTicks(remainingTicks).ToString(@"hh\:mm\:ss");
            }
            else
            {
                OnTimerComplete();
            }
        }

        public void OnTimerComplete()
        {
            HandleStopTimer();
            ShowTimePicker = true;
            ShowCountdownProgressBar = false;
            CountdownProgress = 0;
            ShowStopButton = false;
            ShowStartButton = true;

            ShowTimerUpDialog();
        }

        private void ShowTimerUpDialog()
        {
            // TODO: Buzzer

            var alertMessage = TimerType == TimerType.AmberAlert
                ? AppResources.amber_alert_up_message
                : AppResources.guardian_angel_up_message;

            UserDialogs.Instance.Confirm(new ConfirmConfig()
            {
                Message = alertMessage,
                OkText = AppResources.yes,
                CancelText = AppResources.no,
                OnAction = async result =>
                {
                    if (!result) return;

                    var locationUpdateService = ViewModelLocator.Resolve<ILocationUpdateService>();
                    await locationUpdateService.GetLocationAndSendEvent(EventType.GuardianAngel, true);

                    var deviceSettings = Settings.Current.SalusDeviceSettings;

                    if (!string.IsNullOrEmpty(deviceSettings?.Sos?.DeviceEmergencyNumber))
                    {
                        PhoneDialer.Open(deviceSettings.Sos.DeviceEmergencyNumber);
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.Alert(AppResources.emergency_number_not_found));
                    }
                }
            });
        }

        private static int GetCountdownProgress(long remaining, long total) =>
            total > 0 ? 100 - (int) (remaining * 100 / total) : 0;
    }
}