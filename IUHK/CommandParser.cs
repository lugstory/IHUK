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

            string[] parts = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0];
            string argument = parts.Length > 1 ? parts[1] : "";

            lock (_worldLock)
            {
                switch (command)
                {
                    case "jdi":
                        HandleMove(player, argument);
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

            if (player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                // Odejít z aktuální místnosti
                player.CurrentRoom.Players.Remove(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} odešel směrem na {direction}.");

                // Vstoupit do nové
                player.CurrentRoom = nextRoom;
                player.CurrentRoom.Players.Add(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} přišel do místnosti.");

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

        private void HandleHelp(Player player)
        {
            player.SendMessage("Dostupné příkazy: jdi <směr>, prozkoumej, vezmi <předmět>, odloz <předmět>, inventar, mluv <jméno>, pomoc");
        }

        private void BroadcastToRoom(Room room, string message)
        {
            foreach (var p in room.Players)
            {
                p.SendMessage($"\r\n[Okolí] {message}");
            }
        }
    }
}