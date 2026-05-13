using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class InventoryCommand : ICommand
    {
        public string Name => "inventar";

        public void Execute(Player player, string argument, World world)
        {
            string weaponName = "Pěsti";
            if (!string.IsNullOrEmpty(player.EquippedWeaponId) && world.ItemsDatabase.TryGetValue(player.EquippedWeaponId, out Item weapon))
            {
                weaponName = weapon.Name;
            }

            string output = $"=== STAV HRÁČE ===\r\nZdraví: {player.Hp}/{player.MaxHp} HP\r\nZbraň: {weaponName}\r\n------------------\r\nInventář: ";
            
            if (player.Inventory.Count == 0) output += "prázdný";
            else output += string.Join(", ", player.Inventory);
            
            player.SendMessage(output);
        }
    }
}