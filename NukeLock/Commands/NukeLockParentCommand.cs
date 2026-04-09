using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NukeLock.Commands.Subcmds;
using System;
using LabApi.Features.Console;

namespace NukeLock.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NukeLockParentCommand : ParentCommand
    {
        public NukeLockParentCommand() => LoadGeneratedCommands();

        public override string Command => "nukelock";
        public override string[] Aliases => ["nl"];
        public override string Description => "Parent command for NukeLock";

        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new Arm());
            RegisterCommand(new Lock());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Logger.Debug("NukeLockParentCommand: ExecuteParent called for NukeLock! Running..");
            Player player = Player.Get(sender);
            if (!player.RemoteAdminAccess)
            {
                Logger.Info($"NukeLockParentCommand: Scary. The user did not have RA access, but we still got the command. Player's name is \"{player.Nickname}\", ID64: \"{player.UserId}\" Sending blank response.");
                response = "";
                return false;
            }
            Logger.Debug("NukeLockParentCommand: Mmkay, user has RA! Good.");
            response = "\nPlease enter a valid subcommand:\n";
            Logger.Debug("NukeLockParentCommand: Calling foreach loop for the commands we have.. Also noting we can't check debug here for some reason. Will look into later.");
            foreach (var command in AllCommands)
            {
                Logger.Debug($"NukeLockParentCommand: Checking user: \"{player.Nickname}\" / ID64: \"{player.UserId}\" for \"nl.{command}\" permission.");
                if (player.CheckPermission($"nl.{command.Command}"))
                {
                    Logger.Debug($"NukeLockParentCommand: User \"{player.Nickname}\" / ID64:\"{player.UserId}\" had nl.{command.Command} permission! Adding to response!!");
                    response += $"- {command.Command} ({string.Join(", ", Aliases)})\n";
                }
                else {Logger.Info($"NukeLockParentCommand: Player (\"{player.Nickname}\", ID64 \"{player.UserId}\") did not have nl.{command.Command} permission. Not including it in command list.");}
                Logger.Debug("NukeLockParentCommand: Finished foreach loop.");
            }
            Logger.Debug("NukeLockParentCommand: All done! Returning now! :D");
            return false;
        }
    }
}
