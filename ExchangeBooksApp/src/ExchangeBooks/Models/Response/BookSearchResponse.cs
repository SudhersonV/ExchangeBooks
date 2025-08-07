using System;

namespace ExchangeBooks.Models.Response
{
    public class BookSearchResponse
    {
        public Guid PostId { get; set; }
        public string PostName { get; set; }
        public string CreatedBy { get; set; }
        public BookResponse Book { get; set; }
        public bool IsChatVisible { get; set; }
    }
}
