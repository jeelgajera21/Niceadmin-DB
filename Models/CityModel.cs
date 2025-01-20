using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NiceAdmin.Models
{
    public class CityModel
    {
        public int? CityID { get; set; }
        [Required]
        [DisplayName("City Name")]
        public string CityName { get; set; }
        [Required]
        [DisplayName("Country Name")]
        public int CountryID { get; set; }
        [Required]
        [DisplayName("State Name")]
        public int StateID { get; set; }
        [Required]
        [DisplayName("City Code")]
        public string CityCode { get; set; }
    }

    public class CountryDropDownModel
    {

        public int CountryID { get; set; }
        [Required]
        public string CountryName { get; set; }
    }

    public class StateDropDownModel
    {

        public int StateID { get; set; }
        [Required]
        public string StateName { get; set; }
    }
    public class CityDropDownModel
    {

        public int CityID { get; set; }
        [Required]
        public string CityName { get; set; }
    }
}
