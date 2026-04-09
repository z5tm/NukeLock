using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LabApi.Features.Console;

namespace NukeLock.Events;

internal sealed class ServerHandler(NukeLock plugin)
{
    private double _autoNukeTime; // removed = 0 cuz redundancy !

    public static float? WarheadTime;
    public static float WaitedForTime;

    public void OnRoundStarted()
    {
        Logger.Debug("OnRoundStarted called!");
        Warhead.IsLocked = plugin.Config.WarheadCancelable;
        Warhead.LeverStatus = plugin.Config.WarheadAutoArmed;
        Logger.Debug($"OnRoundStarted here, just set Warhead.IsLocked to {plugin.Config.WarheadCancelable} and Warhead.LeverStatus to {plugin.Config.WarheadAutoArmed}!");

        if (_autoNukeTime > 0 && plugin.Config.CassieWarnings is { Count: > 0 })
        {
            Logger.Debug("OnRountStarted here, CassieWarnings is above zero, and autoNukeTime is too. Running cassie warnings coroutine..");
            plugin.CassieWarnings = Timing.RunCoroutine(CassieWarnings());
            NukeLock.CassieWarningsCalled = true;
        }
        
        if (plugin.Config.AutoNuke > 0)
        {
            Logger.Debug("OnRoundStarted here, autonuke was above zero! Running autonuke coroutine..");
            plugin.NukeCoroutine = Timing.RunCoroutine(AutoNuke());
            NukeLock.AutoNukeTimerCalled = true;
        }

        if (Warhead.DetonationTimer == 0)
        {
            Logger.Debug("OnRoundStarted here, warhead detonation timer was zero! Returning.");
            return;
        }

        Logger.Debug("OnRoundStarted here, just passed those if statements! Setting WarheadTime up for later..");
        WarheadTime = Warhead.DetonationTimer;
        Logger.Debug("OnRoundStarted finished!");
    }

    public void OnRestartingRound()
    {
        Logger.Debug("OnRestartingRound called!! Killing that nuke and radiation coroutine.. (also clearing roombasecolorandroom..)");
        WarheadHandler.RoomBaseColorAndRoom?.Clear();
        Timing.KillCoroutines(plugin.NukeCoroutine, plugin.RadiationCoroutine, plugin.CassieWarnings);
        Logger.Debug("OnRestartingRound finished!! Called for those coroutines to be killed.");
    }

    public void OnWaitingForPlayers()
    {
        Logger.Debug("OnWaitingForPlayers called!! Killing that nuke and radiation coroutine..");
        Timing.KillCoroutines(plugin.NukeCoroutine, plugin.RadiationCoroutine, plugin.CassieWarnings);
        Logger.Debug("OnWaitingForPlayers finished!! Called for those coroutines to be killed.");
    }

    public void OnRespawningTeam(RespawningTeamEventArgs ev)
    {
        Logger.Debug("OnRespawningTeam called!! Checking if we can kill this wave..");
        if (plugin.Config.WarheadDisablesTeamRespawn && Warhead.IsDetonated)
        {
            Logger.Debug("Warhead was detonated, and warhead disables team respawn was true! Killin it off.");
            ev.IsAllowed = false;
            Logger.Debug("Warhead was detonated, and warhead disables team respawn was true! Killed!");
        }
        else
        {
            Logger.Debug(Warhead.IsDetonated
                ? "Warhead was detonated, but WarheadDisablesTeamSpawn was disabled."
                : "Warhead wasn't detonated, so just.. moving on.");
            // NOTE: condition (boolean!!) ? true : false !!!!
        }

        Logger.Debug("OnRespawningTeam finished!");
    }

    private IEnumerator<float> AutoNuke()
    {
        Logger.Debug("AutoNuke called!");
        _autoNukeTime = plugin.Config.AutoNuke;

        var extraDebug = plugin.Config?.ExtraDebug;
        Logger.Debug("AutoNuke: Beginning while autonuketime is above zero if statement..");
        if (_autoNukeTime > 0)
        {
            if (extraDebug ?? false)
                Logger.Debug($"AutoNuke: Inside while loop! Yielding for autoNukeTime.. _autoNukeTime == {_autoNukeTime}");
            bool validBroadcastperma = (_autoNukeTime > plugin.Config?.AutoNukePermaBroadcastTimer);
            if (!validBroadcastperma) Logger.Warn("BroadcastPermaTimer was invalid! Please ensure AutoNukeTime is above AutoNukePermaBroadcastTimer in your configuration!");
            if (plugin.Config?.AutoNukePermaBroadcastTimer != null && validBroadcastperma)
            {
                yield return Timing.WaitForSeconds((float)_autoNukeTime - plugin.Config.AutoNukePermaBroadcastTimer);

                if (extraDebug ?? false)
                    Logger.Debug("AutoNuke [ANPBT Route] Ready for AutoNukePermaBroadcastMessage!"); // i think i'll use [_ROUTE] more often. this seems nice for devs.
                var start = plugin.Config.AutoNukePermaBroadcastTimer; // gets current detonation time, to get with configs - no idea how to check configs directly sadly
                const int end = 0;

                var messageOneSec = plugin.Config.AutoNukePermaBroadcastMessage
                    .Replace("seconds", "second");
                for (var i = start; i > end; i--)
                {
                    if (extraDebug ?? false)
                        Logger.Debug($"AutoNuke [ANPBT Route, for loop]: Broadcast {i}.");
                    if (i > 0.6 && i < 1.5) // if i is higher than 0.6, but lower than 1.6 then replace seconds with second
                    {
                        if (extraDebug ?? false)
                            Logger.Debug("AutoNuke [ANPBT Route, for loop]: This one was not above 0.6, but below 1.5. Using seconds>second replacer!");
                        messageOneSec = messageOneSec
                            .Replace("$(COUNTDOWN)", i.ToString()) // replace $(COUNTDOWN) with iteration
                            .Replace("%COUNTDOWN%", i.ToString()); // replace %COUNTDOWN% (for compatibility with older versions) with iteration
                        Map.Broadcast(duration: 1,
                            messageOneSec); // NOTE: this works explicitly because duration:1 is one second, not one frame. Please do not attempt to copy this code using a frame-based system.
                        if (extraDebug ?? false)
                            Logger.Debug("AutoNuke [ANPBT Route, for loop]: Success on seconds to second method! :D");
                        continue;
                    }

                    if (extraDebug ?? false)
                        Logger.Debug("AutoNuke [ANPBT Route, for loop]: This one ain't lower than 1.5. Continuing with \"seconds\".");
                    var messagePermaBroadcast = plugin.Config.AutoNukePermaBroadcastMessage
                        .Replace("$(COUNTDOWN)", i.ToString()) // replace $(COUNTDOWN) with iteration
                        .Replace("%COUNTDOWN%", i.ToString()); // replace %COUNTDOWN% (for compatibility with older versions) with iteration
                    Map.Broadcast(duration: 1,
                        messagePermaBroadcast); // NOTE: this works explicitly because duration:1 is one second, not one frame. Please do not attempt to copy this code using a frame-based system.
                    if (extraDebug ?? false)
                        Logger.Debug("AutoNuke [ANPBT Route, for loop]: Broadcast attempted.");
                    yield return Timing.WaitForSeconds(1);
                }
            }
            else
                yield return Timing.WaitForSeconds((float)_autoNukeTime);

            if (extraDebug ?? false)
                Logger.Debug($"AutoNuke: Finished yielding for {_autoNukeTime.ToString(CultureInfo.CurrentCulture)} seconds.");

            if (Warhead.IsDetonated)
            {
                if (extraDebug ?? false)
                    Logger.Debug("AutoNuke: Warhead was already detonated. Ignoring.");
                yield break; // cancels the entirety of AutoNuke.
            }

            while (Warhead.IsInProgress)
            {
                if (extraDebug ?? false)
                    Logger.Debug("AutoNuke: Warhead is already in progress. Ignoring.");
                yield return
                    Timing.WaitForSeconds(
                        waitTime:
                        15f); // Checking every 15 seconds to improve performance vs every few seconds, however, we can improve this further by (todo) attaching to onwarheadstopped. will do after the plugin is functional
            }

            if (extraDebug ?? false)
                Logger.Debug("AutoNuke: Checking if perma broadcast timer is less than or equal to autoNukeTime..");

            if (plugin.Config?.Debug ?? false)
                Logger.Debug("AutoNuke: Checking for if CassieWarnings is above zero and not null!");
        }

        if (plugin.Config?.BeginAutoNukeDetonationBroadcastTime > 0) // If, when AutoNuke activates, the broadcast time for AutoNuke being enabled is above zero, go ahead!
        {
            Logger.Debug("AutoNuke: DetonationBroadcastTime was above zero. Broadcasting..");
            Map.Broadcast(plugin.Config.BeginAutoNukeDetonationBroadcastTime, plugin.Config.BeginAutoNukeDetonationBroadcastMessage, Broadcast.BroadcastFlags.Normal, true);
            Logger.Debug("AutoNuke: DetonationBroadcastTime was above zero. Broadcasted.");
        }
        //!! NOTE: The broadcast above is solely for a message when AutoNuke is activating! 

        // Broadcast Logic handled, moving on to AutoNuke's actual start!

        Logger.Debug("AutoNuke: Turning the lever on and lockin the nuke!");
        Warhead.LeverStatus = true;
        Warhead.IsLocked = true;
        Logger.Debug("AutoNuke: Starting up the warhead now!");
        Warhead.Start();
        Logger.Debug("AutoNuke finished!");
    }

    private IEnumerator<float> CassieWarnings()
    {
        Logger.Debug("AutoNuke: Entering foreach loop under pretense of cassie warnings above zero and cassie warnings isn't null.. Also noting debug value.");
        
        var extraDebug = plugin.Config?.Debug;
        foreach (var msg in plugin.Config?.CassieWarnings.Reverse() ?? Configs.Defaults.CassieWarnings) //!! for each message and integer in cassie warnings (reversed order due to it being a sorteddictionary, highest to least), 
        {
            if (Warhead.IsDetonated) yield break;
            if (WaitedForTime == 0)
            {
                if (!plugin.Config?.CassieWarningsMessageCalculations ?? true)
                {
                    var cassieDuration = Exiled.API.Features.Cassie.CalculateDuration(msg.Value);
                    WaitedForTime = msg.Key - cassieDuration;
                }
                else WaitedForTime = msg.Key;
                yield return Timing.WaitForSeconds(WaitedForTime); // I.E.: 30 seconds, waitedfortime is at 30 seconds.
                if (Warhead.IsDetonated) yield break;
            }
            else
            {
                yield return Timing.WaitForSeconds(Math.Max(0, msg.Key - WaitedForTime)); // wait for (i.e. 60 seconds, minus the 30 we already waited! :D there we are.
                if (Warhead.IsDetonated) yield break;
                WaitedForTime = msg.Key; // set waitedfortime to msg.Key, since msg.Key is precisely how long we've waited! - 
            }
            
            if (extraDebug ?? false) //!! if extra debug is true, with a default to false,
                Logger.Debug($"AutoNuke: Inside CassieWarnings foreach loop! Starting.. Key: \"{msg.Key}\" + Value:\"{msg.Value}\""); //!! log key and value if debug, 
            Exiled.API.Features.Cassie.Message(msg.Value);
            
            // float cassieDuration = Exiled.API.Features.Cassie.CalculateDuration(msg.Value); //!! calculate how long the given message will take to play out,
            // double announcement = msg.Key + Math.Round(cassieDuration, /* <-- at what point in time, i.e. 600s til nuke, announce?*/ 0); //!! create a double named "announcement", add the calculated cassie message duration, rounded up, to the integer provided in CassieWarnings,
            
            // if (Math.Abs(announcement - _autoNukeTime) < 0.5) //!! if the absolute value of the defined value above (announcement), minus autonuke time, is less than 0.5,
            // Exiled.API.Features.Cassie.Message(msg.Value); //!! [ACTION] then send the message
            
            if (extraDebug ?? false) //!! if extra debug is true, with a default to false, 
                Logger.Debug($"AutoNuke: Inside CassieWarnings foreach loop! Moving on from Key: \"{msg.Key}\" + Value:\"{msg.Value}\""); //!! log key and value, inform that we are moving on.
        } //!! END CASSIE WARNINGS LOGIC
    }
}