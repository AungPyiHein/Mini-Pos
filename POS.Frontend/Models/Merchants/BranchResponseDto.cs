using System;

namespace POS.Frontend.Models.Merchants;

public class BranchResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int ActiveUsersCount { get; set; }
}
