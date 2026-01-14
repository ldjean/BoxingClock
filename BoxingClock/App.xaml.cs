using BoxingClock.Models;
using BoxingClock.Services;
using BoxingClock.ViewModels;

namespace BoxingClock
{
    public partial class App : Application
    {
        public static TimerService TimerService { get; private set; }
        public static TimerSettings TimerSettings { get; private set; }
        public static BoxingTimerVM MainViewModel { get; private set; }

        public App()
        {
            InitializeComponent();

            // Add debug output
            System.Diagnostics.Debug.WriteLine("App starting...");

            // Initialize services
            TimerService = new TimerService();
            TimerSettings = new TimerSettings();
            TimerService.Configure(TimerSettings.CurrentTimer);

            // Add debug output
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "timersettings.json");
            System.Diagnostics.Debug.WriteLine($"Settings file exists: {File.Exists(filePath)}");

            // Create main view model
            MainViewModel = new BoxingTimerVM(TimerService, TimerSettings);

            MainPage = new BoxingTimer(); // Your main page

            // Load settings AFTER UI is created (but do it immediately)
            _ = LoadSettingsAsync();
        }
        private async Task LoadSettingsAsync()
        {
            try
            {
                // First, check what's in the file
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "timersettings.json");
                if (File.Exists(filePath))
                {
                    var fileContent = await File.ReadAllTextAsync(filePath);
                    System.Diagnostics.Debug.WriteLine($"File content length: {fileContent.Length}");
                    System.Diagnostics.Debug.WriteLine($"File content: {fileContent}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Settings file does NOT exist");
                }

                // Load timer settings from JSON
                var loadedSettings = await TimerSettings.LoadFromFileAsync();
                if (loadedSettings != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded settings!");
                    System.Diagnostics.Debug.WriteLine($"CurrentTimer: {loadedSettings.CurrentTimer?.PresetName}");
                    System.Diagnostics.Debug.WriteLine($"Presets count: {loadedSettings.TimerPresets?.Count}");

                    // Update our current settings with loaded ones
                    TimerSettings.CurrentTimer = loadedSettings.CurrentTimer?.Clone();

                    if (loadedSettings.TimerPresets != null)
                    {
                        TimerSettings.TimerPresets.Clear();
                        foreach (var preset in loadedSettings.TimerPresets)
                        {
                            TimerSettings.TimerPresets.Add(preset.Clone());
                        }
                    }

                    // Configure timer service with current settings
                    if (TimerSettings.CurrentTimer != null)
                    {
                        TimerService.Configure(TimerSettings.CurrentTimer);
                        // Update the ViewModel to match loaded settings
                        MainViewModel.UpdateUISettingsFromCurrent();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("loadedSettings is NULL!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        //private async Task LoadSettingsAsync()
        //{
        //    try
        //    {
        //        // Load timer settings from JSON
        //        var loadedSettings = await TimerSettings.LoadFromFileAsync();
        //        if (loadedSettings != null)
        //        {
        //            // Update our current settings with loaded ones
        //            TimerSettings.CurrentTimer = loadedSettings.CurrentTimer?.Clone();

        //            if (loadedSettings.TimerPresets != null)
        //            {
        //                TimerSettings.TimerPresets.Clear();
        //                foreach (var preset in loadedSettings.TimerPresets)
        //                {
        //                    TimerSettings.TimerPresets.Add(preset.Clone());
        //                }
        //            }

        //            // Configure timer service with current settings
        //            if (TimerSettings.CurrentTimer != null)
        //            {
        //                TimerService.Configure(TimerSettings.CurrentTimer);
        //                // Update the ViewModel to match loaded settings
        //                MainViewModel.UpdateUISettingsFromCurrent();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        //    }
        //}

        public static async Task SaveSettingsAsync()
        {
            try
            {
                await TimerSettings.SaveToFileAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Save when going to background
            _ = SaveSettingsAsync();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Keep the current timer state when resuming
            MainViewModel?.SyncTimerState();
        }
    }
}