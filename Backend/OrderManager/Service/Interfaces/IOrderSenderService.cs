using Domain.Models;

namespace Service.Interfaces;

public interface IOrderSenderService
{
    void SendOrder(Order order);
}
