using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class UseCommand : ICommand
    {
        public string Name => "pouziji";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument)) { player.SendMessage("Co chceš použít? (např. 'pouziji lekarnicka')"); return; }
            if (!player.Inventory.Contains(argument)) { player.SendMessage("Tohle u sebe nemáš."); return; }

            if (world.ItemsDatabase.TryGetValue(argument, out Item item))
            {
                if (item.Type == "Consumable")
                {
                    player.Hp += item.HealAmount;
                    if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
                    
                    string healMsg = $"Použil jsi {item.Name} a doplnil si {item.HealAmount} HP. (Máš {player.Hp}/{player.MaxHp} HP)";
                    
                    if (player.IsBleeding)
                    {
                        player.IsBleeding = false;
                        healMsg += "\n[+] Rána je obvázána. Krvácení bylo zastaveno!";
                    }
                    
                    player.Inventory.Remove(argument); 
                    player.SendMessage(healMsg);
                }
                else if (item.Type == "QuestItem" && item.Id == "fuzni_baterie")
                {
                    if (player.CurrentRoom.Id == "strojovna_1")
                    {
                        player.Inventory.Remove(argument);
                        string winMessage = "\n==================================================\n" +
                                            "[SYSTÉM] Fúzní baterie přijata.\n" +
                                            "[SYSTÉM] Inicializace fúzní reakce... 3... 2... 1...\n" +
                                            "[SYSTÉM] Generátor naběhl na 100% výkon.\n" +
                                            "[SYSTÉM] Podpora života plně obnovena.\n\n" +
                                            "   *** GRATULUJEME! ZACHRÁNIL JSI STANICI TARTARUS! ***\n" +
                                            "==================================================\n";
                        
                        player.SendMessage(winMessage);
                        player.CurrentRoom.Broadcast($"\n[Okolí] {player.Name} vložil baterii do generátoru. Celá stanice se otřásla a zaplavilo ji jasné světlo. Zachránil nás všechny!", player);
                    }
                    else
                    {
                        player.SendMessage("Tady s tím nemůžeš nic udělat. Zkus to odnést do Strojovny k hlavnímu generátoru.");
                    }
                }
                else if (item.Type == "Weapon")
                {
                    player.SendMessage($"Zbraň '{item.Name}' nemůžeš 'použít'. Zkus příkaz 'vybav {argument}'.");
                }
                else
                {
                    player.SendMessage($"{item.Name} se takto nedá použít.");
                }
            }
        }
    }
}