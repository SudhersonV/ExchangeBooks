using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.Interfaces
{
    public interface IAdminService
    {
        Task Clean();
        Task Create();
        Task Delete();
    }
}