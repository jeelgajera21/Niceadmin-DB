using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
namespace NiceAdmin.Models
{
    public class ContactModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string EnrollmentNo { get; set; }

        /*[Required]
        public string Description { get; set; }*/
    }

    public class EmailModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        /*[Required]
        public string Description { get; set; }*/
    }


}
