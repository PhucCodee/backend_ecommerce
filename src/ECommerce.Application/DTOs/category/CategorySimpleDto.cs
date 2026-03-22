namespace ECommerce.Application.DTOs.category
{
    public class CategorySimpleDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}