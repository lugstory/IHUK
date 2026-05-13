using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class EquipCommand : ICommand
    {
        public string Name => "vybav";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument)) { player.SendMessage("Co chceš vybavit? (např. 'vybav plazmovy_rezak')"); return; }
            if (!player.Inventory.Contains(argument)) { player.SendMessage("Tohle u sebe nemáš."); return; }

            if (world.ItemsDatabase.TryGetValue(argument, out Item item))
            {
                if (item.Type == "Weapon")
                {
                    player.EquippedWeaponId = argument;
                    player.SendMessage($"Vybavil sis {item.Name}. Tvé útoky teď způsobují větší poškození!");
                }
                else
                {
                    player.SendMessage($"{item.Name} není zbraň.");
                }
            }
        }
    }
}