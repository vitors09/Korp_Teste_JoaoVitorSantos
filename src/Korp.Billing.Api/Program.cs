using Korp.Billing.Api.Application;
using Korp.Billing.Api.ErrorHandling;
using Korp.Billing.Api.Infrastructure;
using Korp.Billing.Api.Integration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BillingDatabase")));

builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:Inventory"]
        ?? throw new InvalidOperationException("O endereço do serviço de estoque não foi configurado."));
    client.Timeout = TimeSpan.FromSeconds(3);
});

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

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    await database.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program;
