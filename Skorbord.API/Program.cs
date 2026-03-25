using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add MVC services for web pages
builder.Services.AddControllersWithViews();

// Database configuration - SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost\\SQLEXPRESS;Database=SkorbordDB;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<SkorbordDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add custom services
builder.Services.AddScoped<StandingsService>();
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddScoped<DataProcessingService>();

// Add HttpClient for external API
builder.Services.AddHttpClient<ExternalApiService>();

// Add Background Service
builder.Services.AddHostedService<ScoreUpdateBackgroundService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Configure routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// Create database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SkorbordDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("Database created/connected successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
        Console.WriteLine("Application will continue without database. Please check your connection string.");
    }
}

app.Run();
