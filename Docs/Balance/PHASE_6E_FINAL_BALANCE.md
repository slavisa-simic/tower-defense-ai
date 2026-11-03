# Phase 6E FINAL BALANCE

Captured from loaded Unity assets, 2026-11-02. All authored ScriptableObjects (including legacy wave assets and all research effects) are preserved in `Docs/Validation/Phase6E/final_balance-assets.json`. Values below are zero research; range is in cells. DPS is sustained nominal single-target output before armor, overkill, travel, coverage and downtime. Flame includes its continuous burn; mines are one charge.

## Defenses and all levels

|Defense / level|Paid total RSD|HP|Damage|Attacks/s|Range|Splash / minimum|Slow|Nominal DPS|RSD/DPS|RSD/HP|Coverage cells²|RSD/coverage|RSD/(DPS×blast area)|
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---:|---:|---:|---:|
|Auto Cannon 1|600|450|100|2|7.5|0 / 0|—|200|3|1.333|176.71|3.4|0|
|Auto Cannon 2|900|562|125|2.2|7.5|0 / 0|—|275|3.27|1.601|176.71|5.09|0|
|Auto Cannon 3|1350|675|175|2.5|8.25|0 / 0|—|437.5|3.09|2|213.82|6.31|0|
|Barbed Wire 1|50|200|0|1|1|0 / 0|50%|0|0|0.25|1|50|0|
|Barbed Wire 2|80|300|0|1|1|0 / 0|60%|0|0|0.267|1|80|0|
|Barbed Wire 3|130|400|0|1|1|0 / 0|65%|0|0|0.325|1|130|0|
|Flamethrower 1|300|350|12|5|3.5|0 / 0|—|75|4|0.857|5.88|51.02|0|
|Flamethrower 2|450|438|15|5|3.85|0 / 0|—|93.75|4.8|1.027|7.11|63.25|0|
|Flamethrower 3|675|525|19|5|4.025|0 / 0|—|119|5.67|1.286|7.78|86.81|0|
|Land Mine 1|75|1|150|1|1.75|1.75 / 0|—|0|0|75|9.62|7.8|0|
|Land Mine 2|95|1|210|1|1.75|1.75 / 0|—|0|0|95|9.62|9.87|0|
|Land Mine 3|130|1|300|1|2.1|2.1 / 0|—|0|0|130|13.85|9.38|0|
|MG Nest 1|150|300|20|4|5|0 / 0|—|80|1.88|0.5|78.54|1.91|0|
|MG Nest 2|225|375|25|4.4|5|0 / 0|—|110|2.05|0.6|78.54|2.86|0|
|MG Nest 3|375|450|35|5|5.75|0 / 0|—|175|2.14|0.833|103.87|3.61|0|
|Mortar 1|400|250|120|0.333|12|2.25 / 3|—|40|10|1.6|424.12|0.94|0.629|
|Mortar 2|600|312|150|0.417|12|2.25 / 3|—|62.5|9.6|1.923|424.12|1.41|0.604|
|Mortar 3|900|375|210|0.5|12|2.7 / 3|—|105|8.57|2.4|424.12|2.12|0.374|
|Rocket Launcher 1|900|300|350|0.2|10|2.75 / 0|—|70|12.86|3|314.16|2.86|0.541|
|Rocket Launcher 2|1350|375|438|0.25|10|2.75 / 0|—|109.5|12.33|3.6|314.16|4.3|0.519|
|Rocket Launcher 3|2025|450|612|0.3|10|3.3 / 0|—|183.6|11.03|4.5|314.16|6.45|0.322|
|Sniper Tower 1|300|250|180|0.5|11|0 / 0|—|90|3.33|1.2|380.13|0.79|0|
|Sniper Tower 2|450|312|252|0.5|12.1|0 / 0|—|126|3.57|1.442|459.96|0.98|0|
|Sniper Tower 3|675|375|360|0.6|12.65|0 / 0|—|216|3.12|1.8|502.73|1.34|0|

Coverage uses an unobstructed circle/annulus, the actual 55-degree Flame cone, or Wire footprint cells. Mine splash equals its detonation range and is a single charge. Wire's displayed slow applies only while crossing its cell. Zero ratios mean not applicable. Armor penetration, if present, applies to direct hits only and is described in the change record.

## Creatures

|Creature|HP|Speed|Armor|Kill RSD|Homicides|Structure damage / interval|Wave cost|Unlock|
|---|---:|---:|---:|---:|---:|---|---:|---:|
|BROODLING|18|3.8|0 %|1|5|3 / 1s|0.5|5|
|THE BROODMOTHER|6000|1.65|15 %|2200|900|45 / 1s|18|25|
|BULWARK|600|1.35|50 %|8|100|45 / 1s|5|7|
|BURROWER|140|3.2|0 %|4|90|18 / 1s|4|15|
|CRAWLER|100|2.5|0 %|1|25|12 / 1s|1|1|
|THE DREDGER|8000|1.15|35 %|2500|1000|120 / 1.2s|20|25|
|SKITTER|60|5|0 %|3|50|6 / 1s|2|3|
|SPITTER|180|2.2|0 %|4|80|24 / 5s|4|10|

## Seeded wave projections

Seed 1977, no event, additive boss escorts included. Rewards assume every organism is killed; these are maximum income, not a playthrough.

|Wave|Count|HP multiplier|Speed multiplier|Pressure target|Carrier limit|Kill RSD|Wave RSD|
|---|---:|---:|---:|---:|---:|---:|---:|
|1|80|1|1|8|6|80|300|
|3|115|1|1|16|7|205|304|
|5|240|1|1|24|8|360|308|
|10|395|1|1|44|10|660|318|
|15|540|1|1|64|13|950|328|
|20|690|1|1|84|15|1355|338|
|25|966|1.063|1.01|104|18|4735|348|
|30|935|1.125|1.02|124|20|1970|358|
|40|1200|1.25|1.04|164|25|2995|378|
|50|1441|1.375|1.06|204|28|5870|398|
|100|1441|2|1.16|240|28|6565|498|
|250|1441|3.875|1.46|240|28|6135|798|
|1000|1441|13.25|1.6|240|28|6400|2298|
|2147483647|1200|20|1.6|240|28|3675|2147483647|

## Economy, meta, events and abilities

Starting balance 500 RSD (full tree 750). Repair = ceil(paid investment × 0.5 × missing/max HP × phase × research × event); phase Preparation 0.75 / combat 1.25. Dismantle = floor(paid construction + paid upgrades, multiplied by 0.50; full tree 0.55). Repair is excluded from refundable investment. Upgrade prices are the authored II/III costs, research ×0.90, Ammunition Shortage ×1.20. Completion reward = 300 + 2×(wave−1), full tree ×1.10 floored; see projections.

Full tree: starting +250; blocking HP +20%; Wire HP +20% with no additional slow; selected MG/Cannon damage +5%; weapon fire rate +5%; Sniper range +8%; Mortar/Rocket splash +10%; Flame burn duration +10%; Preparation/combat repair ×0.90; upgrades ×0.90; refunds +0.05; wave reward +10%; information flags. Individual node costs/effects and RP milestones are in the complete asset dump.

Broodmother prefab: first release at 3s, then 6 larvae per 8s, lifetime 60, concurrent 18. Burrower: one 5-cell bypass after 1s surface travel, leaves 8 final cells, follows valid navigation. Spitter flight 0.65s, initial reload 1.5s; range 6 cells and holds a valid firing position through reload. Sniper direct hits ignore 100% of armor; Auto Cannon ignores 75%. Dredger direct emergence and Broodmother Leviathan arrival; escorts are authored in boss assets and expanded fivefold. Carrier capacities, release/rupture/remains timings and all thirteen event modifiers are recorded verbatim below.

### Assets/Game/Data/Carriers/brood_carrier.asset

```json
{
  "method": 2,
  "visualPrefab": {
    "instanceID": 47178
  },
  "capacityHalfUnits": 48,
  "halfExtents": {
    "x": 1.8500000238418579,
    "y": 0.92000001668930054
  },
  "speedMultiplier": 1.0,
  "ruptureSeconds": 0.40000000596046448,
  "remainsSeconds": 0.30000001192092896,
  "releaseInterval": 0.039999999105930328
}
```

### Assets/Game/Data/Carriers/direct_emergence.asset

```json
{
  "method": 0,
  "visualPrefab": {
    "instanceID": 47180
  },
  "capacityHalfUnits": 12,
  "halfExtents": {
    "x": 1.5,
    "y": 0.800000011920929
  },
  "speedMultiplier": 1.0,
  "ruptureSeconds": 0.5,
  "remainsSeconds": 0.30000001192092896,
  "releaseInterval": 0.070000000298023224
}
```

### Assets/Game/Data/Carriers/drift_pod.asset

```json
{
  "method": 1,
  "visualPrefab": {
    "instanceID": 46822
  },
  "capacityHalfUnits": 24,
  "halfExtents": {
    "x": 1.6499999761581421,
    "y": 0.85000002384185791
  },
  "speedMultiplier": 1.0,
  "ruptureSeconds": 0.40000000596046448,
  "remainsSeconds": 1.1000000238418579,
  "releaseInterval": 0.070000000298023224
}
```

### Assets/Game/Data/Carriers/leviathan_carrier.asset

```json
{
  "method": 3,
  "visualPrefab": {
    "instanceID": 47176
  },
  "capacityHalfUnits": 64,
  "halfExtents": {
    "x": 2.4000000953674316,
    "y": 1.2000000476837158
  },
  "speedMultiplier": 1.0,
  "ruptureSeconds": 0.40000000596046448,
  "remainsSeconds": 0.30000001192092896,
  "releaseInterval": 0.070000000298023224
}
```

### Assets/Game/Data/Enemies/Broodmother.asset

```json
{
  "armor": 0.15000000596046448,
  "kind": 8,
  "breachPenalty": 900,
  "difficultyCost": 18,
  "unlockWave": 25,
  "transportCapacityUnits": 4,
  "displayName": "THE BROODMOTHER",
  "maxHealth": 6000,
  "movementSpeed": 1.6499999761581421,
  "attackDamage": 10,
  "attackInterval": 1.0,
  "attackRange": 1.6000000238418579,
  "killReward": 2200,
  "structureDamage": 45,
  "structureAttackInterval": 1.0,
  "structureRangeCells": 0.60000002384185791,
  "bossArt": {
    "instanceID": 47694
  },
  "bossBoat": {
    "instanceID": 47176
  },
  "escorts": [
    {
      "prefab": {
        "instanceID": 47172
      },
      "count": 24,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    },
    {
      "prefab": {
        "instanceID": 47166
      },
      "count": 12,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    },
    {
      "prefab": {
        "instanceID": 46834
      },
      "count": 12,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    }
  ],
  "arrivalMultiplier": 0.800000011920929,
  "boatSpeedMultiplier": 1.0
}
```

### Assets/Game/Data/Enemies/Dredger.asset

```json
{
  "armor": 0.34999999403953552,
  "kind": 7,
  "breachPenalty": 1000,
  "difficultyCost": 20,
  "unlockWave": 25,
  "transportCapacityUnits": 4,
  "displayName": "THE DREDGER",
  "maxHealth": 8000,
  "movementSpeed": 1.1499999761581421,
  "attackDamage": 10,
  "attackInterval": 1.0,
  "attackRange": 1.6000000238418579,
  "killReward": 2500,
  "structureDamage": 120,
  "structureAttackInterval": 1.2000000476837158,
  "structureRangeCells": 0.60000002384185791,
  "bossArt": {
    "instanceID": 47696
  },
  "bossBoat": {
    "instanceID": 47180
  },
  "escorts": [
    {
      "prefab": {
        "instanceID": 47168
      },
      "count": 8,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    },
    {
      "prefab": {
        "instanceID": 46834
      },
      "count": 20,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    },
    {
      "prefab": {
        "instanceID": 47170
      },
      "count": 6,
      "spawnInterval": 0.10000000149011612,
      "delayBeforeGroup": 0.0
    }
  ],
  "arrivalMultiplier": 1.0,
  "boatSpeedMultiplier": 1.0
}
```

### Assets/Game/Data/Events/ammunition-shortage.asset

```json
{
  "id": "ammunition-shortage",
  "displayName": "AMMUNITION SHORTAGE",
  "description": "UPGRADES +20% COST / COMBAT REPAIRS +20% COST.",
  "category": 0,
  "minimumWave": 8,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": true,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.2000000476837158,
    "combatRepair": 1.2000000476837158,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/calm-before-storm.asset

```json
{
  "id": "calm-before-storm",
  "displayName": "CALM BEFORE THE STORM",
  "description": "Major assault expected: +30% creatures, +25% river contact slots.",
  "category": 0,
  "minimumWave": 10,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.2999999523162842,
    "pressure": 1.2999999523162842,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.25,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 8.0
}
```

### Assets/Game/Data/Events/concentrated-landing.asset

```json
{
  "id": "concentrated-emergence",
  "displayName": "CONCENTRATED EMERGENCE",
  "description": "River contacts concentrate on one announced emergence sector.",
  "category": 2,
  "minimumWave": 8,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 2,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/emergency-supply.asset

```json
{
  "id": "emergency-supply",
  "displayName": "EMERGENCY SUPPLY",
  "description": "Emergency RSD paper-money supply delivered.",
  "category": 1,
  "minimumWave": 5,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 3,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37360
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 100,
  "supplyPerWave": 8,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/EndlessEvents.asset

```json
{
  "events": [
    {
      "instanceID": 37288
    },
    {
      "instanceID": 37290
    },
    {
      "instanceID": 37292
    },
    {
      "instanceID": 37294
    },
    {
      "instanceID": 37296
    },
    {
      "instanceID": 37298
    },
    {
      "instanceID": 37300
    },
    {
      "instanceID": 37302
    },
    {
      "instanceID": 37304
    },
    {
      "instanceID": 37306
    },
    {
      "instanceID": 37308
    },
    {
      "instanceID": 37310
    },
    {
      "instanceID": 37312
    }
  ],
  "firstEventWave": 5,
  "minimumGap": 2,
  "maximumGap": 4,
  "laterFrequencyWave": 30,
  "laterMinimumGap": 2,
  "laterMaximumGap": 3,
  "recentExclusion": 2,
  "challengeWeight": 0.699999988079071,
  "reliefWeight": 0.20000000298023224,
  "patternWeight": 0.10000000149011612
}
```

### Assets/Game/Data/Events/field-engineers.asset

```json
{
  "id": "field-engineers",
  "displayName": "FIELD ENGINEERS",
  "description": "PREPARATION REPAIRS 30% CHEAPER.",
  "category": 1,
  "minimumWave": 5,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 3,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37362
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 0.699999988079071,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/flooded-shore.asset

```json
{
  "id": "flooded-shore",
  "displayName": "FLOODED SHORE",
  "description": "First inland column: NO NEW BUILD. Existing structures stay.",
  "category": 0,
  "minimumWave": 8,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": true,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 1,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/fog.asset

```json
{
  "id": "fog",
  "displayName": "FOG",
  "description": "DEFENSE RANGE REDUCED BY 20%.",
  "category": 0,
  "minimumWave": 5,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 0.800000011920929,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": true,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/heavy-assault.asset

```json
{
  "id": "deep-water-disturbance",
  "displayName": "DEEP WATER DISTURBANCE",
  "description": "Unusual biological activity detected in deep water.",
  "category": 0,
  "minimumWave": 15,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37358
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": 0.10000000149011612,
    "broodCarrierRatio": 0.64999997615814209,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": [
      {
        "kind": 2,
        "multiplier": 5.0
      },
      {
        "kind": 3,
        "multiplier": 2.0
      },
      {
        "kind": 5,
        "multiplier": 2.0
      },
      {
        "kind": 4,
        "multiplier": 0.30000001192092896
      }
    ]
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/mass-landing.asset

```json
{
  "id": "mass-emergence",
  "displayName": "MASS EMERGENCE",
  "description": "High biological activity. More organisms approach across multiple sectors.",
  "category": 0,
  "minimumWave": 7,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.5,
    "pressure": 1.5,
    "boatSpeed": 1.0,
    "boatInterval": 0.699999988079071,
    "boatConcurrency": 1.5,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 1,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/militant-assault.asset

```json
{
  "id": "armored-migration",
  "displayName": "ARMORED MIGRATION",
  "description": "Armored bottom-dwellers approach with corrosive support.",
  "category": 0,
  "minimumWave": 10,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37358
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 0,
    "composition": [
      {
        "kind": 2,
        "multiplier": 7.0
      },
      {
        "kind": 3,
        "multiplier": 3.0
      },
      {
        "kind": 1,
        "multiplier": 0.40000000596046448
      },
      {
        "kind": 4,
        "multiplier": 0.20000000298023224
      }
    ]
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/multi-sector.asset

```json
{
  "id": "multi-sector",
  "displayName": "MULTI-SECTOR ATTACK",
  "description": "Biological activity detected across multiple emergence sectors.",
  "category": 2,
  "minimumWave": 8,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 0.699999988079071,
    "boatConcurrency": 1.25,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": -1.0,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 1,
    "composition": []
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/pirate-raid.asset

```json
{
  "id": "swarm-surge",
  "displayName": "SWARM SURGE",
  "description": "Fast river organisms surge toward the quarantine line.",
  "category": 0,
  "minimumWave": 8,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.2000000476837158,
    "boatInterval": 0.75,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": 0.800000011920929,
    "broodCarrierRatio": -1.0,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 1,
    "composition": [
      {
        "kind": 1,
        "multiplier": 5.0
      },
      {
        "kind": 4,
        "multiplier": 4.0
      },
      {
        "kind": 0,
        "multiplier": 1.5
      },
      {
        "kind": 2,
        "multiplier": 0.30000001192092896
      }
    ]
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Events/runner-swarm.asset

```json
{
  "id": "brood-tide",
  "displayName": "BROOD TIDE",
  "description": "Brood organisms release dense larval swarms.",
  "category": 0,
  "minimumWave": 7,
  "maximumWave": 0,
  "weight": 1.0,
  "durationType": 1,
  "duration": 1.0,
  "cooldown": 8,
  "allowBossWave": false,
  "preparationEffects": false,
  "incompatibleEvents": [],
  "uiIcon": {
    "instanceID": 37356
  },
  "modifiers": {
    "quantity": 1.0,
    "pressure": 1.0,
    "boatSpeed": 1.0,
    "boatInterval": 1.0,
    "boatConcurrency": 1.0,
    "defenseRange": 1.0,
    "upgradeCost": 1.0,
    "combatRepair": 1.0,
    "preparationRepair": 1.0,
    "floodedColumns": 0,
    "fog": false,
    "directEmergenceRatio": 0.05000000074505806,
    "broodCarrierRatio": 0.800000011920929,
    "boatVisual": {
      "instanceID": 0
    },
    "landingPattern": 1,
    "composition": [
      {
        "kind": 4,
        "multiplier": 9.0
      },
      {
        "kind": 0,
        "multiplier": 0.40000000596046448
      },
      {
        "kind": 2,
        "multiplier": 0.20000000298023224
      }
    ]
  },
  "supplyBase": 0,
  "supplyPerWave": 0,
  "supplyMaximum": 500,
  "preparationWarningSeconds": 0.0
}
```

### Assets/Game/Data/Research/advanced-fire-control.asset

```json
{
  "id": "advanced-fire-control",
  "displayName": "Advanced Fire Control",
  "description": "Ranged weapon defenses fire 5% faster, including Flame damage ticks. Excludes Wire and Mines.",
  "branch": 2,
  "costRP": 60,
  "tier": 4,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002398
    },
    {
      "instanceID": -2002412
    },
    {
      "instanceID": -2002426
    },
    {
      "instanceID": -2002440
    }
  ],
  "icon": {
    "instanceID": 3274312
  },
  "effects": [
    {
      "kind": 12,
      "amount": 0.05000000074505806
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 318.0
  }
}
```

### Assets/Game/Data/Research/advance-warning.asset

```json
{
  "id": "advance-warning",
  "displayName": "Advance Warning",
  "description": "Preparation reveals the dominant threat class: swarm, armored, or high-speed contacts.",
  "branch": 3,
  "costRP": 20,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002468
    }
  ],
  "icon": {
    "instanceID": 3274334
  },
  "effects": [
    {
      "kind": 14,
      "amount": 1.0
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/area-munitions.asset

```json
{
  "id": "area-munitions",
  "displayName": "Area Munitions",
  "description": "Mortars and Rocket Launchers gain +10% explosion radius.",
  "branch": 2,
  "costRP": 30,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002398
    }
  ],
  "icon": {
    "instanceID": 3274318
  },
  "effects": [
    {
      "kind": 10,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.75,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/biological-analysis.asset

```json
{
  "id": "biological-analysis",
  "displayName": "Biological Analysis",
  "description": "Preparation reveals approximate creature composition: High, Medium, or Low.",
  "branch": 3,
  "costRP": 30,
  "tier": 3,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002482
    }
  ],
  "icon": {
    "instanceID": 3274330
  },
  "effects": [
    {
      "kind": 15,
      "amount": 1.0
    }
  ],
  "boardPosition": {
    "x": 0.25,
    "y": 212.0
  }
}
```

### Assets/Game/Data/Research/deep-water-sensor-network.asset

```json
{
  "id": "deep-water-sensor-network",
  "displayName": "Deep-Water Sensor Network",
  "description": "Preparation reveals expected carrier pressure, emergence sectors, and massive boss signatures.",
  "branch": 3,
  "costRP": 55,
  "tier": 4,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002496
    },
    {
      "instanceID": -2002510
    }
  ],
  "icon": {
    "instanceID": 3274332
  },
  "effects": [
    {
      "kind": 17,
      "amount": 1.0
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 318.0
  }
}
```

### Assets/Game/Data/Research/emergency-fund.asset

```json
{
  "id": "emergency-fund",
  "displayName": "Emergency Fund",
  "description": "+100 starting RSD.",
  "branch": 0,
  "costRP": 10,
  "tier": 1,
  "maxRank": 1,
  "prerequisites": [],
  "icon": {
    "instanceID": 3274284
  },
  "effects": [
    {
      "kind": 0,
      "amount": 100.0
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 0.0
  }
}
```

### Assets/Game/Data/Research/event-forecasting.asset

```json
{
  "id": "event-forecasting",
  "displayName": "Event Forecasting",
  "description": "Reveal the following wave's major event one wave early, before its Preparation.",
  "branch": 3,
  "costRP": 35,
  "tier": 3,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002468
    }
  ],
  "icon": {
    "instanceID": 3274328
  },
  "effects": [
    {
      "kind": 16,
      "amount": 1.0
    }
  ],
  "boardPosition": {
    "x": 0.75,
    "y": 212.0
  }
}
```

### Assets/Game/Data/Research/field-maintenance.asset

```json
{
  "id": "field-maintenance",
  "displayName": "Field Maintenance",
  "description": "Combat repairs cost 10% less. Preparation remains cheaper.",
  "branch": 1,
  "costRP": 30,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002328
    }
  ],
  "icon": {
    "instanceID": 3274306
  },
  "effects": [
    {
      "kind": 6,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.75,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/field-salvage.asset

```json
{
  "id": "field-salvage",
  "displayName": "Field Salvage",
  "description": "Dismantling returns 55% of paid construction and upgrade investment.",
  "branch": 0,
  "costRP": 25,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002258
    }
  ],
  "icon": {
    "instanceID": 3274288
  },
  "effects": [
    {
      "kind": 2,
      "amount": 0.05000000074505806
    }
  ],
  "boardPosition": {
    "x": 0.75,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/fortification-doctrine.asset

```json
{
  "id": "fortification-doctrine",
  "displayName": "Fortification Doctrine",
  "description": "Blocking player defenses gain +10% additional Max HP. Total: +20%. Excludes Wire and Mines.",
  "branch": 1,
  "costRP": 55,
  "tier": 4,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002342
    },
    {
      "instanceID": -2002356
    },
    {
      "instanceID": -2002370
    }
  ],
  "icon": {
    "instanceID": 3274302
  },
  "effects": [
    {
      "kind": 4,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 318.0
  }
}
```

### Assets/Game/Data/Research/hardened-wire.asset

```json
{
  "id": "hardened-wire",
  "displayName": "Hardened Wire",
  "description": "Barbed Wire gains +20% Max HP. Slow strength is unchanged.",
  "branch": 1,
  "costRP": 20,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002328
    }
  ],
  "icon": {
    "instanceID": 3274298
  },
  "effects": [
    {
      "kind": 5,
      "amount": 0.20000000298023224
    }
  ],
  "boardPosition": {
    "x": 0.25,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/improved-ammunition.asset

```json
{
  "id": "improved-ammunition",
  "displayName": "Improved Ammunition",
  "description": "MG Nests and Auto Cannons gain +5% damage.",
  "branch": 2,
  "costRP": 20,
  "tier": 1,
  "maxRank": 1,
  "prerequisites": [],
  "icon": {
    "instanceID": 3274320
  },
  "effects": [
    {
      "kind": 8,
      "amount": 0.05000000074505806
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 0.0
  }
}
```

### Assets/Game/Data/Research/logistics-network.asset

```json
{
  "id": "logistics-network",
  "displayName": "Logistics Network",
  "description": "Wave-completion RSD rewards increase by 10%. Kill income is unchanged.",
  "branch": 0,
  "costRP": 35,
  "tier": 3,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002272
    }
  ],
  "icon": {
    "instanceID": 3274292
  },
  "effects": [
    {
      "kind": 3,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 212.0
  }
}
```

### Assets/Game/Data/Research/modular-construction.asset

```json
{
  "id": "modular-construction",
  "displayName": "Modular Construction",
  "description": "Level II and III upgrades cost 10% less. Base construction costs stay unchanged.",
  "branch": 1,
  "costRP": 35,
  "tier": 3,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002328
    }
  ],
  "icon": {
    "instanceID": 3274300
  },
  "effects": [
    {
      "kind": 7,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 212.0
  }
}
```

### Assets/Game/Data/Research/optical-calibration.asset

```json
{
  "id": "optical-calibration",
  "displayName": "Optical Calibration",
  "description": "Sniper Towers gain +8% range.",
  "branch": 2,
  "costRP": 25,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002398
    }
  ],
  "icon": {
    "instanceID": 3274316
  },
  "effects": [
    {
      "kind": 9,
      "amount": 0.079999998211860657
    }
  ],
  "boardPosition": {
    "x": 0.25,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/reinforced-emplacements.asset

```json
{
  "id": "reinforced-emplacements",
  "displayName": "Reinforced Emplacements",
  "description": "Blocking player defenses gain +10% Max HP. Excludes Wire and Mines.",
  "branch": 1,
  "costRP": 15,
  "tier": 1,
  "maxRank": 1,
  "prerequisites": [],
  "icon": {
    "instanceID": 3274304
  },
  "effects": [
    {
      "kind": 4,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 0.0
  }
}
```

### Assets/Game/Data/Research/ResearchRewards.asset

```json
{
  "minimumWave": 5,
  "bossReward": 10,
  "qualityWaveStep": 20,
  "qualityPerStep": 5,
  "qualityMaximum": 20,
  "waveFactor": 0.5,
  "qualityHomicidesMaximum": 250,
  "qualityHomicidesPerWaveMaximum": 25.0,
  "maximumStandardReward": 10000,
  "milestones": [
    {
      "id": "first-wave-5",
      "mapId": "",
      "condition": 0,
      "wave": 5,
      "reward": 8,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-wave-10",
      "mapId": "",
      "condition": 0,
      "wave": 10,
      "reward": 5,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-wave-25",
      "mapId": "",
      "condition": 0,
      "wave": 25,
      "reward": 10,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-wave-50",
      "mapId": "",
      "condition": 0,
      "wave": 50,
      "reward": 20,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-boss",
      "mapId": "",
      "condition": 1,
      "wave": 0,
      "reward": 10,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-dredger",
      "mapId": "",
      "condition": 2,
      "wave": 0,
      "reward": 10,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "first-broodmother",
      "mapId": "",
      "condition": 3,
      "wave": 0,
      "reward": 10,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "clean-wave-10",
      "mapId": "",
      "condition": 4,
      "wave": 10,
      "reward": 5,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 25.0
    },
    {
      "id": "border-wave-20",
      "mapId": "river_border",
      "condition": 5,
      "wave": 20,
      "reward": 5,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    },
    {
      "id": "port-wave-20",
      "mapId": "industrial_port",
      "condition": 5,
      "wave": 20,
      "reward": 5,
      "maxHomicides": 250,
      "maxHomicidesPerWave": 0.0
    }
  ]
}
```

### Assets/Game/Data/Research/reserve-stockpile.asset

```json
{
  "id": "reserve-stockpile",
  "displayName": "Reserve Stockpile",
  "description": "+150 additional starting RSD. Total starting research bonus: +250 RSD.",
  "branch": 0,
  "costRP": 50,
  "tier": 4,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002286
    },
    {
      "instanceID": -2002300
    }
  ],
  "icon": {
    "instanceID": 3274290
  },
  "effects": [
    {
      "kind": 0,
      "amount": 150.0
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 318.0
  }
}
```

### Assets/Game/Data/Research/river-monitoring.asset

```json
{
  "id": "river-monitoring",
  "displayName": "River Monitoring",
  "description": "Preparation reveals the approximate next-wave organism count.",
  "branch": 3,
  "costRP": 10,
  "tier": 1,
  "maxRank": 1,
  "prerequisites": [],
  "icon": {
    "instanceID": 3274326
  },
  "effects": [
    {
      "kind": 13,
      "amount": 1.0
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 0.0
  }
}
```

### Assets/Game/Data/Research/supply-contracts.asset

```json
{
  "id": "supply-contracts",
  "displayName": "Supply Contracts",
  "description": "Preparation repairs cost 10% less, after the normal Preparation discount.",
  "branch": 0,
  "costRP": 20,
  "tier": 2,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002258
    }
  ],
  "icon": {
    "instanceID": 3274286
  },
  "effects": [
    {
      "kind": 1,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.25,
    "y": 106.0
  }
}
```

### Assets/Game/Data/Research/thermal-mixture.asset

```json
{
  "id": "thermal-mixture",
  "displayName": "Thermal Mixture",
  "description": "Flamethrower burns last 10% longer.",
  "branch": 2,
  "costRP": 30,
  "tier": 3,
  "maxRank": 1,
  "prerequisites": [
    {
      "instanceID": -2002398
    }
  ],
  "icon": {
    "instanceID": 3274314
  },
  "effects": [
    {
      "kind": 11,
      "amount": 0.10000000149011612
    }
  ],
  "boardPosition": {
    "x": 0.5,
    "y": 212.0
  }
}
```

### Assets/Game/Data/Waves/EndlessSurvival.asset

```json
{
  "enemies": [
    {
      "instanceID": 46834
    },
    {
      "instanceID": 47166
    },
    {
      "instanceID": 47168
    },
    {
      "instanceID": 47170
    },
    {
      "instanceID": 47172
    },
    {
      "instanceID": 47174
    }
  ],
  "baseBudget": 16,
  "linearGrowth": 8.0,
  "nonlinearGrowth": 0.079999998211860657,
  "maximumEnemiesPerWave": 1200,
  "maximumActiveEnemies": 400,
  "seed": 1977,
  "randomizeSeed": true,
  "baseReward": 300,
  "rewardGrowth": 2,
  "scalingStartWave": 20,
  "healthGrowth": 0.012500000186264515,
  "speedGrowth": 0.0020000000949949026,
  "maximumHealthMultiplier": 20.0,
  "maximumSpeedMultiplier": 1.6000000238418579,
  "earlyArrivalInterval": 1.0,
  "minimumArrivalInterval": 0.30000001192092896,
  "arrivalGrowth": 0.079999998211860657,
  "wavesPerBoatConcurrency": 2,
  "maximumConcurrentBoats": 28,
  "initialConcurrentBoats": 6,
  "bossEncounters": {
    "instanceID": 40990
  },
  "eventSettings": {
    "instanceID": 37200
  },
  "enemyQuantityMultiplier": 5,
  "globalEnemyHealthMultiplier": 1.0,
  "bossHealthMultiplier": 1.0,
  "pressureDirectorEnabled": true,
  "earlyPressureTarget": 8,
  "maximumPressureTarget": 240,
  "pressureGrowthPerWave": 4.0,
  "pressureHysteresis": 0.20000000298023224,
  "directorEvaluationInterval": 0.20000000298023224,
  "acceleratedArrivalInterval": 0.15000000596046448,
  "firstAttackDelay": 0.25,
  "boatCapacity": 12,
  "boatSpeed": 9.0,
  "unloadInterval": 0.070000000298023224,
  "disembarkSeparation": 0.40000000596046448,
  "departureDelay": 0.15000000596046448,
  "assaultSurgesEnabled": true,
  "surgePeriod": 24.0,
  "surgeDuration": 7.0,
  "surgeExtraBoats": 3,
  "showPressureDebug": false,
  "writeBalanceLog": false
}
```

