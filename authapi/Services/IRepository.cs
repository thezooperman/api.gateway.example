using System.Threading.Tasks;
using authapi.Models;

namespace authapi.Services
{
    public interface IRepository
    {
        Task<User> GetUser(string userName);
        Task AddUser(string userName, string password);
    }
}