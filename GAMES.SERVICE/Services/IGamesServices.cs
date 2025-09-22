using First.API.Models;

namespace First.API.Services
{
    public interface IGamesServices
    {   Games Create(Games game);
        List<Games> Get();
        Games Get(string id);
        void Remove(string id);
        void Update(string id, Games game);
    }
}