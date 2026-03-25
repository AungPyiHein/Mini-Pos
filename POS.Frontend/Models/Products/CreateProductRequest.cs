using System;

namespace POS.Frontend.Models.Products;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid MerchantId { get; set; }
}
