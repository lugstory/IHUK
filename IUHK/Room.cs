using System.Collections.Generic;

namespace TartarusMUD.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Mapa východů (Klíč = směr, Hodnota = cílová místnost)
        public Dictionary<string, Room> Exits { get; set; } = new Dictionary<string, Room>();
        
        public List<string> Items { get; set; } = new List<string>();
        public Dictionary<string, string> Npcs { get; set; } = new Dictionary<string, string>();
        
        // Seznam hráčů aktuálně v této místnosti
        public List<Player> Players { get; set; } = new List<Player>();

        public Room(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}