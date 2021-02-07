using authapi.Models;

namespace authapi.Services
{
    public interface IMemCache
    {
        void Put(string userName, Jwt token);
        Jwt Get(string userName);
    }
}