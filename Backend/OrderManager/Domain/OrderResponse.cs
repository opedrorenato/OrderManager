namespace Domain
{
    public class OrderResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = null!;
    }
}
