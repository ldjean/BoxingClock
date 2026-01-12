using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BoxingClock.Models;
using BoxingClock.Services;

namespace BoxingClock.ViewModels
{
    public class BoxingTimerVM : INotifyPropertyChanged
    {
        private readonly TimerService _timerService;
        private readonly TimerSettings _timerSettings;

        public BoxingTimerVM(TimerService timerService, TimerSettings timerSettings)
        {
            _timerService = timerService;
            _timerSettings = timerSettings;

            // Initialize commands
            StartCommand = new Command(ExecuteStart);
            StopCommand = new Command(ExecuteStop);
            ResetCommand = new Command(ExecuteReset);
            SavePresetCommand = new Command(ExecuteSavePreset);
            DeletePresetCommand = new Command<TimerConfig>(ExecuteDeletePreset);
            SelectPresetCommand = new Command<TimerConfig>(ExecuteSelectPreset);
            ShowSettingsCommand = new Command(ExecuteShowSettings);
            ShowPresetsCommand = new Command(ExecuteShowPresets);
            ExitOverlayCommand = new Command(ExecuteExitOverlay);
            MuteToggledCommand = new Command(ExecuteMuteToggled);

            // Subscribe to timer events
            _timerService.PropertyChanged += OnTimerServicePropertyChanged;
            _timerService.ReadyCountdownChanged += OnReadyCountdownChanged;
            _timerService.ReadyCountdownFinished += OnReadyCountdownFinished;

            // Initialize current UI settings
            CurrentUISettings = new TimerConfig();
            UpdateUISettingsFromCurrent();
        }

        // Current timer configuration from UI
        private TimerConfig _currentUISettings;
        public TimerConfig CurrentUISettings
        {
            get => _currentUISettings;
            set => SetProperty(ref _currentUISettings, value);
        }

        // UI State properties
        private bool _isSettingsVisible = true;
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set => SetProperty(ref _isSettingsVisible, value);
        }

        private bool _isPresetsVisible;
        public bool IsPresetsVisible
        {
            get => _isPresetsVisible;
            set => SetProperty(ref _isPresetsVisible, value);
        }

        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private bool _isStopButtonVisible;
        public bool IsStopButtonVisible
        {
            get => _isStopButtonVisible;
            set => SetProperty(ref _isStopButtonVisible, value);
        }

        private bool _isStartButtonVisible = true;
        public bool IsStartButtonVisible
        {
            get => _isStartButtonVisible;
            set => SetProperty(ref _isStartButtonVisible, value);
        }

        private bool _isResetButtonVisible = true;
        public bool IsResetButtonVisible
        {
            get => _isResetButtonVisible;
            set => SetProperty(ref _isResetButtonVisible, value);
        }

        private bool _isGearEnabled = true;
        public bool IsGearEnabled
        {
            get => _isGearEnabled;
            set => SetProperty(ref _isGearEnabled, value);
        }

        // Add these properties to your BoxingTimerVM class:

        private bool _isStopButtonEnabled;
        public bool IsStopButtonEnabled
        {
            get => _isStopButtonEnabled;
            set => SetProperty(ref _isStopButtonEnabled, value);
        }

        private bool _isStartButtonEnabled = true;
        public bool IsStartButtonEnabled
        {
            get => _isStartButtonEnabled;
            set => SetProperty(ref _isStartButtonEnabled, value);
        }

        private bool _isResetButtonEnabled = true;
        public bool IsResetButtonEnabled
        {
            get => _isResetButtonEnabled;
            set => SetProperty(ref _isResetButtonEnabled, value);
        }

        private ObservableCollection<CarouselPage> _pages;
        public ObservableCollection<CarouselPage> Pages
        {
            get => _pages;
            set => SetProperty(ref _pages, value);
        }

        private CarouselPage _currentPage;
        public CarouselPage CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        // Commands
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SavePresetCommand { get; }
        public ICommand DeletePresetCommand { get; }
        public ICommand SelectPresetCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowPresetsCommand { get; }
        public ICommand ExitOverlayCommand { get; }
        public ICommand MuteToggledCommand { get; }

        // Public properties for data binding
        public TimerConfig CurrentTimer => _timerSettings.CurrentTimer;
        public ObservableCollection<TimerConfig> Presets => _timerSettings.TimerPresets;
        public bool IsMuted => _timerService.IsMuted;
        public string CountDownDisplay
        {
            get
            {
                if (_timerService.IsInReadyCountdown && _timerService.ReadyCount > 0)
                {
                    return _timerService.ReadyCount.ToString();
                }
                else
                {
                    TimeSpan time = TimeSpan.FromSeconds(_timerService.CountDown);
                    return $"{(int)time.TotalMinutes:0}:{time.Seconds:00}";
                }
            }
        }
        public string RoundStatusText => _timerService.RoundStatusText;
        public int ReadyCount => _timerService.ReadyCount;
        public bool IsInReadyCountdown => _timerService.IsInReadyCountdown;

        // Static options for UI
        public System.Collections.Generic.List<string> RoundTimes => TimerOptions.RoundTimes;
        public System.Collections.Generic.List<string> BreakTimes => TimerOptions.BreakTimes;
        public System.Collections.Generic.List<string> Intervals => TimerOptions.Intervals;
        public System.Collections.Generic.List<string> ReadyTimes => TimerOptions.ReadyTimes;
        public System.Collections.Generic.List<string> RoundsOptions => TimerOptions.RoundsOptions;

        private void ExecuteStart()
        {
            // Update current timer with UI settings
            UpdateCurrentFromUISettings();

            // If timer was never configured OR was reset, get new settings from UI
            if (_timerService.SettingsConfig == null)
            {
                _timerService.Configure(CurrentUISettings);
            }

            IsGearEnabled = false;

            // Start the timer - this will trigger the ready countdown
            _timerService.Start();

            // Immediately update UI for ready countdown
            OnPropertyChanged(nameof(IsInReadyCountdown));
            OnPropertyChanged(nameof(ReadyCount));
            OnPropertyChanged(nameof(CountDownDisplay));
        }

        private void ExecuteStop()
        {
            _timerService.Stop();
        }

        private void ExecuteReset()
        {
            _timerService.Reset();
            UpdateUISettingsFromCurrent();
        }

        private void ExecuteSavePreset()
        {
            if (_timerSettings.TimerPresets.Any(x => x.PresetName.ToLower().Equals(CurrentUISettings.PresetName.ToLower())))
            {
                // Show alert - you'll need to handle this via a service
                // Application.Current.MainPage.DisplayAlert("Can't Save", "That name already exists", "ok");
                return;
            }

            // Add a new preset
            var newPreset = CurrentUISettings.Clone();
            _timerSettings.TimerPresets.Add(newPreset);

            // Sort the collection
            var sorted = _timerSettings.TimerPresets.OrderByDescending(x => x.PresetName).ToList();
            _timerSettings.TimerPresets.Clear();
            sorted.Reverse();
            foreach (var item in sorted) _timerSettings.TimerPresets.Add(item);

            // Clear preset name
            CurrentUISettings.PresetName = string.Empty;

            _ = App.SaveSettingsAsync();
        }

        private void ExecuteDeletePreset(TimerConfig preset)
        {
            if (preset != null)
            {
                _timerSettings.TimerPresets.Remove(preset);
            }
        }

        private async void ExecuteSelectPreset(TimerConfig preset)
        {
            if (preset == null) return;

            // Update UI settings with the selected preset
            CurrentUISettings = preset.Clone();

            // Update current timer
            UpdateCurrentFromUISettings();

            // Configure timer with the selected preset
            _timerService.Configure(CurrentUISettings);

            // Reset timer to show new time
            _timerService.Reset();

            // Update display
            RefreshTimerDisplay();

            // Save settings to file
            await App.SaveSettingsAsync();

            IsOverlayVisible = false;
        }

        public void RefreshTimerDisplay()
        {
            // Force update of all timer-related properties
            OnPropertyChanged(nameof(CountDownDisplay));
            OnPropertyChanged(nameof(RoundStatusText));
            OnPropertyChanged(nameof(ReadyCount));
        }

        private void ExecuteShowSettings()
        {
            IsSettingsVisible = true;
            IsPresetsVisible = false;
            CurrentPage = Pages?.FirstOrDefault(p => p.Title == "Timer Options");
        }

        private void ExecuteShowPresets()
        {
            IsSettingsVisible = false;
            IsPresetsVisible = true;
            CurrentPage = Pages?.FirstOrDefault(p => p.Title == "Presets");
        }
        public void SyncSettings()
        {
            UpdateCurrentFromUISettings();
        }

        // MODIFY ExecuteExitOverlay to ensure proper sync
        private async void ExecuteExitOverlay()
        {
            IsOverlayVisible = false;

            // Update current timer with UI settings
            UpdateCurrentFromUISettings();

            // IMPORTANT: Configure the timer with the new settings
            _timerService.Configure(_timerSettings.CurrentTimer);

            // Reset the timer to show the new time immediately
            _timerService.Reset();

            // Force refresh of ALL timer display properties
            OnPropertyChanged(nameof(CountDownDisplay));
            OnPropertyChanged(nameof(RoundStatusText));
            OnPropertyChanged(nameof(ReadyCount));
            OnPropertyChanged(nameof(IsInReadyCountdown));

            // Save settings to file
            await App.SaveSettingsAsync();
        }

        private void ExecuteMuteToggled()
        {
            _timerService.IsMuted = !_timerService.IsMuted;
            OnPropertyChanged(nameof(IsMuted));
        }

        // Helper methods
        public void UpdateUISettingsFromCurrent()
        {
            CurrentUISettings = _timerSettings.CurrentTimer.Clone();
        }

        public void UpdateCurrentFromUISettings()
        {
            _timerSettings.CurrentTimer = CurrentUISettings.Clone();
        }

        // Event handlers
        private void OnTimerServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update display properties ALWAYS (even during ready countdown)
            if (e.PropertyName == nameof(_timerService.CountDown) ||
                e.PropertyName == nameof(_timerService.IsInReadyCountdown) ||
                e.PropertyName == nameof(_timerService.ReadyCount) ||
                e.PropertyName == nameof(_timerService.IsRunning))
            {
                RefreshTimerDisplay();
            }
            UpdateButtonStateFromTimer();
        }

        private void OnReadyCountdownChanged(int count)
        {
            // Update the ReadyCount property for binding
            OnPropertyChanged(nameof(ReadyCount));
            OnPropertyChanged(nameof(CountDownDisplay));
        }

        private void OnReadyCountdownFinished()
        {
            // Update UI state after ready countdown
            IsStopButtonVisible = IsStopButtonEnabled = true;
            IsStartButtonVisible = IsStartButtonEnabled = false;
            IsResetButtonVisible = IsResetButtonEnabled = false;
            IsGearEnabled = false;

            // Force update of display
            RefreshTimerDisplay();
        }

        public void SyncTimerState()
        {
            RefreshTimerDisplay();
            UpdateButtonStateFromTimer();
            OnPropertyChanged(nameof(IsInReadyCountdown));
        }

        private void UpdateButtonStateFromTimer()
        {
            if (_timerService.IsInReadyCountdown)
            {
                // During ready countdown: hide ALL buttons
                IsStopButtonVisible = IsStopButtonEnabled = false;
                IsStartButtonVisible = IsStartButtonEnabled = false;
                IsResetButtonVisible = IsResetButtonEnabled = false;
                IsGearEnabled = false;
                return;
            }

            // Normal state: update button states based on running state
            IsStopButtonVisible = IsStopButtonEnabled = _timerService.IsRunning;
            IsStartButtonVisible = IsStartButtonEnabled = !_timerService.IsRunning;
            IsResetButtonVisible = IsResetButtonEnabled = !_timerService.IsRunning;
            IsGearEnabled = !_timerService.IsRunning;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}