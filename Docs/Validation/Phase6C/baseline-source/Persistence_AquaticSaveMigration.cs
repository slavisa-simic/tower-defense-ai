using System;
using System.Collections.Generic;
using UnityEngine;
namespace ShorelineDefense.Persistence
{
    /// <summary>Pure in-memory conversion; durable files change only at the next explicit checkpoint.</summary>
    public static class AquaticSaveMigration
    {
        public static string EventId(string id) => id switch {
            "pirate-raid" => "swarm-surge", "militant-assault" => "armored-migration",
            "heavy-assault" => "deep-water-disturbance", "runner-swarm" => "brood-tide",
            "mass-landing" => "mass-emergence", "concentrated-landing" => "concentrated-emergence", _ => id };
        public static bool Upgrade(RunSaveData data)
        {
            if (!SaveGameService.Valid(data)) return false;
            if (data.saveVersion >= 3) return true;
            if (data.saveVersion == 1) data.mapId = ShorelineDefense.Grid.MapCatalog.OriginalId;
            var old = data.statistics.kills;
            long Add(long a, long b) => a > long.MaxValue - b ? long.MaxValue : a + b;
            var next = new long[9];
            next[0] = Add(Add(old[0], old[3]), old[6]); next[1] = Add(old[1], old[5]);
            next[2] = Add(old[2], old[4]); next[7] = old[7]; next[8] = old[8];
            data.statistics.kills = next;
            var events = data.events;
            events.currentId = EventId(events.currentId); events.mostRecent = EventId(events.mostRecent);
            for (int i = 0; i < events.recent.Count; i++) events.recent[i] = EventId(events.recent[i]);
            foreach (var list in new[]{events.cooldowns, events.counts, events.history}) foreach (var item in list) item.id = EventId(item.id);
            data.saveVersion = 3; return true;
        }
    }
}
