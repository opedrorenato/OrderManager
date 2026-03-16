using ConsoleApp.OrderAccumulator;

namespace Tests.OrderAccumulator;

public class ExposureCalculatorTests
{
    private readonly ExposureCalculator _calculator;

    public ExposureCalculatorTests()
    {
        _calculator = new ExposureCalculator();
    }

    [Fact]
    public void ProcessOrder_Compra_DeveAdicionarExposicao()
    {
        // Arrange
        string symbol = "PETR4";
        string side = "COMPRA";
        int quantity = 100;
        decimal price = 25.50m;
        decimal expectedExposure = 100 * 25.50m; // 2550

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, side, quantity, price);

        // Assert
        decimal actualExposure = _calculator.GetExposure(symbol);
        Assert.Equal(expectedExposure, actualExposure);
    }

    [Fact]
    public void ProcessOrder_Venda_DeveSubtrairExposicao()
    {
        // Arrange
        string symbol = "VALE3";
        string side = "VENDA";
        int quantity = 50;
        decimal price = 60.00m;
        decimal expectedExposure = -50 * 60.00m; // -3000

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, side, quantity, price);

        // Assert
        decimal actualExposure = _calculator.GetExposure(symbol);
        Assert.Equal(expectedExposure, actualExposure);
    }

    [Fact]
    public void ProcessOrder_CompraEVenda_DeveCalcularExposicaoCorreta()
    {
        // Arrange
        string symbol = "ITUB4";
        decimal expectedExposure = 1500;

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, "COMPRA", 100, 30.00m); // +3000
        _calculator.ProcessOrder(symbol, "VENDA", 50, 30.00m);  // -1500

        // Assert
        decimal actualExposure = _calculator.GetExposure(symbol);
        Assert.Equal(expectedExposure, actualExposure);
    }

    [Fact]
    public void ProcessOrder_MultiplosSimbolos_DeveManterExposicoesSeparadas()
    {
        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder("PETR4", "COMPRA", 100, 25.00m); // +2500
        _calculator.ProcessOrder("VALE3", "COMPRA", 200, 60.00m); // +12000
        _calculator.ProcessOrder("PETR4", "VENDA", 50, 25.00m);   // -1250

        // Assert
        Assert.Equal(1250, _calculator.GetExposure("PETR4"));
        Assert.Equal(12000, _calculator.GetExposure("VALE3"));
        Assert.Equal(0, _calculator.GetExposure("ITUB4")); // Não processado
    }

    [Theory]
    [InlineData("COMPRA", 100, 10.00)]
    [InlineData("BUY", 200, 20.00)]
    [InlineData("1", 50, 30.00)]
    public void ProcessOrder_DiferentesSidesCompra_DeveAdicionarExposicao(string side, int quantity, decimal price)
    {
        // Arrange
        string symbol = "TESTE";
        decimal expectedExposure = quantity * price;

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, side, quantity, price);

        // Assert
        Assert.Equal(expectedExposure, _calculator.GetExposure(symbol));
    }

    [Theory]
    [InlineData("VENDA", 100, 10.00)]
    [InlineData("SELL", 200, 20.00)]
    [InlineData("2", 50, 30.00)]
    public void ProcessOrder_DiferentesSidesVenda_DeveSubtrairExposicao(string side, int quantity, decimal price)
    {
        // Arrange
        string symbol = "TESTE";
        decimal expectedExposure = -quantity * price;

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, side, quantity, price);

        // Assert
        Assert.Equal(expectedExposure, _calculator.GetExposure(symbol));
    }

    [Fact]
    public void GetExposure_SemOrdens_DeveRetornarZero()
    {
        // Act & Assert
        _calculator.ClearExposure();
        Assert.Equal(0, _calculator.GetExposure("SEI_LA"));
        Assert.Equal(0, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void ProcessOrder_MultiplasOrdens_DeveAcumularCorretamente()
    {
        // Arrange
        string symbol = "ITSA4";
        decimal expectedExposure = (100 * 10.00m) + (50 * 12.00m) - (30 * 11.00m) + (200 * 9.50m); // 3170

        // Act
        _calculator.ClearExposure();
        _calculator.ProcessOrder(symbol, "COMPRA", 100, 10.00m);  // +1000
        _calculator.ProcessOrder(symbol, "COMPRA", 50, 12.00m);   // +600 (total 1600)
        _calculator.ProcessOrder(symbol, "VENDA", 30, 11.00m);     // -330 (total 1270)
        _calculator.ProcessOrder(symbol, "COMPRA", 200, 9.50m);    // +1900 (total 3170)

        var result = _calculator.GetExposure(symbol);

        // Assert
        Assert.Equal(expectedExposure, result);
    }

    [Fact]
    public void ClearExposure_DeveLimparTodasAsExposicoes()
    {
        // Arrange
        _calculator.ProcessOrder("PETR4", "COMPRA", 100, 25.00m);
        _calculator.ProcessOrder("VALE3", "VENDA", 50, 60.00m);

        // Verifica se as exposições foram criadas
        Assert.NotEqual(0, _calculator.GetExposure("PETR4"));
        Assert.NotEqual(0, _calculator.GetExposure("VALE3"));

        // Act
        _calculator.ClearExposure();

        // Assert
        Assert.Equal(0, _calculator.GetExposure("PETR4"));
        Assert.Equal(0, _calculator.GetExposure("VALE3"));
    }

    [Fact]
    public void ProcessOrder_AposClear_DeveRecomecarDoZero()
    {
        // Arrange
        string symbol = "PETR4";

        // Primeiro lote de ordens
        _calculator.ProcessOrder(symbol, "COMPRA", 100, 25.00m); // +2500
        Assert.Equal(2500, _calculator.GetExposure(symbol));

        // Limpa
        _calculator.ClearExposure();
        Assert.Equal(0, _calculator.GetExposure(symbol));

        // Act - Segundo lote de ordens
        _calculator.ProcessOrder(symbol, "COMPRA", 50, 30.00m); // +1500

        // Assert
        Assert.Equal(1500, _calculator.GetExposure(symbol));
    }

    [Fact]
    public async Task ProcessOrder_OrdensConcorrentes_DeveManterConsistencia()
    {
        // Arrange
        string symbol = "CONCORRENTE";
        int totalOrdens = 100;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act - Executa as ordens em paralelo
        await Task.Run(() =>
        {
            Parallel.For(0, totalOrdens, new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i =>
            {
                _calculator.ProcessOrder(symbol, "COMPRA", 1, 10.00m);
            });
        }, cancellationToken);

        // Assert
        var result = _calculator.GetExposure(symbol);
        Assert.Equal(1000, result); // 100 * 1 * 10 = 1000
    }
}
