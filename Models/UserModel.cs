using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class UserModel
    {
        public int? UserID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]

        public string Password { get; set; }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        public string Address { get; set; }
        [Required(ErrorMessage = "One of is to be selected")]

        public bool IsActive { get; set; }
    }
    public class UserDropDownModel
    {

        public int UserID { get; set; }
        [Required]
        public string UserName { get; set; }
    }
    public class UserRegisterModel
    {
        public int? UserID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
    }
    public class UserLoginModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
