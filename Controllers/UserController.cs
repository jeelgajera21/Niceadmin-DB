using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers;

public class UserController : Controller
{
    private IConfiguration configuration;
    public UserController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserList()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_User_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        return View(table);
    }

   /* public IActionResult UserList()
    {
        // Get the current user's username from the session
        var userName = HttpContext.Session.GetString("UserName") ?? "Guest";

        // Check if the user is logged in (replace "Guest" if you want to treat guests differently)
        if (userName == "Guest")
        {
            // Handle cases where the user is not logged in, e.g., redirect to login or show an error
            return RedirectToAction("Login", "User"); // Assuming you have a login action
        }

        // Create the database connection and command
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_SelectAll"; // Assuming this stored procedure exists

                // Pass the UserName parameter to the stored procedure
                command.Parameters.AddWithValue("@UserName", userName);

                // Execute the query and load the results into a DataTable
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return View(table); // Pass the filtered data to the view
                }
            }
        }
    }
*/
    public IActionResult SaveUser(UserModel users)
    {
       

        if (ModelState.IsValid)
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
            return RedirectToAction("UserList");
        }

        return View("AddUserForm", users);
    }

    public IActionResult UserAddEdit(int UserID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        #region ProductByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_User_SelectByPK";
        command.Parameters.AddWithValue("@UserID", UserID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        UserModel userModel = new UserModel();

        foreach (DataRow dataRow in table.Rows)
        {
            userModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
            userModel.UserName = @dataRow["UserName"].ToString();
            userModel.Email = @dataRow["Email"].ToString();
            userModel.Password =@dataRow["Password"].ToString();
            userModel.MobileNo = @dataRow["MobileNo"].ToString();
            userModel.Address = @dataRow["Address"].ToString();
            userModel.IsActive = Convert.ToBoolean(@dataRow["IsActive"]);
        }

        #endregion

        return View("AddUserForm", userModel);
    }


    public IActionResult AddUserForm()
    {
        return View();
    }
    
    public IActionResult UserListToExcel()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_User_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("User");

            worksheet.Cells["A1"].LoadFromDataTable(table, true);

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Center-align all cells
            var range = worksheet.Cells[worksheet.Dimension.Address];
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Apply bold formatting to the first row (headers)
            var headerRange = worksheet.Cells[1, 1, 1, table.Columns.Count];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            var fileBytes = package.GetAsByteArray();


            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "User.xlsx");
        }
    }

    public IActionResult UserListToPDF()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            // Initialize the PDF document
            iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4);
            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;
            pdfDoc.Open();

            // Database connection and data retrieval
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PR_User_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("User Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(7); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f,2f });

                        // Add headers
                        pdfTable.AddCell("UserID");
                        pdfTable.AddCell("UserName");
                        pdfTable.AddCell("Email");
                        pdfTable.AddCell("Password");
                        pdfTable.AddCell("MobileNo");
                        pdfTable.AddCell("Address");
                        pdfTable.AddCell("IsActive");







                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["UserID"].ToString());
                            pdfTable.AddCell(row["UserName"].ToString());
                            pdfTable.AddCell(row["Email"].ToString());
                            pdfTable.AddCell(row["Password"].ToString());
                            pdfTable.AddCell(row["MobileNo"].ToString());
                            pdfTable.AddCell(row["Address"].ToString());
                            pdfTable.AddCell(row["IsActive"].ToString());

                        }

                        // Add the table to the PDF document
                        pdfDoc.Add(pdfTable);
                    }
                }
            }

            // Close the PDF document
            pdfDoc.Close();

            // Convert the PDF to a byte array and return it as a file
            byte[] bytes = stream.ToArray();
            stream.Close();

            return File(bytes, "application/pdf", "user_list.pdf");
        }
    }

    public IActionResult UserDelete(int id)
    {
        try
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_DeleteByPK";
            command.Parameters.AddWithValue("@UserID", id);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("UserList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("UserList");
        }
    }

    public IActionResult UserRegister(UserRegisterModel userRegisterModel)
    {
        try
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_User_Register";
                sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userRegisterModel.UserName;
                sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userRegisterModel.Password;
                sqlCommand.Parameters.Add("@Email", SqlDbType.VarChar).Value = userRegisterModel.Email;
                sqlCommand.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = userRegisterModel.MobileNo;
                sqlCommand.Parameters.Add("@Address", SqlDbType.VarChar).Value = userRegisterModel.Address;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("Login", "User");
            }
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return RedirectToAction("Register");
        }
        return RedirectToAction("Register");
    }
    public IActionResult Register()
    {
        return View();
    }
    public IActionResult UserLogin(UserLoginModel userLoginModel)
    {
        try
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_User_Login";
                sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userLoginModel.UserName;
                sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userLoginModel.Password;
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(sqlDataReader);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        HttpContext.Session.SetString("UserID", dr["UserID"].ToString());
                        HttpContext.Session.SetString("UserName", dr["UserName"].ToString());
                    }

                    return RedirectToAction("ProductList", "Product");
                }
                else
                {
                    return RedirectToAction("Login", "User");
                }

            }
        }
        catch (Exception e)
        {
            TempData["LoginErrorMessage"] = e.Message;
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "User");
    }
    public IActionResult Login()
    {
        return View();
    }
}
