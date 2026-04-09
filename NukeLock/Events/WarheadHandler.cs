using Exiled.API.Features;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Globalization;
using LabApi.Events.Arguments.WarheadEvents;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace NukeLock.Events;

internal sealed class WarheadHandler(NukeLock plugin)
{
    public static Dictionary<Room, Color>? RoomBaseColorAndRoom = [];
    public static CoroutineHandle DetonationTimerCoroutine;
    public void OnStarting(WarheadStartingEventArgs ev)
    {
        Logger.Debug("WarheadOnStarting: OnStarting called!");
        RoomBaseColorAndRoom?.Clear();
        ev.SuppressSubtitles = true;
        if (plugin.Config.WarheadDetonationTimer)
        {
            Logger.Debug("WarheadOnStarting: Plugin.Config.WarheadDetonationTimer is true on warhead starting. Calling DetonationTimer..");
            DetonationTimerCoroutine = Timing.RunCoroutine(DetonationTimer());
        }
        else Logger.Debug("WarheadDetonationTimer was false. Ignoring.");

        // if (/*plugin.Config.WarheadColor is { Red: 0, Blue: 0, Green: 0 } || */!Warhead.IsInProgress)
        // {
        //     Logger.Debug("WarheadOnStarting: Either warhead color was 0 0 0, or warhead was not in progress. Unsure how it'd be out of progress on onstarting, but I assume this is to handle edge cases of onstarting and onstopping happening at similar times.");
        //     return;
        // }
        
        RoomBaseColorAndRoom ??= new Dictionary<Room, Color>();
        Logger.Debug("WarheadOnStarting: Color should've been reset now. Since it's static, this new creator SHOULD just be a reset. Hopefully. Also, noting the debug value outside so it doesn't get set each time inside.");
        var debug = plugin.Config.ExtraDebug; // set to extradebug cuz what in the world
        foreach (var room in Room.List)
        {
            if (debug)
                Logger.Debug($"WarheadOnStarting: Foreach loop iteration {room.Name} starting.");
            if (room && room.RoomLightController) // implicit !=null
            {
                if (debug)
                    Logger.Debug($"WarheadOnStarting: Foreach loop iteration {room.Name} is adding the room name and color to _color.");
                RoomBaseColorAndRoom.Add(room, room.Color);
                // room.RoomLightController.NetworkOverrideColor = new UnityEngine.Color(
                if (debug)
                    Logger.Debug($"WarheadOnStarting: Foreach loop iteration {room.Name} is setting the room color to {plugin.Config.WarheadColor.Red}, {plugin.Config.WarheadColor.Green}, {plugin.Config.WarheadColor.Blue}");
                room.Color = new Color(
                    plugin.Config.WarheadColor.Red / 255f, plugin.Config.WarheadColor.Green / 255f,
                    plugin.Config.WarheadColor.Blue / 255f);
                if (debug)
                    Logger.Debug($"WarheadOnStarting: Foreach loop iteration {room.Name} has passed setting the room color to {plugin.Config.WarheadColor.Red}, {plugin.Config.WarheadColor.Green}, {plugin.Config.WarheadColor.Blue}");
            }
            if (debug)
                Logger.Debug($"WarheadOnStarting: Regardless of output, foreach loop iteration {room?.Name} done.");
        }
        Logger.Debug("WarheadOnStarting: OnStarting finished!");
    }

    public void OnStopping(StoppingEventArgs ev)
    {
        Logger.Debug("(Warhead) OnStopping called!");
        if (!Warhead.IsLocked || ev.Player == null)
        {
            Logger.Debug("Warhead wasn't locked, or player was null. Continuing with stop for warhead.. In (Warhead) OnStopping, by the way.");
            Map.ClearBroadcasts();
            Logger.Debug("Clearing broadcasts.. In OnStopping, by the way.");
            if (RoomBaseColorAndRoom == null) return;
            Logger.Debug("_color was not null. Entering foreach loop.. also noting debug values.");
            var debug = plugin.Config.Debug;
            var extraDebug = plugin.Config.ExtraDebug;
            foreach (var list in RoomBaseColorAndRoom)
            {
                if (debug && extraDebug)
                    Logger.Debug($"""Clearing "{list.Key?.Name}" of its color..\nColor: "{list.Value}". In (Warhead) OnStopping, by the way."""); 
                if (list.Key) list.Key.Color = list.Value; // genius. // also, list.Key == does this room exist?
            }
            RoomBaseColorAndRoom.Clear();
            Logger.Debug("Cleared _color . Returning. In (Warhead) OnStopping, by the way.");
            return;
        }

        if (plugin.Config.HintTime > 0 && plugin.Config.HintMessage != null)
        {
            Logger.Debug("HintTime was above 0. Continuing to hint. In (Warhead) OnStopping, by the way.");
            ev.Player.ShowHint(plugin.Config.HintMessage, plugin.Config.HintTime);
        }

        if (plugin.Config.WarheadDetonationTimer)
        {
            Logger.Debug("This method currently does nothing, but warhead detonation timer was true. In (Warhead) OnStopping, by the way.");
            // Map.ClearBroadcasts();
        }

        Logger.Debug("Now, disabling the event. Warhead OnStopping, by the way.");
        ev.IsAllowed = false;
    }

    public void OnDetonated()
    {
        if (!string.IsNullOrEmpty(plugin.Config.RadiationWarningMessage))
        {
            Logger.Debug("Radiation warning was NOT null, sending.");
            Map.Broadcast(5, plugin.Config.RadiationWarningMessage, Broadcast.BroadcastFlags.Normal, true);
            Logger.Debug("Radiation warning was NOT null, passed the broadcast send."); // didn't make it passed cuz what if it silently failed !!
        }

        if (plugin.Config.RadiationDelay <= 0) return;
        
        Logger.Debug("RadiationDelay was above zero! Running radiation..");
        plugin.RadiationCoroutine = Timing.RunCoroutine(Radiation());
        NukeLock.RadiationCalled = true;
    }

    private IEnumerator<float> Radiation()
    {
        Logger.Debug("Radiation called.");
        yield return Timing.WaitForSeconds(plugin.Config.RadiationDelay);

        Logger.Debug("Radiation passed the yield return waitforseconds..");
        if (!Warhead.IsDetonated)
        {
            Logger.Debug("The warhead has not been detonated. Yielding.");
            yield break;
        }

        if (!string.IsNullOrEmpty(plugin.Config.RadiationBeginMessage))
        {
            Logger.Debug("String in RadiationBeginMessage was not null! Continuing. with radiation broadcast.");
            Map.Broadcast(5, plugin.Config.RadiationBeginMessage, Broadcast.BroadcastFlags.Normal, true);
        }

        // todo: optimize this below!
        while (true) // this is a while true script because uhm.. the nuke can't really un-go-off. I'll optimize it eventually though, it probably should only be run after a timer check !and.. i'll make it an async function
        {
            // not adding logger.debug here. would do too much cuz.. it's a while true loop. also we can make it an epic for loop if we want to iterate damage outputs
            yield return Timing.WaitForSeconds(plugin.Config.RadiationInterval);
            Logger.Debug("Passed yield return.");
            Logger.Debug("Heading into foreach loop! Getting debug value to ensure we save resources..");
            var debug = plugin.Config.Debug;
            foreach (var player in Player.List)
            {
                if (player.Role.Team == Team.Dead)
                {
                    if (debug) Logger.Debug($"WarheadHandler while true loop here. Player \"{player.Nickname}\" (ID: \"{player.UserId}\") was dead.");
                    continue;
                }
                player.Hurt(new CustomReasonDamageHandler(plugin.Config.RadiationDeathReason, plugin.Config.RadiationDamage));
                if (debug)
                    Logger.Debug($"\"{player.Nickname}\" has taken \"{plugin.Config.RadiationDamage}\" damage. [or at least we attempted to do that]");
            }
        }
    }

    private IEnumerator<float> DetonationTimer() // todo for later, uh... "Warhead.Controller.CurScenario.TimeToDetonate" exists. uhm.
    {
        Logger.Debug("Detonation timer called.");
        // if (!Warhead.IsInProgress)
        // {
        //     Logger.Debug("Detonation timer somehow called when warhead wasn't in process. Canceling.");
        //     return;
        // }
        // var start = Warhead.Controller.CurScenario.TotalTime; // gets current time to detonate, before any detonation shit happens! // also additional time is the time necessary for the cassie message to ensure it's actually accurate. total time is time to detonate + additional time!
        const int end = 0;
        
        Logger.Debug("About to send a shitload of broadcasts.. Also noting debug value so we don't waste CPU.");
        var debug = plugin.Config.Debug;
        string? messageOneSec = NukeLock.Instance?.Config.WarheadDetonationMessage.Replace("seconds", "second");
        for (var i = Mathf.Round(Warhead.Controller.CurScenario.TotalTime); i > end; i--)
        {
            if (debug)
                Logger.Debug($"Broadcast {i}.");
            if (Mathf.Approximately(i, 1) && !Mathf.Approximately(i, 0))
            {
                if (debug)
                    Logger.Debug("This one was not zero, but below 1.5. Using different method!");
                messageOneSec = messageOneSec?.Replace("$(COUNTDOWN)", i.ToString(CultureInfo.CurrentCulture)).Replace("%COUNTDOWN%", i.ToString(CultureInfo.CurrentCulture));
                Map.Broadcast(duration:1, messageOneSec, default, true);
                if (debug)
                    Logger.Debug("Success on seconds to second method! :D");
                yield return Timing.WaitForSeconds(1);
                continue;
            }
            if (debug)
                Logger.Debug("This one ain't lower than 1.5. Continuing with \"seconds\".");
            var message = plugin.Config.WarheadDetonationMessage.Replace("%COUNTDOWN%", i.ToString(CultureInfo.CurrentCulture)).Replace("$(COUNTDOWN)", i.ToString(CultureInfo.CurrentCulture));
            Map.Broadcast(duration:1, message);
            if (debug)
                Logger.Debug("Broadcast attempted.");
            yield return Timing.WaitForSeconds(1);
        }
        Logger.Debug("DetonationTimer finished!");
    }
}
// todo: fix this, cause it doesn't have a kill token like the rest- will implement kill token soon
//todo add is warhead in progress check to msgs that play over cassie