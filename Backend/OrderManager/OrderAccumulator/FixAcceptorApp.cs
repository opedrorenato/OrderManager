using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace ConsoleApp.OrderAccumulator;

public class FixAcceptorApp : MessageCracker, IApplication
{
    private readonly ExposureCalculator _calculator;

    public FixAcceptorApp(ExposureCalculator calculator)
    {
        _calculator = calculator;
    }

    public void OnCreate(SessionID sessionID)
    {
        Console.WriteLine($"[ACCUMULATOR] OnCreate: {sessionID}");
    }

    public void OnLogon(SessionID sessionID)
    {
        Console.WriteLine($"[ACCUMULATOR] OnLogon: {sessionID} - CONECTADO!");
    }

    public void OnLogout(SessionID sessionID)
    {
        Console.WriteLine($"[ACCUMULATOR] OnLogout: {sessionID}");
    }

    public void FromAdmin(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"[ACCUMULATOR] FromAdmin: {message.GetType().Name}");

        if (message is Logon logon)
        {
            Console.WriteLine($"[ACCUMULATOR] >>> LOGON RECEBIDO DO GENERATOR! <<<");
            Console.WriteLine($"   HeartBtInt: {logon.HeartBtInt}");
            Console.WriteLine($"   EncryptMethod: {logon.EncryptMethod}");
        }
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"[ACCUMULATOR] ToAdmin: {message.GetType().Name}");

        if (message is Logon)
            Console.WriteLine($"[ACCUMULATOR] >>> RESPONDENDO LOGON PARA GENERATOR <<<");
    }

    public void FromApp(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat) 
            return;

        Console.WriteLine($"[ACCUMULATOR] FromApp: {message.GetType().Name}");
        try
        {
            Crack((QuickFix.FIX44.Message)message, sessionID);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ACCUMULATOR] Erro no FromApp: {ex.Message}");
        }
    }

    public void ToApp(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"[ACCUMULATOR] ToApp: {message.GetType().Name}");
    }

    public void OnMessage(NewOrderSingle order, SessionID sessionID)
    {
        string symbol = order.Symbol.Value;
        char side = order.Side.Value;
        int quantity = (int)order.OrderQty.Value;
        decimal price = order.Price?.Value ?? 0;
        string sideText = side == Side.BUY ? "COMPRA" : "VENDA";

        Console.WriteLine($"\n[ACCUMULATOR] NOVA ORDEM RECEBIDA:");
        Console.WriteLine($"[ACCUMULATOR] =========================================");
        Console.WriteLine($"[ACCUMULATOR]   Simbolo: {symbol}");
        Console.WriteLine($"[ACCUMULATOR]   Lado: {sideText}");
        Console.WriteLine($"[ACCUMULATOR]   Quantidade: {quantity}");
        Console.WriteLine($"[ACCUMULATOR]   Preco: {price:F2}");
        Console.WriteLine($"[ACCUMULATOR] =========================================");

        _calculator.ProcessOrder(symbol, sideText, quantity, price);
        _calculator.PrintExposure();

        SendExecutionReport(order, sessionID);
    }

    private void SendExecutionReport(NewOrderSingle order, SessionID sessionID)
    {
        var orderId = new OrderID(order.GetString(ClOrdID.TAG));
        var execId = new ExecID(Guid.NewGuid().ToString());
        var execType = new ExecType(ExecType.FILL);
        var ordStatus = new OrdStatus(OrdStatus.FILLED);
        var symbol = order.Symbol;
        var side = order.Side;
        var leavesQty = new LeavesQty(0);
        var cumQty = new CumQty(order.OrderQty.Value);
        var avgPx = new AvgPx(order.Price?.Value ?? 0);

        var report = new ExecutionReport(
            orderId, execId, execType, ordStatus,
            symbol, side, leavesQty, cumQty, avgPx
        );

        report.SetField(new ClOrdID(order.GetString(ClOrdID.TAG)));
        report.SetField(new OrderQty(order.OrderQty.Value));
        report.SetField(new LastQty(order.OrderQty.Value));
        report.SetField(new LastPx(order.Price?.Value ?? 0));

        Session.SendToTarget(report, sessionID);
    }
}
