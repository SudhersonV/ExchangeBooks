using System;
using System.Collections.Generic;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Models.Response;

namespace ExchangeBooks.Services.Data
{
    public class SearchBooksDataService : ISearchBooksDataService
    {
        public List<BookSearchResponse> Books { get; set; }
        public BookSearchResponse CurrentBook { get; set; }

        public void ResetPost()
        {
            Books.Clear();
        }
    }
}
