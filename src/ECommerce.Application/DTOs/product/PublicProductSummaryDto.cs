public class PublicProductSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Brand { get; set; }
    public string? PrimaryCategoryName { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool InStock { get; set; }
    public string? ThumbnailUrl { get; set; }
}
