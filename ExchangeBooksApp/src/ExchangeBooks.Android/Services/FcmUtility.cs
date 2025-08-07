using System.Threading.Tasks;
using Android.Gms.Extensions;
using ExchangeBooks.Droid.Services;
using ExchangeBooks.Interfaces.Framework;
using Firebase.Messaging;
using Xamarin.Forms;

[assembly: Dependency(typeof(FcmUtility))]
namespace ExchangeBooks.Droid.Services
{
    public class FcmUtility: IFcmUtility
    {
        public FcmUtility()
        {
        }

        public async Task SubscribeToTopic(string topic)
        {
            await FirebaseMessaging.Instance.SubscribeToTopic(topic);
        }

        public async Task UnsubscribeToTopic(string topic)
        {
            await FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
        }
    }
}
