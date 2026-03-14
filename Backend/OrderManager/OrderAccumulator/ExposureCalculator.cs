namespace ConsoleApp.OrderAccumulator;


public class ExposureCalculator
{
    private static readonly Dictionary<string, decimal> _exposure = [];
    private readonly Lock _lock = new();

    public void ProcessOrder(string symbol, string side, int quantity, decimal price)
    {
        lock (_lock)
        {
            if (!_exposure.ContainsKey(symbol))
                _exposure[symbol] = 0;

            decimal amount = quantity * price;
            string[] buySide = ["COMPRA", "BUY", "1"];

            if (buySide.Contains(side))
            {
                _exposure[symbol] += amount;
                Console.WriteLine($"\n↑ COMPRA: +{amount:C} = {_exposure[symbol]:C}");
            }
            else
            {
                _exposure[symbol] -= amount;
                Console.WriteLine($"\n↓ VENDA: -{amount:C} = {_exposure[symbol]:C}");
            }
        }
    }

    public void PrintExposure()
    {
        lock (_lock)
        {
            if (_exposure.Count == 0)
            {
                Console.WriteLine("Nenhuma exposição encontrada!");
                return;
            }

            Console.WriteLine("# EXPOSIÇÃO POR ATIVO:");
            foreach (var item in _exposure)
            {
                string signal = item.Value >= 0 ? "+" : "";
                Console.WriteLine($"{item.Key}: {signal}{item.Value:C}");
            }

            Console.Write("\n");
        }
    }
}
