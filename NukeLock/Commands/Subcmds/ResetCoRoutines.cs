using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using LabApi.Features.Console;

namespace NukeLock.Commands.Subcmds;

public class ResetCoRoutines : ICommand
{
    public string Command { get; } = "rcr";
    public string[] Aliases { get; } = ["resetcoroutines", "reset"];
    public string Description { get; } = "Resets coroutines. Use this after disabling and re-enabling the plugin.";

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

        Logger.Debug($"NukeLockArmCommand: Execute called for `nukelock resetcoroutines` by \"{plr.Nickname}\" (ID: \"{plr.UserId}\")");
        if (!sender.CheckPermission("nl.rcr"))
        {
            Logger.Debug($"NukeLockArmCommand: \"{plr.Nickname}\" (ID: \"{plr.UserId}\")does not have the \"nl.rcr\" permission. Informing the user.");
            response =
                "<color=blue>NukeLockArmCommand</color><color=orange>: You do</color> <color=red>not</color> <color=orange>have</color> <color=blue>permission</color> <color=orange>to execute this command. Required</color> <color=blue>permission</color><color=orange>:</color> <color=blue>nl.rcr</color>";
            return false;
        }

        if (Warhead.IsDetonated)
        {
            Logger.Debug("NukeLockArmCommand: Warhead was already detonated. Informing user.");
            response =
                "<color=orange>The</color> <color=blue>Alpha Warhead</color> <color=orange>has already been</color> <color=red>detonated</color><color=orange>. Ignoring command.</color>";
            Logger.Debug("NukeLockArmCommand: Warhead was already detonated. Informed user, now exiting.");
            return true;
        }

        NukeLock.Instance?.ServerHandler?.OnRoundStarted();


        response = "<color=orange>All</color> <color=blue>coroutines</color> <color=orange>have been re-called.</color>";
        return true;
    }
}