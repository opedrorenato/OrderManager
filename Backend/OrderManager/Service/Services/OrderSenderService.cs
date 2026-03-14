using Domain.Models;
using Service.Interfaces;

namespace Service.Services;

public class OrderSenderService : IOrderSenderService
{
    private readonly FixInitiatorApp _app;

    public OrderSenderService(FixInitiatorApp app)
    {
        _app = app;
    }

    public async Task<string> SendOrder(Order order)
    {
        try
        {
            return await _app.SendOrder(order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar ordem: {ex}");
            throw;
        }
    }
}
