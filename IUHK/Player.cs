using System.Collections.Generic;
using System.IO;

namespace TartarusMUD.Models
{
    public class Player
    {
        public string Name { get; set; }
        public Room CurrentRoom { get; set; }
        public List<string> Inventory { get; set; } = new List<string>();
        public StreamWriter Writer { get; private set; }
        public int Hp { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public int BaseDamage { get; set; } = 10;
        public string EquippedWeaponId { get; set; } = null; // ID aktuálně držené zbraně

        public Player(StreamWriter writer)
        {
            Writer = writer;
        }

        // Pomocná metoda pro odeslání textu hráči
        public void SendMessage(string message)
        {
            try
            {
                Writer.Write(message + "\r\n> ");
                Writer.Flush();
            }
            catch
            {
                // Pokud spojení spadne během zápisu, chybu ignorujeme
            }
        }
    }
}