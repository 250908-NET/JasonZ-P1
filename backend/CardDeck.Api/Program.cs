using CardDeck.Api.Endpoints;
using CardDeck.Api.Middleware;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;
using CardDeck.Api.Services;
using DotNetEnv;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

Env.Load(); // load .env file

var builder = WebApplication.CreateBuilder(args);

// register DbContext
// this code runs before Moq is able to intercept it in tests, so it needs to be skipped when in testing environment
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<CardDeckContext>(options =>
    {
        // load connection string from environment variable
        string? connectionString = builder.Configuration.GetValue<string>("DB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DB_CONNECTION_STRING' not found in configuration."
            );
        }

        options.UseSqlServer(connectionString);
    });
}

// register Fluent validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();

builder.Services.AddScoped<ISuitRepository, SuitRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IAvailableCardRepository, AvailableCardRepository>();

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ISuitService, SuitService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IAvailableCardService, AvailableCardService>();

// configure logger
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger(); // read from appsettings.json
builder.Host.UseSerilog();

var app = builder.Build();

app.UseGlobalExceptionHandler();

// configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// map endpoints
app.MapStatusEndpoints();
app.MapSuitEndpoints();
app.MapCardEndpoints();
app.MapDeckEndpoints();

// disable start message when in testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    Log.Information("Application starting...");
}

app.Run();

public partial class Program { } // for integration testing
