using System;
using ExchangeBooks.Bootstrap;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Models;
using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;
using static ExchangeBooks.Constants.Constants;
using FCMMessaging = Firebase.CloudMessaging.Messaging;

namespace ExchangeBooks.iOS.Services
{
    public class FCMessagingService
    {
        #region Variables
        private IAuthenticationService _authenticationService;
        private IMessagingCenterService _messagingCenterService;
        #endregion

        public event EventHandler<object> MessageReceived;

        public FCMessagingService()
        {
        }

        public void DidReceiveRegistrationToken(FCMMessaging messaging, string fcmToken)
        {
            Console.WriteLine($"Firebase registration token: {fcmToken}");
            #region Send to server
            _authenticationService = AppContainer.Resolve<IAuthenticationService>();
            _authenticationService.UpdateFcmToken(fcmToken);
            #endregion
        }

        #region Only if the message has apns config
        //Receive message while app is in the foreground
        //Only if the message has apns config
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var userInfo = notification.Request.Content.UserInfo;

            // With swizzling disabled you must let Messaging know about the message, for Analytics
            //Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

            // Print full message.
            Console.WriteLine(userInfo);

            ProcessCloudMessage(userInfo);

            // Change this to your preferred presentation option
            completionHandler(UNNotificationPresentationOptions.None);
        }

        //Receive message as a response when the message is tapped while app is in the background
        //Only if the message has apns config
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            var userInfo = response.Notification.Request.Content.UserInfo;

            // Print full message.
            Console.WriteLine(userInfo);

            ProcessCloudMessage(userInfo, false);

            completionHandler();
        }
        #endregion

        #region For non apns config
        //Receive message while the app is in foreground as well background
        public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired till the user taps on the notification launching the application.
            // Handle data of notification

            // With swizzling disabled you must let Messaging know about the message, for Analytics
            // Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

            // Print full message.
            Console.WriteLine(userInfo);
            ProcessCloudMessage(userInfo, false);
            completionHandler(UIBackgroundFetchResult.NewData);
        }
        #endregion

        #region Unused
        public void DidReceiveMessage(FCMMessaging messaging, RemoteMessage remoteMessage)
        {
            // Handle Data messages for iOS 10 and above.
            HandleMessage(remoteMessage.AppData);

            Console.WriteLine(nameof(DidReceiveMessage), remoteMessage.AppData);
        }

        public void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired till the user taps on the notification launching the application.
            // Handle data of notification

            // With swizzling disabled you must let Messaging know about the message, for Analytics
            // Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

            // Print full message.
            Console.WriteLine(nameof(ReceivedRemoteNotification), userInfo);
        }
        #endregion

        #region Private Methods
        private void ProcessCloudMessage(NSDictionary userInfo, bool showAlert = true)
        {
            _messagingCenterService = AppContainer.Resolve<IMessagingCenterService>();
            var pushMessage = new PushMessage
            {
                MessageId = Guid.Parse(userInfo[PushMessages.MessageId].ToString()),
                TopicId = Guid.Parse(userInfo[PushMessages.TopicId].ToString()),
                Type = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), userInfo[PushMessages.Type].ToString()),
                Title = userInfo[PushMessages.Title].ToString(),
                Content = userInfo[PushMessages.Content].ToString(),
                CreatedBy = userInfo[PushMessages.CreatedBy].ToString(),
                CreatedOn = DateTime.Parse(userInfo[PushMessages.CreatedOn].ToString())
            };

            switch (pushMessage.Type)
            {
                case SubscriptionType.Push:
                    _messagingCenterService.Send(Xamarin.Forms.Application.Current, Constants.Constants.Messaging.PushMessage, pushMessage);
                    if (showAlert)
                        ShowMessage(pushMessage.Title, pushMessage.Content);
                    break;
                case SubscriptionType.Chat:
                    _messagingCenterService.Send(Xamarin.Forms.Application.Current, Constants.Constants.Messaging.ChatMessage, pushMessage);
                    break;
            }
        }
        private void ShowMessage(string title, string content, Action actionForOk = null)
        {
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void ShowMessage(string title, string content, UIViewController fromViewController, Action actionForOk = null)
        {
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (obj) => actionForOk?.Invoke()));
            fromViewController.PresentViewController(alert, true, null);
        }

        private void HandleMessage(NSDictionary message)
        {
            if (MessageReceived == null)
                return;

            //INMessageType messageType;
            //if (message.ContainsKey(new NSString("aps")))
            //    messageType = MessageType.Notification;
            //else
            //    messageType = MessageType.Data;

            //var e = new UserInfoEventArgs(message, messageType);
            //MessageReceived(this, e);
        }
        #endregion
    }
}
