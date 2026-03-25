using System;

namespace POS.Frontend.Models;

public class MerchantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool isActive { get; set; }
}
