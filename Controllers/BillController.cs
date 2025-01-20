using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;

using System.Text;

namespace NiceAdmin.Controllers;


public class BillController : Controller
{
    private IConfiguration configuration;
    public BillController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
    }




    public IActionResult BillList()
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
        string connectionString = configuration.GetConnectionString("ConnectionString");
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Bills_SelectByUser"; // Assuming this stored procedure exists

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

    public IActionResult AddBillForm()
    {
        ViewBag.OrderList = GetOrderDropDown();
        ViewBag.UserList = GetUserDropDown();
        return View();
    }
    public IActionResult BillListToExcel()
    {
        string connectionString = configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Bills_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        if (reader == null)
        {
            TempData["EmptyList"] = "Empty List";
        }


        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Bill");

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

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bill.xlsx");
        }
    }

    public IActionResult BillListToPDF()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            // Initialize the PDF document
            Document pdfDoc = new Document(PageSize.A4);
            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;
            pdfDoc.Open();

            // Database connection and data retrieval
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PR_Bills_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("Bill Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(8); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 1.5f });

                        // Add headers
                        pdfTable.AddCell("BillID");
                        pdfTable.AddCell("BillNumber");
                        pdfTable.AddCell("BillDate");
                        pdfTable.AddCell("OrderID");
                        pdfTable.AddCell("TotalAmount");
                        pdfTable.AddCell("Discount");
                        pdfTable.AddCell("NetAmount");
                        pdfTable.AddCell("UserID");



                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["BillID"].ToString());
                            pdfTable.AddCell(row["BillNumber"].ToString());
                            pdfTable.AddCell(row["BillDate"].ToString());
                            pdfTable.AddCell(row["OrderID"].ToString());
                            pdfTable.AddCell(row["TotalAmount"].ToString());
                            pdfTable.AddCell(row["Discount"].ToString());
                            pdfTable.AddCell(row["NetAmount"].ToString());
                            pdfTable.AddCell(row["UserID"].ToString());
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

            return File(bytes, "application/pdf", "bill_list.pdf");
        }
    }
    public List<UserDropDownModel> GetUserDropDown()
    {
        string connectionString = configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = CommandType.StoredProcedure;
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
    public List<OrderDropDownModel> GetOrderDropDown()
    {
        string connectionString = configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = CommandType.StoredProcedure;
        command1.CommandText = "PR_Order_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();
        foreach (DataRow data in dataTable1.Rows)
        {
            OrderDropDownModel orderDDModel = new OrderDropDownModel();
            orderDDModel.OrderNumber = data["OrderNumber"].ToString();
            orderDDModel.OrderID = Convert.ToInt32(data["OrderID"]);
            orderList.Add(orderDDModel);
        }
        //ViewBag.UserList = userList;
        return orderList;
    }
    public IActionResult SaveBill(BillModel bills)
    {
        if (bills.UserID <= 0)
        {
            ModelState.AddModelError("UserID", "A valid User is required.");
        }
        if (bills.OrderID <= 0)
        {
            ModelState.AddModelError("OrderID", "A valid Order is required.");
        }


        if (ModelState.IsValid)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            if (bills.BillID == null)
            {
                command.CommandText = "PR_Bills_Insert";
            }
            else
            {
                command.CommandText = "PR_Bills_UpdateByPK";
                command.Parameters.Add("@BillID", SqlDbType.Int).Value = bills.BillID;
            }
            command.Parameters.Add("@BillNumber", SqlDbType.VarChar).Value = bills.BillNumber;
            command.Parameters.Add("@BillDate", SqlDbType.DateTime).Value = bills.BillDate;
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = bills.OrderID;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = bills.TotalAmount;
            command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = bills.Discount;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = bills.NetAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = bills.UserID;

            command.ExecuteNonQuery();
            return RedirectToAction("BillList");
        }

        return View("AddBillForm", bills);
    }

    public IActionResult BillAddEdit(int BillID)
    {
        string connectionString = configuration.GetConnectionString("ConnectionString");



        ViewBag.UserList = GetUserDropDown();

        ViewBag.OrderList = GetOrderDropDown();

        #region BillByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Bills_SelectByPK";
        command.Parameters.AddWithValue("@BillID", BillID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        BillModel billModel = new BillModel();

        foreach (DataRow dataRow in table.Rows)
        {
            billModel.BillID = Convert.ToInt32(@dataRow["BillID"]);
            billModel.BillNumber = @dataRow["BillNumber"].ToString();
            billModel.BillDate = Convert.ToDateTime(@dataRow["BillDate"]);
            billModel.OrderID = Convert.ToInt32(@dataRow["OrderID"]);
            billModel.TotalAmount = Convert.ToDouble(@dataRow["TotalAmount"]);
            billModel.Discount = Convert.ToDouble(@dataRow["Discount"].ToString());
            billModel.NetAmount = Convert.ToDouble(@dataRow["NetAmount"]);
            billModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
        }

        #endregion

        return View("AddBillForm", billModel);
    }

    public IActionResult BillDelete(int BillID)
    {
        try
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Bills_DeleteByPK";
            command.Parameters.AddWithValue("@BillID", BillID);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("BillList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("BillList");
        }
    }
}
