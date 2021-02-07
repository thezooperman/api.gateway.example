using System.Threading.Tasks;
using authapi.Models;

namespace authapi.Services
{
    public interface IJwtAuthentication
    {
        Task<Jwt> GetJwtToken(User user);

        Task<Jwt> RefreshToken(Jwt refreshToken);
    }
}