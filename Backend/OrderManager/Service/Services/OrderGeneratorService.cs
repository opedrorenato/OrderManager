using Domain.Models;
using FluentValidation;
using FluentValidation.Results;
using Service.Interfaces;

namespace Service.Services;

public class OrderGeneratorService : IOrderGeneratorService
{
    private readonly IValidator<Order> _validator;

    public OrderGeneratorService(IValidator<Order> validator)
    {
        _validator = validator;
    }

    public async Task<ValidationResult> ValidateOrder(Order order)
    {
        var result = await _validator.ValidateAsync(order);
        return result;
    }
}
