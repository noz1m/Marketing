using Domain.Pagination;

namespace Domain.Filters;

public sealed class ProductFilter : ValidFilter
{
    public string? Name { get; set; }
    public decimal? StartPrice { get; set; }
    public decimal? ToPrice { get; set; }
}
