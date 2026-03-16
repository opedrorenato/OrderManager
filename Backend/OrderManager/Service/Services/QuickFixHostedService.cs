using Microsoft.Extensions.Hosting;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using System.Text.Json;

namespace Service.Services;

public class QuickFixHostedService : IHostedService
{
    private readonly SocketInitiator _initiator;

    public QuickFixHostedService(FixInitiatorApp app)
    {
        try
        {

#if DEBUG
            var quickFixConfig = "_fix_initiator.local.cfg";
#else
            var quickFixConfig = "_fix_initiator.cfg";
#endif

            var settings = new SessionSettings(quickFixConfig);
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
