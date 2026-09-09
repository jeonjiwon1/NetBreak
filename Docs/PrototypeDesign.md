# NETBREAK
## Prototype Design Document v1.0

**Purpose:** Validate the core gameplay loop  
**Engine:** Unity 6 / C#  
**Team Size:** 1 developer  
**Target Development Time:** Approximately 2–4 weeks  
**Target Session Length:** Approximately 10–15 minutes

---

# 1. Prototype Goal

The prototype is not intended to implement the full NETBREAK design.

It exists to answer one primary question:

> **Is it fun to manipulate fish schools, prepare a capture setup, and then catch a large number of fish at once?**

Do not expand major content until this loop is proven.

---

# 2. Prototype Core Loop

**Fish school enters**

↓

**Observe movement**

↓

**Place bait**

↓

**Gather fish**

↓

**Use nets to restrict movement**

↓

**Capture with landing net / cast net**

↓

**Gain Gold and EXP**

↓

**Choose simple augments**

↓

**Larger fish schools enter**

↓

**Final school**

↓

**Final Fishing countdown**

↓

**Catch-rate result**

↓

**Success / Failure**

---

# 3. Prototype Scope

Implement only the systems necessary to test the core loop.

Primary content:

- 1 map
- 3 fish species
- landing-net direct capture
- bait
- net
- cast net
- simple school movement
- Gold
- EXP
- simple augments
- finite fish influx
- final countdown
- catch-rate clear/fail
- minimal UI
- basic game feel

---

# 4. Fish Species

## Sardine

- low Resistance
- small size
- strong schooling
- appears in large groups

Purpose: validate mass capture.

## Mackerel

- average Resistance
- average speed
- standard school behavior

Purpose: baseline fish.

## Tuna

- high Resistance
- fast movement
- high Catch Value
- smaller schools

Purpose: validate priority-target and high-value decisions.

---

# 5. FishData

Use ScriptableObject-based FishData.

Minimum fields:

- FishName
- Resistance
- MoveSpeed
- CatchValue
- BaitAttraction
- SchoolStrength
- Lifetime

Do not implement complex randomized weight/size systems yet.

---

# 6. Fish Movement

No fixed lanes.

Fish move freely in the water.

Minimum lifecycle:

**Enter → Active Movement → Bait Response → Exit → Despawn**

School movement requires only enough logic to make group behavior readable.

Minimum components:

- base movement direction
- movement toward school center
- separation
- bait attraction

Do not attempt realistic Boids simulation.

---

# 7. Fish Count

Initial target:

**50 simultaneous fish**

Then test:

**50 → 100 → 200**

Do not design the prototype around thousands of fish.

Optimize only when profiling shows a real bottleneck.

---

# 8. Landing Net

The cursor controls a small capture area.

Input:

**LMB → Use Landing Net**

Fish inside the radius lose Resistance.

When Resistance reaches zero, the fish is captured.

Minimum parameters:

- CapturePower
- CaptureRadius
- AttackCooldown

Avoid pixel-perfect clicking on individual fish.

---

# 9. Bait

Place bait at the cursor position.

Fish inside the influence radius are attracted toward it.

Minimum parameters:

- AttractionRadius
- AttractionStrength
- Duration
- Cooldown or Cost

The system succeeds if the player can intentionally gather schools into better capture positions.

---

# 10. Net

Free-placement control/capture tool.

Input:

**Click start point → Drag → Release at end point**

A simple line net is created.

Fish touching the net:

- slow down
- lose Resistance over time

Do not simulate realistic rope/net physics.

Net durability is optional for the prototype and can be postponed.

---

# 11. Cast Net

Manual active skill.

Input:

**Q → Cast at cursor position**

Applies high capture power in a circular area.

Minimum feedback:

- targeting area
- cast effect
- number of fish caught
- cooldown display
- basic sound/VFX feedback

This is the primary mass-capture feel test.

---

# 12. Gold

Captured fish grant Gold immediately.

Gold usage should remain simple.

Possible prototype uses:

- place net
- place bait
- basic gear upgrade

Do not build a large shop system.

---

# 13. EXP and Augments

Captured fish grant Fishing EXP.

Level-up pauses gameplay.

Present three random augments and choose one.

Target approximately **8–12 augments**.

Example augments:

- landing-net radius +
- landing-net capture power +
- landing-net speed +
- cast-net radius +
- cast-net cooldown -
- cast-net power +
- bait radius +
- bait duration +
- net capture power +
- net length +

Add only a few rule-changing augments after the basic systems are stable.

---

# 14. Fish Influx

Internally, a WaveManager-like system can control groups.

The player-facing terminology should fit fishing rather than tower defense.

Examples:

- Small school approaching
- Large school approaching
- Large target detected

Target approximately 8–10 school arrivals in one prototype session.

---

# 15. Final Fishing

After the last school enters, begin a final countdown.

Example:

**Final Fishing: 60 seconds remaining**

At timer end, calculate final Catch Rate.

Exact duration is playtest-tuned.

---

# 16. Catch Rate

Each fish contributes Catch Value.

Formula:

**CatchRate = CapturedCatchValue / TotalSpawnedCatchValue**

Prototype clear rule can initially be:

**Catch Rate ≥ 80% → Success**

The threshold is not final and must be tuned through testing.

---

# 17. Prototype Map

Only one map.

Prefer one-screen gameplay or only minimal camera movement.

Placeholder art is acceptable.

Examples:

- simple fish sprites
- colored temporary shapes
- bait icon
- LineRenderer net
- circular cast-net indicator

Do not spend time on final visual assets.

---

# 18. Minimum UI

Implement only:

- Gold
- EXP
- current Catch Rate
- cast-net cooldown
- final countdown
- augment selection
- success/failure result

Do not implement:

- encyclopedia UI
- meta-progression menus
- full shop
- complex settings UI

---

# 19. Explicitly Excluded from Prototype

Do not implement yet:

- six full areas
- first job advancement
- second job advancement
- permanent skill tree
- Hard Mode
- Hard-exclusive currency
- fantasy expansion regions
- encyclopedia
- rare variants
- randomized size/weight
- shark ecosystem
- complex fish interactions
- fishing rod
- traps/pots
- longlines
- bosses
- story
- NPCs
- final graphics
- final audio
- Steam integration
- multiplayer

These systems are deliberately postponed until the core loop is proven.

---

# 20. Development Order

## STEP 1
Create Unity project.

Configure Git/GitHub.

Create base folders and scene.

## STEP 2
Spawn one fish.

Make it enter, move, and leave the area.

## STEP 3
Create FishData ScriptableObject.

Create Sardine, Mackerel, Tuna data.

## STEP 4
Implement landing-net direct capture.

Resistance reaches zero → Capture → Gold.

## STEP 5
Apply Object Pooling.

Test 50 → 100 → 200 fish.

## STEP 6
Implement simplified school movement.

## STEP 7
Implement bait attraction.

## STEP 8
Implement free-placement net.

## STEP 9
Implement cast net.

## STEP 10
Implement Gold and EXP.

## STEP 11
Implement 8–12 simple augments.

## STEP 12
Implement fish-school influx manager.

## STEP 13
Implement Final Fishing and Catch Rate.

## STEP 14
Implement success/failure.

## STEP 15
Add minimum VFX/SFX/catch feedback.

## STEP 16
Playtest.

---

# 21. Prototype Success Questions

1. Does a large school make the screen more interesting?
2. Can players understand school movement?
3. Is gathering fish with bait satisfying?
4. Does net placement create meaningful decisions?
5. Do players wait for a good cast-net timing?
6. Does catching many fish at once feel strongly rewarding?
7. Is landing-net interaction repetitive?
8. Do augments noticeably change play?
9. Does Catch Rate create tension until the end?
10. Can players understand why they failed?
11. Does the game remain interesting for 10+ minutes?
12. Do players want to immediately play another run?

Most important playtest question:

> **What was the most fun moment of the run?**

Ideally, multiple players mention mass capture or the preparation that led to it.

---

# 22. Prototype Evaluation

## Success

The **LURE → TRAP → CATCH** loop is clearly fun.

→ Move to Vertical Slice.

Potential Vertical Slice additions:

- fishing rod
- more fish species
- rule-changing augments
- simple first-job system
- second-job test
- first polished area

## Partial Success

Mass capture feels good, but bait or nets are weak.

→ Redesign the weak system.

## Failure

Gathering and mass-catching fish is not fun.

→ Do not add content.

Rework the core loop first.

---

# 23. Prototype Completion Condition

The prototype is complete when the following experience exists:

1. A visible school of fish enters.
2. The player uses bait to influence and gather it.
3. Nets are used to shape or restrict movement.
4. The player waits for an advantageous moment.
5. A cast net captures many fish at once.
6. The catch produces strong enough feedback to feel rewarding.
7. The player wants to try another run.

Feature count alone does not define completion.
