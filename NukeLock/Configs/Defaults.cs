using System.Collections.Generic;

namespace NukeLock.Configs;

public class Defaults
{
    public static SortedDictionary<int, string> CassieWarnings { get; set; } = new()
    {
        { 600, "Attention, all personnel, The Alpha Warhead Emergency Detonation Sequence will be started in TMinus 10 Minutes" },
        { 300, "Danger, The Alpha Warhead Emergency Detonation Sequence will be started in TMinus 5 Minutes" },
        { 120, "Danger, The Alpha Warhead Emergency Detonation Sequence will be started in TMinus 2 Minutes" },
        { 30, "Danger, The Alpha Warhead Emergency Detonation Sequence will be started in TMinus 30 Seconds" }
    };
}