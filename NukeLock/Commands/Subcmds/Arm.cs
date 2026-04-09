using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using LabApi.Features.Console;

namespace NukeLock.Commands.Subcmds
{
    public class Arm : ICommand
    {
        public string Command { get; } = "arm";
        public string[] Aliases { get; } = ["a"];
        public string Description { get; } = "Toggles the the Alpha Warhead's lever. [Toggles armed/unarmed]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plr = Player.Get(sender);
            if (plr == null)
            {
                Logger.Debug("NukeLockArmCommand: Player was null. Sending an empty response and hoping for this server.");
                response = "";
                Logger.Debug("NukeLockArmCommand: Player was null. Sent empty response..");
                return false;
            }
            Logger.Debug($"NukeLockArmCommand: Execute called for `nl arm` by \"{plr.Nickname}\" (ID: \"{plr.UserId}\")");
            if (!sender.CheckPermission("nl.arm"))
            {
                Logger.Debug($"NukeLockArmCommand: \"{plr.Nickname}\" (ID: \"{plr.UserId}\")does not have the \"nl.arm\" permission. Informing the user.");
                response = "NukeLockArmCommand: You don't have permission to execute this command. Required permission: nl.arm";
                return false;
            }

            if (!Warhead.LeverStatus)
            {
                Logger.Debug("NukeLockArmCommand: Warhead's lever status was false. Switching!");
                Warhead.LeverStatus = true;
                response = "The Alpha Warhead is now armed.";
                Logger.Debug("NukeLockArmCommand: Warhead's lever status was false. Swapped! Exiting command now..");
                return true;
            }

            Logger.Debug("NukeLockArmCommand: Warhead's lever status was true. Swapping!");
            Warhead.LeverStatus = false;
            Logger.Debug("NukeLockArmCommand: Warhead's lever status was true. Swapped, and now exiting.");
            response = "The Alpha Warhead is now unarmed.";
            return true;
        }
    }
}
