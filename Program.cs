using Microsoft.EntityFrameworkCore;
using FormSimulatorApi.Data;
using FormSimulatorApi.Models;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Cast<string>()
    .ToArray();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (bool.TryParse(builder.Configuration["Database:ApplyMigrations"], out var applyMigrations) && applyMigrations)
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/forms", async (AppDbContext context, FormCreateRequest request) =>
{
    if (!IsValidFormData(request.Nome, request.Sobrenome, request.CidadeNascimento, request.DataNascimento))
    {
        return Results.BadRequest(new
        {
            message = "Nome, sobrenome, cidade de nascimento e uma data de nascimento valida sao obrigatorios."
        });
    }

    var formResponse = new FormResponse
    {
        Nome = request.Nome.Trim(),
        Sobrenome = request.Sobrenome.Trim(),
        DataNascimento = NormalizeBirthDate(request.DataNascimento),
        CidadeNascimento = request.CidadeNascimento.Trim(),
        CreatedAtUtc = DateTime.UtcNow
    };

    context.FormResponses.Add(formResponse);
    await context.SaveChangesAsync();
    return Results.Created($"/api/forms/{formResponse.Id}", new
    {
        formResponse.Id,
        formResponse.Nome,
        formResponse.Sobrenome,
        formResponse.DataNascimento,
        formResponse.CidadeNascimento,
        formResponse.CreatedAtUtc
    });
});

app.MapPost("/api/forms/check", async (
    AppDbContext context,
    FormCheckRequest request) =>
{
    if (!IsValidFormData(request.Nome, request.Sobrenome, request.CidadeNascimento, request.DataNascimento))
    {
        return Results.BadRequest(new
        {
            message = "Nome, sobrenome e cidade de nascimento sao obrigatorios."
        });
    }

    var dataInformada = NormalizeBirthDate(request.DataNascimento);
    var cadastrado = await context.FormResponses
        .AsNoTracking()
        .AnyAsync(response =>
            response.Nome == request.Nome.Trim() &&
            response.Sobrenome == request.Sobrenome.Trim() &&
            response.DataNascimento.Date == dataInformada &&
            response.CidadeNascimento == request.CidadeNascimento.Trim());

    return Results.Ok(new { cadastrado });
});

static bool IsValidFormData(
    string nome,
    string sobrenome,
    string cidadeNascimento,
    DateTime dataNascimento)
{
    return !string.IsNullOrWhiteSpace(nome) &&
        nome.Trim().Length <= 100 &&
        !string.IsNullOrWhiteSpace(sobrenome) &&
        sobrenome.Trim().Length <= 100 &&
        !string.IsNullOrWhiteSpace(cidadeNascimento) &&
        cidadeNascimento.Trim().Length <= 150 &&
        dataNascimento.Date <= DateTime.UtcNow.Date;
}

static DateTime NormalizeBirthDate(DateTime date)
{
    return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
}

app.Run();

public partial class Program
{
}