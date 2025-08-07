using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ExchangeBooks.Enums;
using ExchangeBooks.Exceptions;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Interfaces.Repository;
using ExchangeBooks.Models;
using ExchangeBooks.Models.Request;
using ExchangeBooks.Models.Response;
using ExchangeBooks.Services.Data;
using static ExchangeBooks.Constants.Constants;

namespace ExchangeBooks.Services.Http
{
    public class BooksService : IBooksService
    {
        private readonly IGenericRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMyPostsDataService _myPostsDataService;
        private readonly ISearchBooksDataService _searchBooksDataService;
        private readonly IHidePostsDataService _hidePostsDataService;
        private readonly IFlagPostsDataService _flagPostsDataService;

        public BooksService(IGenericRepository repository, IAuthenticationService authenticationService
            , IMyPostsDataService myPostsDataService, ISearchBooksDataService searchBooksDataService
            , IHidePostsDataService hidePostsDataService, IFlagPostsDataService flagPostsDataService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
            _myPostsDataService = myPostsDataService;
            _searchBooksDataService = searchBooksDataService;
            _hidePostsDataService = hidePostsDataService;
            _flagPostsDataService = flagPostsDataService;
        }

        public async Task<PostRequest> Post(PostRequest post)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                return await _repository.PostAsync($"{Api.Url}/{Api.Paths.Post.AddPost}", post, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return null;
            }
        }

        public async Task<List<BookSearchResponse>> GetRecentBooks(int count = 10)
        {
            try
            {
                var books = await _repository.GetAsync<List<BookSearchResponse>>($"{Api.Url}/{Api.Paths.Post.RecentBooks.Replace("{count}", count.ToString())}");
                _searchBooksDataService.Books = books;
                return books;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent books, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<BookSearchResponse>();
            }
        }

        public async Task<List<BookSearchResponse>> SearchBooks(string[] tags)
        {
            try
            {
                var books = await _repository.GetAsync<List<BookSearchResponse>>($"{Api.Url}/{Api.Paths.Post.SearchBooks.Replace("{tags}", string.Join(TagSeparator, tags))}");
                _searchBooksDataService.Books = books;
                return books;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching books, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<BookSearchResponse>();
            }
        }

        public async Task<List<PostResponse>> GetUserPosts()
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var userPosts = await _repository.GetAsync<List<PostResponse>>($"{Api.Url}/{Api.Paths.Post.UserPosts}", accessToken);
                _myPostsDataService.Posts = userPosts;
                return userPosts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user posts, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<PostResponse>();
            }
        }

        public async Task<PostRequest> GetPostById(Guid id)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                return await _repository.GetAsync<PostRequest>($"{Api.Url}/{Api.Paths.Post.GetPostById.Replace("{id}", id.ToString())}", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return null;
            }
        }

        public async Task MarkPostStatus(Guid id, PostStatus status)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PatchAsync(@$"{Api.Url}/{Api.Paths.Post.MarkPostStatus
                    .Replace("{id}", id.ToString()).Replace("{postStatus}", status.ToString())}", string.Empty, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task MarkBookStatus(Guid postId, Guid id, BookStatus status)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PatchAsync(@$"{Api.Url}/{Api.Paths.Post.MarkBookStatus
                    .Replace("{postId}", postId.ToString())
                    .Replace("{id}", id.ToString()).Replace("{bookStatus}", status.ToString())}", string.Empty, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task DeletePost(Guid id)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.DeleteAsync(@$"{Api.Url}/{Api.Paths.Post.DeletePost
                    .Replace("{id}", id.ToString())}", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task DeleteBook(Guid postId, Guid id)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.DeleteAsync(@$"{Api.Url}/{Api.Paths.Post.DeleteBook
                    .Replace("{postId}", postId.ToString())
                    .Replace("{id}", id.ToString())}", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task GetHiddenPosts()
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var hiddenPosts = await _repository.GetAsync<HidePosts>($"{Api.Url}/{Api.Paths.Post.HidePost}", accessToken);
                _hidePostsDataService.HiddenPosts = hiddenPosts;
            }
            catch (HttpRequestExceptionEx httpEx)
            {
                if (httpEx.HttpCode == HttpStatusCode.NotFound)
                    _hidePostsDataService.HiddenPosts = new HidePosts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent books, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task HidePost(HidePostRequest request)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PostAsync($"{Api.Url}/{Api.Paths.Post.HidePost}", request, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task GetFlaggedPosts()
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var flaggedPosts = await _repository.GetAsync<FlagPosts>($"{Api.Url}/{Api.Paths.Post.FlagPost}", accessToken);
                _flagPostsDataService.FlaggedPosts = flaggedPosts;
            }
            catch (HttpRequestExceptionEx httpEx)
            {
                if (httpEx.HttpCode == HttpStatusCode.NotFound)
                    _flagPostsDataService.FlaggedPosts = new FlagPosts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent books, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task FlagPost(FlagPostRequest request)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PostAsync($"{Api.Url}/{Api.Paths.Post.FlagPost}", request, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }
    }
}
