using Evimsensin.Data;
using Evimsensin.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "evimsensin.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<AppService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    _ = scope.ServiceProvider.GetRequiredService<AppService>();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Required for user-uploaded files under wwwroot/img/uploads
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.Use(async (context, next) =>
{
    var hasRemember = context.Request.Cookies.TryGetValue("EvimsensinRemember", out var rememberedEmail) ||
                      context.Request.Cookies.TryGetValue("EvimsensinRemember", out rememberedEmail);

    if (!context.Session.GetInt32("UserId").HasValue &&
        hasRemember &&
        !string.IsNullOrWhiteSpace(rememberedEmail))
    {
        using var scope = context.RequestServices.CreateScope();
        var appService = scope.ServiceProvider.GetRequiredService<AppService>();
        var user = appService.GetUserByEmail(rememberedEmail);
        if (user is not null)
        {
            context.Session.SetInt32("UserId", user.Id);
            context.Session.SetString("UserName", user.FullName);
            context.Session.SetString("Role", user.Role.ToString());
        }
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
