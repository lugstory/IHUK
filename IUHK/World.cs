using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TartarusMUD.Models;

namespace TartarusMUD.Core
{
    public class World
    {
        public Room StartRoom { get; private set; }
        
        // Slovník všech místností pro rychlé vyhledávání podle ID
        private Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

        public World()
        {
            LoadWorld();
        }

        private void LoadWorld()
        {
            string filePath = Path.Combine("Data", "rooms.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Kritická chyba: Soubor s mapou nebyl nalezen na cestě {filePath}");
            }

            try
            {
                // 1. FÁZE: Načtení dat z JSONu
                string jsonString = File.ReadAllText(filePath);
                List<Room> loadedRooms = JsonSerializer.Deserialize<List<Room>>(jsonString);

                // Uložení do slovníku
                foreach (var room in loadedRooms)
                {
                    _rooms[room.Id] = room;
                }

                // 2. FÁZE: Propojení východů (převod textových ID na skutečné objekty Room)
                foreach (var room in _rooms.Values)
                {
                    foreach (var exit in room.ExitIds)
                    {
                        string direction = exit.Key;
                        string targetRoomId = exit.Value;

                        if (_rooms.TryGetValue(targetRoomId, out Room targetRoom))
                        {
                            room.Exits[direction] = targetRoom;
                        }
                        else
                        {
                            Console.WriteLine($"[Varování mapy] Místnost {room.Id} má východ '{direction}' do neexistující místnosti '{targetRoomId}'!");
                        }
                    }
                }

                // Nastavíme startovní místnost (např. první z načtených)
                if (loadedRooms.Count > 0)
                {
                    StartRoom = loadedRooms[0];
                    Console.WriteLine($"[Systém] Mapa úspěšně načtena. Počet místností: {_rooms.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Kritická chyba] Nepodařilo se načíst mapu: {ex.Message}");
            }
        }
        
        // Pomocná metoda pro pozdější použití (např. při obnově pozice hráče)
        public Room GetRoomById(string id)
        {
            _rooms.TryGetValue(id, out Room room);
            return room;
        }
    }
}