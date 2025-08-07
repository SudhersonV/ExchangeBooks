using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Models.Domain;
using ExchangeBooks.Infra.Models.Request;
using ExchangeBooks.Infra.Models.Response;

namespace ExchangeBooks.Infra.Interfaces
{
    public interface IPostsService
    {
         Task<Post> AddPost(Post post);
         Task<Post> GetPost(Guid id);
         Task<List<Post>> GetUserPosts(int count = 25);
         Task<List<BookSearchResponse>> GetRecentBooks(int count);
         Task<List<BookSearchResponse>> SearchBooks(List<string> searchTags);
         Task MarkPostStatus(Guid id, PostStatus status);
         Task MarkBookStatus(Guid postId, Guid id, BookStatus status);
         Task DeletePost(Guid id);
         Task DeleteBook(Guid postId, Guid id);
         Task<HidePosts> GetHidePosts();
         Task AddHidePosts(HidePostRequest request);
         Task<FlagPosts> GetFlagPosts();
         Task AddFlagPost(FlagPostRequest request);
    }
}