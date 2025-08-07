using System;
using System.Collections.Generic;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Request
{
    public class HidePostRequest
    {
        // Hide this Post
        public Guid? PostId { get; set; }

        // Hide posts from this user
        public string UserEmail { get; set; }
    }

    public class FlagPostRequest
    {
        // Flag these Post
        public Guid PostId { get; set; }

        // Flag reason
        public FlagPostEnum Reason { get; set; }
    }
}