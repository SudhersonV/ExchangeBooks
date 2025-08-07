using System;
using ExchangeBooks.Enums;
using ExchangeBooks.Services.Data;

namespace ExchangeBooks.Interfaces.Data
{
    public interface IHidePostsDataService
    {
        HidePosts HiddenPosts { get; set; }
        void HidePost(Guid postId);
        void HideUser(string emailId);
    }

    public interface IFlagPostsDataService
    {
        FlagPosts FlaggedPosts { get; set; }
        void FlagPost(Guid postId, FlagPostEnum reason);
    }
}
