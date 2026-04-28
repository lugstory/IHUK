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
        private readonly ConcurrentDictionary<Guid, TcpClient> _connectedClients = new();
        
        // Naše herní a systémové komponenty
        private readonly World _world;
private readonly CommandParser _parser;
private readonly SaveManager _saveManager = new SaveManager();
private readonly ServerLogger _logger = new ServerLogger();

// A přidej konstruktor GameServeru, kde se propojí svět a parser:
public GameServer()
{
    _world = new World();
    _parser = new CommandParser(_world);
}
        public async Task StartAsync(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            
            string startupMsg = $"Server naslouchá na portu {port}. Čekám na hráče...";
            Console.WriteLine(startupMsg);
            _logger.Log(startupMsg); // Zapíšeme start do logu

            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Guid clientId = Guid.NewGuid();
                    _connectedClients.TryAdd(clientId, client);

                    _logger.Log($"Připojeno nové zařízení (ID: {clientId}).");
                    _ = HandleClientAsync(clientId, client);
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Kritická chyba serveru: {ex.Message}");
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
            string passwordHash = ""; // Uchováme si hash pro ukládání při odpojení

            try
            {
                // 1. Získání jména
                await writer.WriteAsync("Zadej sve jmeno:\r\n> ");
                string name = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(name)) name = $"Hrac_{clientId.ToString().Substring(0,4)}";
                
                newPlayer.Name = name;

                // 2. Kontrola účtu a hesla
                if (_saveManager.AccountExists(name))
                {
                    await writer.WriteAsync("Ucet nalezen. Zadej heslo:\r\n> ");
                    string password = await reader.ReadLineAsync();
                    passwordHash = _saveManager.HashPassword(password);

                    PlayerSaveData saveData = _saveManager.LoadPlayer(name);
                    
                    if (saveData.PasswordHash != passwordHash)
                    {
                        await writer.WriteAsync("Spatne heslo! Odpojuji...\r\n");
                        _logger.Log($"Neúspěšný pokus o přihlášení na účet {name}.");
                        return; // Okamžitě ukončí spojení
                    }

                    // Obnova dat hráče
                    newPlayer.Inventory = saveData.Inventory ?? new System.Collections.Generic.List<string>();
                    
                    // Pokusíme se najít místnost, kde se odpojil. Pokud neexistuje (např. jsme ji smazali z mapy), hodíme ho na start.
                    newPlayer.CurrentRoom = _world.GetRoomById(saveData.CurrentRoomId) ?? _world.StartRoom;
                    
                    await writer.WriteAsync("\r\nHeslo prijato. Vitej zpet na stanici!\r\n");
                    _logger.Log($"Hráč {name} se úspěšně přihlásil.");
                }
                else
                {
                    await writer.WriteAsync("Tento ucet neexistuje. Zadej heslo pro vytvoreni noveho uctu:\r\n> ");
                    string password = await reader.ReadLineAsync();
                    passwordHash = _saveManager.HashPassword(password);

                    newPlayer.CurrentRoom = _world.StartRoom;
                    await writer.WriteAsync("\r\nUcet vytvoren. Vitej na stanici Tartarus!\r\n");
                    _logger.Log($"Vytvořen nový účet: {name}.");
                }

                // 3. Vstup do světa
                lock (_world)
                {
                    newPlayer.CurrentRoom.Players.Add(newPlayer);
                    foreach (var p in newPlayer.CurrentRoom.Players)
                    {
                        if (p != newPlayer) p.SendMessage($"\r\n[Okolí] {newPlayer.Name} se právě připojil.");
                    }
                }

                _parser.ProcessCommand(newPlayer, "prozkoumej");

                // 4. Hlavní herní smyčka
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
                        newPlayer.SendMessage(""); 
                    }
                }
            }
            catch (IOException) { /* Běžné odpojení */ }
            catch (Exception ex)
            {
                _logger.Log($"[Chyba] Klient {clientId}: {ex.Message}");
            }
            finally
            {
                // 5. BEZPEČNÉ ULOŽENÍ A ODHLÁŠENÍ (Spustí se vždy, i při pádu internetu u klienta)
                if (!string.IsNullOrEmpty(newPlayer.Name) && !string.IsNullOrEmpty(passwordHash))
                {
                    _saveManager.SavePlayer(newPlayer, passwordHash);
                    _logger.Log($"Stav hráče {newPlayer.Name} byl bezpečně uložen.");
                }

                lock (_world)
                {
                    if (newPlayer.CurrentRoom != null)
                    {
                        newPlayer.CurrentRoom.Players.Remove(newPlayer);
                        foreach (var p in newPlayer.CurrentRoom.Players)
                        {
                            p.SendMessage($"\r\n[Okolí] {newPlayer.Name} se odpojil.");
                        }
                    }
                }
                
                _connectedClients.TryRemove(clientId, out _);
                client.Close();
                _logger.Log($"Spojení s {newPlayer.Name ?? clientId.ToString()} ukončeno.");
            }
        }
    }
}