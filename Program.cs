using NiceAdmin.Models;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services to the container
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Data Source=MASCOT\\SQLEXPRESS;Initial Catalog=CSMS;Integrated Security=true")); // or another storage provider

builder.Services.AddHangfireServer();  // Add Hangfire background job server



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

builder.Services.AddControllersWithViews();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}



app.UseHangfireDashboard();// 🔹 Hangfire Dashboard to monitor jobs
app.UseHangfireServer();     // 🔹 Enables Hangfire job execution

// Enable session middleware
app.UseSession(); // Must be added before app.MapControllerRoute

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// enables the authentication middleware in ASP.NET Core to handle user authentication for securing endpoints.​
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DashBoard}/{action=Index}/{id?}");

app.Run();

