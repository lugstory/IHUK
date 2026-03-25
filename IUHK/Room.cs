using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TartarusMUD.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Z JSONu načteme jen textová ID (např. "sever" -> "chodba_1")
        public Dictionary<string, string> ExitIds { get; set; } = new Dictionary<string, string>();
        
        // Herní logika bude stále používat toto (skutečné reference)
        [JsonIgnore] // Toto nechceme ukládat/načítat přímo z JSONu
        public Dictionary<string, Room> Exits { get; set; } = new Dictionary<string, Room>();
        
        public List<string> Items { get; set; } = new List<string>();
        public Dictionary<string, string> Npcs { get; set; } = new Dictionary<string, string>();
        
        [JsonIgnore]
        public List<Player> Players { get; set; } = new List<Player>();

        // Bezparametrický konstruktor je nutný pro JSON deserializaci
        public Room() { } 

        public Room(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}