using System;
using System.Collections.Generic;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Data;

namespace ExchangeBooks.Services.Data
{
    public class FlagPostsDataService: IFlagPostsDataService
    {
        private FlagPosts _current;

        public FlagPostsDataService()
        {
            _current = new FlagPosts();
        }

        public FlagPosts FlaggedPosts { get => _current; set => _current = value; }

        public void FlagPost(Guid postId, FlagPostEnum reason)
        {
            _current.FlaggedPosts.Add(new FlaggedPost
            {
                PostId = postId,
                Reason = reason
            });
        }
    }

    public class FlagPosts
    {
        public List<FlaggedPost> FlaggedPosts { get; set; } = new List<FlaggedPost>();
    }

    public class FlaggedPost
    {
        public Guid PostId { get; set; }
        public FlagPostEnum Reason { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActedOn { get; set; }
        public DateTime? ActedOnDate { get; set; }
    }
}
