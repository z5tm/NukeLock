using Exiled.API.Features;
using MEC;
using NukeLock.Events;
using System;
using Server = Exiled.Events.Handlers.Server;
using Warhead = Exiled.Events.Handlers.Warhead;

namespace NukeLock
{
    public class NukeLock : Plugin<Config>
    {
        public override string Author => "Marco15453";
        public override string Name => "NukeLock";
        public override Version Version => new Version(1, 11, 0);
        public override Version RequiredExiledVersion => new (9,13,2);

        public CoroutineHandle NukeCoroutine;
        public CoroutineHandle RadiationCoroutine;
        public CoroutineHandle DetonationCoroutine;

        private WarheadHandler? _warheadHandler;
        private ServerHandler? _serverHandler;

        public override void OnEnabled()
        {
            RegisterEvents();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            UnregisterEvents();
            base.OnDisabled();
        }

        private void RegisterEvents()
        {
            _warheadHandler = new WarheadHandler(this);
            _serverHandler = new ServerHandler(this);

            // Server
            Server.RoundStarted += _serverHandler.OnRoundStarted;
            Server.WaitingForPlayers += _serverHandler.OnWaitingForPlayers;
            Server.RestartingRound += _serverHandler.OnRestartingRound;

            // Warhead
            Warhead.Starting += _warheadHandler.OnStarting;
            Warhead.Stopping += _warheadHandler.OnStopping;
            Warhead.Detonated += _warheadHandler.OnDetonated;
        }

        private void UnregisterEvents()
        {
            // Server
            if (_serverHandler != null)
            {
                Server.RoundStarted -= _serverHandler.OnRoundStarted;
                Server.WaitingForPlayers -= _serverHandler.OnWaitingForPlayers;
                Server.RestartingRound -= _serverHandler.OnRestartingRound;
            }
            // Warhead
            if (_warheadHandler != null)
            {
                Warhead.Starting -= _warheadHandler.OnStarting;
                Warhead.Stopping -= _warheadHandler.OnStopping;
                Warhead.Detonated -= _warheadHandler.OnDetonated;
            }
            _warheadHandler = null;
            _serverHandler = null;
        }
    }
}
