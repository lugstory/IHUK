using System;
using System.Collections.Generic;
using TartarusMUD.Models;
using TartarusMUD.Commands;

namespace TartarusMUD.Core
{
    public class CommandParser
    {
        private readonly World _world;
        private readonly Dictionary<string, ICommand> _commands;

        public CommandParser(World world)
{
    _world = world;
    _commands = new Dictionary<string, ICommand>();

    // tady registrujeme commandy lukasi
    RegisterCommand(new LookCommand());
    RegisterCommand(new MoveCommand());
    RegisterCommand(new AttackCommand());
    RegisterCommand(new UseCommand());
    RegisterCommand(new EquipCommand());
    RegisterCommand(new TakeCommand());
    RegisterCommand(new DropCommand());
    RegisterCommand(new InventoryCommand());
    RegisterCommand(new TalkCommand());
    RegisterCommand(new HelpCommand());
    RegisterCommand(new ShoutCommand());
}

        private void RegisterCommand(ICommand command)
        {
            _commands[command.Name] = command;
        }

        public void ProcessCommand(Player player, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string commandName = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1] : "";

            // Místo switche se jen podíváme do slovníku!
            if (_commands.TryGetValue(commandName, out ICommand command))
            {
                command.Execute(player, argument, _world);
            }
            else
            {
                player.SendMessage("Neznámý příkaz. Napiš 'pomoc' pro seznam příkazů.");
            }
        }
    }
}