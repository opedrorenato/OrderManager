using Domain.Models;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Service.Services;

public class FixInitiatorApp : MessageCracker, IApplication
{
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingOrders = new();
    private SessionID? _sessionId;

    public bool IsLoggedOn { get; private set; }

    public void OnCreate(SessionID sessionID)
    {
        _sessionId = sessionID;
        Console.WriteLine($"Sessão criada: {sessionID}");
    }

    public void OnLogon(SessionID sessionID)
    {
        IsLoggedOn = true;
        Console.WriteLine("Conectado ao Accumulator!");
    }

    public void OnLogout(SessionID sessionID)
    {
        IsLoggedOn = false;
        Console.WriteLine("Desconectado do Accumulator");
    }

    public void FromAdmin(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"[GENERATOR] FromAdmin: {message.GetType().Name}");

        if (message is Logon)
        {
            Console.WriteLine($"[GENERATOR] Logon recebido do Accumulator!");
        }
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"[GENERATOR] ToAdmin: {message.GetType().Name}");

        if (message is Logon logon)
        {
            Console.WriteLine($"[GENERATOR] Enviando Logon para o Accumulator...");
            Console.WriteLine($"[GENERATOR]   HeartBtInt: {logon.HeartBtInt}");
            Console.WriteLine($"[GENERATOR]   EncryptMethod: {logon.EncryptMethod}");
        }
    }

    public void FromApp(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"Resposta recebida: {message}");

        try
        {
            Crack((QuickFix.FIX44.Message)message, sessionID);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar resposta: {ex}");
        }
    }

    public void ToApp(QuickFix.Message message, SessionID sessionID)
    {
        if (message is Heartbeat)
            return;

        Console.WriteLine($"Enviando: {message}");
    }

    public void OnMessage(ExecutionReport report, SessionID sessionID)
    {
        var clOrdId = report.ClOrdID.Value;
        var ordStatus = report.OrdStatus.Value;

        Console.WriteLine($"Ordem {clOrdId} executada - Status: {ordStatus}");

        if (_pendingOrders.TryGetValue(clOrdId, out var tcs))
        {
            tcs.TrySetResult($"Ordem executada - Status: {ordStatus}");
            _pendingOrders.Remove(clOrdId);
        }
    }

    public async Task<string> SendOrder(Order order)
    {
        if (!IsLoggedOn)
            throw new InvalidOperationException("Não conectado ao Accumulator. Verifique se o OrderAccumulator está rodando.");

        if (_sessionId == null)
            throw new InvalidOperationException("Sessão FIX não inicializada");

        var clOrdId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();
        _pendingOrders[clOrdId] = tcs;

        var orderSide = order.Side.Equals("COMPRA", StringComparison.CurrentCultureIgnoreCase)
            ? Side.BUY
            : Side.SELL;

        var newOrder = new NewOrderSingle(
            new ClOrdID(clOrdId),
            new Symbol(order.Symbol),
            new Side(orderSide),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT)
        );

        newOrder.Set(new OrderQty(order.Quantity));
        newOrder.Set(new Price(order.Price));

        Session.SendToTarget(newOrder, _sessionId);
        Console.WriteLine($"\nOrdem Enviada: {order.Symbol}, {order.Side}, {order.Quantity} @ {order.Price:F2}");

        // Aguarda resposta por até 10 segundos
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        cts.Token.Register(() => tcs.TrySetCanceled(), false);

        return await tcs.Task;
    }
}
