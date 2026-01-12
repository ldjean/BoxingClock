namespace BoxingClock.Services
{
    public static partial class BackgroundServiceHelper
    {
        public static void StartBackgroundService(string statusText)
        {
            StartBackgroundServicePlatform(statusText);
        }

        public static void UpdateNotification(string statusText)
        {
            UpdateBackgroundServicePlatform(statusText);
        }

        public static void StopBackgroundService()
        {
            StopBackgroundServicePlatform();
        }

        static partial void StartBackgroundServicePlatform(string statusText);
        static partial void UpdateBackgroundServicePlatform(string statusText);
        static partial void StopBackgroundServicePlatform();
    }
}
