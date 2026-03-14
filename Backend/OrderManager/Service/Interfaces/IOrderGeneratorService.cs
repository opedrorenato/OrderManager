using Domain.Models;

namespace Service.Interfaces;

public interface IOrderGeneratorService
{
    Task<ResultModel<Order>> ProcessOrder(Order order);
}
