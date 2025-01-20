namespace NiceAdmin.Models
{
    public class Employee
    {
        public int EID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email {  get; set; }
        public long PhoneNumber { get; set; }
        public string HireDate { get; set; }
        public string JobTitle { get; set; }
        public int DepartmentID { get; set; }
        public int Salary { get; set; }
    }
}
