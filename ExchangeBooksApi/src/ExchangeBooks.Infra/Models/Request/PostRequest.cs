using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Models.Domain;

namespace ExchangeBooks.Infra.Models.Request
{
    public class PostRequest
    {

        [Required]
        public string Name { get; set; }
        public double Price { get; set; }
        [Required]
        public IEnumerable<BookRequest> Books { get; set; }
        public PostStatus Status { get; set; }
    }
}