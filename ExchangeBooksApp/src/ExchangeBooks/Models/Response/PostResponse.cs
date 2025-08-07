using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExchangeBooks.Enums;

namespace ExchangeBooks.Models.Response
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public ObservableCollection<BookResponse> Books { get; set; }
        public PostStatus Status { get; set; }
        public string FcmToken { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
