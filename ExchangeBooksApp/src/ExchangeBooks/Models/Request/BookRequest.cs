using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExchangeBooks.Enums;

namespace ExchangeBooks.Models
{
    public class BookRequest
    {
        [Required, MinLength(5)]
        public string Name { get; set; }
        public string Description { get; set; }
        public BookCondition Condition { get; set; }
        public BookClass Class { get; set; }
        public BookStatus Status { get; set; }
        public double Price { get; set; }
        public List<BookImage> Images { get; set; }
    }
}
