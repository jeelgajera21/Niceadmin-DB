using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers;


public class CustomerController : Controller
{
    private IConfiguration configuration;
    public CustomerController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
    }


    /*  public IActionResult CustomerList()
      {
          string connectionString = this.configuration.GetConnectionString("ConnectionString");
          SqlConnection connection = new SqlConnection(connectionString);
          connection.Open();
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.StoredProcedure;
          command.CommandText = "PR_Customer_SelectAll";
          SqlDataReader reader = command.ExecuteReader();
          DataTable table = new DataTable();
          table.Load(reader);
          return View(table);
      }*/

    public IActionResult CustomerList()
    {
        // Get the current user's UserID from the session
        var userName = HttpContext.Session.GetString("UserName") ?? "Guest";

        // Check if the user is logged in (if UserID is null, treat the user as not logged in)
        if (userName == null)
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
                command.CommandText = "PR_Customer_SelectByUser"; // Assuming this stored procedure exists

                // Pass the UserID parameter to the stored procedure
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
    #region AddForm
    public IActionResult AddCustomerForm()
    {
        ViewBag.UserList = GetUserDropDown();
        return View();
    }
    #endregion
    #region ListToExcel
    public IActionResult CustomerListToExcel()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Customer_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Customer");

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

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customer.xlsx");
        }
    }
    #endregion
    #region ListToPDF
    public IActionResult CustomerListToPDF()
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
                using (SqlCommand command = new SqlCommand("PR_Customer_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("Customer Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(10); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f,2f });

                        var headerFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD);
                        var headerBackgroundColor = new iTextSharp.text.BaseColor(200, 200, 200); // Light gray color

                        // Add headers with styling
                        PdfPCell cell = new PdfPCell();

                        cell = new PdfPCell(new Phrase("CustomerID", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("CustomerName", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("HomeAddress", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Email", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("MobileNo", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("GSTNO", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("CityName", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("NetAmount", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("PinCode", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell(new Phrase("UserName", headerFont));
                        cell.BackgroundColor = headerBackgroundColor;
                        pdfTable.AddCell(cell);





                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["CustomerID"].ToString());
                            pdfTable.AddCell(row["CustomerName"].ToString());
                            pdfTable.AddCell(row["HomeAddress"].ToString());
                            pdfTable.AddCell(row["Email"].ToString());
                            pdfTable.AddCell(row["MobileNo"].ToString());
                            pdfTable.AddCell(row["GST_NO"].ToString());
                            pdfTable.AddCell(row["CityName"].ToString());
                            pdfTable.AddCell(row["NetAmount"].ToString());
                            pdfTable.AddCell(row["PinCode"].ToString());
                            pdfTable.AddCell(row["UserName"].ToString());


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

            return File(bytes, "application/pdf", "customer_list.pdf");
        }
    }

    public List<UserDropDownModel> GetUserDropDown()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
        command1.CommandText = "PR_User_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        List<UserDropDownModel> userList = new List<UserDropDownModel>();
        foreach (DataRow data in dataTable1.Rows)
        {
            UserDropDownModel userDDModel = new UserDropDownModel();
            userDDModel.UserName = data["UserName"].ToString();
            userDDModel.UserID = Convert.ToInt32(data["UserID"]);
            userList.Add(userDDModel);
        }
        //ViewBag.UserList = userList;
        return userList;
    }
    #endregion
    #region SaveCustomer
    public IActionResult SaveCustomer(CustomerModel customers)
    {
        if (customers.UserID <= 0)
        {
            ModelState.AddModelError("UserID", "A valid User is required.");
        }
       


        if (ModelState.IsValid)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            if (customers.CustomerID == null)
            {
                command.CommandText = "PR_Customer_Insert";
            }
            else
            {
                command.CommandText = "PR_Customer_UpdateByPK";
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = customers.CustomerID;
            }
            command.Parameters.Add("@CustomerName", SqlDbType.VarChar).Value = customers.CustomerName;
            command.Parameters.Add("@HomeAddress", SqlDbType.VarChar).Value = customers.HomeAddress;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = customers.Email;
            command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = customers.MobileNo;
            command.Parameters.Add("@GST_NO", SqlDbType.VarChar).Value = customers.GSTNO;
            command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = customers.CityName;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = customers.NetAmount;
            command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = customers.PinCode;


            command.Parameters.Add("@UserID", SqlDbType.Int).Value = customers.UserID;

            command.ExecuteNonQuery();
            return RedirectToAction("CustomerList");
        }

        return View("AddCustomerForm", customers);
    }
    #endregion
    #region AddEdit
    public IActionResult CustomerAddEdit(int CustomerID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");



        ViewBag.UserList = GetUserDropDown();


        #region CustomerByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Customer_SelectByPK";
        command.Parameters.AddWithValue("@CustomerID", CustomerID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        CustomerModel customerModel = new CustomerModel();

        foreach (DataRow dataRow in table.Rows)
        {
            customerModel.CustomerID = Convert.ToInt32(@dataRow["CustomerID"]);
            customerModel.CustomerName = @dataRow["CustomerName"].ToString();
            customerModel.HomeAddress = @dataRow["HomeAddress"].ToString();
            customerModel.Email = @dataRow["Email"].ToString();
            customerModel.MobileNo= @dataRow["MobileNo"].ToString();
            customerModel.GSTNO = @dataRow["GST_NO"].ToString();
            customerModel.CityName= @dataRow["CityName"].ToString();
            customerModel.NetAmount = Convert.ToDouble(@dataRow["NetAmount"]);
            customerModel.PinCode = @dataRow["PinCode"].ToString();

            customerModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
        }

        #endregion

        return View("AddCustomerForm", customerModel);
    }
    #endregion
    #region Delete
    public IActionResult CustomerDelete(int CustID)
    {
        try
        {
            TempData["id"] = CustID;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Customer_DeleteByPK";
            command.Parameters.AddWithValue("@CustomerID", CustID);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("CustomerList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("CustomerList");
        }
        }
}
#endregion
