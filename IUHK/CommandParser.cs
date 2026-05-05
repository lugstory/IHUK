using System;
using System.Linq;
using TartarusMUD.Models;

namespace TartarusMUD.Core
{
    public class CommandParser
    {
        private readonly int _maxInventorySize = 5;
        // Zámek pro synchronizaci změn ve světě napříč asynchronními vlákny
        private static readonly object _worldLock = new object();
		private readonly World _world; // Přidáno

		public CommandParser(World world) // Nový konstruktor
		{
   		 _world = world;
		}

        public void ProcessCommand(Player player, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // ZMĚNA: Rozdělíme text maximálně na 2 části, aby zbytek věty zůstal neporušený
            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower(); // Příkaz samotný chceme vždy malými písmeny
            string argument = parts.Length > 1 ? parts[1].Trim() : "";

            lock (_worldLock)
            {
                switch (command)
                {
                    case "jdi":
                        HandleMove(player, argument);
                        break;
                    // ... (ostatní stávající příkazy nech beze změny) ...
                    // NOVÝ PŘÍKAZ PRO CHAT:
                    case "rekni":
                        HandleSay(player, argument);
                        break;
                    case "prozkoumej":
                        HandleLook(player);
                        break;
case "pouziji":
    HandleUse(player, argument);
    break;
case "vybav":
    HandleEquip(player, argument);
    break;
                    case "vezmi":
                        HandleTake(player, argument);
                        break;
					case "utoc":
    					HandleAttack(player, argument);
   						 break;
                    case "odloz":
                        HandleDrop(player, argument);
                        break;
                    case "inventar":
                        HandleInventory(player);
                        break;
                    case "mluv":
                        HandleTalk(player, argument);
                        break;
                    case "pomoc":
                        HandleHelp(player);
                        break;
                    case "odemkni":
                        HandleUnlock(player, argument);
                        break;
                    default:
                        player.SendMessage("Neznámý příkaz. Napiš 'pomoc' pro seznam příkazů.");
                        break;
                }
            }
        }

        private void HandleMove(Player player, string direction)
        {
            if (string.IsNullOrEmpty(direction))
            {
                player.SendMessage("Musíš zadat směr (např. 'jdi sever').");
                return;
            }

            // 1. KONTROLA ZÁMKU: Musí být jako první!
            if (player.CurrentRoom.LockedExits.ContainsKey(direction))
            {
                player.SendMessage($"Dveře směrem na '{direction}' jsou pevně zavřené a zamčené.");
                return; // Příkaz return okamžitě ukončí metodu, takže se kód pro pohyb vůbec nespustí.
            }

            // 2. VLASTNÍ POHYB: Provede se jen tehdy, pokud nás nezastavil zámek výše.
            if (player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                // Odejít z aktuální místnosti
                player.CurrentRoom.Players.Remove(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} odešel směrem na {direction}.");

                // Vstoupit do nové
                player.CurrentRoom = nextRoom;
                player.CurrentRoom.Players.Add(player);
                BroadcastToRoom(player.CurrentRoom, $"{player.Name} přišel z jiného směru.");

                // Automaticky prozkoumat po příchodu
                HandleLook(player);
            }
            else
            {
                player.SendMessage("Tímto směrem jít nemůžeš.");
            }
        }

        private void HandleLook(Player player)
        {
            Room r = player.CurrentRoom;
            string output = $"--- {r.Name.ToUpper()} ---\r\n{r.Description}\r\n";
            
            output += $"Východy: {(r.Exits.Count > 0 ? string.Join(", ", r.Exits.Keys) : "žádné")}\r\n";
            if (r.LockedExits.Count > 0)
                output += $"Zamčené dveře: {string.Join(", ", r.LockedExits.Keys)}\r\n";
            
            if (r.Items.Count > 0)
                output += $"Předměty zde: {string.Join(", ", r.Items)}\r\n";
                
            if (r.Npcs.Count > 0)
                output += $"Postavy/Stroje: {string.Join(", ", r.Npcs.Keys)}\r\n";

            var otherPlayers = r.Players.Where(p => p != player).Select(p => p.Name).ToList();
            if (otherPlayers.Count > 0)
                output += $"Ostatní hráči: {string.Join(", ", otherPlayers)}\r\n";
            

            player.SendMessage(output);
        }

        private void HandleTake(Player player, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                player.SendMessage("Co chceš vzít?");
                return;
            }

            if (!player.CurrentRoom.Items.Contains(item))
            {
                player.SendMessage($"Předmět '{item}' tady není.");
                return;
            }

            if (player.Inventory.Count >= _maxInventorySize)
            {
                player.SendMessage("Máš plný inventář!");
                return;
            }

            player.CurrentRoom.Items.Remove(item);
            player.Inventory.Add(item);
            player.SendMessage($"Vzal jsi {item}.");
            BroadcastToRoom(player.CurrentRoom, $"{player.Name} sebral {item}.");
        }

        private void HandleDrop(Player player, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                player.SendMessage("Co chceš odložit?");
                return;
            }

            if (!player.Inventory.Contains(item))
            {
                player.SendMessage($"Předmět '{item}' nemáš u sebe.");
                return;
            }

            player.Inventory.Remove(item);
            player.CurrentRoom.Items.Add(item);
            player.SendMessage($"Odložil jsi {item}.");
            BroadcastToRoom(player.CurrentRoom, $"{player.Name} odložil {item}.");
        }
	private void HandleAttack(Player player, string targetId)
{
    if (string.IsNullOrEmpty(targetId))
    {
        player.SendMessage("Na koho chceš zaútočit? (např. 'utoc mutant')");
        return;
    }

    if (!player.CurrentRoom.Npcs.TryGetValue(targetId, out Npc target))
    {
        player.SendMessage($"Nikdo takový ('{targetId}') tu není.");
        return;
    }

    // 1. Útok hráče
    int totalDamage = player.BaseDamage;
string attackText = "Praštil jsi";

// Pokud má zbraň, přidáme její poškození
if (!string.IsNullOrEmpty(player.EquippedWeaponId) && _world.ItemsDatabase.TryGetValue(player.EquippedWeaponId, out Item weapon))
{
    totalDamage += weapon.DamageBonus;
    attackText = $"Použil jsi {weapon.Name} a zasáhl";
}

target.Hp -= totalDamage;
player.SendMessage($"{attackText} {target.Name} za {totalDamage} poškození. (Zbývá mu {target.Hp} HP)");
BroadcastToRoom(player.CurrentRoom, $"{player.Name} zaútočil na {target.Name}.");

    // 2. Smrt nepřítele
    if (target.Hp <= 0)
    {
        player.SendMessage($"Zabil jsi {target.Name}!");
        BroadcastToRoom(player.CurrentRoom, $"{target.Name} s řevem padl mrtev k zemi.");
        player.CurrentRoom.Npcs.Remove(targetId);
        return; // Souboj končí
    }

    // 3. Protiútok (Pokud je nepřítel agresivní)
    if (target.IsHostile)
    {
        player.Hp -= target.Damage;
        player.SendMessage($"[POZOR] {target.Name} se ohnal a zasáhl tě za {target.Damage} poškození! (Máš {player.Hp}/{player.MaxHp} HP)");
        BroadcastToRoom(player.CurrentRoom, $"{target.Name} brutálně zasáhl hráče {player.Name}.");

        // 4. Smrt hráče (Respawn)
        if (player.Hp <= 0)
        {
            player.SendMessage("\nZEMŘEL JSI...\nTemnota tě pohltila, ale klonovací systém tě znovu probudil v kryokomoře.");
            BroadcastToRoom(player.CurrentRoom, $"{player.Name} utržil smrtelnou ránu a jeho tělo se rozpadlo na prach.");
            
            player.CurrentRoom.Players.Remove(player);
            
            // Oživíme ho, doplníme HP a pošleme na start
            player.Hp = player.MaxHp;
            player.CurrentRoom = _world.StartRoom;
            player.CurrentRoom.Players.Add(player);
            
            HandleLook(player); // Vykreslíme mu novou místnost
        }
    }

    if (target.IsHostile)
    {
        player.Hp -= target.Damage;
        string attackMsg =
            $"\n[POZOR] {target.Name} se ohnal a zasáhl tě za {target.Damage} poškození! (Máš {player.Hp}/{player.MaxHp} HP)";

        // 50% šance, že tě mutant poškrábe a začneš krvácet
        if (!player.IsBleeding && new Random().Next(0, 2) == 0)
        {
            player.IsBleeding = true;
            attackMsg += "\n[!!!] Jeho pařáty zašly hluboko. ZAČÍNÁŠ KRVÁCET!";
        }

        player.SendMessage(attackMsg);
        BroadcastToRoom(player.CurrentRoom, $"{target.Name} brutálně zasáhl hráče {player.Name}.");

        // 4. Smrt hráče... (zbytek zůstává stejný, jen přidej vynulování krvácení)
        if (player.Hp <= 0)
        {
            // ... (tvůj stávající kód pro smrt)
            player.Hp = player.MaxHp;
            player.IsBleeding = false; // Po oživení už nekrvácí
            // ...
        }
    }
}

        private void HandleInventory(Player player)
{
    string weaponName = "Pěsti";
    if (!string.IsNullOrEmpty(player.EquippedWeaponId) && _world.ItemsDatabase.TryGetValue(player.EquippedWeaponId, out Item weapon))
    {
        weaponName = weapon.Name;
    }

    string output = $"=== STAV HRÁČE ===\r\nZdraví: {player.Hp}/{player.MaxHp} HP\r\nZbraň: {weaponName}\r\n------------------\r\nInventář: ";
    
    if (player.Inventory.Count == 0) output += "prázdný";
    else output += string.Join(", ", player.Inventory);
    
    player.SendMessage(output);
}

        private void HandleTalk(Player player, string npcName)
        {
            if (string.IsNullOrEmpty(npcName))
            {
                player.SendMessage("S kým chceš mluvit?");
                return;
            }

           if (player.CurrentRoom.Npcs.TryGetValue(npcName, out Npc npc))
			{
   			 player.SendMessage($"[{npc.Name}]: {npc.Dialogue}");
			}
            else
            {
                player.SendMessage($"Nikdo jménem '{npcName}' tu není.");
            }
        }
        private void HandleSay(Player player, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                player.SendMessage("Co chceš říct? (použití: rekni <text>)");
                return;
            }

            // 1. Odezva samotnému hráči
            player.SendMessage($"Říkáš: \"{message}\"");

            // 2. Odeslání všem ostatním v téže místnosti
            // Používáme .Where, abychom zprávu neposlali znovu odesílateli
            var otherPlayers = player.CurrentRoom.Players.Where(p => p != player);
    
            foreach (var p in otherPlayers)
            {
                p.SendMessage($"\r\n[{player.Name}] říká: \"{message}\"");
            }
        }

        private void HandleHelp(Player player)
        {
            player.SendMessage("Dostupné příkazy: jdi <směr>, prozkoumej, vezmi <předmět>, odloz <předmět>, inventar, mluv <jméno>, rekni <text>, pomoc, odemkni <smer>, utoc <jmeno>");
        }

        private void BroadcastToRoom(Room room, string message)
        {
            foreach (var p in room.Players)
            {
                p.SendMessage($"\r\n[Okolí] {message}");
            }
        }
        private void HandleUnlock(Player player, string direction)
        {
            if (string.IsNullOrEmpty(direction))
            {
                player.SendMessage("Co chceš odemknout? (např. 'odemkni zapad')");
                return;
            }

            if (player.CurrentRoom.LockedExits.TryGetValue(direction, out string requiredKey))
            {
                // Má hráč potřebný klíč v inventáři?
                if (player.Inventory.Contains(requiredKey))
                {
                    // Odemkneme dveře (smažeme je ze seznamu zamčených)
                    player.CurrentRoom.LockedExits.Remove(direction);
            
                    player.SendMessage($"Použil jsi {requiredKey} a odemkl dveře na {direction}!");
                    BroadcastToRoom(player.CurrentRoom, $"{player.Name} odemkl dveře na {direction}.");
                }
                else
                {
                    player.SendMessage($"K odemčení těchto dveří potřebuješ: {requiredKey}");
                }
            }
            else if (player.CurrentRoom.Exits.ContainsKey(direction))
            {
                player.SendMessage("Tyto dveře nejsou zamčené.");
            }
            else
            {
                player.SendMessage("Tímto směrem žádné dveře nejsou.");
            }
        }
    
private void HandleUse(Player player, string itemId)
{
    if (string.IsNullOrEmpty(itemId)) { player.SendMessage("Co chceš použít? (např. 'pouziji lekarnicka')"); return; }
    if (!player.Inventory.Contains(itemId)) { player.SendMessage("Tohle u sebe nemáš."); return; }

    if (_world.ItemsDatabase.TryGetValue(itemId, out Item item))
    {
        if (item.Type == "Consumable")
        {
            player.Hp += item.HealAmount;
            if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
    
            string healMsg = $"Použil jsi {item.Name} a doplnil si {item.HealAmount} HP. (Máš {player.Hp}/{player.MaxHp} HP)";
    
            // Zastavení krvácení
            if (player.IsBleeding)
            {
                player.IsBleeding = false;
                healMsg += "\n[+] Rána je obvázána. Krvácení bylo zastaveno!";
            }
    
            player.Inventory.Remove(itemId); 
            player.SendMessage(healMsg);
        }
        else if (item.Type == "QuestItem" && item.Id == "fuzni_baterie")
        {
            // Zkontrolujeme, zda hráč stojí u generátoru
            if (player.CurrentRoom.Id == "strojovna_1")
            {
                player.Inventory.Remove(itemId);
                
                string winMessage = "\n==================================================\n" +
                                    "[SYSTÉM] Fúzní baterie přijata.\n" +
                                    "[SYSTÉM] Inicializace fúzní reakce... 3... 2... 1...\n" +
                                    "[SYSTÉM] Generátor naběhl na 100% výkon.\n" +
                                    "[SYSTÉM] Podpora života plně obnovena.\n\n" +
                                    "   *** GRATULUJEME! ZACHRÁNIL JSI STANICI TARTARUS! ***\n" +
                                    "==================================================\n";
                
                player.SendMessage(winMessage);
                BroadcastToRoom(player.CurrentRoom, $"\n[Okolí] {player.Name} vložil baterii do generátoru. Celá stanice se otřásla a zaplavilo ji jasné světlo. Zachránil nás všechny!");
            }
            else
            {
                player.SendMessage("Tady s tím nemůžeš nic udělat. Zkus to odnést do Strojovny k hlavnímu generátoru.");
            }
        }
        else if (item.Type == "Weapon")
        {
            player.SendMessage($"Zbraň '{item.Name}' nemůžeš 'použít'. Zkus příkaz 'vybav {itemId}'.");
        }
        else
        {
            player.SendMessage($"{item.Name} se takto nedá použít.");
        }
    }
}

private void HandleEquip(Player player, string itemId)
{
    if (string.IsNullOrEmpty(itemId)) { player.SendMessage("Co chceš vybavit? (např. 'vybav plazmovy_rezak')"); return; }
    if (!player.Inventory.Contains(itemId)) { player.SendMessage("Tohle u sebe nemáš."); return; }

    if (_world.ItemsDatabase.TryGetValue(itemId, out Item item))
    {
        if (item.Type == "Weapon")
        {
            player.EquippedWeaponId = itemId;
            player.SendMessage($"Vybavil sis {item.Name}. Tvé útoky teď způsobují větší poškození!");
        }
        else
        {
            player.SendMessage($"{item.Name} není zbraň.");
        }
    }
}}}
