using System;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Models;
using Xamarin.Forms;

namespace ExchangeBooks.Services.Framework
{
    public class MessagingCenterService : IMessagingCenterService
    {
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            MessagingCenter.Send(sender, message, args);
        }

        public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> action) where TSender: class
        {
            MessagingCenter.Subscribe(subscriber, message, action);
        }
    }
}
