using Microsoft.Extensions.Hosting;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace Service.Services;

public class QuickFixHostedService : IHostedService
{
    private readonly SocketInitiator _initiator;

    public QuickFixHostedService(FixInitiatorApp app)
    {
        try
        {
            var fixHost = Environment.GetEnvironmentVariable("FIX_HOST") ?? "127.0.0.1";
            var fixPort = int.Parse(Environment.GetEnvironmentVariable("FIX_PORT") ?? "9876");
            
            var settings = new SessionSettings("_fix_initiator.cfg");
            settings.Get().SetString("SocketConnectHost", fixHost);
            settings.Get().SetLong("SocketConnectPort", fixPort);

            Console.WriteLine($"[GENERATOR] Tentando conectar em {fixHost}:{fixPort}");

            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            _initiator = new SocketInitiator(app, storeFactory, settings, logFactory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao configurar QuickFix: {ex}");
            throw;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("Iniciando conexão FIX com Accumulator...");
            _initiator.Start();

            Console.WriteLine("QuickFix iniciado. Aguardando conexão...");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar QuickFix: {ex}");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Parando QuickFix...");
        _initiator?.Stop();
        return Task.CompletedTask;
    }
}
