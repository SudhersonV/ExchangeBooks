using System;
using System.Collections.Generic;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Models;
using ExchangeBooks.Models.Response;

namespace ExchangeBooks.Services.Data
{
    public class MyPostsDataService: IMyPostsDataService
    {
        public List<PostResponse> Posts { get; set; }

        public PostResponse CurrentPost { get; set; }

        public void ResetPost()
        {
            Posts.Clear();
        }
    }
}
