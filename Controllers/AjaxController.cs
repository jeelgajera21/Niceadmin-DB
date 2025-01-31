using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Data.SqlClient;
using System.Data;

namespace NiceAdmin.Controllers
{
    public class AjaxController : Controller
    {
        private IConfiguration configuration;
        public AjaxController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult Index()
        {

            return View();
        }
        
            [HttpGet]
            public IActionResult GetUsers()
            {
                List<UserModel> userList = new List<UserModel>();

                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT * FROM [User]", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userList.Add(new UserModel
                                {
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    UserName = reader["UserName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    MobileNo = reader["MobileNo"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                });
                            }
                        }
                    }
                }

                return Json(userList); // Return JSON response
            }

           
        [HttpPost]
        public IActionResult JSaveUser(UserModel users)
        {
            /*if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (users.UserID == null)
                {
                    command.CommandText = "PR_User_Insert";
                }
                else
                {
                    command.CommandText = "PR_User_UpdateByPK";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = users.UserID;
                }
                command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = users.UserName;
                command.Parameters.Add("@Email", SqlDbType.VarChar).Value = users.Email;
                command.Parameters.Add("@Password", SqlDbType.VarChar).Value = users.Password;
                command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = users.MobileNo;
                command.Parameters.Add("@Address", SqlDbType.VarChar).Value = users.Address;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = users.IsActive;

                command.ExecuteNonQuery();
                return RedirectToAction("Index");
            Console.WriteLine(users);
            }
*/
            Console.WriteLine(users);
            return View();
        }
    }
}
