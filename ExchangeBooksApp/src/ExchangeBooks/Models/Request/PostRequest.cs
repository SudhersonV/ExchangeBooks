using System.Collections.Generic;
using ExchangeBooks.Enums;
using System.ComponentModel.DataAnnotations;
using System;

namespace ExchangeBooks.Models
{
    public class PostRequest
    {
        public PostRequest()
        {
            Books = new List<BookRequest>();
        }

        [Required, MinLength(5)]
        public string Name { get; set; }
        public double Price { get; set; }
        public List<BookRequest> Books { get; set; }
        public PostStatus Status { get; set; }
    }
}