
using Microsoft.EntityFrameworkCore;
using MovieTracker.Data;
using MovieTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC: Controllers + Views.
builder.Services.AddControllersWithViews();

// Регистрираме SQLite контекста в Dependency Injection.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Design Pattern: Singleton.
// DI контейнерът ще предоставя един екземпляр на AppSettings.
builder.Services.AddSingleton<AppSettings>();

var app = builder.Build();

// Конфигурация на HTTP pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Начален маршрут към списъка с филми.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.Run();
