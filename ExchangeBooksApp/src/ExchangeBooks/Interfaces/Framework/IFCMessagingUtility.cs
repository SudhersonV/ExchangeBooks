using System;
using System.Threading.Tasks;

namespace ExchangeBooks.Interfaces.Framework
{
    public interface IFcmUtility
    {
        Task SubscribeToTopic(string topic);
        Task UnsubscribeToTopic(string topic);
    }
}
