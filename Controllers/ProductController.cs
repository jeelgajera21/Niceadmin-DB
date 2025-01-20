using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers;


public class ProductController : Controller
{
    private IConfiguration configuration;
    public ProductController(IConfiguration _configuration)
    {
        configuration = _configuration;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ProductList()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Product_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        return View(table);
    }
    
    public IActionResult AddProductForm()
    {
        
        ViewBag.UserList = GetUserDropDown();
        return View();
    }
    
    public IActionResult SaveProduct(ProductModel products)
    {
        if (products.UserID <= 0)
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
            if (products.ProductID == null)
            {
                command.CommandText = "PR_Product_Insert";
            }
            else
            {
                command.CommandText = "PR_Product_UpdateByPK";
                command.Parameters.Add("@ProductID", SqlDbType.Int).Value = products.ProductID;
            }
            command.Parameters.Add("@ProductName", SqlDbType.VarChar).Value = products.ProductName;
            command.Parameters.Add("@ProductCode", SqlDbType.VarChar).Value = products.ProductCode;
            command.Parameters.Add("@ProductPrice", SqlDbType.Decimal).Value = products.ProductPrice;
            command.Parameters.Add("@Description", SqlDbType.VarChar).Value = products.Description;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = products.UserID;
            command.ExecuteNonQuery();
            return RedirectToAction("ProductList");
        }

        return View("AddProductForm", products);
    }
    
    public IActionResult ProductAddEdit(int ProductID)
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");

        #region User Drop-Down

        SqlConnection connection1 = new SqlConnection(connectionString);
        connection1.Open();
        SqlCommand command1 = connection1.CreateCommand();
        command1.CommandType = System.Data.CommandType.StoredProcedure;
        command1.CommandText = "PR_User_DropDown";
        SqlDataReader reader1 = command1.ExecuteReader();
        DataTable dataTable1 = new DataTable();
        dataTable1.Load(reader1);
        connection1.Close();

        List<UserDropDownModel> users = new List<UserDropDownModel>();

        foreach (DataRow dataRow in dataTable1.Rows)
        {
            UserDropDownModel userDropDownModel = new UserDropDownModel();
            userDropDownModel.UserID = Convert.ToInt32(dataRow["UserID"]);
            userDropDownModel.UserName = dataRow["UserName"].ToString();
            users.Add(userDropDownModel);
        }

        ViewBag.UserList = users;

        #endregion

        #region ProductByID

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Product_SelectByPK";
        command.Parameters.AddWithValue("@ProductID", ProductID);
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);
        ProductModel productModel = new ProductModel();

        foreach (DataRow dataRow in table.Rows)
        {
            productModel.ProductID = Convert.ToInt32(@dataRow["ProductID"]);
            productModel.ProductName = @dataRow["ProductName"].ToString();
            productModel.ProductCode = @dataRow["ProductCode"].ToString();
            productModel.ProductPrice = Convert.ToDouble(@dataRow["ProductPrice"]);
            productModel.Description = @dataRow["Description"].ToString();
            productModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
        }

        #endregion

        return View("AddProductForm", productModel);
    }

    public IActionResult ProductListToExcel()
    {
        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        SqlCommand command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "PR_Product_SelectAll";
        SqlDataReader reader = command.ExecuteReader();
        DataTable table = new DataTable();
        table.Load(reader);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Product");

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

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Product.xlsx");
        }
    }

    public IActionResult ProductListToPDF()
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
                using (SqlCommand command = new SqlCommand("PR_Product_SelectAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Add title to the PDF
                        pdfDoc.Add(new Paragraph("Product Data"));
                        pdfDoc.Add(new Paragraph("\n"));

                        // Create a table for the PDF
                        PdfPTable pdfTable = new PdfPTable(6); // Number of columns
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f });

                        // Add headers
                        pdfTable.AddCell("ProductID");
                        pdfTable.AddCell("ProductName");
                        pdfTable.AddCell("Description");
                        pdfTable.AddCell("ProductPrice");
                        pdfTable.AddCell("ProductCode");
                        pdfTable.AddCell("UserName");
                      





                        // Add data rows from the DataTable
                        foreach (DataRow row in dataTable.Rows)
                        {
                            pdfTable.AddCell(row["ProductID"].ToString());
                            pdfTable.AddCell(row["ProductName"].ToString());
                            pdfTable.AddCell(row["Description"].ToString());
                            pdfTable.AddCell(row["ProductPrice"].ToString());
                            pdfTable.AddCell(row["ProductCode"].ToString());
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

            return File(bytes, "application/pdf", "product_list.pdf");
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
    
    public IActionResult ProductDelete(int id)
    {
        try
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Product_DeleteByPK";
            command.Parameters.AddWithValue("@ProductID", id);
            command.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("ProductList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex);

            return RedirectToAction("ProductList");
        }
        }

}
