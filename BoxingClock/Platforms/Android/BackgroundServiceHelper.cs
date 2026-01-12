using Android.Content;
using Android.OS;
using BoxingClock.Platforms.Android;

namespace BoxingClock.Services
{
    public static partial class BackgroundServiceHelper
    {
        static partial void StartBackgroundServicePlatform(string statusText)
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(TimerForegroundService));
            intent.SetAction(TimerForegroundService.ActionStart);
            intent.PutExtra(TimerForegroundService.ExtraStatusText, statusText);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }
        }

        static partial void UpdateBackgroundServicePlatform(string statusText)
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(TimerForegroundService));
            intent.SetAction(TimerForegroundService.ActionUpdate);
            intent.PutExtra(TimerForegroundService.ExtraStatusText, statusText);
            context.StartService(intent);
        }

        static partial void StopBackgroundServicePlatform()
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(TimerForegroundService));
            intent.SetAction(TimerForegroundService.ActionStop);
            context.StartService(intent);
        }
    }
}
