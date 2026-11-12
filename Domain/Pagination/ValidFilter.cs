namespace Domain.Pagination;

public class ValidFilter
{
    private int pageNumber = 1;
    private int pageSize = 10;

    public int PageNumber
    {
        get => pageNumber;
        set => pageNumber = Math.Max(1, value);
    }

    public int PageSize
    {
        get => pageSize;
        set => pageSize = value < 1 ? 10 : Math.Min(value, 100);
    }
}
