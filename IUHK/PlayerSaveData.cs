using System.Collections.Generic;

namespace TartarusMUD.Models
{
    public class PlayerSaveData
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string CurrentRoomId { get; set; }
        public List<string> Inventory { get; set; } = new List<string>();
    }
}