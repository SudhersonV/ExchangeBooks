using System;
using ExchangeBooks.Models;
using Xamarin.Forms;

namespace ExchangeBooks.Interfaces.Framework
{
    public interface IMessagingCenterService
    {
        void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
        void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback) where TSender: class;
    }
}
