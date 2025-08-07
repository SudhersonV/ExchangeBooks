using System;
using System.Collections.Generic;

namespace ExchangeBooks.Interfaces.Framework
{
    public interface IEventTracker
    {
        void SendEvent(string eventId);
        void SendEvent(string eventId, string paramName, string value);
        void SendEvent(string eventId, IDictionary<string, string> parameters);
        void SetUserId(string userId);
        void SetUserProperty(string name, string value);
        void SetCurrentScreen(string screenName, string screenClassOverride = "");
    }
}
