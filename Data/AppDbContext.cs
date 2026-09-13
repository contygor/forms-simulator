using Microsoft.EntityFrameworkCore;
using FormSimulatorApi.Models;

namespace FormSimulatorApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<FormResponse> FormResponses => Set<FormResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FormResponse>(entity =>
            {
                entity.ToTable("form_responses");
                entity.HasKey(response => response.Id);
                entity.Property(response => response.Nome).HasMaxLength(100).IsRequired();
                entity.Property(response => response.Sobrenome).HasMaxLength(100).IsRequired();
                entity.Property(response => response.CidadeNascimento).HasMaxLength(150).IsRequired();
                entity.Property(response => response.CreatedAtUtc).IsRequired();
                entity.HasIndex(response => response.CreatedAtUtc);
            });
        }
    }
}