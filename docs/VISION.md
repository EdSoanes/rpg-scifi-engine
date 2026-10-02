# Vision

This document is the reference for what the project is trying to achieve. Every design decision and every
piece of functionality should be checked against it. Where the code and this document disagree, the code
falls short.

## The goal

A tabletop rpg system where the players and the gamesmaster **do not need to understand the rules in order
to play**.

The product is a digital character sheet that runs the rules and does the number crunching, so that players
can focus on what their characters do rather than on the math needed to work out what happens.

## Motivation

Sci-fi tabletop rpgs are fun, and the best ones are very number crunchy. The level of detail needed to make
a good one is more than people can remember and calculate at the table. The usual result is a compromised
game system: either the detail is cut, or play slows to a crawl.

If the character sheet carries the rules and the arithmetic, the system can keep the detail without
burdening the people playing it.

## Critical design features

### 1. Flexibility

Tabletop rpgs excel where computer rpgs fail. Tabletop players can ignore rules, arbitrarily change values,
cancel conditions for any reason and at any time, even cheat. That is how a group makes the game their own.

The system must allow all of this.

What this demands:

- Any value can be changed by hand at any time, and the change behaves like any other modification:
  dependent values update, it is visible, and it can be undone.
- Any state or condition can be switched on or off by hand, regardless of what the rules say.
- Any step of an action can be redone, skipped or abandoned.
- The system never refuses. It has no "you cannot do that".

### 2. Constraints are known but never enforced

A rules system might say a gun only fires one type of bullet. A GM can always decide it works with other
ammo. The system must know the constraint and must let it be ignored **without friction**.

What this demands:

- Rules are expressed as information: "this is not normally allowed, and here is why". They are never
  expressed as a block.
- The user interface can warn, grey out or ask, but the engine performs the action when told to.
- Limits such as minimums, maximums, capacity and prerequisites are advisory.

### 3. Real dice or app dice

A player can roll real dice and type in the result, or let the app roll with a button press. Both are
first-class.

What this demands:

- A roll is always presented as a dice expression first, for example 2d6+4.
- Nothing is rolled until the player asks for it. The app never rolls on the player's behalf unprompted.
- A typed-in result and an app-rolled result are stored and treated identically.
- A roll can be redone or replaced.

### 4. Transparency

At any point a player can see clearly how a stat or a dice roll is derived. Why is the damage roll 2d6+4?
Why does this weapon get +4 against this target under these conditions?

What this demands:

- Every value can be described as the tree of modifications that produce it, down to the original sources.
- Every modification has a human-readable origin: which stat, item, state, action or manual change it came
  from.
- Values calculated inside rules code must still leave a trail. A number that appears with no explanation
  is a defect.
- Transparency applies to transient values too, such as the dice expression and target number of an action
  in progress.

### 5. Fuzzy time

Time keeping is critical in tabletop rpgs, but it can be fast-forwarded or reversed at will. The GM might
replay a turn, or declare "now it is nightfall". The system must handle this and keep every derived value
correct.

What this demands:

- Time can move forwards and backwards, inside and outside an encounter.
- Moving backwards restores the state that held at that time: values, states, where items were.
- Nothing that might be needed for a replay is destroyed while it can still be reached.
- Derived values are always consistent with the current time, without the caller doing anything extra.

### 6. The core engine is not system or genre specific

A game system is built on top of the core engine. The engine knows about properties, modifications, states,
actions, time and objects. It knows nothing about stamina, cyborgs or sidearms.

What this demands:

- No game concepts in the core engine.
- A game system is a separate assembly that declares its objects, states and actions.
- Tools built on the engine, such as the content editor and the character sheet, work for any system from
  its declared meta data.

## How the solution maps to the vision

| Part | Role |
|---|---|
| `Rpg.Experimental` | The core engine: objects, properties, mods, mod sets, states, actions, activities, time, and the meta system that describes a game system. |
| `Rpg.Cyborgs` | One game system built on the engine. The proof that the engine is not system specific. |
| `Rpg.Cms` | An Umbraco server that reads a system's meta data, creates the matching content types, and lets an editor author items, characters and so on. |
| `apps/rpg-cyborgs` | The digital character sheet the players use. |
| `Rpg.ModObjects` | The previous engine, being replaced by `Rpg.Experimental`. |

## Where the current code falls short

Assessed against the new engine after the Cyborgs port. This list should shrink over time, and should be
updated when a gap is closed or a new one is found.

### 1. Flexibility

- Works: override mods on any property, and manual on, off and reset for states and mod sets.
- A completed action step cannot be cleanly redone. Resetting a step does not remove the mods it added to
  the action's result.
- Nothing stops a value being changed for a step that has already run, and nothing reconciles it either.
  A cost paid with one value stays paid when the value changes.
- Manual changes carry no record of being manual, so they cannot be listed or undone as a group.

### 2. Constraints known but not enforced

- Works in spirit: whether an action can be performed is reported, and starting the action does not check it.
- There is no general way to express a constraint. Limits declared on properties, such as minimum, maximum
  and maximum items, are neither enforced nor reported.
- Performability is a bare true or false with no reason attached.
- Some actions can never report as performable, because their check needs values that only exist once the
  action has started.
- A wrong object type passed as an argument throws, which is a refusal.

### 3. Real dice or app dice

- Works: a roll is held as a dice expression, and a supplied result replaces it.
- An unsupplied roll is rolled silently whenever its value is read as a number, and re-rolled on every
  refresh. There is no explicit roll, and no distinction between "not rolled yet" and "rolled".
- An app roll is not stored, so it cannot be shown, kept or replaced.

### 4. Transparency

- Works: every value is the sum of its mods, each mod knows its source property, and base values are tracked.
- Describe has not been ported to the new engine. There is no way to ask for the derivation tree.
- Mods have no meaningful names or origins. Most are anonymous.
- A value calculation function on a mod is opaque. It can be named but not explained.
- Rules code often computes a number and stores the result as a constant, which discards the derivation.
  Reduced damage after an armour check is an example.
- Mods whose source is a nested path do not resolve.

### 5. Fuzzy time

- Works: turns within an encounter can move backwards, and expired mods and child references are kept
  while the encounter lasts, so values and item locations follow.
- Outside an encounter, anything expired is destroyed at once, so that time cannot be replayed.
- Time outside an encounter only moves forwards.
- Combining permanent mods when an encounter ends erases their history.
- There is no notion of time of day or duration beyond a count of "time passes".
- Turn-based effects created outside an encounter lie dormant until the next encounter starts.

### 6. Core engine is not system specific

- Works: Cyborgs is a separate assembly and the engine has no game concepts.
- The engine assumes one system. The first one discovered is used.
- The character sheet graph is in the core engine. It is generic, but it bakes in a single actor.
- The encounter and turn model is built into the engine's time. Systems with a different time structure
  would need it generalised.
