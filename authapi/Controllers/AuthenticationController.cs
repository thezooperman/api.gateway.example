using System.Threading.Tasks;
using authapi.Models;
using authapi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace authapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JwtAuthenticationController : ControllerBase
    {
        private readonly ILogger<JwtAuthenticationController> _logger;
        private IRepository _repository;
        private readonly IJwtAuthentication _jwtAuthentication;

        public JwtAuthenticationController(ILogger<JwtAuthenticationController> logger,
                IRepository repo, IJwtAuthentication jwtAuthentication)
        {
            _logger = logger;
            _repository = repo;
            this._jwtAuthentication = jwtAuthentication;
        }

        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] User userCred)
        {
            if (await _repository.GetUser(userCred.UserName) != null)
            {
                _logger.LogDebug($"Cannot add existing user.");
                return StatusCode(StatusCodes.Status304NotModified);
            }
            await _repository.AddUser(userCred.UserName, userCred.Password);

            _logger.LogInformation($"User-{userCred.UserName} created");

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userCred)
        {
            var token = await _jwtAuthentication.GetJwtToken(userCred);
            if (token != null)
            {
                _logger.LogDebug($"User: {userCred.UserName} is authenticated.");
                return StatusCode(StatusCodes.Status302Found, token);
            }
            _logger.LogDebug($"User: {userCred.UserName} is not authenticated.");

            return Unauthorized("Incorrect User Id or Password");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Jwt refreshToken)
        {
            var token = await _jwtAuthentication.RefreshToken(refreshToken);
            if (token == null)
                return Unauthorized("Token not present or already expired. Please re-authenticate.");

            _logger.LogDebug($"Token: {refreshToken.Token} refreshed");
            return StatusCode(StatusCodes.Status202Accepted, token);
        }
    }
}
