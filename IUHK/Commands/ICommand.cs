using TartarusMUD.Core;
using TartarusMUD.Models;

namespace TartarusMUD.Commands
{
    public interface ICommand
    {
        // Slovo, kterým se příkaz spouští (např. "jdi")
        string Name { get; } 
        
        // Samotná logika příkazu
        void Execute(Player player, string argument, World world);
    }
}