using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJudge.Consts;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;
using OnlineJudge.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
.WriteTo
.File("logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 21)
.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddDbContext<Context>(options => options.UseSqlite("filename=../db.db"));
builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<CodeService>();
builder.Services.AddTransient<ICodeExecutorService, GodboltCodeExecutor>();
builder.Services.AddHostedService<BackgroundExecutorService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddRazorRuntimeCompilation();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/";
    options.LoginPath = "/Account/SignIn/";
});

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<Context>();
    var hasher = serviceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var user = new User("admin@localhost", Roles.Administrator);
        var pw = 12345; // new Random().Next(100_000, 999_999);
        File.WriteAllText("admin_passphrase.txt", pw.ToString());
        user.PasswordHash = hasher.HashPassword(user, pw.ToString());
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
