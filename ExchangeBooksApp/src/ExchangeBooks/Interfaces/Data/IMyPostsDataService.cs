using System;
using System.Collections.Generic;
using ExchangeBooks.Models;
using ExchangeBooks.Models.Response;

namespace ExchangeBooks.Interfaces.Data
{
    public interface IMyPostsDataService
    {
        void ResetPost();
        List<PostResponse> Posts { get; set; }
        PostResponse CurrentPost { get; set; }
    }
}
