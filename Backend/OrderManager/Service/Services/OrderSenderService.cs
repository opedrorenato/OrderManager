using Domain.Models;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using Service.Interfaces;

namespace Service.Services;

public class OrderSenderService : IOrderSenderService
{
    private readonly SocketInitiator _initiator;

    public OrderSenderService(SocketInitiator initiator)
    {
        _initiator = initiator;
    }

    public void SendOrder(Order order)
    {
        char orderSide = GetOrderSide(order);

        var msg = new QuickFix.FIX44.NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString()),
            new Symbol(order.Symbol),
            new Side(orderSide),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.MARKET)
        );

        msg.Set(new OrderQty(order.Quantity));
        msg.Set(new Price(order.Price));

        var sessionId = _initiator.GetSessionIDs().FirstOrDefault();
        if (sessionId == null)
            throw new InvalidOperationException("Nenhuma sessão encontrada.");

        var session = Session.LookupSession(sessionId);
        if (session == null || !session.IsLoggedOn)
            throw new InvalidOperationException("Nenhuma sessão FIX ativa/logada.");

        Session.SendToTarget(msg, sessionId);
        Console.WriteLine($"Ordem enviada: {order.Symbol} {order.Side} {order.Quantity} @ {order.Price}");
    }

    private static char GetOrderSide(Order order)
    {
        return order.Side.Equals("COMPRA", StringComparison.CurrentCultureIgnoreCase)
            ? Side.BUY
            : Side.SELL;
    }
}
