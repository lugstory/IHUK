using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TartarusMUD.Core;
using TartarusMUD.Models;

namespace TartarusMUD
{
    public class GameServer
    {

        // Vláknově bezpečná kolekce pro udržení aktivních spojení (hráčů)
        private readonly ConcurrentDictionary<Guid, TcpClient> _connectedClients = new();
        private readonly World _world = new World();
        private readonly CommandParser _parser = new CommandParser();

        public async Task StartAsync(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server naslouchá na portu {port}. Čekám na hráče...");

            try
            {
                while (true)
                {
                    // Asynchronně čekáme na připojení nového klienta
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Guid clientId = Guid.NewGuid();
                    _connectedClients.TryAdd(clientId, client);

                    Console.WriteLine($"[Připojeno] Klient {clientId} se připojil.");

                    // Spustíme obsluhu klienta na pozadí a nečekáme na její dokončení (Fire and Forget)
                    _ = HandleClientAsync(clientId, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kritická chyba serveru: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleClientAsync(Guid clientId, TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream);
            using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            Player newPlayer = new Player(writer);

            try
            {
                // 1. Přihlášení (Získání jména)
                await writer.WriteAsync("Zadej sve jmeno:\r\n> ");
                string name = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(name)) name = $"Hráč_{clientId.ToString().Substring(0, 4)}";

                newPlayer.Name = name;

                // 2. Vstup do světa
                newPlayer.CurrentRoom = _world.StartRoom;
                lock (_world) // Ochrana proti souběhu při přidávání hráče
                {
                    newPlayer.CurrentRoom.Players.Add(newPlayer);
                }

                await writer.WriteAsync($"\r\nVitej na stanici Tartarus, {newPlayer.Name}!\r\n");
                _parser.ProcessCommand(newPlayer, "prozkoumej"); // Automatický rozhlédnutí

                // 3. Hlavní herní smyčka pro tohoto klienta
                while (client.Connected)
                {
                    string input = await reader.ReadLineAsync();
                    if (input == null) break;

                    input = input.Trim();
                    if (!string.IsNullOrEmpty(input))
                    {
                        _parser.ProcessCommand(newPlayer, input);
                    }
                    else
                    {
                        newPlayer.SendMessage(""); // Jen znovu vykreslí prompt ">"
                    }
                }
            }
            catch (IOException)
            {
                /* Ignorujeme odpojení */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chyba] Klient {clientId}: {ex.Message}");
            }
            finally
            {
                // 4. Bezpečné odhlášení
                lock (_world)
                {
                    if (newPlayer.CurrentRoom != null)
                    {
                        newPlayer.CurrentRoom.Players.Remove(newPlayer);
                        // Upozorníme ostatní, že hráč zmizel
                        foreach (var p in newPlayer.CurrentRoom.Players)
                        {
                            p.SendMessage($"\r\n[Okolí] {newPlayer.Name} se rozplynul ve vzduchu (odpojen).");
                        }
                    }
                }

                _connectedClients.TryRemove(clientId, out _);
                client.Close();
                Console.WriteLine($"[Odpojeno] {newPlayer.Name} se odpojil.");
            }
        }
    }
}