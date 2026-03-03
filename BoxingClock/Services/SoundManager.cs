using System.Collections.Concurrent;
using Plugin.Maui.Audio;

namespace BoxingClock.Services
{
    public static class SoundManager
    {
        private static readonly ConcurrentDictionary<string, IAudioPlayer> Players = new();
        private static readonly Dictionary<SoundTheme, SoundThemeFiles> ThemeFiles = new()
        {
            [SoundTheme.Default] = new SoundThemeFiles(
                BellStart: "BoxingBell_1.wav",
                BellEnd: "BoxingBell_1.wav",
                Warning10s: "10sec.wav",
                Tick: "Tick.wav"),
            [SoundTheme.Alt] = new SoundThemeFiles(
                BellStart: "BoxingBell - Start.wav",
                BellEnd: "BoxingBell.wav",
                Warning10s: "10sec.wav",
                Tick: "Tick.wav")
        };

        private static IAudioManager _audioManager;
        private static SoundTheme _currentTheme = SoundTheme.Default;

        public static bool IsMuted { get; set; }

        public static void Initialize(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public static void ConfigureThemeFiles(SoundTheme theme, SoundThemeFiles files)
        {
            ThemeFiles[theme] = files;
        }

        public static void SetTheme(SoundTheme theme)
        {
            _currentTheme = theme;
        }

        public static void PlayBellStart()
        {
            Play(ThemeFiles[_currentTheme].BellStart);
        }

        public static void PlayBellEnd()
        {
            Play(ThemeFiles[_currentTheme].BellEnd);
        }

        public static void PlayWarning10s()
        {
            Play(ThemeFiles[_currentTheme].Warning10s);
        }

        public static void PlayTick()
        {
            Play(ThemeFiles[_currentTheme].Tick);
        }

        private static void Play(string fileName)
        {
            if (IsMuted)
            {
                return;
            }

            _ = PlayInternalAsync(fileName);
        }

        private static async Task PlayInternalAsync(string fileName)
        {
            if (_audioManager == null)
            {
                return;
            }

            if (Players.TryGetValue(fileName, out var existingPlayer))
            {
                existingPlayer.Stop();
                existingPlayer.Play();
                return;
            }

            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            var player = _audioManager.CreatePlayer(stream);
            Players.TryAdd(fileName, player);
            player.Play();
        }
    }

    public enum SoundTheme
    {
        Default,
        Alt
    }

    public record SoundThemeFiles(string BellStart, string BellEnd, string Warning10s, string Tick);
}
