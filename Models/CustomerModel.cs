using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class CustomerModel
    {
        public int? CustomerID { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        [StringLength(250)]
        public string HomeAddress { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        
        public string GSTNO { get; set; }
        [Required]

        public string CityName { get; set; }
        [Required]

        public double NetAmount { get; set; }
        [Required]
        [StringLength(6,ErrorMessage ="Pincode must be of 6 letter ")]
        public string PinCode { get; set; }
        
        [Required(ErrorMessage = "Select any one of the User")]

        public int UserID { get; set; }
    }
    public class CustomerDropDownModel
    {
        public int CustomerID { get; set; }
        [Required(ErrorMessage ="Select any one of the customer")]
        public string CustomerName { get; set; }
    }
}
