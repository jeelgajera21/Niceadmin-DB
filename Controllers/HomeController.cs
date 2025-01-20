using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Diagnostics;

namespace NiceAdmin.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public static List<Employee> employees = new List<Employee>
    {
        new Employee{FirstName="ACB",LastName="DEF",Email="abc@gmail.com",PhoneNumber=1234567890,HireDate="12-12-24",JobTitle="Sr. Developer",Salary=23000,DepartmentID=12}
    };

    public IActionResult SaveEmployee(Employee Emp)
    {
        if (Emp.EID == 0)
        {
            Emp.EID=employees.Max(x => x.EID)+1;
            employees.Add(Emp);
        }
        return View("Employee",employees);
    }

/*
    public static List<ProjectModel> project = new List<ProjectModel>
    {
        new ProjectModel{ProjectID=1,ProjectName="ACB",StartDate="01-01-24",EndDate="01-01-24",Budget=23000}
    };*/
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Employee()
    {
        return View(employees);
    }
    public IActionResult Project()
    {
        return View();
    }
    public IActionResult EmployeeProject()
    {
        return View();
    }
    public IActionResult Department()
    {
        return View();
    }
    public IActionResult AddEmployee(int EID =0)
    {
        NiceAdmin.Models.Employee Eobj = new NiceAdmin.Models.Employee();
        
        if(EID != 0)
        {
            var SelectEmployee = employees.Find(x => x.EID == EID);

            Eobj.EID = SelectEmployee.EID;
            Eobj.FirstName = SelectEmployee.FirstName;
            Eobj.LastName = SelectEmployee.LastName;
            Eobj.Email = SelectEmployee.Email;
            Eobj.Salary = SelectEmployee.Salary;
        }
        return View();
    }
    public IActionResult AddProject()
    {
        return View();
    }
    public IActionResult AddDepartment()
    {
        return View();
    }
    public IActionResult AddEmployeeProject()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
