using System;

namespace authapi.Models
{
    [Serializable]
    public class Jwt
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}