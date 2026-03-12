using Domain.Models;
using Domain.Utils;
using FluentValidation;

namespace Service.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(order => order.Symbol)
            .NotEmpty()
                .WithMessage("O 'SÍMBOLO' da operação não pode ser nulo")
            .Must(Constants.VALID_SYMBOLS.Contains)
                .WithMessage("Símbolo inválido");

        RuleFor(order => order.Side)
            .NotEmpty()
                .WithMessage("O 'LADO' da operação não pode ser nulo")
            .Must(Constants.VALID_SIDES.Contains)
                .WithMessage("Lado inválido");

        RuleFor(order => order.Quantity)
            .NotNull()
                .WithMessage("A 'QUANTIDADE' da operação não pode ser nula")
            .InclusiveBetween(Constants.MIN_QUANTITY, Constants.MAX_QUANTITY)
                .WithMessage($"A 'QUANTIDADE' deve ser no mínimo {Constants.MIN_QUANTITY} e no máximo {Constants.MAX_QUANTITY}");

        RuleFor(order => order.Price)
            .NotNull()
                .WithMessage("O 'PREÇO' da operação não pode ser nulo")
            .Must(price => price >= 0.01m || price % 0.01m == 0m)
                .WithMessage("O 'PREÇO' deve ser múltiplo de 0,01")
            .LessThanOrEqualTo(999.99m)
                .WithMessage("O 'PREÇO' deve ser no máximo 999,99");
    }
}
