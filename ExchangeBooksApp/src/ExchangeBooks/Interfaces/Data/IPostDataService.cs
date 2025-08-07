using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Models;

namespace ExchangeBooks.Interfaces.Data
{
    public interface IPostDataService
    {
        void ResetPost();
        List<BookRequest> Books { get; }
        PostRequest Post { get; }
        BookRequest CurrentBook { get; set; }
        List<BookImage> CurrentBookImages { get; }
    }
}
