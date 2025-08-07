using System;
using ExchangeBooks.Infra.Models.Domain;

namespace ExchangeBooks.Infra.Models.Response
{
    public class BookSearchResponse
    {
        public Guid PostId { get; set; }
        public string PostName { get; set; }
        public string CreatedBy { get; set; }
        public Book Book { get; set; }
    }
}