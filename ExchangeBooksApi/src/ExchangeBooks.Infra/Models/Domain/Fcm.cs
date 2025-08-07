using System;

namespace ExchangeBooks.Infra.Models.Domain
{
    public class Fcm
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}