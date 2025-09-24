using CardDeck.Api.Models;
using CardDeck.Api.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;

Env.Load(); // load .env file

var builder = WebApplication.CreateBuilder(args);

// load connection string from environment variable
string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found in environment variables.");
}

// register DbContext 
builder.Services.AddDbContext<CardDeckContext>(options =>
    options.UseSqlServer(connectionString));

// add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CardDeckContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IStatusService, StatusService>();

// configure logger
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger(); // read from appsettings.json
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/status", async (IStatusService service) =>
{
    return Results.Ok(await service.CheckConnectionAsync());
})
.WithName("GetStatus")
.WithTags("Status");

Log.Information("Application starting...");

app.Run();
