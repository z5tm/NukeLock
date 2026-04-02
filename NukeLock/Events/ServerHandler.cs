using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NukeLock.Events
{
    internal sealed class ServerHandler
    {
        private readonly NukeLock _plugin;
        public ServerHandler(NukeLock plugin) => _plugin = plugin;

        private double _autoNukeTime; // removed = 0 cuz redundancy !

        public void OnRoundStarted()
        {
            Warhead.IsLocked = _plugin.Config.WarheadCancelable;
            Warhead.LeverStatus = _plugin.Config.WarheadAutoArmed;

            if (_plugin.Config.AutoNuke > 0)
                _plugin.NukeCoroutine = Timing.RunCoroutine(AutoNuke());
        }

        public void OnRestartingRound()
        {
            Timing.KillCoroutines(_plugin.NukeCoroutine, _plugin.RadiationCoroutine, _plugin.DetonationCoroutine);
        }

        public void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(_plugin.NukeCoroutine, _plugin.RadiationCoroutine, _plugin.DetonationCoroutine);

            if (_plugin.Config.WarheadColor.Red != 0 || _plugin.Config.WarheadColor.Blue != 0 || _plugin.Config.WarheadColor.Green != 0)
                foreach (Room room in Room.List)
                    if (room != null && room.RoomLightController != null)
                        room.RoomLightController.NetworkOverrideColor = new UnityEngine.Color(_plugin.Config.WarheadColor.Red / 255f, _plugin.Config.WarheadColor.Green / 255f, _plugin.Config.WarheadColor.Blue / 255f);
        }

        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (_plugin.Config.WarheadDisablesTeamRespawn && Warhead.IsDetonated)
                ev.IsAllowed = false;
        }

        private IEnumerator<float> AutoNuke()
        {
            _autoNukeTime = _plugin.Config.AutoNuke;

            while (_autoNukeTime > 0)
            {
                yield return Timing.WaitForSeconds(1f);

                if (Warhead.IsDetonated)
                    yield break;

                if (_autoNukeTime <= _plugin.Config?.AutoNukePermaBroadcastTimer)
                {
                    string message = _plugin.Config.AutoNukePermaBroadcastMessage.Replace("%COUNTDOWN%", _autoNukeTime.ToString(CultureInfo.CurrentCulture));
                    if (_autoNukeTime <= 1)
                        message = message.Replace("seconds", "second");
                    Map.Broadcast(1, message, Broadcast.BroadcastFlags.Normal, true);
                }

                if (_plugin.Config?.CassieWarnings?.Count > 0 && _plugin.Config?.CassieWarnings != null)
                {
                    foreach (var msg in _plugin.Config.CassieWarnings)
                    {
                        float cassieDuration = Exiled.API.Features.Cassie.CalculateDuration(msg.Value);
                        double announcement = msg.Key + Math.Round(cassieDuration, 0);
                        if (announcement == _autoNukeTime) // swap to coroutine instead of this.
                            Exiled.API.Features.Cassie.Message(msg.Value);
                    }
                }
                _autoNukeTime -= 1;
            }

            // if (_plugin.Config?.CassieWarnings != null)
            // {
                // foreach (var time in _plugin.Config.CassieWarnings)
                
                // for (int i = 0; i < _plugin.Config.CassieWarnings.Count; i++)
                // {
                //     var io = _plugin.Config.CassieWarnings.Comparer;
                // }

                yield return Timing.WaitForSeconds(1f);

                if (_plugin.Config?.DetonationBroadcastTime > 0)
                    Map.Broadcast(_plugin.Config.DetonationBroadcastTime, _plugin.Config.DetonationBroadcastMessage,
                        Broadcast.BroadcastFlags.Normal, true);
            // }

            Warhead.LeverStatus = true;
            Warhead.IsLocked = true;
            Warhead.Start();
        }
    }
}
