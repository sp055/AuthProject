using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthProject.Models
{
    public class AppUser : IdentityUser
    {
        [NotMapped]
        public string? RoleId { get; set; }

        [NotMapped]
        public string? Role { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? RoleList { get; set; }

        public bool FirstLogin { get; set; }

        //[NotMapped]
        public string? PasswordAm { get; set; }

        public DateTime? LastPasswChange { get; set; }
    }
}
