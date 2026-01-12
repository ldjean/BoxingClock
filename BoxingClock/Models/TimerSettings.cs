using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BoxingClock.Models
{
    public class TimerSettings : INotifyPropertyChanged
    {
        public TimerSettings()
        {
            CurrentTimer = new TimerConfig();
            TimerPresets = new ObservableCollection<TimerConfig>();
        }

        private ObservableCollection<TimerConfig>? _timerPresets;
        public ObservableCollection<TimerConfig>? TimerPresets
        {
            get => _timerPresets;
            set => SetProperty(ref _timerPresets, value);
        }

        private TimerConfig? _currentTimer;
        public TimerConfig? CurrentTimer
        {
            get => _currentTimer;
            set => SetProperty(ref _currentTimer, value);
        }

        public int CurrentTimerIndex { get; set; } = -1;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // SIMPLE JSON Serialization
        public async Task SaveToFileAsync()
        {
            try
            {
                if (CurrentTimer != null && TimerPresets != null)
                {
                    CurrentTimerIndex = TimerPresets.IndexOf(CurrentTimer);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var filePath = Path.Combine(FileSystem.AppDataDirectory, "timersettings.json");
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static async Task<TimerSettings> LoadFromFileAsync()
        {
            try
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "timersettings.json");
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var settings = JsonSerializer.Deserialize<TimerSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Restore current timer from index if needed
                if (settings != null &&
                    settings.CurrentTimerIndex != -1 &&
                    settings.TimerPresets != null &&
                    settings.CurrentTimerIndex < settings.TimerPresets.Count)
                {
                    settings.CurrentTimer = settings.TimerPresets[settings.CurrentTimerIndex];
                }

                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return null;
            }
        }
    }
}