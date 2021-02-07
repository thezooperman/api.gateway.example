using System;
using System.ComponentModel.DataAnnotations;

namespace authapi.Models
{
    [Serializable]
    public class User
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public string Salt { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public DateTime TimeIdentifier { get; set; }
    }
}