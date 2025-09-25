using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;
using CardDeck.Api.Services;
using DotNetEnv;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

Env.Load(); // load .env file

var builder = WebApplication.CreateBuilder(args);

// register DbContext
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

// register Fluent validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISuitRepository, SuitRepository>();

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ISuitService, SuitService>();

// configure logger
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger(); // read from appsettings.json
builder.Host.UseSerilog();

var app = builder.Build();

// configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// -- status ---

app.MapGet(
        "/status",
        async (IStatusService service) =>
        {
            return Results.Ok(await service.CheckConnectionAsync());
        }
    )
    .WithName("GetStatus")
    .WithTags("Status");

// -- suits ---

app.MapGet(
        "/suits",
        async (ISuitService service) =>
        {
            return Results.Ok(await service.GetAllSuitsAsync());
        }
    )
    .WithName("GetAllSuits")
    .WithTags("Suits");

app.MapGet(
        "/suits/{suitId:int}",
        async (int suitId, ISuitService service) =>
        {
            var suit = await service.GetSuitByIdAsync(suitId);
            return suit == null ? Results.NotFound() : Results.Ok(suit);
        }
    )
    .WithName("GetSuitById")
    .WithTags("Suits");

app.MapPost(
        "/suits",
        async (CreateSuitDTO newSuit, ISuitService service) =>
        {
            var createdSuit = await service.CreateSuitAsync(newSuit);
            return Results.Created($"/suits/{createdSuit.Id}", createdSuit);
        }
    )
    .WithName("CreateSuit")
    .WithTags("Suits");

app.MapPut(
        "/suits/{suitId:int}",
        async (int suitId, UpdateSuitDTO updateSuit, ISuitService service) =>
        {
            var success = await service.UpdateSuitAsync(suitId, updateSuit);
            return success ? Results.NoContent() : Results.NotFound();
        }
    )
    .WithName("UpdateSuit")
    .WithTags("Suits");

app.MapDelete(
        "/suits/{suitId:int}",
        async (int suitId, ISuitService service) =>
        {
            var success = await service.DeleteSuitAsync(suitId);
            return success ? Results.NoContent() : Results.NotFound();
        }
    )
    .WithName("DeleteSuit")
    .WithTags("Suits");

// disable start message when in testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    Log.Information("Application starting...");
}

app.Run();

public partial class Program { } // for integration testing
