using System.Threading.Tasks;

namespace ExchangeBooks.Infra.Interfaces
{
    public interface IAdminService
    {    
         Task Clean();
         Task Create();
         Task Delete();
    }
}