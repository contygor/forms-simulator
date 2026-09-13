using Microsoft.EntityFrameworkCore;
using FormSimulatorApi.Data;
using FormSimulatorApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/forms", async (AppDbContext context, FormResponse formResponse) =>
{
    context.FormResponses.Add(formResponse);
    await context.SaveChangesAsync();
    return Results.Created($"/api/forms/{formResponse.Id}", formResponse);
});

app.Run();