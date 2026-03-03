using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Java.Lang;
using AndroidX.Core.App;

namespace BoxingClock.Platforms.Android
{
    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeSpecialUse)]
    [IntentFilter(new[] { ActionStart, ActionUpdate, ActionStop })]
    public class TimerForegroundService : Service
    {
        public const string ActionStart = "BoxingClock.Action.START_TIMER";
        public const string ActionUpdate = "BoxingClock.Action.UPDATE_TIMER";
        public const string ActionStop = "BoxingClock.Action.STOP_TIMER";
        public const string ExtraStatusText = "BoxingClock.Extra.STATUS_TEXT";

        const int NotificationId = 1001;
        const string ChannelId = "boxingclock_timer_channel";
        const string ChannelName = "Boxing Timer";

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            string action = intent?.Action ?? string.Empty;
            string statusText = intent?.GetStringExtra(ExtraStatusText) ?? "Boxing timer running";

            if (action == ActionStop)
            {
                StopForeground(true);
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            CreateNotificationChannel();

            if (action == ActionStart)
            {
                StartForeground(NotificationId, BuildNotification(statusText));
            }
            else if (action == ActionUpdate)
            {
                var notification = BuildNotification(statusText);
                NotificationManagerCompat.From(this).Notify(NotificationId, notification);
            }

            return StartCommandResult.Sticky;
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Low)
            {
                Description = "Keeps the boxing timer running in the background",
                LockscreenVisibility = NotificationVisibility.Public
            };

            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }

        Notification BuildNotification(string statusText)
        {
            var launchIntent = new Intent(this, typeof(MainActivity));
            launchIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                launchIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var customView = new global::Android.Widget.RemoteViews(PackageName, Resource.Layout.notification_timer);
            customView.SetTextViewText(Resource.Id.timer_status, global::Java.Lang.String.ValueOf(statusText));

            return new NotificationCompat.Builder(this, ChannelId)
                //.SetContentText(statusText)
                .SetSmallIcon(CommunityToolkit.Maui.Resource.Drawable.bsb1)
                .SetContentIntent(pendingIntent)
                .SetCustomContentView(customView)
                .SetStyle(new NotificationCompat.DecoratedCustomViewStyle())
                .SetColorized(true)
                .SetColor(global::Android.Graphics.Color.Black)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .SetShowWhen(false)
                .Build();

        }
    }
}
