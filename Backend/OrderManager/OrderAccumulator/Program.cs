using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace ConsoleApp.OrderAccumulator;

static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("### OrderAccumulator ###");

        try
        {
            string configPath = "_fix_acceptor.cfg";
            string fullPath = Path.GetFullPath(configPath);

            Console.WriteLine($"Procurando config em: {fullPath}");
            Console.WriteLine($"Arquivo existe: {File.Exists(fullPath)}");

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("ERRO: Arquivo de configuracao nao encontrado!");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            var settings = new SessionSettings(configPath);

            var calculator = new ExposureCalculator();
            var app = new FixAcceptorApp(calculator);

            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            var acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);

            Console.WriteLine("Iniciando acceptor FIX na porta 9876...");
            acceptor.Start();

            Console.WriteLine("OrderAccumulator rodando! Aguardando ordens...");
            Console.WriteLine("Pressione 'E' para ver exposicao, 'Q' para sair\n");

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.E)
                {
                    calculator.PrintExposure();
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

            acceptor.Stop();
            Console.WriteLine("OrderAccumulator finalizado");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro fatal: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
