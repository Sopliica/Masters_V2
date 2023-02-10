using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJudge.Consts;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;
using OnlineJudge.Services;
using Serilog;
using System.Security.Cryptography;

Log.Logger = new LoggerConfiguration()
.WriteTo
.File("../logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 21)
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
        var user = new User("admin_ath@localhost", Roles.Administrator);

        var passphrase = "";

        if (app.Environment.IsDevelopment())
        {
            passphrase = "12345";
        }
        else
        {
            var count = new Random().Next(30, 50);
            var bytes = RandomNumberGenerator.GetBytes(15);
            passphrase = Convert.ToBase64String(bytes);
        }

        File.WriteAllText("../admin_passphrase.txt", passphrase);
        user.PasswordHash = hasher.HashPassword(user, passphrase);
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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
