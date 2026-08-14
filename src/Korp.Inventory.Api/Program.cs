using Korp.Inventory.Api.Application;
using Korp.Inventory.Api.ErrorHandling;
using Korp.Inventory.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("InventoryDatabase")));

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<FailureSimulationState>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevelopment", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AngularDevelopment");
app.MapControllers();

await ApplyMigrationsAsync(app);

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await database.Database.EnsureCreatedAsync();
}

public partial class Program;
