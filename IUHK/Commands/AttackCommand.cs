using System;
using TartarusMUD.Models;
using TartarusMUD.Core;

namespace TartarusMUD.Commands
{
    public class AttackCommand : ICommand
    {
        public string Name => "utoc";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument))
            {
                player.SendMessage("Na koho chceš zaútočit? (např. 'utoc mutant')");
                return;
            }

            if (!player.CurrentRoom.Npcs.TryGetValue(argument, out Npc target))
            {
                player.SendMessage($"Nikdo takový ('{argument}') tu není.");
                return;
            }

            // 1. Útok hráče s bonusem ze zbraně
            int totalDamage = player.BaseDamage;
            string attackText = "Praštil jsi";

            if (!string.IsNullOrEmpty(player.EquippedWeaponId) && world.ItemsDatabase.TryGetValue(player.EquippedWeaponId, out Item weapon))
            {
                totalDamage += weapon.DamageBonus;
                attackText = $"Použil jsi {weapon.Name} a zasáhl";
            }

            target.Hp -= totalDamage;
            player.SendMessage($"{attackText} {target.Name} za {totalDamage} poškození. (Zbývá mu {target.Hp} HP)");
            player.CurrentRoom.Broadcast($"{player.Name} zaútočil na {target.Name}.", player);

            // 2. Smrt nepřítele
            if (target.Hp <= 0)
            {
                player.SendMessage($"Zabil jsi {target.Name}!");
                player.CurrentRoom.Broadcast($"{target.Name} s řevem padl mrtev k zemi.", player);
                player.CurrentRoom.Npcs.Remove(argument);
                return;
            }

            // 3. Protiútok a krvácení
            if (target.IsHostile)
            {
                player.Hp -= target.Damage;
                string attackMsg = $"\n[POZOR] {target.Name} se ohnal a zasáhl tě za {target.Damage} poškození! (Máš {player.Hp}/{player.MaxHp} HP)";

                if (!player.IsBleeding && new Random().Next(0, 2) == 0)
                {
                    player.IsBleeding = true;
                    attackMsg += "\n[!!!] Jeho pařáty zašly hluboko. ZAČÍNÁŠ KRVÁCET!";
                }

                player.SendMessage(attackMsg);
                player.CurrentRoom.Broadcast($"{target.Name} brutálně zasáhl hráče {player.Name}.", player);

                // 4. Smrt hráče
                if (player.Hp <= 0)
                {
                    player.SendMessage("\nZEMŘEL JSI...\nTemnota tě pohltila, ale klonovací systém tě znovu probudil v kryokomoře.");
                    player.CurrentRoom.Broadcast($"{player.Name} utržil smrtelnou ránu a jeho tělo se rozpadlo na prach.", player);
                    
                    player.CurrentRoom.Players.Remove(player);
                    
                    player.Hp = player.MaxHp;
                    player.IsBleeding = false;
                    player.CurrentRoom = world.StartRoom;
                    player.CurrentRoom.Players.Add(player);
                    
                    // Automaticky rozhlédnout po respawnu
                    new LookCommand().Execute(player, "", world);
                }
            }
        }
    }
}