namespace CountryExplorer.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = new List<T>();

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
