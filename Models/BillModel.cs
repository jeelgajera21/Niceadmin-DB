using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace NiceAdmin.Models
{
    public class BillModel
    {
        public int? BillID { get; set; }
        [Required]
        public string BillNumber { get; set; }
        [Required]
        public DateTime BillDate { get; set; }
        [Required]

        public int OrderID { get; set; }
        [Required(ErrorMessage = "Total amount must be less than 1 lakh")]

        public double TotalAmount { get; set; }
        [Required]
        public double Discount { get; set; }
        [Required]
        public double NetAmount { get; set; }

        [Required]
        [DisplayName("Username")]
        public int UserID { get; set; }
    }
}
