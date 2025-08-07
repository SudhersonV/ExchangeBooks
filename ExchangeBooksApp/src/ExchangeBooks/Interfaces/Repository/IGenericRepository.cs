using System.Net.Http;
using System.Threading.Tasks;

namespace ExchangeBooks.Interfaces.Repository
{
    public interface IGenericRepository
    {
        Task GetAsync(string uri, string authToken = "");
        Task<T> GetAsync<T>(string uri, string authToken = "");
        Task<T> PostAsync<T>(string uri, T data, string authToken = "");
        Task<T> PutAsync<T>(string uri, T data, string authToken = "");
        Task<R> PostAsync<T, R>(string uri, T data, string authToken = "");
        Task<HttpResponseMessage> PatchAsync<T>(string uri, T data, string authToken = "");
        Task<HttpResponseMessage> DeleteAsync(string uri, string authToken = "");
    }
}
