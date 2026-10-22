using System;
using System.Collections.Generic;

namespace ShorelineDefense.Persistence
{
    [Serializable] public sealed class RunSaveData
    {
        public int saveVersion = 2;
        public string mapId = ShorelineDefense.Grid.MapCatalog.OriginalId;
        public string runId, savedUtc;
        public int wave, completedWaves, seed, money, maxHomicides;
        public double duration;
        public long earned, spent, refunds;
        public int dismantled;
        public StatisticsSaveData statistics = new StatisticsSaveData();
        public EventSaveData events = new EventSaveData();
        public List<StructureSaveData> structures = new List<StructureSaveData>();
    }
    [Serializable] public sealed class StructureSaveData
    {
        public string defenseId;
        public int x, y, level, health, maxHealth, constructionCost;
        public long investment;
        public float mineArmingRemaining;
    }
    [Serializable] public sealed class StatisticsSaveData
    {
        public long[] kills = new long[9];
        public long homicides, breaches, bossesBreached;
        public int highestWave, peakEnemies, peakBoats, built, upgraded, repaired;
    }
    [Serializable] public sealed class EventCounter
    {
        public string id;
        public int value;
        public EventCounter(string key, int count) { id = key; value = count; }
    }
    [Serializable] public sealed class EventSaveData
    {
        public string currentId, mostRecent;
        public int nextWave, wave, sector, wavesRemaining, supplyReceived;
        public float secondsRemaining, warningRemaining;
        public bool active;
        public long experienced;
        public List<string> recent = new List<string>();
        public List<EventCounter> cooldowns = new List<EventCounter>(), counts = new List<EventCounter>(), history = new List<EventCounter>();
    }
    [Serializable] public sealed class RunRecord
    {
        public string mapId;
        public string runId, endedUtc;
        public int wave, peakEnemies;
        public long kills, homicides, bosses, earned;
        public double duration;
    }
    [Serializable] public sealed class HistoryData
    {
        public int saveVersion = 1;
        public List<RunRecord> runs = new List<RunRecord>();
    }
}
