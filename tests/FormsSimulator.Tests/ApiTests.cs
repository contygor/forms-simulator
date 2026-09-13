using System.Net;
using System.Net.Http.Json;
using FormSimulatorApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FormsSimulator.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public ApiTests(ApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Create_then_check_returns_registered_true()
    {
        var createResponse = await client.PostAsJsonAsync("/api/forms", new
        {
            nome = "Ana",
            sobrenome = "Silva",
            dataNascimento = new DateTime(1990, 5, 20),
            cidadeNascimento = "Recife"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var checkResponse = await client.PostAsJsonAsync("/api/forms/check", new
        {
            nome = " Ana ",
            sobrenome = "Silva",
            dataNascimento = new DateTime(1990, 5, 20),
            cidadeNascimento = "Recife"
        });

        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);
        Assert.Equal("{\"cadastrado\":true}", await checkResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Check_for_unknown_person_returns_registered_false()
    {
        var response = await client.PostAsJsonAsync("/api/forms/check", new
        {
            nome = "Pessoa",
            sobrenome = "Inexistente",
            dataNascimento = new DateTime(2000, 1, 1),
            cidadeNascimento = "Recife"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"cadastrado\":false}", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Create_with_future_birth_date_returns_bad_request()
    {
        var response = await client.PostAsJsonAsync("/api/forms", new
        {
            nome = "Ana",
            sobrenome = "Silva",
            dataNascimento = DateTime.UtcNow.AddDays(1),
            cidadeNascimento = "Recife"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
