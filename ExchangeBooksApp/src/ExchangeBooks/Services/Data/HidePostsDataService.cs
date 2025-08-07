using System;
using System.Collections.Generic;
using ExchangeBooks.Interfaces.Data;

namespace ExchangeBooks.Services.Data
{
    public class HidePostsDataService: IHidePostsDataService
    {
        private HidePosts _current;

        public HidePostsDataService()
        {
            _current = new HidePosts();
        }

        public HidePosts HiddenPosts { get => _current; set => _current = value; }

        public void HidePost(Guid postId)
        {
            _current.PostIds.Add(postId);
        }

        public void HideUser(string emailId)
        {
            _current.UserEmailIds.Add(emailId);
        }
    }

    public class HidePosts
    {
        public List<Guid> PostIds { get; set; } = new List<Guid>();
        public List<string> UserEmailIds { get; set; } = new List<string>();
    }
}
