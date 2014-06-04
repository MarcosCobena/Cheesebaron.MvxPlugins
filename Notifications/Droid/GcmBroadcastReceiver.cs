using System;
using Android.App;
using Android.Content;
using Android.Gms.Gcm;
using Android.Support.V4.Content;

namespace Cheesebaron.MvxPlugins.Notifications
{
    [BroadcastReceiver(Enabled = true, Name = "GCM Receiver for some app", 
        Permission = "com.google.android.c2dm.permission.SEND")]
    [IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" }, 
        Categories = new []{ "@PACKAGE_NAME@" })]
    public class GcmBroadcastReceiver 
        : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var comp = new ComponentName(context.PackageName, typeof(GcmIntentService).Name);

            StartWakefulService(context, (intent.SetComponent(comp)));
            SetResult(Result.Ok, null, null);
        }
    }

    [Service]
    public class GcmIntentService 
        : IntentService
    {
        // yeah...
        internal static Action<string> OnNotification { get; set; }
        internal static Action<string> OnNotificationDeleted { get; set; }
        internal static Action<string> OnNotificationSendError { get; set; }

        public GcmIntentService() 
            : base("GcmIntentService") { }

        protected override void OnHandleIntent(Intent intent)
        {
            var extras = intent.Extras;
            var gcm = GoogleCloudMessaging.GetInstance(this);

            // intent must be the one received in GcmBroadcastReceiver
            var messageType = gcm.GetMessageType(intent);

            if(!extras.IsEmpty) {
                /*
                 * Filter messages based on type here. GCM will likely be 
                 * extended in the future with new message types.
                 */

                switch(messageType) {
                    case GoogleCloudMessaging.MessageTypeSendError:
                        if (OnNotificationSendError != null)
                            OnNotificationSendError(extras.ToString());
                        break;
                    case GoogleCloudMessaging.MessageTypeDeleted:
                        if (OnNotificationDeleted != null)
                            OnNotificationDeleted(extras.ToString());
                        break;
                    case GoogleCloudMessaging.MessageTypeMessage:
                        if(OnNotification != null)
                            OnNotification(extras.ToString());
                        break;
                }
            }

            // Release wake lock from the GcmBroadcastReceiver
            GcmBroadcastReceiver.CompleteWakefulIntent(intent);
        }
    }
}