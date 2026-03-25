using System;
using System.Linq;
using TartarusMUD.Models;

namespace TartarusMUD.Core
{
    public class CommandParser
    {
        private readonly int _maxInventorySize = 5;
        // Zámek pro synchronizaci změn ve světě napříč asynchronními vlákny
        private static readonly object _worldLock = new object();

        public void ProcessCommand(Player player, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // ZMĚNA: Rozdělíme text maximálně na 2 části, aby zbytek věty zůstal neporušený
            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower(); // Příkaz samotný chceme vždy malými písmeny
            string argument = parts.Length > 1 ? parts[1].Trim() : "";

            lock (_worldLock)
            {
                switch (command)
                {
                    case "jdi":
                        HandleMove(player, argument);
                        break;
                    // ... (ostatní stávající příkazy nech beze změny) ...
                    // NOVÝ PŘÍKAZ PRO CHAT:
                    case "rekni":
                        HandleSay(player, argument);
                        break;
                    case "prozkoumej":
                        HandleLook(player);
                        break;
                    case "vezmi":
                        HandleTake(player, argument);
                        break;
                    case "odloz":
                        HandleDrop(player, argument);
                        break;
                    case "inventar":
                        HandleInventory(player);
                        break;
                    case "mluv":
                        HandleTalk(player, argument);
                        break;
                    case "pomoc":
                        HandleHelp(player);
                        break;
                    case "odemkni":
                        HandleUnlock(player, argument);
                        break;
                    default:
                        player.SendMessage("Neznámý příkaz. Napiš 'pomoc' pro seznam příkazů.");
                        break;
                }
            }
        }

        private void HandleMove(Player player, string direction)
        {
            if (string.IsNullOrEmpty(direction))
            {
                player.SendMessage("Musíš zadat směr (např. 'jdi sever').");
                return;
            }

            // 1. KONTROLA ZÁMKU: Musí být jako první!
            if (player.CurrentRoom.LockedExits.ContainsKey(direction))
            {
                player.SendMessage($"Dveře směrem na '{direction}' jsou pevně zavřené a zamčené.");
                return; // Příkaz return okamžitě ukončí metodu, takže se kód pro pohyb vůbec nespustí.
            }

            // 2. VLASTNÍ POHYB: Provede se jen tehdy, pokud nás nezastavil zámek výše.
            if (player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                // Odejít z aktuální místnosti
                player.CurrentRoom.Players.Remove(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} odešel směrem na {direction}.");

                // Vstoupit do nové
                player.CurrentRoom = nextRoom;
                player.CurrentRoom.Players.Add(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} přišel z jiného směru.");

                // Automaticky prozkoumat po příchodu
                HandleLook(player);
            }
            else
            {
                player.SendMessage("Tímto směrem jít nemůžeš.");
            }
        }

        private void HandleLook(Player player)
        {
            Room r = player.CurrentRoom;
            string output = $"--- {r.Name.ToUpper()} ---\r\n{r.Description}\r\n";
            
            output += $"Východy: {(r.Exits.Count > 0 ? string.Join(", ", r.Exits.Keys) : "žádné")}\r\n";
            if (r.LockedExits.Count > 0)
                output += $"Zamčené dveře: {string.Join(", ", r.LockedExits.Keys)}\r\n";
            
            if (r.Items.Count > 0)
                output += $"Předměty zde: {string.Join(", ", r.Items)}\r\n";
                
            if (r.Npcs.Count > 0)
                output += $"Postavy/Stroje: {string.Join(", ", r.Npcs.Keys)}\r\n";

            var otherPlayers = r.Players.Where(p => p != player).Select(p => p.Name).ToList();
            if (otherPlayers.Count > 0)
                output += $"Ostatní hráči: {string.Join(", ", otherPlayers)}\r\n";
            

            player.SendMessage(output);
        }

        private void HandleTake(Player player, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                player.SendMessage("Co chceš vzít?");
                return;
            }

            if (!player.CurrentRoom.Items.Contains(item))
            {
                player.SendMessage($"Předmět '{item}' tady není.");
                return;
            }

            if (player.Inventory.Count >= _maxInventorySize)
            {
                player.SendMessage("Máš plný inventář!");
                return;
            }

            player.CurrentRoom.Items.Remove(item);
            player.Inventory.Add(item);
            player.SendMessage($"Vzal jsi {item}.");
            BroadcastToRoom(player.CurrentRoom, $"{player.Name} sebral {item}.");
        }

        private void HandleDrop(Player player, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                player.SendMessage("Co chceš odložit?");
                return;
            }

            if (!player.Inventory.Contains(item))
            {
                player.SendMessage($"Předmět '{item}' nemáš u sebe.");
                return;
            }

            player.Inventory.Remove(item);
            player.CurrentRoom.Items.Add(item);
            player.SendMessage($"Odložil jsi {item}.");
            BroadcastToRoom(player.CurrentRoom, $"{player.Name} odložil {item}.");
        }

        private void HandleInventory(Player player)
        {
            if (player.Inventory.Count == 0)
            {
                player.SendMessage("Tvůj inventář je prázdný.");
            }
            else
            {
                player.SendMessage($"Inventář ({player.Inventory.Count}/{_maxInventorySize}): {string.Join(", ", player.Inventory)}");
            }
        }

        private void HandleTalk(Player player, string npcName)
        {
            if (string.IsNullOrEmpty(npcName))
            {
                player.SendMessage("S kým chceš mluvit?");
                return;
            }

            if (player.CurrentRoom.Npcs.TryGetValue(npcName, out string dialogue))
            {
                player.SendMessage($"[{npcName}]: {dialogue}");
            }
            else
            {
                player.SendMessage($"Nikdo jménem '{npcName}' tu není.");
            }
        }
        private void HandleSay(Player player, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                player.SendMessage("Co chceš říct? (použití: rekni <text>)");
                return;
            }

            // 1. Odezva samotnému hráči
            player.SendMessage($"Říkáš: \"{message}\"");

            // 2. Odeslání všem ostatním v téže místnosti
            // Používáme .Where, abychom zprávu neposlali znovu odesílateli
            var otherPlayers = player.CurrentRoom.Players.Where(p => p != player);
    
            foreach (var p in otherPlayers)
            {
                p.SendMessage($"\r\n[{player.Name}] říká: \"{message}\"");
            }
        }

        private void HandleHelp(Player player)
        {
            player.SendMessage("Dostupné příkazy: jdi <směr>, prozkoumej, vezmi <předmět>, odloz <předmět>, inventar, mluv <jméno>, rekni <text>, pomoc, odemkni <smer>");
        }

        private void BroadcastToRoom(Room room, string message)
        {
            foreach (var p in room.Players)
            {
                p.SendMessage($"\r\n[Okolí] {message}");
            }
        }
        private void HandleUnlock(Player player, string direction)
        {
            if (string.IsNullOrEmpty(direction))
            {
                player.SendMessage("Co chceš odemknout? (např. 'odemkni zapad')");
                return;
            }

            if (player.CurrentRoom.LockedExits.TryGetValue(direction, out string requiredKey))
            {
                // Má hráč potřebný klíč v inventáři?
                if (player.Inventory.Contains(requiredKey))
                {
                    // Odemkneme dveře (smažeme je ze seznamu zamčených)
                    player.CurrentRoom.LockedExits.Remove(direction);
            
                    player.SendMessage($"Použil jsi {requiredKey} a odemkl dveře na {direction}!");
                    BroadcastToRoom(player.CurrentRoom, $"{player.Name} odemkl dveře na {direction}.");
                }
                else
                {
                    player.SendMessage($"K odemčení těchto dveří potřebuješ: {requiredKey}");
                }
            }
            else if (player.CurrentRoom.Exits.ContainsKey(direction))
            {
                player.SendMessage("Tyto dveře nejsou zamčené.");
            }
            else
            {
                player.SendMessage("Tímto směrem žádné dveře nejsou.");
            }
        }
    }
}