using Domain.Models;
using FluentValidation.Results;

namespace Service.Interfaces;

public interface IOrderGeneratorService
{
    Task<ValidationResult> ValidateOrder(Order order);
}
