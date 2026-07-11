# Dungeon Surge Game Manual

This manual describes the currently implemented playable flow in the project.

## Overview

Dungeon Surge is a top-down survival action game built around short melee combat, enemy waves, boss encounters, and between-level stat growth. Your goal is to survive each level's waves, defeat its boss, and carry your upgraded character into the next stage.

## Starting A Run

From the main menu, select New Game to begin a fresh run.

- A new game resets your saved run stats.
- The game starts in Level 1.
- Clearing Level 1 automatically loads Level 2 after a short completion screen.
- Clearing Level 2 ends the run and returns you to the main menu after the final victory screen.

## Controls

- Move: uses the game's Horizontal and Vertical movement inputs. In a typical setup this means WASD or the arrow keys.
- Attack: uses the Fire1 input. In a typical setup this is left mouse button or a keyboard attack binding.
- Pause: Escape.

If the project input bindings are changed in Unity, the exact keys may differ.

## Core Gameplay Loop

Each level follows the same structure:

1. A short countdown starts the stage.
2. Enemy waves begin spawning.
3. Defeat enemies and collect the gold they drop.
4. Collected gold also grants experience.
5. When you level up, the game pauses and offers upgrade choices.
6. After all regular waves are cleared, a boss encounter begins.
7. Defeat the boss to finish the level or complete the run.

## Level Structure

The current project contains three scenes:

- MainMenu
- Level1
- Level2

### Level 1

- Contains 5 enemy waves.
- Finishing the waves triggers the Goblin King boss fight.
- Defeating the boss completes the level.
- Your current build and stats are saved before Level 2 loads.

### Level 2

- Contains 5 enemy waves.
- Finishing the waves triggers the Vampire Bat boss fight.
- Defeating the boss triggers the final victory sequence and returns you to the main menu.

## Combat

The player uses a close-range melee attack.

- Attacks have a short cooldown, so timing matters.
- You must be near enemies to hit them.
- Taking damage can knock you back and briefly interrupt your control.
- Boss intros temporarily disable movement and attacking during the entrance cutscene.

## Health And Survival

- Your run starts with a health pool.
- When health reaches zero, the run ends in Game Over.
- Defense upgrades reduce incoming damage.
- Defense is capped, so it cannot scale forever.
- Max Health upgrades increase both your maximum health and your current health.
- Health Regeneration upgrades restore health automatically over time.

## Gold, Experience, And Leveling

Gold is both a resource pickup and your source of experience.

- Enemies can drop gold when defeated.
- Gold is collected by moving near it; it then pulls into the player automatically.
- Each point of collected gold also adds the same amount of experience.
- Every new level requires more experience than the last.

This means efficient gold collection is part of progression, not just a score mechanic.

## Upgrade System

When you level up, the game opens an upgrade panel and pauses gameplay. You are shown a random set of upgrade cards and choose one.

The current upgrade categories are:

- Attack: increases melee damage.
- Defense: reduces incoming damage.
- Max Health: increases total survivability.
- Move Speed: improves mobility.
- Health Regen: restores health passively over time.

Because the choices are randomized, each run can develop differently.

## Menus And Game States

### Main Menu

- Start a new run.
- Adjust audio volume.
- Apply, cancel, or reset audio changes.
- Exit the game.

### Pause Menu

Press Escape during gameplay to open the pause menu.

- Resume the run.
- Open the sound settings.
- Return to the main menu.

### Game Over

If your health reaches zero:

- Music stops.
- A Game Over panel appears.
- Gameplay stops.

### Victory And Level Completion

- Level completion hides active gameplay UI and shows a completion panel.
- Final victory hides gameplay UI, shows the victory panel, and returns you to the main menu after a short delay.

## Practical Tips

- Stay close enough to collect gold after each skirmish, because gold directly powers leveling.
- Early survivability upgrades can make later wave sets easier to manage.
- Mobility becomes more valuable as wave density increases.
- Expect a short loss of control when a boss encounter starts.
- Treat each level as two phases: wave survival first, boss duel second.

## Short Version

If you want the entire game explained in a few lines:

1. Start a new run from the main menu.
2. Survive 5 waves in Level 1.
3. Beat the Goblin King.
4. Carry your upgraded stats into Level 2.
5. Survive 5 more waves.
6. Beat the Vampire Bat.
7. Win and return to the main menu.