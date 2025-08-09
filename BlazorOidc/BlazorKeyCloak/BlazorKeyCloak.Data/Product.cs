namespace BlazorKeycloak.Data;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public bool RequiresAdminAccess { get; set; }
}