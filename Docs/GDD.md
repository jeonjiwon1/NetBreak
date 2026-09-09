# NETBREAK
## Game Design Document v1.0

**Genre:** 2D Roguelite Fishing Defense / Strategy  
**Platform:** PC  
**Engine:** Unity 6 / C#  
**Development:** Solo  
**Project Name:** NETBREAK (working title)  
**Core Keywords:** Fish schools / Fishing / Free placement / Roguelite / Builds / Job progression / Mass capture

---

# 1. Game Overview

NETBREAK is a 2D roguelite fishing-defense game in which the player manipulates freely moving fish schools with bait, nets, rods, cast nets, and direct cursor-based capture.

Unlike traditional tower defense, fish do not move along fixed lanes. They move through a broad water area according to species-specific behavior, school tendencies, bait response, and environmental effects.

The player observes these behaviors, manipulates the flow of fish, prepares capture zones, and ultimately captures large groups at once.

Every run begins from Area 1. During the run, the player acquires augments and advances from a Beginner Fisher into a specialized fishing profession. The Normal-mode objective is to clear all major areas in a single expedition and capture the final legendary creature.

---

# 2. Core Fantasy

The player begins as an inexperienced fisher catching small coastal fish with basic gear.

As the run progresses, the player develops a specialized fishing method, reaches increasingly dangerous waters, and eventually captures enormous and fantastical creatures.

The early game should feel relatively grounded. The later game can gradually become more exaggerated and comic-fantasy in tone.

Long-term expansion may include volcanic seas, frozen waters, ghost seas, hellish waters, void zones, and even space.

---

# 3. Core Design Principles

## 3.1 LURE → TRAP → CATCH

**LURE**  
Use bait and species behavior to influence fish movement.

**TRAP**  
Use nets and other gear to restrict or redirect the school.

**CATCH**  
Use direct capture tools, cast nets, rods, and other specialized equipment to complete the catch.

## 3.2 Primary Reward Moment

The most important moment is not catching a single fish.

It is the moment when the player successfully gathers a large school into a favorable position and captures a large number of fish at once.

> Preparation is the strategy. Mass capture is the reward.

---

# 4. Run Structure

Every run begins as a **Beginner Fisher** in Area 1.

A full Normal run:

**Area 1 → Area 2 → Area 3 → Area 4 → Area 5 → Area 6 → Final Boss → Ending**

There are no checkpoints.

Failure in any area ends the run. The next run begins again from Area 1.

Repetition is made interesting through different augments, gear priorities, and job specializations.

Run-specific power resets after the run ends.

---

# 5. Area Progression

Target Normal areas:

1. **Coast** — fundamentals and basic fish schools
2. **Open Sea** — faster fish and larger targets
3. **Coral Sea** — terrain interaction and more constrained movement
4. **Deep Sea** — unusual fish and information restrictions
5. **Storm Sea** — currents and environmental disruption
6. **Abyss** — final test combining previous systems

The exact number, order, and rules of areas may change during development.

---

# 6. Fish-School System

Fish do not use fixed lanes.

Typical fish lifecycle:

**Enter Area → School Activity → React to Bait/Environment → Exit Warning → Leave Area**

School movement should be predictable enough for planning, rather than purely random.

Prototype movement can use simplified versions of:

- cohesion
- alignment
- separation
- base movement
- random variation
- bait attraction
- species-specific behavior

Game readability is more important than realistic simulation.

---

# 7. Fish Data

Potential fish attributes:

- Resistance
- Move Speed
- Size
- Weight
- School Affinity
- Bait Response
- Catch Value
- Rarity
- Traits

Fish are **captured** when Resistance reaches zero.

They are not framed as being killed.

---

# 8. Catch Rate and Area Clear

Fish have different Catch Values.

A sardine and a large tuna do not contribute equally.

Area performance is calculated using the percentage of total available Catch Value successfully captured.

After the final school enters, a **Final Fishing** countdown begins.

When the timer ends, the area result is calculated.

Possible ranks:

- C
- B
- A
- S
- PERFECT

Normal progression should require a reasonable clear threshold rather than 100%.

Exact thresholds are determined by playtesting.

---

# 9. Direct Capture

There is no directly controlled walking character in the core design.

The cursor represents the player's intervention point.

The basic capture tool is a small landing-net area around the cursor.

Early-game direct capture is important.

Later, for most builds, it becomes a precision tool for:

- rare fish
- escaping fish
- high-value targets
- cleanup

A landing-net specialization can keep it viable as a primary late-game build.

---

# 10. Major Fishing Gear

## Landing Net

Direct cursor-based capture.

Possible upgrade axes:

- capture power
- attack speed
- radius
- simultaneous targets
- chain capture

## Bait

Manipulates fish movement.

Possible upgrades:

- attraction radius
- duration
- species-specific attraction
- school concentration

## Net

Free-placement control/capture tool.

Recommended input:

**Click → Drag → Release**

The length and direction of the net are chosen freely.

Nets can slow fish and reduce Resistance.

In the full game, nets may have durability.

## Cast Net

Manual active skill aimed at the cursor.

Primary mass-capture tool.

## Fishing Rod

Automatic/specialized single-target tool, especially effective against large fish.

## Future Gear

- traps/pots
- longlines
- specialized bait
- environment-specific gear
- fantasy equipment

---

# 11. Free Placement

No grid is required.

Fishing gear is placed freely in the water.

Some areas may contain invalid placement zones such as rocks, coral, hazards, or currents.

Minimum spacing or collision rules can prevent stacking all gear in a single optimal point.

When moving to a new area, physical placement resets.

Owned gear, upgrades, augments, Gold, and the run build remain.

---

# 12. Run Economy

Captured fish grant Gold immediately.

Gold is spent during the current run on:

- gear placement
- gear purchase
- upgrades
- repairs
- shops
- rerolls
- other run decisions

Gold resets when the run ends.

A small base reward between schools/areas may prevent irreversible early snowball failure.

---

# 13. Augments

Captured fish also provide Fishing EXP.

Level-up pauses the game and presents three random augments.

The player chooses one.

Possible rarity structure:

- Common
- Rare
- Legendary

Some stat augments can stack.

Rule-changing augments are generally unique.

Augments reset after the run.

---

# 14. Build Philosophy

A player should not maximize every available gear type in a single run.

The intended build usually focuses on **2–3 systems**.

Typical structure:

**Primary + Secondary + Utility**

Examples:

- Cast Net + Bait + Landing Net
- Net + Bait + Fishing Rod
- Landing Net + Cast Net + Bait
- Fishing Rod + Net + Special Bait

Focused investment should outperform spreading upgrades evenly across all gear.

---

# 15. Job Progression

Job progression is a major run-growth system.

Every run begins as a:

**Beginner Fisher**

## 1st Job Advancement

Early/mid-run, the player selects a broad specialization.

Examples:

- Cast-Net Fisher
- Net Fisher
- Angler
- Landing-Net Fisher

The first job determines the direction of the build and increases access to relevant augments.

## 2nd Job Advancement

Later in the run, the first job branches into more specialized professions.

Example structure:

**Cast-Net Fisher**
- Special Cast-Net Unit
- Big-Fish Net Hunter

**Net Fisher**
- Net Craftsman
- School Blockader

**Angler**
- Big-Game Angler
- Longline Specialist

**Landing-Net Fisher**
- Veteran Netter
- Chain Catcher

The first job chooses the build direction.

The second job determines the final play style.

All job progress resets after the run.

---

# 16. Job Selection Rules

Job progression should not be purely random.

The game can recommend jobs based on current gear usage and acquired augments.

However, the player should retain meaningful agency over the final choice.

Second-job branches should generally be directly selectable rather than randomly offered.

After specialization, the relevant augment pool receives increased weight.

---

# 17. Late-Game Power

Late-game builds are allowed to become extremely powerful.

Examples:

- cast nets covering a huge portion of the screen
- hundreds of fish captured at once
- cooldown refunds after mass capture
- connected nets creating large lockdown zones
- chain landing-net captures
- instant high-value target capture

The goal is not perfect numerical uniformity.

Different builds should break the game in different ways.

Later areas become correspondingly more extreme.

---

# 18. Meta Progression

Meta progression should primarily unlock **options**, not raw power.

> Meta progression expands what can appear. Run progression creates the actual power.

Potential permanent unlocks:

- new gear
- new augments
- new first jobs
- new second jobs
- shop systems
- additional starting choices
- encyclopedia features
- quality-of-life features

Permanent raw-stat growth should be limited.

---

# 19. Persistent Currency

Both success and failure grant persistent currency.

The reward can be based on:

- capture score
- area reached
- bosses captured
- special objectives
- full-run clear bonus

Winning should be clearly more efficient than repeatedly failing.

---

# 20. Fish Ecosystem Interactions

Long-term fish species should interact with each other.

Examples:

- sharks consume small fish
- school leaders influence movement
- pufferfish interfere with nets
- squid temporarily disable gear
- predators scatter nearby schools

A predator that consumes other fish may become heavier, harder to catch, and more valuable.

This creates risk/reward decisions such as whether to catch it early or allow it to grow.

---

# 21. Normal Ending

The Normal-mode objective is to clear Areas 1–6 in one expedition.

The final boss should test the player's complete fishing system rather than simply having excessive Resistance.

Capturing the final creature unlocks a clear Normal Ending.

---

# 22. Post-Game

Normal completion unlocks two separate content directions.

## Risk Fishing / Hard Mode

Existing areas gain modifiers such as:

- stronger currents
- larger schools
- increased Resistance
- lower net durability
- lower economy
- night fishing
- environmental hazards

Hard mode may use a Risk Level system.

Hard-exclusive currency should primarily unlock new build possibilities rather than huge permanent power.

## Uncharted Routes

New regions are separate from difficulty scaling.

Potential expansions:

- volcanic sea
- frozen sea
- ghost sea
- hell sea
- void sea
- space sea

Every new region must introduce at least one new gameplay rule.

---

# 23. Collection

Long-term records may include:

- species discovery
- catch count
- maximum size
- maximum weight
- rare variants
- best catch score

This provides goals beyond the Normal ending.

---

# 24. Technical Direction

Target engine:

- Unity 6
- C#

Technical priorities:

- Object Pooling
- data-driven FishData / GearData / AugmentData
- simplified school simulation
- minimize unnecessary Rigidbody2D usage
- avoid expensive per-fish operations
- introduce Spatial Grid/Hash only if profiling justifies it

Do not begin with ECS/DOTS unless actual performance requirements demand it.

---

# 25. Final Identity

NETBREAK must not become conventional tower defense with fish-themed enemies.

Its identity comes from:

1. large moving fish schools
2. bait/path manipulation
3. free-form fishing-gear placement
4. active player intervention
5. preparation followed by explosive mass capture
6. specialized roguelite job/build progression

The intended run-growth arc is:

**Beginner Fisher → 1st Job → 2nd Job → Completed Fishing Build → Abyss Clear**
