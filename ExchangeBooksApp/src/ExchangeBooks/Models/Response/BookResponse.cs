using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExchangeBooks.Enums;

namespace ExchangeBooks.Models.Response
{
    public class BookResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BookCondition Condition { get; set; }
        public BookClass Class { get; set; }
        public BookStatus Status { get; set; }
        public double Price { get; set; }
        public List<string> Tags { get; set; }
        public ObservableCollection<BookImage> Images { get; set; }
    }
}
