using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Timers;
using BoxingClock.Models;
using Microsoft.Maui.Controls;

namespace BoxingClock.Services
{
    public class TimerService : INotifyPropertyChanged
    {
        private System.Timers.Timer _timer;
        private System.Timers.Timer _readyTimer;

        private int _intervals = -1;
        private TimerConfig _settingsConfig; // Reference to the timer configuration
        public TimerConfig SettingsConfig => _settingsConfig;
        private int _totalRounds = -1;

        // Ready countdown properties
        private int _readyCount;
        public int ReadyCount
        {
            get => _readyCount;
            private set => SetProperty(ref _readyCount, value);
        }

        private bool _isInReadyCountdown;
        public bool IsInReadyCountdown
        {
            get => _isInReadyCountdown;
            private set => SetProperty(ref _isInReadyCountdown, value);
        }

        // Public state that the UI can bind to
        private int _countDown;
        public int CountDown
        {
            get => _countDown;
            private set => SetProperty(ref _countDown, value);
        }

        private int _currentRound = 1;
        public int CurrentRound
        {
            get => _currentRound;
            private set
            {
                if (SetProperty(ref _currentRound, value))
                {
                    UpdateRoundStatusText();
                }
            }
        }

        private bool _isRoundTime = true;
        public bool IsRoundTime
        {
            get => _isRoundTime;
            private set
            {
                if (SetProperty(ref _isRoundTime, value))
                {
                    UpdateRoundStatusText();
                }
            }
        }

        private string _roundStatusText = "Round 1";
        public string RoundStatusText
        {
            get => _roundStatusText;
            private set => SetProperty(ref _roundStatusText, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        // Handle the muting of sound
        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        // Events for ready countdown
        public event Action<int> ReadyCountdownChanged;
        public event Action ReadyCountdownFinished;

        public event PropertyChangedEventHandler PropertyChanged;

        public TimerService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;

            _readyTimer = new System.Timers.Timer(1000);
            _readyTimer.Elapsed += ReadyTimer_Elapsed;
            _readyTimer.AutoReset = true;
        }

        public void Configure(TimerConfig config)
        {
            _settingsConfig = config;

            // Initialize state FROM THE CONFIG
            TimeSpan roundTime = TimeSpan.ParseExact(_settingsConfig.RoundTime, @"m\:ss", CultureInfo.InvariantCulture);
            CountDown = (int)roundTime.TotalSeconds;
            CurrentRound = 1;
            IsRoundTime = true;

            if (string.IsNullOrEmpty(_settingsConfig.NumberOfRounds))
                _settingsConfig.NumberOfRounds = "∞";

            // Parse number of rounds
            if (_settingsConfig.NumberOfRounds == "∞")
            {
                _totalRounds = -1; // Infinite
            }
            else
            {
                _totalRounds = int.Parse(_settingsConfig.NumberOfRounds);
            }

            // Initialize ready count - use -1 for "Off" like intervals
            if (_settingsConfig.ReadyTime == "Off")
            {
                ReadyCount = -1;
            }
            else
            {
                ReadyCount = int.Parse(_settingsConfig.ReadyTime);
            }

            // Initialize intervals from config
            if (string.Equals(_settingsConfig.IntervalNotification, "Off", StringComparison.OrdinalIgnoreCase))
                _intervals = -1;
            else
                _intervals = int.Parse(_settingsConfig.IntervalNotification);

            // Force UI update
            OnPropertyChanged(nameof(CountDown));
            OnPropertyChanged(nameof(RoundStatusText));
        }


        public void StartReadyCountdown()
        {
            IsInReadyCountdown = true;

            if (ReadyCount == -1)
            {
                // Immediately finish ready countdown without starting the timer
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StopReadyCountdown();
                    ReadyCountdownFinished?.Invoke();
                    IsRunning = true;
                    _timer.Start();
                    //SoundManager.PlayBellStart();
                });
                return;
            }

            ReadyCount = int.Parse(_settingsConfig.ReadyTime);

            // Trigger the initial animation for the starting count
            ReadyCountdownChanged?.Invoke(ReadyCount);

            _readyTimer.Start();
        }

        public void StopReadyCountdown()
        {
            IsInReadyCountdown = false;
            _readyTimer.Stop();
        }

        public void Start()
        {
            if (_settingsConfig == null)
                throw new InvalidOperationException("TimerService must be configured with TimerConfig first.");

            // Don't reset the countdown - continue from where we left off
            IsRunning = true;

            // Run in background (you'll need to implement MAUI background service)
            // Start with initial status text
            string initialStatus = GetNotificationStatusText();
            // BackgroundServiceHelper.StartBackgroundService(initialStatus);

            // Always start with the ready countdown
            StartReadyCountdown();
        }

        public void Stop()
        {
            IsRunning = false;
            _timer.Stop();

            // Stop running in background
            // BackgroundServiceHelper.StopBackgroundService();
        }

        public void Reset()
        {
            IsRunning = false;  // Ensure this property changes
            // Reset state based on the current config
            if (_settingsConfig != null)
            {
                Configure(_settingsConfig);
            }
        }

        private void ReadyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReadyCount--;
            ReadyCountdownChanged?.Invoke(ReadyCount);

            if (ReadyCount <= 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StopReadyCountdown();
                    ReadyCountdownFinished?.Invoke();

                    // Start the main timer after ready countdown finishes
                    IsRunning = true;
                    _timer.Start();
                    //SoundManager.PlayBellStart();
                });
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CountDown--;

            // Update notification text - must be on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateBackgroundNotification();
            });

            // Play 10-second warning
            if (IsRoundTime && CountDown == 10)
            {
                //SoundManager.PlayWarning10s();
            }

            // Handle interval notifications
            if (_intervals > 0 && IsRoundTime)
            {
                _intervals--;
                if (_intervals == 0)
                {
                    //SoundManager.PlayTick();
                    _intervals = int.Parse(_settingsConfig.IntervalNotification);
                }
            }

            // Transition when countdown reaches 0
            if (CountDown <= 0)
            {
                if (IsRoundTime)
                {
                    TransitionToBreak();
                }
                else
                {
                    TransitionToRound();
                }
            }
        }

        private void TransitionToBreak()
        {
            // Check if we've reached the round limit
            if (_totalRounds > 0 && CurrentRound >= _totalRounds)
            {
                // Round limit reached - stop the timer
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Stop();
                    Reset();
                    //SoundManager.PlayBellStart(); // Final bell
                    // You could show a message here: "Workout Complete!"
                });
                return;
            }

            if (_settingsConfig.BreakTime == "Off")
            {
                TransitionToRound();
                return;
            }

            TimeSpan breakTime = TimeSpan.ParseExact(_settingsConfig.BreakTime, @"m\:ss", CultureInfo.InvariantCulture);
            CountDown = (int)breakTime.TotalSeconds;

            //SoundManager.PlayBellEnd();
            IsRoundTime = false;

            MainThread.BeginInvokeOnMainThread(() => UpdateBackgroundNotification());
        }

        private void TransitionToRound()
        {
            TimeSpan roundTime = TimeSpan.ParseExact(_settingsConfig.RoundTime, @"m\:ss", CultureInfo.InvariantCulture);
            CountDown = (int)roundTime.TotalSeconds;
            IsRoundTime = true;
            CurrentRound++;

            //SoundManager.PlayBellStart();

            MainThread.BeginInvokeOnMainThread(() => UpdateBackgroundNotification());
        }

        // Background service implementation
        private string GetNotificationStatusText()
        {
            int secondsRemaining = Math.Max(0, CountDown);
            string formattedTime = TimeSpan.FromSeconds(secondsRemaining).ToString("m\\:ss");

            if (IsRoundTime)
            {
                return $"Round {CurrentRound} : {formattedTime}";
            }

            return $"Break : {formattedTime}";
        }

        private void UpdateRoundStatusText()
        {
            RoundStatusText = IsRoundTime ? $"Round {CurrentRound}" : "Break";
        }

        public void UpdateBackgroundNotification()
        {
            string statusText = GetNotificationStatusText();
            // BackgroundServiceHelper.UpdateNotification(statusText);
        }

        // INotifyPropertyChanged implementation
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            if (MainThread.IsMainThread)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}