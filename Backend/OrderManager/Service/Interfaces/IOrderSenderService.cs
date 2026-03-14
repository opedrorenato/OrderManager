using Domain.Models;

namespace Service.Interfaces;

public interface IOrderSenderService
{
    Task<string> SendOrder(Order order);
}
