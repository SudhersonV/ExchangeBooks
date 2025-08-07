using System.Collections.Generic;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Models.Domain;

namespace ExchangeBooks.Infra.Models.Request
{
    public class BookRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BookCondition Condition { get; set; }
        public BookClass Class { get; set; }
        public BookStatus Status { get; set; }
        public double Price { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public List<BookImage> Images { get; set; }
    }
}