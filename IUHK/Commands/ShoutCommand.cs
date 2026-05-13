using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class ShoutCommand : ICommand
    {
        public string Name => "kric";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument))
            {
                player.SendMessage("Co chceš zakřičet? (např. 'kric Pomoc, je tu mutant!')");
                return;
            }

            // Projdeme úplně všechny místnosti a všechny hráče na serveru
            foreach (var room in world.Rooms.Values)
            {
                foreach (var p in room.Players)
                {
                    if (p == player)
                    {
                        // Zpráva pro toho, kdo křičí
                        p.SendMessage($"Zakřičel jsi z plných plic: \"{argument}\"");
                    }
                    else
                    {
                        // Zpráva pro všechny ostatní na stanici
                        p.SendMessage($"\n[Z dálky se rozléhá] {player.Name} křičí: \"{argument}\"");
                    }
                }
            }
        }
    }
}