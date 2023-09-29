using System.ComponentModel.DataAnnotations;

namespace BlazorServerAppWithIdentity.Models
{
    public class User
    {
        [Required]
        public string Name { get; set; }
        public string UserName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } = "Welcome@2023";

        [Required]
        public string Team { get; set; }
       
        [Required]
        public string Department { get; set; }

        [Required]
        public string Roles { get; set; }

    }
}
