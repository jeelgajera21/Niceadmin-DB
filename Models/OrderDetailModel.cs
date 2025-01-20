using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class OrderDetailModel
    {
        public int? OrderDetailID { get; set; }
        [Required]
        [DisplayName("Order")]
        public int OrderID { get; set; }
        [Required]
        [DisplayName("Product")]
        public int ProductID { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]

        public double Amount { get; set; }
        [Required]

        public double TotalAmount { get; set; }
        [Required]
        [DisplayName("Username")]
        public int UserID { get; set; }
    }
}
