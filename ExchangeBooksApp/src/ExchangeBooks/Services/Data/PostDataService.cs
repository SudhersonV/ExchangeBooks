using System.Collections.Generic;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Models;

namespace ExchangeBooks.Services.Data
{
    public class PostDataService : IPostDataService
    {
        public PostDataService()
        { }

        public List<BookRequest> Books => CurrentBook is null ? null : Post.Books;
        public PostRequest Post { get; private set; }
        public BookRequest CurrentBook { get; set; }
        public List<BookImage> CurrentBookImages => CurrentBook?.Images;

        public void ResetPost()
        {
            Post = new PostRequest();
            var book = CurrentBook = new BookRequest();
            Post.Books.Add(book);
        }
    }
}
