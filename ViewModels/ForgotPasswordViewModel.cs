using System.ComponentModel.DataAnnotations;

namespace AuthProject.ViewModels
{
    public class ForgotPasswordViewModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
