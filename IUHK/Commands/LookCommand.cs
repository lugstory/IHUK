using TartarusMUD.Models;
using TartarusMUD.Core;

namespace TartarusMUD.Commands
{
    public class LookCommand : ICommand
    {
        public string Name => "prozkoumej";

        public void Execute(Player player, string argument, World world)
        {
            Room r = player.CurrentRoom;
            string output = $"\n--- {r.Name.ToUpper()} ---\r\n{r.Description}\r\n";
            output += $"Východy: {string.Join(", ", r.Exits.Keys)}\r\n";

            if (r.LockedExits.Count > 0)
                output += $"Zamčené dveře: {string.Join(", ", r.LockedExits.Keys)}\r\n";

            if (r.Items.Count > 0)
                output += $"Na zemi leží: {string.Join(", ", r.Items)}\r\n";

            if (r.Npcs.Count > 0)
            {
                output += "Postavy v místnosti:\r\n";
                foreach (var npc in r.Npcs.Values)
                    output += $"- {npc.Name} ({npc.Id})\r\n";
            }

            player.SendMessage(output);
        }
    }
}