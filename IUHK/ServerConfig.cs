namespace TartarusMUD.Models
{
    public class ServerConfig
    {
        public int Port { get; set; } = 4000;
        public int MaxPlayers { get; set; } = 20;
        
        // Nové cesty k souborům
        public string RoomsPath { get; set; } = "Data/rooms.json";
        public string ItemsPath { get; set; } = "Data/items.json";
    }
}