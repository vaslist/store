using Store;
using Store.Contractors;
using Store.Messages;
using Store.Web.App;
using Store.Data.EF;
using Store.Web.Contractors;
using Store.YandexKassa;
using System.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Store.Web;

var builder = WebApplication.CreateBuilder(args);


// AddItem services to the container.
builder.Services.AddControllersWithViews(options=>
{
    options.Filters.Add(typeof(ExceptionFilter));
});
builder.Services.AddHttpContextAccessor();

// получаем строку подключения из файла конфигурации
string connectionString = builder.Configuration.GetConnectionString("Store");
builder.Services.AddEfRepositories(connectionString);

builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<IDeliveryService, PostamateDeliveryService>();
builder.Services.AddSingleton<IPaymentService, CashPaymentService>();
builder.Services.AddSingleton<IPaymentService, YandexKassaPaymentService>();
builder.Services.AddSingleton<IWebContractorService, YandexKassaPaymentService>();
builder.Services.AddSingleton<INotificationService, DebugNotificationService>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
