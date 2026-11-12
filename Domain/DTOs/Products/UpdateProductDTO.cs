using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Products;

public sealed class UpdateProductDTO
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
    ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }
}
