using System;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Util;
using ExchangeBooks.Bootstrap;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Models;
using Firebase.Messaging;
using static ExchangeBooks.Constants.Constants;

namespace ExchangeBooks.Droid.Services
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FCMessagingService : FirebaseMessagingService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessagingCenterService _messagingCenterService;

        public FCMessagingService()
        {
            _authenticationService = AppContainer.Resolve<IAuthenticationService>();
            _messagingCenterService = AppContainer.Resolve<IMessagingCenterService>();
        }

        /// <summary>
        /// This static property can be accessed by any other class to get the registration token.
        /// </summary>
        public static string FirebaseRegistrationToken;

        public override void OnMessageReceived(RemoteMessage p0)
        {
            base.OnMessageReceived(p0);
            var pushMessage = new PushMessage {
                MessageId = Guid.Parse(p0.Data[PushMessages.MessageId]),
                TopicId = Guid.Parse(p0.Data[PushMessages.TopicId]),
                Type = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), p0.Data[PushMessages.Type]),
                Title = p0.Data[PushMessages.Title],
                Content = p0.Data[PushMessages.Content],
                CreatedBy = p0.Data[PushMessages.CreatedBy],
                CreatedOn = DateTime.Parse(p0.Data[PushMessages.CreatedOn])
            };
            
            switch (pushMessage.Type)
            {
                case SubscriptionType.Push:
                    _messagingCenterService.Send(Xamarin.Forms.Application.Current, Messaging.PushMessage, pushMessage);
                    DisplayNotification(pushMessage.Title, pushMessage.Content);
                    break;
                case SubscriptionType.Chat:
                    _messagingCenterService.Send(Xamarin.Forms.Application.Current, Messaging.ChatMessage, pushMessage);
                    break;
            }
        }
        /// <summary>
        /// When a token is generated or renewed this method will be called with the token as parameter p0
        /// </summary>
        /// <param name="p0"></param>
        public override void OnNewToken(string p0)
        {
            base.OnNewToken(p0);
            FirebaseRegistrationToken = p0;
            Console.WriteLine($"token: {p0}");
            _authenticationService.UpdateFcmToken(p0).GetAwaiter().GetResult();
            //FirebaseInstanceId.Instance.GetInstanceId().AddOnSuccessListener(new OnSuccessListener());
        }
        /// <summary>
        /// Listener for token success event
        /// </summary>
        internal class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            public void OnSuccess(Java.Lang.Object result)
            {
                var refreshedToken = result.Class.GetMethod("getToken").Invoke(result).ToString();
                Log.Debug(MainActivity.FCM_TAG, "Refreshed token: " + refreshedToken);
            }
        }

        /// <summary>
        /// Display Notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        internal void DisplayNotification(string title, string message)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new Notification.Builder(this, MainActivity.FCM_CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.icon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            MainActivity.NotificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}
