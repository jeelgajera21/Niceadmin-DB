using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class OrderModel
    {
        public int? OrderID  { get; set; }
        [Required]

        public string OrderNumber { get; set; }

        [Required]

        public DateTime OrderDate { get; set; }
        [Required]
        [DisplayName("Customer Name")]
        public int CustomerID { get; set; }
        [Required]
        public string PaymentMode { get; set; }
        [Required]
        public double TotalAmount { get; set; }
        [Required]
        public string ShippingAddress { get; set; }
        [Required]
        [DisplayName("Username")]
        public int UserID { get; set; }

    }
    public class OrderDropDownModel
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
    }

}
