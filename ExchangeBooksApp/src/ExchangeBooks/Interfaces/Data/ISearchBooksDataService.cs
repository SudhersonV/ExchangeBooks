using System;
using System.Collections.Generic;
using ExchangeBooks.Models.Response;

namespace ExchangeBooks.Interfaces.Data
{
    public interface ISearchBooksDataService
    {
        void ResetPost();
        List<BookSearchResponse> Books { get; set; }
        BookSearchResponse CurrentBook { get; set; }
    }
}
