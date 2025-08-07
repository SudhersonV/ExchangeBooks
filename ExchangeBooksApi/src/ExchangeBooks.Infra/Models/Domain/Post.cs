using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Domain
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public List<Book> Books {get; set;}
        public PostStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}