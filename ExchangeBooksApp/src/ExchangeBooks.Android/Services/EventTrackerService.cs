using System.Collections.Generic;
using Android.OS;
using ExchangeBooks.Interfaces.Framework;
using Firebase.Analytics;
using FirebasePOC.Droid;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(EventTrackerService))]
namespace FirebasePOC.Droid
{
    public class EventTrackerService : IEventTracker
    {
        FirebaseAnalytics _firebaseAnalytics;

        public EventTrackerService()
        {
            _firebaseAnalytics = FirebaseAnalytics.GetInstance(Android.App.Application.Context);
        }

        public void SendEvent(string eventId)
        {
            SendEvent(eventId, null);
        }

        public void SendEvent(string eventId, string paramName, string value)
        {
            SendEvent(eventId, new Dictionary<string, string>
            {
                {paramName, value}
            });
        }

        public void SendEvent(string eventId, IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                _firebaseAnalytics.LogEvent(eventId, null);
                return;
            }

            var bundle = new Bundle();
            foreach (var param in parameters)
            {
                bundle.PutString(param.Key, param.Value);
            }

            _firebaseAnalytics.LogEvent(eventId, bundle);
        }

        [System.Obsolete]
        public void SetCurrentScreen(string screenName, string screenClassOverride)
        {
            _firebaseAnalytics.SetCurrentScreen(Platform.CurrentActivity, screenName, screenClassOverride);
        }

        public void SetUserId(string userId)
        {
            _firebaseAnalytics.SetUserId(userId);
        }

        public void SetUserProperty(string name, string value)
        {
            _firebaseAnalytics.SetUserProperty(name, value);
        }
    }
}