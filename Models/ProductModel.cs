using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace NiceAdmin.Models
{
    public class ProductModel
    {
        public int? ProductID { get; set; }
        [Required(ErrorMessage ="Name is not valid")]
        [StringLength(200)]
        public string ProductName { get; set; }
        [Required]
        [StringLength(250,ErrorMessage ="Descriptioin must be less than 250 words")]
        public string Description { get; set; }
        [Required]
        [Range(20,100000)]
        public double ProductPrice { get; set; }
        [Required]
        [StringLength(15,ErrorMessage ="Enter proper product code")]
        public string ProductCode { get; set; }
        [Required]
        [DisplayName("Username")]
        public int UserID { get; set; }
    }
    public class ProductDropDownModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
    }

}
