using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using LabApi.Features.Console;

namespace NukeLock.Commands.Subcmds
{
    public class Lock : ICommand
    {
        public string Command { get; } = "lock";
        public string[] Aliases { get; } = { "l" };
        public string Description { get; } = "Locks/Unlocks the Alpha Warhead.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plr =  Player.Get(sender);
            Logger.Debug($"NukeLockLockCommand: Execute called by {plr.Nickname} (ID: {plr.UserId}! Executing..");
            if (!sender.CheckPermission("nl.lock"))
            {
                Logger.Debug($"NukeLockLockCommand: \"{plr.Nickname}\" (ID: \"{plr.UserId}\") attempted to run `nl l`, but had no permission.");
                response = "You don't have permission to execute this command. Required permission: nl.lock";
                return false;
            }

            if (!Warhead.IsLocked)
            {
                Logger.Debug($"NukeLockLockCommand: Warhead was not locked! Locking..");
                Warhead.IsLocked = true;
                response = "The Alpha Warhead is now locked.";
                Logger.Debug($"NukeLockLockCommand: \"{plr.Nickname}\" (ID: \"{plr.UserId}\")'s command execution is complete!");
                return true;
            }

            Logger.Debug("NukeLockLockCommand: Warhead was unlocked. Locking!");
            Warhead.IsLocked = false;
            response = "The Alpha Warhead is no longer locked.";
            Logger.Debug($"NukeLockLockCommand: \"{plr.Nickname}\" (ID: \"{plr.UserId}\")'s command execution is complete!");
            return true;
        }
    }
}
