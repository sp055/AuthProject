using System.ComponentModel.DataAnnotations;

namespace AuthProject.ViewModels
{
    public class LogInViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember Me?")]

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        public bool? FirstLogin { get; set; } = true;
        public double OTP { get; set; }
        public double OTPResult { get; set; }
    }
}
