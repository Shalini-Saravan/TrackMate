using System.ComponentModel.DataAnnotations;

namespace TrackMate.Models
{
    public class UserRole
    {
        [Required]
        public string RoleName { get; set; }


    }
}
