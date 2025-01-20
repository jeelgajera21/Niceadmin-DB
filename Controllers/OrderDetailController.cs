using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers;

public class OrderDetailController : Controller
{
    private IConfiguration configuration;
    public OrderDetailController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
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
    public List<OrderDropDownModel> GetOrderDropDown()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
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
    public List<ProductDropDownModel> GetProductDropDown()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
        command1.CommandText = "PR_Product_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        List<ProductDropDownModel> productList = new List<ProductDropDownModel>();
        foreach (DataRow data in dataTable1.Rows)
        {
            ProductDropDownModel productDDModel = new ProductDropDownModel();
            productDDModel.ProductName = data["ProductName"].ToString();
            productDDModel.ProductID = Convert.ToInt32(data["ProductID"]);
            productList.Add(productDDModel);
        }
        //ViewBag.UserList = userList;
        return productList;
    }
    #region OrderList
    /*public IActionResult OrderDetailList()
     {
         string connectionString = this.configuration.GetConnectionString("ConnectionString");
         SqlConnection connection = new SqlConnection(connectionString);
         connection.Open();
         SqlCommand command = connection.CreateCommand();
         command.CommandType = CommandType.StoredProcedure;
         command.CommandText = "PR_OrderDetail_SelectAll";
         SqlDataReader reader = command.ExecuteReader();
         DataTable table = new DataTable();
         table.Load(reader);
         return View(table);
     }*/

    public IActionResult OrderDetailList()
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
                command.CommandText = "PR_OrderDetail_SelectByUser"; // Assuming this stored procedure exists

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
    public IActionResult SaveOrderDetail(OrderDetailModel orderdetails)
    {
        if (orderdetails.UserID <= 0)
        {
            ModelState.AddModelError("UserID", "A valid User is required.");
        }
        if (orderdetails.OrderID <= 0)
        {
            ModelState.AddModelError("OrderID", "A valid Order is required.");
        }
        if (orderdetails.ProductID <= 0)
        {
            ModelState.AddModelError("ProductID", "A valid Product is required.");
        }

        if (ModelState.IsValid)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            if (orderdetails.OrderDetailID == null)
            {
                command.CommandText = "PR_OrderDetail_Insert";
            }
            else
            {
                command.CommandText = "PR_OrderDetail_UpdateByPK";
                command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = orderdetails.OrderDetailID;
            }
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderdetails.OrderID;
            command.Parameters.Add("@ProductID", SqlDbType.Int).Value = orderdetails.ProductID;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = orderdetails.Quantity;
            command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = orderdetails.Amount;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = orderdetails.TotalAmount;

            command.Parameters.Add("@UserID", SqlDbType.Int).Value = orderdetails.UserID;
            command.ExecuteNonQuery();
            return RedirectToAction("OrderDetailList");
        }

        return View("AddOrderDetailForm", orderdetails);
    }

    public IActionResult OrderDetailAddEdit(int ODID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        ViewBag.UserList = GetUserDropDown();

        ViewBag.OrderList = GetOrderDropDown();

        ViewBag.ProductList = GetProductDropDown();

        #region ODByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_OrderDetail_SelectByPK";
        command.Parameters.AddWithValue("@OrderDetailID", ODID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        OrderDetailModel orderdetailModel = new OrderDetailModel();

        foreach (DataRow dataRow in table.Rows)
        {
            //orderdetailModel.OrderDetailID = Convert.ToInt32(@dataRow["OrderDetailID"]);
            orderdetailModel.OrderID =Convert.ToInt32(@dataRow["OrderID"]);
            orderdetailModel.ProductID = Convert.ToInt32(@dataRow["ProductID"]);
            orderdetailModel.Quantity = Convert.ToInt32(@dataRow["Quantity"]);
            orderdetailModel.Amount = Convert.ToDouble(@dataRow["Amount"]);
            orderdetailModel.TotalAmount = Convert.ToDouble(@dataRow["TotalAmount"]);
            orderdetailModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
        }

        #endregion

        return View("AddOrderDetailForm", orderdetailModel);
    }

    public IActionResult AddOrderDetailForm()
    {
        ViewBag.OrderList = GetOrderDropDown();
        ViewBag.UserList = GetUserDropDown();
        ViewBag.ProductList = GetProductDropDown();

        return View();
    }

    public IActionResult OrderDetailListToExcel()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_OrderDetail_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("OrderDetail");

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

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderDetail.xlsx");
        }
    }
    public IActionResult OrderDetailListToPDF()
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
                using (SqlCommand command = new SqlCommand("PR_OrderDetail_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("Order Detail Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(7); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f,2f });

                        // Add headers
                        pdfTable.AddCell("OrderDetailID");
                        pdfTable.AddCell("OrderID");
                        pdfTable.AddCell("ProductID");
                        pdfTable.AddCell("Quantity");
                        pdfTable.AddCell("Amount");
                        pdfTable.AddCell("TotalAmount");
                        pdfTable.AddCell("UserName");






                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["OrderDetailID"].ToString());
                            pdfTable.AddCell(row["OrderID"].ToString());
                            pdfTable.AddCell(row["ProductID"].ToString());
                            pdfTable.AddCell(row["Quantity"].ToString());
                            pdfTable.AddCell(row["Amount"].ToString());
                            pdfTable.AddCell(row["TotalAmount"].ToString());
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

            return File(bytes, "application/pdf", "orderdetail_list.pdf");
        }
    }
    public IActionResult OrderDetailDelete(int ODID)
    {
        try
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_OrderDetail_DeleteByPK";
            command.Parameters.AddWithValue("@OrderDetailID", ODID);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("OrderDetailList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("OrderDetailList");
        }
    }
}
