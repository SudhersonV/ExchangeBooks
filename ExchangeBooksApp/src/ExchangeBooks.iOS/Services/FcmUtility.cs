using System.Threading.Tasks;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.iOS.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FcmUtility))]
namespace ExchangeBooks.iOS.Services
{
    public class FcmUtility: IFcmUtility
    {
        public FcmUtility()
        {
        }

        public async Task SubscribeToTopic(string topic)
        {
            await Task.FromResult(0);
        }

        public async Task UnsubscribeToTopic(string topic)
        {
            await Task.FromResult(0);
        }
    }
}
