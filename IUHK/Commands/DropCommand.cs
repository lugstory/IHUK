using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class DropCommand : ICommand
    {
        public string Name => "poloz";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument)) { player.SendMessage("Co chceš položit? (např. 'poloz lekarnicka')"); return; }
            if (!player.Inventory.Contains(argument)) { player.SendMessage("Takový předmět u sebe nemáš."); return; }

            if (player.EquippedWeaponId == argument) player.EquippedWeaponId = null;

            player.Inventory.Remove(argument);
            player.CurrentRoom.Items.Add(argument);
            player.SendMessage($"Položil jsi {argument} na zem.");
            player.CurrentRoom.Broadcast($"{player.Name} upustil na zem {argument}.", player);
        }
    }
}