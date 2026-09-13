using FormSimulatorApi.Data;
using FormSimulatorApi.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FormsSimulator.Tests;

public class AppDbContextTests
{
    [Fact]
    public void FormResponse_has_expected_relational_mapping()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateContext(connection);
        var entity = context.Model.FindEntityType(typeof(FormResponse));

        Assert.NotNull(entity);
        Assert.Equal("form_responses", entity!.GetTableName());
        Assert.Equal(6, entity.GetProperties().Count());
        Assert.Single(entity.FindPrimaryKey()!.Properties);
    }

    [Fact]
    public async Task FormResponse_can_be_persisted_and_loaded()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        await using (var context = CreateContext(connection))
        {
            await context.Database.EnsureCreatedAsync();
            context.FormResponses.Add(new FormResponse
            {
                Nome = "Ana",
                Sobrenome = "Silva",
                DataNascimento = new DateTime(1990, 5, 20),
                CidadeNascimento = "Recife"
            });
            await context.SaveChangesAsync();
        }

        await using (var verificationContext = CreateContext(connection))
        {
            var response = await verificationContext.FormResponses.SingleAsync();

            Assert.Equal("Ana", response.Nome);
            Assert.Equal("Silva", response.Sobrenome);
            Assert.Equal(new DateTime(1990, 5, 20), response.DataNascimento);
            Assert.Equal("Recife", response.CidadeNascimento);
            Assert.NotEqual(default, response.CreatedAtUtc);
        }
    }

    private static AppDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        return new AppDbContext(options);
    }
}
