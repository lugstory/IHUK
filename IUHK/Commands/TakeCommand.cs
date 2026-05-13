using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class TakeCommand : ICommand
    {
        public string Name => "vezmi";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument)) { player.SendMessage("Co chceš vzít? (např. 'vezmi lekarnicka')"); return; }
            if (!player.CurrentRoom.Items.Contains(argument)) { player.SendMessage("Takový předmět tu není."); return; }
            if (player.Inventory.Count >= 5) { player.SendMessage("Tvůj inventář je plný (max 5 předmětů)."); return; }

            player.CurrentRoom.Items.Remove(argument);
            player.Inventory.Add(argument);
            player.SendMessage($"Vzal jsi {argument}.");
            player.CurrentRoom.Broadcast($"{player.Name} sebral ze země {argument}.", player);
        }
    }
}