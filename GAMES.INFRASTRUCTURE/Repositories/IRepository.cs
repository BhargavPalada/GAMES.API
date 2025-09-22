using First.API.Models;
using GAMES.CORE.Models;

namespace GAMES.INFRASTRUCTURE.Repositories
{
    public interface IRepository
    {
        Task<IEnumerable<Games>> GetAllAsync();
        Task<Games?> GetByIdAsync(string id);
        Task CreateAsync(Games entity);
        Task UpdateAsync(string id, Games entity);
        Task DeleteAsync(string id);
       
    }
}