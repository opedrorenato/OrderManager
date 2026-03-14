using Domain.Models;
using FluentValidation;
using Service.Interfaces;

namespace Service.Services;

public class OrderGeneratorService : IOrderGeneratorService
{
    private readonly IValidator<Order> _validator;
    private readonly IOrderSenderService _orderSenderService;

    public OrderGeneratorService(
        IValidator<Order> validator,
        IOrderSenderService orderSenderService
    )
    {
        _validator = validator;
        _orderSenderService = orderSenderService;
    }

    public async Task<ResultModel<Order>> ProcessOrder(Order order)
    {
        var orderValidation = await _validator.ValidateAsync(order);
        if (!orderValidation.IsValid)
        {
            return ResultModel<Order>.InvalidResult(orderValidation.Errors.Select(e => e.ErrorMessage));
        }

        await _orderSenderService.SendOrder(order);
        return ResultModel<Order>.ValidResult(order);
    }
}
