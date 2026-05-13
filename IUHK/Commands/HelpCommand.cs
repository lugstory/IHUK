using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class HelpCommand : ICommand
    {
        public string Name => "pomoc";

        public void Execute(Player player, string argument, World world)
        {
            player.SendMessage(
                "--- DOSTUPNÉ PŘÍKAZY ---\r\n" +
                "jdi <směr>      - Pohyb (sever, jih, vychod, zapad)\r\n" +
                "prozkoumej      - Rozhlédne se po místnosti\r\n" +
                "vezmi <věc>     - Sebere předmět\r\n" +
                "poloz <věc>     - Zahodí předmět\r\n" +
                "inventar        - Zobrazí tvé zdraví, zbraň a věci\r\n" +
                "vybav <zbraň>   - Vybaví postavu zbraní pro boj\r\n" +
                "pouziji <věc>   - Použije lékárničku nebo klíčový předmět\r\n" +
                "utoc <cíl>      - Zaútočí na nepřítele (např. utoc mutant)\r\n" +
                "mluv <npc>      - Promluví s postavou\r\n" +
                "konec           - Bezpečně uloží hru a odpojí tě"
            );
        }
    }
}