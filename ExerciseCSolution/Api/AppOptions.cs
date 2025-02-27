using System.ComponentModel.DataAnnotations;

namespace Api;

public sealed class AppOptions
{
    [Required] public string DbConnectionString { get; set; } = null!;

}