using System;
using System.Threading.Tasks;
using ExchangeBooks.iOS.Services;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using UIKit;
using UserNotifications;
using Xamarin.Forms.Platform.iOS;

namespace ExchangeBooks.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        #region Variables
        private FCMessagingService _fCMessagingService;
        #endregion

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Xamarin.Forms.Forms.SetFlags(new string[] { "CollectionView_Experimental", "CarouselView_Experimental", "SwipeView_Experimental", "IndicatorView_Experimental" });
            Xamarin.Forms.Forms.Init();
            Xamarin.Forms.FormsMaterial.Init();
            Rg.Plugins.Popup.Popup.Init();
            Firebase.Core.App.Configure();
            GetPermissionForMessaging();
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
                return true;

            return base.OpenUrl(app, url, options);
        }

        #region Cloud Messaging
        #region Public Methods
        /// <summary>
        /// Receive Fcm Token
        /// </summary>
        /// <param name="messaging"></param>
        /// <param name="fcmToken"></param>
        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            _fCMessagingService.DidReceiveRegistrationToken(messaging, fcmToken);
        }

        /// <summary>
        /// Receive displayed notifications for iOS 10 devices.
        /// Handle incoming notification messages while app is in the foreground.
        /// Only if the message has apns config
        /// </summary>
        /// <param name="center"></param>
        /// <param name="notification"></param>
        /// <param name="completionHandler"></param>
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            _fCMessagingService.WillPresentNotification(center, notification, completionHandler);
        }

        /// <summary>
        /// Background after click of message
        /// Handle notification messages after display notification is tapped by the user.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="response"></param>
        /// <param name="completionHandler"></param>
        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            _fCMessagingService.DidReceiveNotificationResponse(center, response, completionHandler);
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            _fCMessagingService.DidReceiveRemoteNotification(application, userInfo, completionHandler);
        }

        [Export("messaging:didReceiveMessage:")]
        public void DidReceiveMessage(Messaging messaging, RemoteMessage remoteMessage)
        {
            _fCMessagingService.DidReceiveMessage(messaging, remoteMessage);
        }

        // You'll need this method if you set "FirebaseAppDelegateProxyEnabled": NO in GoogleService-Info.plist
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            //Messaging.SharedInstance.ApnsToken = deviceToken;
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            _fCMessagingService.ReceivedRemoteNotification(application, userInfo);
        }
        #endregion

        #region Private Methods
        private void GetPermissionForMessaging()
        {
            _fCMessagingService = new FCMessagingService();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // For iOS >=10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;

                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    Console.WriteLine(granted);
                });
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            Firebase.InstanceID.InstanceId.Notifications.ObserveTokenRefresh(async (sender, e) =>
            {
                Console.WriteLine("ObserveTokenRefresh:");
                await Task.FromResult(e);
            });

            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            Messaging.SharedInstance.Delegate = this;

            InstanceId.SharedInstance.GetInstanceId(InstanceIdResultHandler);
        }

        private void InstanceIdResultHandler(InstanceIdResult result, NSError error)
        {
            if (error != null)
            {
                Console.WriteLine(nameof(InstanceIdResultHandler), $"Error: {error.LocalizedDescription}");
                return;
            }

            Console.WriteLine(nameof(InstanceIdResultHandler), $"Remote Instance Id token: {result.Token}");
        }
        #endregion
        #endregion
    }
}
