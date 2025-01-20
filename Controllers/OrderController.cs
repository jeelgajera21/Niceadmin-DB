using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers;


public class OrderController : Controller
{
    private IConfiguration configuration;
    public OrderController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
    }
    #region OrderList
    /*public IActionResult()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Order_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        return View(table);
    }*/

    public IActionResult OrderList()
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
                command.CommandText = "PR_Order_SelectByUser"; // Assuming this stored procedure exists

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
    #endregion
    #region AddFrom
    public IActionResult AddOrderForm()
    {
        ViewBag.CustomerList = GetCustomerDropDown();
        ViewBag.UserList = GetUserDropDown();

        return View();
    }
    #endregion
    #region ListToExcel
    public IActionResult OrderListToExcel()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Order_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Order");

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

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Order.xlsx");
        }
    }
    #endregion
    #region ListToPF
    public IActionResult OrderListToPDF()
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
                using (SqlCommand command = new SqlCommand("PR_Order_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("Order Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(8); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f,2f,2f, 2f, 2f });

                        // Add headers
                        pdfTable.AddCell("OrderID");
                        pdfTable.AddCell("OrderNumber");
                        pdfTable.AddCell("OrderDate");
                        pdfTable.AddCell("CustomerName");
                        pdfTable.AddCell("PaymentMode");
                        pdfTable.AddCell("TotalAmount");
                        pdfTable.AddCell("ShippingAddress");
                        pdfTable.AddCell("UserName");






                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["OrderID"].ToString());
                            pdfTable.AddCell(row["OrderNumber"].ToString());
                            pdfTable.AddCell(row["OrderDate"].ToString());
                            pdfTable.AddCell(row["CustomerName"].ToString());
                            pdfTable.AddCell(row["PaymentMode"].ToString());
                            pdfTable.AddCell(row["TotalAmount"].ToString());
                            pdfTable.AddCell(row["ShippingAddress"].ToString());
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

            return File(bytes, "application/pdf", "order_list.pdf");
        }
    }
    #endregion
    #region DropDown
    public List<CustomerDropDownModel> GetCustomerDropDown()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
        command1.CommandText = "PR_Customer_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        List<CustomerDropDownModel> customerlist = new List<CustomerDropDownModel>();
        foreach (DataRow data in dataTable1.Rows)
        {
            CustomerDropDownModel CustDDModel = new CustomerDropDownModel();
            CustDDModel.CustomerName = data["CustomerName"].ToString();
            CustDDModel.CustomerID = Convert.ToInt32(data["CustomerID"]);
            customerlist.Add(CustDDModel);
        }
        //ViewBag.CustomerList = customerlist;
        return customerlist;
            
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
    #region SaveOrder

    public IActionResult SaveOrder(OrderModel orders)
    {
        if (orders.UserID <= 0)
        {
            ModelState.AddModelError("UserID", "A valid User is required.");
        }
        if (orders.CustomerID <= 0)
        {
            ModelState.AddModelError("Customer", "A valid Customer is required.");
        }


        if (ModelState.IsValid)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            if (orders.OrderID == null)
            {
                command.CommandText = "PR_Order_Insert";
            }
            else
            {
                command.CommandText = "PR_Order_UpdateByPK";
                command.Parameters.Add("@OrderID", SqlDbType.Int).Value = orders.OrderID;
            }
            command.Parameters.Add("@OrderNumber", SqlDbType.VarChar).Value = orders.OrderNumber;
            command.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = orders.OrderDate;
            command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = orders.CustomerID;
            command.Parameters.Add("@PaymentMode", SqlDbType.VarChar).Value = orders.PaymentMode;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = orders.TotalAmount;
            command.Parameters.Add("@ShippingAddress", SqlDbType.VarChar).Value = orders.ShippingAddress;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = orders.UserID;

            command.ExecuteNonQuery();
            return RedirectToAction("OrderList");
        }

        return View("AddOrderForm", orders);
    }
    #endregion
    #region AddEdit
    public IActionResult OrderAddEdit(int OrderID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");



        ViewBag.UserList = GetUserDropDown();

        ViewBag.CustomerList = GetCustomerDropDown();

        #region OrderByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Order_SelectByPK";
        command.Parameters.AddWithValue("@OrderID", OrderID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        OrderModel orderModel = new OrderModel();

        foreach (DataRow dataRow in table.Rows)
        {
            orderModel.OrderID = Convert.ToInt32(@dataRow["OrderID"]);
            orderModel.OrderNumber = @dataRow["OrderNumber"].ToString();
            orderModel.OrderDate = Convert.ToDateTime(@dataRow["OrderDate"]);
            orderModel.CustomerID = Convert.ToInt32(@dataRow["CustomerID"]);
            orderModel.PaymentMode = @dataRow["PaymentMode"].ToString();
            orderModel.TotalAmount = Convert.ToDouble(@dataRow["TotalAmount"]);
            orderModel.ShippingAddress =@dataRow["ShippingAddress"].ToString();
            orderModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
        }

        #endregion

        return View("AddOrderForm", orderModel);
    }
    #endregion
    #region Delete
    public IActionResult OrderDelete(int OrderID)
    {
        try
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Order_DeleteByPK";
            command.Parameters.AddWithValue("@OrderID", OrderID);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("OrderList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("OrderList");
        }
    }
    #endregion

}
