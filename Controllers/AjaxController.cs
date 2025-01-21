using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers
{
    public class AjaxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult JSaveUser(UserModel user)
        {
            Console.WriteLine(user);
            return View("Index");
        }
    }
}
