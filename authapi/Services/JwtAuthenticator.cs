using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using authapi.Models;
using Microsoft.IdentityModel.Tokens;

namespace authapi.Services
{
    class JwtAuthenticator : IJwtAuthentication
    {
        private readonly string _token;
        private readonly IRepository _repository;
        private readonly IMemCache _cache;

        public JwtAuthenticator(IRepository repository, IMemCache cache, string token)
        {
            this._token = token;
            this._repository = repository;
            this._cache = cache;
        }
        public async Task<Jwt> GetJwtToken(User userFromUI)
        {
            var userFromDB = await _repository.GetUser(userFromUI.UserName.Trim());
            if (userFromDB == null)
                return null;

            // Calculate password hash and compare against DB
            var saltPassword = userFromDB.Salt + Convert.ToBase64String(Encoding.ASCII.GetBytes(userFromUI.Password));
            var sha512Pwd = SHA512.HashData(Encoding.ASCII.GetBytes(saltPassword));
            var pwdMatched = Convert.ToBase64String(sha512Pwd)
                .Equals(userFromDB.Password, StringComparison.InvariantCultureIgnoreCase);

            if (pwdMatched)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_token);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                        new Claim(ClaimTypes.Name, userFromUI.UserName),
                        new Claim(ClaimTypes.Email, userFromUI.UserName)
                        }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha512Signature
                    )
                };
                var jwt = new Jwt();
                jwt.Token = tokenHandler.WriteToken(tokenHandler.CreateJwtSecurityToken(tokenDescriptor));
                jwt.ExpiresAt = tokenDescriptor.Expires.Value;

                // Add generated token to in-memory cache
                _cache.Put(userFromUI.UserName.Trim(), jwt);
                return jwt;
            }
            return null;
        }

        public async Task<Jwt> RefreshToken(Jwt refreshToken)
        {
            if (refreshToken == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_token);

            SecurityToken validatedToken;

            // validate the token context
            var principal = tokenHandler.ValidateToken(refreshToken.Token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true //here we are saying that we care about the token's expiration date
                }, out validatedToken);

            var securityToken = validatedToken as JwtSecurityToken;

            if (securityToken == null || !securityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                return null;

            var uName = principal.Identity.Name;

            // check if user is valid
            var user = await _repository.GetUser(uName);
            if (user == null)
                return null;

            // Get user token from cache
            var jwtToken = _cache.Get(uName);

            // user needs to authorize again - token is swiped from memory or not added
            if (jwtToken == null)
                return null;

            // check if token has expired
            if (securityToken.IssuedAt > jwtToken.ExpiresAt.AddHours(1))
                return null;

            Jwt jwt = null;

            // refresh token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                        new Claim(ClaimTypes.Name, uName),
                        new Claim(ClaimTypes.Email, uName)
                        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };

            jwt = new Jwt();
            jwt.Token = tokenHandler.WriteToken(tokenHandler.CreateJwtSecurityToken(tokenDescriptor));
            jwt.ExpiresAt = tokenDescriptor.Expires.Value;

            _cache.Put(uName, jwt);

            return jwt;
        }
    }
}