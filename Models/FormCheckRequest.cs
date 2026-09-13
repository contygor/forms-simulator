using System.ComponentModel.DataAnnotations;

namespace FormSimulatorApi.Models;

public class FormCheckRequest
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    [Required]
    [MaxLength(150)]
    public string CidadeNascimento { get; set; } = string.Empty;
}
