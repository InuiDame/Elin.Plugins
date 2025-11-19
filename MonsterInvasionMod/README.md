# Monster Invasion
Mod Url ———— 

## CFG Configuration Options:
```
 CFG_Chance – Invasion trigger probability
Default: 15%, accepts values 1–100

CFG_CooldownDays – Cooldown in days
Default: 90, accepts any integer value

CFG_EnableLog – Logging toggle
Default: Enabled

CFG_NpcMinLv / CFG_NpcMaxLv – NPC level range
Default: 1–1999

CFG_SizeRoll – Invasion size control
0 = Random: 50% Small (population/8), 35% Medium (population/4), 15% Large (population/2)
1 = Small: Fixed at population/8
2 = Medium: Fixed at population/4
3 = Large: Fixed at population/2
4 = Extra Large: Fixed at population count
≥5 = Custom number, max 200
```

## Console Commands:
```
invasion.Trigger [int] – Immediately spawns an invasion in the current town or territory

invasion.SetCooldown [int] – Sets invasion cooldown

invasion.SetChance [0–100] – Sets the probability of an invasion triggering the first time you enter a map each day

invasion.ClearCooldown – No parameters, clears invasion cooldown

invasion.Status – Checks if the current zone is a town, whether an invasion can occur, and displays cooldown and probability

invasion.Clear – Clears the current invasion event and enemies
```