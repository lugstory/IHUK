using System;
using System.Threading.Tasks;

namespace TartarusMUD
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Spouštím server Stanice Tortarus...");
            int port = 65525;
            
            GameServer server = new GameServer();
            await server.StartAsync(port);
        }
    }
}