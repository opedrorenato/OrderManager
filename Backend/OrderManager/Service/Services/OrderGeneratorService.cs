using Domain.Models;
using FluentValidation;
using FluentValidation.Results;
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

    public async Task<ValidationResult> ValidateOrder(Order order)
    {
        var result = await _validator.ValidateAsync(order);
        return result;
    }

    public async Task ProcessOrder(Order order)
    {
        _orderSenderService.SendOrder(order);
    }
}
