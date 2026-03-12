using QuickFix;

namespace Service.Services;

public class OrderSenderApp : IApplication
{
    public void OnCreate(SessionID sessionID) 
    {
        Console.WriteLine("Session criada: " + sessionID);
    }

    public void OnLogon(SessionID sessionID)
    {
        Console.WriteLine("Logon realizado com sucesso!");
    }

    public void OnLogout(SessionID sessionID) { }

    public void FromApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("Mensagem RECEBIDA: " + message);

        // Verifica se é um ExecutionReport
        if (message is QuickFix.FIX44.ExecutionReport execReport)
        {
            var orderId = execReport.OrderID.Value;
            var clOrdId = execReport.ClOrdID.Value;
            var status = execReport.OrdStatus.Value;
            var execType = execReport.ExecType.Value;

            Console.WriteLine(
                $"ExecutionReport recebido: " +
                $"OrderID={orderId}, " +
                $"ClOrdID={clOrdId}, " +
                $"Status={status}, " +
                $"ExecType={execType}"
            );
        }
    }

    public void ToApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("Mensagem ENVIADA: " + message);
    }

    public void FromAdmin(Message message, SessionID sessionID) { }

    public void ToAdmin(Message message, SessionID sessionID) { }
}
