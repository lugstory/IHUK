using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace TartarusMUD.Core
{
    public class ServerLogger
    {
        // Vláknově bezpečná fronta zpráv
        private readonly ConcurrentQueue<string> _logQueue = new();
        private readonly string _logFilePath = "server.log";

        public ServerLogger()
        {
            // Spustíme nekonečnou smyčku pro zápis na pozadí, aniž bychom blokovali server
            Task.Run(ProcessLogsAsync);
        }

        // Tuto metodu budeme volat odkudkoliv ze serveru
        public void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string formattedMessage = $"[{timestamp}] {message}";
            
            _logQueue.Enqueue(formattedMessage);
            Console.WriteLine(formattedMessage); // Vypíšeme rovnou i do konzole
        }

        private async Task ProcessLogsAsync()
        {
            while (true)
            {
                if (_logQueue.TryDequeue(out string logEntry))
                {
                    try
                    {
                        // Připíše řádek na konec souboru
                        await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
                    }
                    catch 
                    { 
                        // Ignorujeme chybu zápisu (např. pokud jiný program soubor zamkl), 
                        // aby nám nespadlo logovací vlákno
                    }
                }
                else
                {
                    // Pokud je fronta prázdná, vlákno si na chvíli "odpočine"
                    await Task.Delay(100);
                }
            }
        }
    }
}