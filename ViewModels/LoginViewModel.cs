using System.ComponentModel.DataAnnotations;

namespace User_Management.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // Add RememberMe property
        //[Display(Name = "Remember Me?")]
        //public bool RememberMe { get; set; }
    }
}
