using TartarusMUD.Models;
using TartarusMUD.Core;
namespace TartarusMUD.Commands
{
    public class TalkCommand : ICommand
    {
        public string Name => "mluv";

        public void Execute(Player player, string argument, World world)
        {
            if (string.IsNullOrEmpty(argument)) { player.SendMessage("S kým chceš mluvit? (např. 'mluv bot')"); return; }
            
            if (player.CurrentRoom.Npcs.TryGetValue(argument, out Npc npc))
            {
                player.SendMessage($"[{npc.Name}]: {npc.Dialogue}");
                player.CurrentRoom.Broadcast($"{player.Name} mluví s {npc.Name}.", player);
            }
            else
            {
                player.SendMessage("Nikdo takový tu není.");
            }
        }
    }
}