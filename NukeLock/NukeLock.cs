using Exiled.API.Features;
using MEC;
using NukeLock.Events;
using System;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using NukeLock.Configs;
using Server = Exiled.Events.Handlers.Server;
using Warhead = Exiled.Events.Handlers.Warhead;

namespace NukeLock;

public class NukeLock : Plugin<Config>
{
    public static NukeLock? Instance;
    public override string Author => "Marco15453";
    public override string Name => "NukeLock";
    public override Version Version => new Version(1, 11, 0);

    public CoroutineHandle NukeCoroutine;
    public CoroutineHandle CassieWarnings; // related to NukeCoroutine directly
    public CoroutineHandle RadiationCoroutine;
    public CoroutineHandle DetonationCoroutine;

    public static bool CassieWarningsCalled;
    public static bool AutoNukeTimerCalled;
    public static bool RadiationCalled;

    internal WarheadHandler? WarheadHandler;
    internal ServerHandler? ServerHandler;
    
    public override void OnEnabled()
    {
        Instance = this;
        Logger.Debug("OnEnabled has begun!");
        RegisterEvents();
        Logger.Debug("OnEnabled here, just finished calling RegisterEvents.. Going to base.OnEnabled();, and settings WaiedForTime back to 0!");
        ServerHandler.WaitedForTime = 0;
        base.OnEnabled();
        Logger.Debug("OnEnabled done!");
    }

    public override void OnDisabled()
    {
        Logger.Debug("OnDisable events called!");
        UnregisterEvents();
        Logger.Debug("Unregister events continuing, just called unregister events..");
        base.OnDisabled();
        Logger.Debug("OnDisabled done!");
    }

    private void RegisterEvents()
    {
        Logger.Debug("Register events called!");
        WarheadHandler = new WarheadHandler(this);
        ServerHandler = new ServerHandler(this);

        // Server
        Logger.Debug("Server events registering!");
        Server.RoundStarted += ServerHandler.OnRoundStarted;
        Server.WaitingForPlayers += ServerHandler.OnWaitingForPlayers;
        Server.RestartingRound += ServerHandler.OnRestartingRound;

        Logger.Debug("RWarhead events registering!");
        // Warhead
        WarheadEvents.Starting += WarheadHandler.OnStarting;
        Warhead.Stopping += WarheadHandler.OnStopping;
        Warhead.Detonated += WarheadHandler.OnDetonated;
        Logger.Debug("Register events finished!");
    }

    private void UnregisterEvents()
    {
        Logger.Debug("Unregister events called!");
        // Server
        if (ServerHandler != null)
        {
            Logger.Debug("ServerHandler was NOT null. Unsubscribing!");
            Server.RoundStarted -= ServerHandler.OnRoundStarted;
            Server.WaitingForPlayers -= ServerHandler.OnWaitingForPlayers;
            Server.RestartingRound -= ServerHandler.OnRestartingRound;
            Logger.Debug("ServerHandler was NOT null. Unsubscribed!");
        }

        // Warhead
        if (WarheadHandler != null)
        {
            Logger.Debug("WarheadHandler was NOT null. Unsubscribing!");
            WarheadEvents.Starting -= WarheadHandler.OnStarting;
            Warhead.Stopping -= WarheadHandler.OnStopping;
            Warhead.Detonated -= WarheadHandler.OnDetonated;
            Logger.Debug("WarheadHandler was NOT null. Unsubscribed!");
        }

        Logger.Debug("Killing coroutines and clearing rooms list..");
        WarheadHandler.RoomBaseColorAndRoom?.Clear();
        Timing.KillCoroutines(NukeCoroutine, RadiationCoroutine, CassieWarnings);
        AutoNukeTimerCalled = false;
        CassieWarningsCalled = false;
        RadiationCalled = false;
        Logger.Debug("Passed warheadhandler isn't null and serverhandler isn't null - moving on to nullify these two.");
        WarheadHandler = null;
        ServerHandler = null;
        Logger.Debug("Unregister events finished!");
    }
}