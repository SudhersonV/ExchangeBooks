using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Domain
{
    public class Book
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(1, Int16.MaxValue)]
        public BookCondition Condition { get; set; }
        [Range(1, Int16.MaxValue)]
        public BookClass Class { get; set; }
        [Range(1, Int16.MaxValue)]
        public BookStatus Status { get; set; }
        public double Price { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<BookImage> Images { get; set; }
    }
}