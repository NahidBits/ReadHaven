using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ReadHaven;
using ReadHaven.Services;
using jsreport.AspNetCore;
using jsreport.Local;
using jsreport.Binary;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseIISIntegration();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });


builder.Services.AddJsReport(new LocalReporting()
    .UseBinary(JsReportBinary.GetBinary())
    .KillRunningJsReportProcesses()
    .Configure(cfg => cfg.BaseUrlAsWorkingDirectory())
    .AsUtility()
    .Create());

builder.Services.AddSession();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/NotFound");
    app.UseHsts();
}
app.UseExceptionHandler("/Error/NotFound"); 
app.UseStatusCodePagesWithReExecute("/Error/NotFound");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Book}/{action=Index}/{id?}");

app.Run();