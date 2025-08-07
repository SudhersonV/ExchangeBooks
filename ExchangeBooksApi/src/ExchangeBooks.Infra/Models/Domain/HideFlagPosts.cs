using System;
using System.Collections.Generic;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Domain
{
    public class HidePosts
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public List<Guid> PostIds { get; set; } = new List<Guid>();
        public List<string> UserEmailIds { get; set; } = new List<string>();
    }

    public class FlagPosts
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public List<FlaggedPost> FlaggedPosts { get; set; } = new List<FlaggedPost>();
    }

    public class FlaggedPost
    {
        public Guid PostId { get; set; }
        public FlagPostEnum Reason { get; set; }
        public DateTime CreatedOn {get; set;}
        public bool ActedOn { get; set; }
        public DateTime? ActedOnDate { get; set; }
    }
}