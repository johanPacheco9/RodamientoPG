namespace Domain.Models.Generics;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalRegistros { get; set; }
}