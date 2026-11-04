using Microsoft.EntityFrameworkCore;
using MovieTracker.Configuration;
using MovieTracker.Data;
using MovieTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC: Controllers + Views.
builder.Services.AddControllersWithViews();

// SQLite база данни.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Design Pattern: Singleton.
builder.Services.AddSingleton<AppSettings>();

// Настройки за външните API услуги (Options Pattern).
builder.Services.Configure<ExternalApisOptions>(
    builder.Configuration.GetSection(ExternalApisOptions.SectionName));

// HTTP клиент за комуникация с Gemini.
builder.Services.AddHttpClient<IGeminiQueryService, GeminiQueryService>();
builder.Services.AddScoped<JsonQuerySqlTranslator>();
builder.Services.AddScoped<IMovieQueryService, MovieQueryService>();
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
