using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Infra.Models.Domain;
using System.Net;
using ExchangeBooks.Infra.Models.Response;
using Microsoft.Extensions.Options;
using ExchangeBooks.Infra.Models.Request;

namespace ExchangeBooks.Services
{
    public class PostsService : IPostsService
    {
        private Container _postsContainer, _hidePostsContainer, _flagPostsContainer;
        private readonly IClaimsAccessor _claimsAccessor;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public PostsService(CosmosClient dbClient, IClaimsAccessor claimsAccessor, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _claimsAccessor = claimsAccessor;
            _cosmosDbOptions = cosmosDbOptions.Value;
            _postsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.PostsContainerId);
            _hidePostsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.HidePostsContainerId);
            _flagPostsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.FlagPostsContainerId);
        }
        public async Task<Post> AddPost(Post post)
        {
            post.Books.ToList().ForEach(b =>
            {
                b.Tags ??= new List<string>();
                var tempTags = b.Tags.ToList();
                tempTags.AddRange(b.Name.Split(' '));
                tempTags.Add(b.Class.ToString());
                tempTags.Add(b.Condition.ToString());
                b.Tags = tempTags.AsEnumerable();
            });
            try
            {
                post.CreatedBy = post.ModifiedBy = _claimsAccessor.Email;
                var response = await _postsContainer.CreateItemAsync(post);
                return response.StatusCode == HttpStatusCode.Created ?
                    response.Resource : null; ;
            }
            catch (Exception ex)
            {
                var x = ex;
                return null;
            }
        }
        public async Task<Post> GetPost(Guid id)
        {
            try
            {
                var post = await _postsContainer.ReadItemAsync<Post>(id.ToString(), new PartitionKey(id.ToString()));
                return post;
            }
            catch (Exception ex)
            {
                var x = ex;
                return null;
            }
        }
        public async Task<List<BookSearchResponse>> SearchBooks(List<string> searchTags)
        {
            var dbScripts = _postsContainer.Scripts;
            var bookSearchResults = new List<BookSearchResponse>();
            try
            {
                var iterator = _postsContainer.GetItemLinqQueryable<Post>(true).SelectMany(p => p.Books
                    .Select(pb => new { p, pb })).Where(ppb => ppb.pb.Tags.Any(t => searchTags.Contains(t.ToLower())))
                    .Select(pbr => new BookSearchResponse
                    {
                        PostId = pbr.p.Id,
                        PostName = pbr.p.Name,
                        CreatedBy = pbr.p.CreatedBy,
                        Book = pbr.pb
                    }).ToFeedIterator();
                var searchResults = await iterator.ReadNextAsync();
                bookSearchResults = searchResults.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bookSearchResults;
        }
        public async Task<List<Post>> GetUserPosts(int count = 25)
        {
            var iterator = _postsContainer.GetItemLinqQueryable<Post>(true)
                .Where(p => p.CreatedBy.ToLower() == _claimsAccessor.Email.ToLower())
                .OrderByDescending(p => p.ModifiedOn).Take(count).ToFeedIterator();
            var searchResults = await iterator.ReadNextAsync();
            return searchResults.ToList();
        }
        public async Task<List<BookSearchResponse>> GetRecentBooks(int count)
        {
            var iterator = _postsContainer.GetItemLinqQueryable<Post>(true)
                .Where(p => p.Status != PostStatus.Closed)
                .OrderByDescending(p => p.ModifiedOn)
                .SelectMany(p => p.Books.Select(pb => new { p, pb }))
                    .Select(pbr => new BookSearchResponse
                    {
                        PostId = pbr.p.Id,
                        PostName = pbr.p.Name,
                        CreatedBy = pbr.p.CreatedBy,
                        Book = pbr.pb
                    }).Take(count).ToFeedIterator();
            var searchResults = await iterator.ReadNextAsync();
            return searchResults.ToList();
        }

        public async Task MarkPostStatus(Guid id, PostStatus status)
        {
            var post = await GetPost(id);
            if (post is null || post.CreatedBy.ToLower() != _claimsAccessor.Email.ToLower())
                return;

            post.Status = status;
            await _postsContainer.ReplaceItemAsync<Post>(post, post.Id.ToString());
        }
        public async Task MarkBookStatus(Guid postId, Guid id, BookStatus status)
        {
            var post = await GetPost(postId);
            if (post is null || post.CreatedBy.ToLower() != _claimsAccessor.Email.ToLower())
                return;

            var book = post.Books.FirstOrDefault(b => b.Id == id);
            if (book is null) return;

            book.Status = status;
            var index = post.Books.IndexOf(book);
            if (index < 0) return;

            post.Books[index] = book;
            await _postsContainer.ReplaceItemAsync<Post>(post, post.Id.ToString());
        }
        public async Task DeletePost(Guid id)
        {
            var post = await GetPost(id);
            if (post is null || post.CreatedBy.ToLower() != _claimsAccessor.Email.ToLower())
                return;

            await _postsContainer.DeleteItemAsync<Post>(id.ToString(), new PartitionKey(id.ToString()));
        }

        public async Task DeleteBook(Guid postId, Guid id)
        {
            var post = await GetPost(postId);
            if (post is null || post.CreatedBy.ToLower() != _claimsAccessor.Email.ToLower())
                return;

            if (post.Books == null || post.Books.Count < 2)
                return;

            var book = post.Books.FirstOrDefault(b => b.Id == id);
            if (book is null)
                return;

            post.Books.Remove(book);
            await _postsContainer.ReplaceItemAsync<Post>(post, post.Id.ToString());
        }

        public async Task AddHidePosts(HidePostRequest request)
        {
            var email = _claimsAccessor.Email;
            try
            {
                var iterator = _hidePostsContainer.GetItemLinqQueryable<HidePosts>(true)
                    .Where(hp => hp.Email == email).Take(1).ToFeedIterator();
                var hidePosts = await iterator.ReadNextAsync();
                var existingHidePost = hidePosts.FirstOrDefault();
                if (existingHidePost is null)
                {
                    var hidePost = new HidePosts
                    {
                        Id = Guid.NewGuid(),
                        Email = email
                    };

                    if (request.PostId.HasValue)
                        hidePost.PostIds = new List<Guid> { (Guid)request.PostId };

                    if (!string.IsNullOrEmpty(request.UserEmail))
                        hidePost.UserEmailIds = new List<string> { request.UserEmail };

                    await _hidePostsContainer.CreateItemAsync(hidePost);
                }
                else
                {
                    var updateValid = false;
                    if (request.PostId.HasValue && existingHidePost.PostIds.Where(ehp => ehp == request.PostId).Count() == 0)
                    {
                        existingHidePost.PostIds.Add((Guid)request.PostId);
                        updateValid = true;
                    }
                    if (!string.IsNullOrEmpty(request.UserEmail) &&
                            existingHidePost.UserEmailIds.Where(eem => eem == request.UserEmail).Count() == 0)
                    {
                        existingHidePost.UserEmailIds.Add(request.UserEmail);
                        updateValid = true;
                    }

                    if (updateValid)
                        await _hidePostsContainer.ReplaceItemAsync<HidePosts>(existingHidePost, existingHidePost.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddFlagPost(FlagPostRequest request)
        {
            var email = _claimsAccessor.Email;
            try
            {
                var iterator = _flagPostsContainer.GetItemLinqQueryable<FlagPosts>(true)
                    .Where(hp => hp.Email == email).Take(1).ToFeedIterator();
                var flagPosts = await iterator.ReadNextAsync();
                var existingFlagPost = flagPosts.FirstOrDefault();
                if (existingFlagPost is null)
                {
                    var flagPost = new FlagPosts
                    {
                        Id = Guid.NewGuid(),
                        Email = email
                    };
                    flagPost.FlaggedPosts.Add(
                            new FlaggedPost
                            {
                                PostId = request.PostId,
                                Reason = request.Reason,
                                CreatedOn = DateTime.UtcNow
                            });
                    //Send email
                    await _flagPostsContainer.CreateItemAsync(flagPost);
                }
                else
                {
                    if (existingFlagPost.FlaggedPosts.Any(efp => efp.PostId == request.PostId)) return;

                    existingFlagPost.FlaggedPosts.Add(new FlaggedPost
                    {
                        PostId = request.PostId,
                        Reason = request.Reason
                    });

                    await _flagPostsContainer.ReplaceItemAsync<FlagPosts>(existingFlagPost, existingFlagPost.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HidePosts> GetHidePosts()
        {
            var email = _claimsAccessor.Email;
            HidePosts hidePostsResult;
            try
            {
                var iterator = _hidePostsContainer.GetItemLinqQueryable<HidePosts>(true)
                    .Where(hp => hp.Email == email).Take(1).ToFeedIterator();
                var searchResults = await iterator.ReadNextAsync();
                hidePostsResult = searchResults.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return hidePostsResult;
        }

        public async Task<FlagPosts> GetFlagPosts()
        {
            var email = _claimsAccessor.Email;
            FlagPosts flagPostsResult;
            try
            {
                var iterator = _flagPostsContainer.GetItemLinqQueryable<FlagPosts>(true)
                    .Where(hp => hp.Email == email).Take(1).ToFeedIterator();
                var searchResults = await iterator.ReadNextAsync();
                flagPostsResult = searchResults.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flagPostsResult;
        }
    }
}
