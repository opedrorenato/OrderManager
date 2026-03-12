namespace Domain.Models;

public class OrderResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = null!;
}
