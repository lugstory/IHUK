using TartarusMUD.Models;
using TartarusMUD.Core;

namespace TartarusMUD.Commands
{
    public class MoveCommand : ICommand
    {
        public string Name => "jdi";

        public void Execute(Player player, string direction, World world)
        {
            if (string.IsNullOrEmpty(direction))
            {
                player.SendMessage("Musíš zadat směr (např. 'jdi sever').");
                return;
            }

            if (player.CurrentRoom.LockedExits.TryGetValue(direction, out string requiredKey))
            {
                if (player.Inventory.Contains(requiredKey))
                {
                    player.CurrentRoom.LockedExits.Remove(direction);
                    player.SendMessage($"[Automaticky jsi použil {requiredKey} a odemkl dveře!]");
                    player.CurrentRoom.Broadcast($"{player.Name} odemkl dveře na {direction}.", player);
                }
                else
                {
                    player.SendMessage($"Dveře směrem na '{direction}' jsou zamčené. Potřebuješ: {requiredKey}");
                    return; 
                }
            }

            if (player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                player.CurrentRoom.Players.Remove(player);
                player.CurrentRoom.Broadcast($"{player.Name} odešel směrem na {direction}.");

                player.CurrentRoom = nextRoom;
                player.CurrentRoom.Players.Add(player);
                player.CurrentRoom.Broadcast($"{player.Name} přišel z jiného směru.", player);

                // Místo volání staré metody si zavoláme LookCommand
                new LookCommand().Execute(player, "", world);
            }
            else
            {
                player.SendMessage("Tímto směrem jít nemůžeš.");
            }
        }
    }
}