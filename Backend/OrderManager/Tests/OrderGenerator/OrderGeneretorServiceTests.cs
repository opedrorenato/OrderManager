using Domain.Models;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Service.Interfaces;
using Service.Services;

namespace Tests.OrderGenerator;

public class OrderGeneratorServiceTests
{
    private readonly Mock<IValidator<Order>> _mockValidator;
    private readonly Mock<IOrderSenderService> _mockOrderSenderService;
    private readonly OrderGeneratorService _service;

    public OrderGeneratorServiceTests()
    {
        _mockValidator = new Mock<IValidator<Order>>();
        _mockOrderSenderService = new Mock<IOrderSenderService>();
        _service = new OrderGeneratorService(
            _mockValidator.Object,
            _mockOrderSenderService.Object
        );
    }

    [Fact]
    public async Task ProcessOrder_ValidOrder_ShouldReturnValidResult()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 100,
            Price = 25.50m
        };

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        _mockOrderSenderService.Setup(s => s.SendOrder(order))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(order.Symbol, result.Data.Symbol);
        Assert.Equal(order.Side, result.Data.Side);
        Assert.Equal(order.Quantity, result.Data.Quantity);
        Assert.Equal(order.Price, result.Data.Price);
        Assert.Empty(result.Errors);

        _mockValidator.Verify(v => v.ValidateAsync(order, default), Times.Once);
        _mockOrderSenderService.Verify(s => s.SendOrder(order), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_InvalidOrder_ShouldReturnInvalidResult()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "INVALIDO",
            Side = "INVALIDO",
            Quantity = 0,
            Price = -10
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Symbol", "O Símbolo deve ter 4 letras seguidas de 1 ou 2 números"),
            new("Symbol", "O Símbolo informado não está na lista de símbolos válidos"),
            new("Side", "O 'LADO' deve ser COMPRA ou VENDA"),
            new("Quantity", "A 'QUANTIDADE' deve ser no mínimo 1 e no máximo 99999"),
            new("Price", "O 'PREÇO' deve ser positivo e maior que 0")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Equal(5, result.Errors.Count);

        // Verifica se todas as mensagens de erro estão presentes
        Assert.Contains(result.Errors, e => e.Contains("Símbolo deve ter 4 letras"));
        Assert.Contains(result.Errors, e => e.Contains("lista de símbolos válidos"));
        Assert.Contains(result.Errors, e => e.Contains("deve ser COMPRA ou VENDA"));
        Assert.Contains(result.Errors, e => e.Contains("no mínimo 1"));
        Assert.Contains(result.Errors, e => e.Contains("positivo e maior que 0"));

        _mockValidator.Verify(v => v.ValidateAsync(order, default), Times.Once);
        _mockOrderSenderService.Verify(s => s.SendOrder(order), Times.Never);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithInvalidSymbol_ShouldReturnSymbolErrors()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "ABC", // Inválido - menos de 4 letras
            Side = "COMPRA",
            Quantity = 100,
            Price = 25.50m
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Symbol", "O Símbolo deve ter 4 letras seguidas de 1 ou 2 números"),
            new ValidationFailure("Symbol", "O Símbolo informado não está na lista de símbolos válidos")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Equal(2, result.Errors.Count);

        foreach (var error in result.Errors)
        {
            Assert.Contains("Símbolo", error);
        }
    }

    [Fact]
    public async Task ProcessOrder_OrderWithInvalidSide_ShouldReturnSideErrors()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "INVALIDO", // Inválido
            Quantity = 100,
            Price = 25.50m
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Side", "O 'LADO' deve ser COMPRA ou VENDA")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("O 'LADO' deve ser COMPRA ou VENDA", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithInvalidQuantity_ShouldReturnQuantityErrors()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 0, // Inválido - menor que mínimo
            Price = 25.50m
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Quantity", "A 'QUANTIDADE' deve ser no mínimo 1 e no máximo 99999")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("A 'QUANTIDADE' deve ser no mínimo 1 e no máximo 99999", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithInvalidPrice_ShouldReturnPriceErrors()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 100,
            Price = -10 // Inválido - negativo
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Price", "O 'PREÇO' deve ser positivo e maior que 0")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("O 'PREÇO' deve ser positivo e maior que 0", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithPriceNotMultipleOf001_ShouldReturnPriceError()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 100,
            Price = 25.55m
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Price", "O 'PREÇO' deve ser múltiplo de 0,01")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("O 'PREÇO' deve ser múltiplo de 0,01", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithPriceAboveMax_ShouldReturnPriceError()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 100,
            Price = 1000.00m // Inválido - acima do máximo
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Price", "O 'PREÇO' deve ser no máximo 999,99")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("O 'PREÇO' deve ser no máximo 999,99", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessOrder_WhenSendOrderFails_ShouldPropagateException()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "PETR4",
            Side = "COMPRA",
            Quantity = 100,
            Price = 25.50m
        };

        var validationResult = new ValidationResult(); // Válido
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        _mockOrderSenderService.Setup(s => s.SendOrder(order))
            .ThrowsAsync(new Exception("Falha ao enviar ordem"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.ProcessOrder(order));
        Assert.Equal("Falha ao enviar ordem", exception.Message);

        _mockValidator.Verify(v => v.ValidateAsync(order, default), Times.Once);
        _mockOrderSenderService.Verify(s => s.SendOrder(order), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_OrderWithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var order = new Order
        {
            Symbol = "X", // Múltiplos erros
            Side = "X",   // Múltiplos erros
            Quantity = -5,
            Price = -1
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Symbol", "O Símbolo deve ter 4 letras seguidas de 1 ou 2 números"),
            new ValidationFailure("Symbol", "O Símbolo informado não está na lista de símbolos válidos"),
            new ValidationFailure("Side", "O 'LADO' deve ser COMPRA ou VENDA"),
            new ValidationFailure("Quantity", "A 'QUANTIDADE' deve ser no mínimo 1 e no máximo 99999"),
            new ValidationFailure("Price", "O 'PREÇO' deve ser positivo e maior que 0"),
            new ValidationFailure("Price", "O 'PREÇO' deve ser múltiplo de 0,01")
        };

        var validationResult = new ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(order, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Null(result.Data);
        Assert.Equal(6, result.Errors.Count);

        // Verifica se há erros para cada propriedade
        Assert.Contains(result.Errors, e => e.Contains("Símbolo"));
        Assert.Contains(result.Errors, e => e.Contains("LADO"));
        Assert.Contains(result.Errors, e => e.Contains("QUANTIDADE"));
        Assert.Contains(result.Errors, e => e.Contains("PREÇO"));
    }
}