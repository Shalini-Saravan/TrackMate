using System.ComponentModel.DataAnnotations;

namespace BlazorServerAppWithIdentity.Models
{
    public class UserRole
    {
        [Required]
        public string RoleName { get; set; }

        
    }
}
