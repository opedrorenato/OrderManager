namespace Domain.Utils;

public static class Constants
{
    public static readonly List<string> VALID_SYMBOLS = ["PETR4", "VALE3", "VIIA4"];
    public static readonly List<string> VALID_SIDES = ["COMPRA", "VENDA"];

    public static readonly int MIN_QUANTITY = 1;
    public static readonly int MAX_QUANTITY = 99_999;

    public static readonly decimal MIN_PRICE = 0.01m;
    public static readonly decimal MAX_PRICE = 999.99m;
}
