using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage ="Username is Required!")]
        public string? UserName { get; set; }

        [Required(ErrorMessage ="Password is Required!")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name="Remember Me")]
        public bool RememberMe { get; set; }
    }
}
