using Domain.Models;
using Domain.Utils;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Service.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(order => order.Symbol)
            .NotEmpty()
                .WithMessage("O 'SÍMBOLO' da operação não pode ser nulo")
            .Must(IsValidSymbolFormat)
                .WithMessage("O Símbolo deve ter 4 letras seguidas de 1 ou 2 números")
            .Must(Constants.VALID_SYMBOLS.Contains)
                .WithMessage("O Símbolo informado não está na lista de símbolos válidos");

        RuleFor(order => order.Side)
            .NotEmpty()
                .WithMessage("O 'LADO' da operação não pode ser nulo")
            .Must(Constants.VALID_SIDES.Contains)
                .WithMessage("O 'LADO' deve ser COMPRA ou VENDA");

        RuleFor(order => order.Quantity)
            .NotNull()
                .WithMessage("A 'QUANTIDADE' da operação não pode ser nula")
            .InclusiveBetween(Constants.MIN_QUANTITY, Constants.MAX_QUANTITY)
                .WithMessage($"A 'QUANTIDADE' deve ser no mínimo {Constants.MIN_QUANTITY} e no máximo {Constants.MAX_QUANTITY}");

        RuleFor(order => order.Price)
            .NotNull()
                .WithMessage("O 'PREÇO' da operação não pode ser nulo")
            .GreaterThan(0)
                .WithMessage("O 'PREÇO' deve ser positivo e maior que 0")
            .Must(price => price >= 0.01m || price % 0.01m == 0m)
                .WithMessage("O 'PREÇO' deve ser múltiplo de 0,01")
            .LessThanOrEqualTo(999.99m)
                .WithMessage("O 'PREÇO' deve ser no máximo 999,99");
    }

    private static bool IsValidSymbolFormat(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return false;

        return Regex.IsMatch(symbol, @"^[A-Z]{4}\d{1,2}$");
    }
}
