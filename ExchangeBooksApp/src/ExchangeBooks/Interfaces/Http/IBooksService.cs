using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Enums;
using ExchangeBooks.Models;
using ExchangeBooks.Models.Request;
using ExchangeBooks.Models.Response;
using ExchangeBooks.Services.Data;

namespace ExchangeBooks.Interfaces.Http
{
    public interface IBooksService
    {
        Task<PostRequest> Post(PostRequest post);
        Task<List<BookSearchResponse>> GetRecentBooks(int count = 10);
        Task<List<BookSearchResponse>> SearchBooks(string[] tags);
        Task<List<PostResponse>> GetUserPosts();
        Task<PostRequest> GetPostById(Guid id);
        Task MarkPostStatus(Guid id, PostStatus status);
        Task MarkBookStatus(Guid postId, Guid id, BookStatus status);
        Task DeletePost(Guid id);
        Task DeleteBook(Guid postId, Guid id);
        Task GetHiddenPosts();
        Task HidePost(HidePostRequest request);
        Task GetFlaggedPosts();
        Task FlagPost(FlagPostRequest request);
    }
}
