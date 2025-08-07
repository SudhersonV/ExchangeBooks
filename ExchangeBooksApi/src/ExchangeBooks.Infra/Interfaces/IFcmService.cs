using System;
using System.Threading.Tasks;

namespace ExchangeBooks.Infra.Interfaces
{
    public interface IFcmService
    {
        string GenerateFcmTopicId(string topicName, Guid topicId);
        Task<string> GetToken(string email = null);
        Task SetToken(string token);
        Task UpdateToken(string token);
    }
}