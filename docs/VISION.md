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
- Whenever a roll is needed, the player chooses: roll real dice and type in the result, or let the app
  roll.
- Nothing is rolled until the player asks for it. The app never rolls on the player's behalf unprompted.
- A typed-in result and an app-rolled result are stored and treated identically.
- A roll can be redone or replaced.

#### Reading a value never rolls

This is the one engine rule the rest follows from.

- Reading, showing or recalculating a value never rolls dice. A value that has not been rolled stays a
  dice expression.
- A roll that is needed becomes a **pending roll** on the sheet. It holds its expression and what it is
  for.
- A pending roll is settled by the app rolling it or by a typed-in result. Either way the result is stored
  once and stays until it is redone or replaced.
- A stored roll keeps the expression it was rolled from and whether the app or the player supplied it.
- Stored and pending rolls are part of the sheet. They are saved with it, and going back a turn restores
  them.

#### Scenarios this has to handle

- **A roll discovered part way through a step.** Rules code cannot pause to ask. So every roll is a
  declared input of a step, known before the step runs. Rules code never rolls dice itself. A roll that
  depends on an earlier result, such as damage only on a hit or a random hit location, is its own step or
  a follow-on action.
- **A roll caused by time.** A turn tick or a time event can demand a roll, with no action button pressed.
  Time advances anyway, because the sheet never refuses. The roll is queued as pending. Advancing several
  turns can queue several. Later turns may be worked out on values that are not settled yet, and the sheet
  shows which rolls are outstanding.
- **A calculated stat that contains dice.** A stat calculated from other stats recalculates on its own and
  cannot ask. So a calculated stat either stays a dice expression or contains no dice. Only actions and
  events turn an expression into a number.
- **The expression changes after the roll.** A player rolls 2d6+4, then a bonus makes it 2d6+6. The result
  of the dice is stored separately from the bonuses, so the dice stay as rolled and the total follows the
  bonus.
- **Asking every time is a burden.** A GM running ten opponents does not want a prompt for every roll.
  Each sheet has a default, such as "the app rolls unless I say otherwise". Auto completing an action lets
  the app roll whatever is still pending. Every roll is still visible and replaceable afterwards.
- **Dice that are more than a total.** Exploding dice, keeping the highest, counting successes and doubles
  need each die, not a sum. The player then enters each die. This waits until a game system needs it.
  Until then a roll is a sum, and the player types the result of the dice, not the total with bonuses.

Not tricky: a roll made by someone else is typed in, which is already how sheets work without networking.

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
- Something the rules skipped is shown as skipped, with the reason. A cost that was not charged is an
  example.

### 5. Fuzzy time

Time keeping is critical in tabletop rpgs, but it can be fast-forwarded or reversed at will. The GM might
replay a turn, or declare "now it is nightfall". The system must handle this and keep every derived value
correct.

Time has two modes, and the distinction between them is a feature.

#### Encounter time is turn tracking

An encounter is a declaration to the players that turns need to be tracked right now. It may be a fight
written into the adventure. It may equally be one character who has taken an injury and is bleeding out.

- Turn tracking is on when the GM declares it, or when a turn-based effect begins on a sheet.
- It is precise. Durations are counted exactly in turns: starts in N turns, lasts N turns.
- It can be rewound. Going back to an earlier turn restores the state that held at that turn: values,
  states, where items were. Nothing needed for a replay is destroyed while turn tracking lasts.
- The engine never ends it. The engine reports when nothing turn-based remains, and the players end it
  when they choose. Ending it while effects are still running is allowed.
- Ending it settles what happened. Lasting changes carry on, and the turn-by-turn history is cleared.
- An effect with turns left to run is dropped when turn tracking is ended. A state that is on because of a
  condition, such as bleeding from an injury, stays on.
- A state can declare that it needs turns counted. It starts turn tracking when it switches on.
- Going back is to the start of a turn. The later turns are discarded, so replaying a turn means doing it
  again.

#### Costs and effects

Two kinds of turn-based thing behave differently outside turn tracking.

- **An effect** is an outcome that lasts a number of turns: bleeding for ten turns, a stimulant for three,
  an aim bonus for one. An effect needs turns to be counted, so it starts turn tracking.
- **A cost** is spending from a per-turn budget, such as this turn's action point. Outside turn tracking
  there is no budget to spend from. A cost never starts turn tracking.

The rule for costs:

> A turn-based cost is skipped only if the sheet is still outside turn tracking once the action has
> completed.

Dropping a sword outside a fight leaves the sheet outside turn tracking, so its action point cost is
skipped. Aiming outside a fight gives a one-turn bonus, which starts turn tracking, so the sheet is now on
turn one and the action point is spent.

Costs that are not measured in turns are always applied. That covers permanent costs such as ammunition,
and costs that last until a time event such as a power usable once per day.

A consequence for game system authors: a turn-based cost is no limit outside turn tracking. An action with
a lasting benefit needs a permanent or until-event cost if it is to be limited there.

A game system can mark an effect as too trivial to start turn tracking, such as "moving" for one turn after
running. Outside turn tracking a trivial effect is simply dropped.

Effects that recur every turn, such as losing a point each turn while bleeding, are a later feature.

#### Each sheet counts its own turns

Sheets are free-standing (feature 7), so nothing synchronises them. The table does it verbally, as in any
tabletop game. The GM calls each turn and says when the encounter is over.

- A sheet can be in turn tracking alone. A character bleeding out counts turns while the others do not.
- When something starts that involves everyone, the other players switch to turn tracking and set their
  turn number to match the sheet that is already counting.
- A sheet can **advance** to a turn. Time passes, and effects expire as they should. This is also how a
  player who fell behind catches up.
- A sheet can **renumber** its current turn. No time passes. Every running effect shifts by the same
  amount. This is for two sheets that were each counting alone and need to agree on a number.
- Advancing several turns at once gives the same result as advancing one turn at a time.
- The current turn number is always prominent, so drift is easy to spot.

#### Non-encounter time is loose

- It moves forwards through events, not a clock.
- The built-in event is "time passes". It means the story has moved on and nothing in particular is
  happening. Its purpose is to give effects a fuzzy duration: they last for as long as something is going
  on, then end.
- Counting turns is not time passing in this sense. Neither starting turn tracking, nor a turn tick, nor
  ending turn tracking ends something that lasts "until time passes". Such an effect sits through a whole
  fight and ends when the GM says time has moved on.
- Rolling it back is manual. The players change values and switch states by hand, using feature 1. The
  engine does not rewind it.
- A game system defines its own named time events, such as sunrise and sunset in a fantasy game. The GM
  triggers any of them at will.
- A lifespan can begin or end at a named time event, for example "until sunrise".
- Rules can react to a named time event, for example to restore points at dawn.
- A time event can occur during turn tracking without ending it.

In both modes, derived values are always consistent with the current time, without the caller doing
anything extra.

### 6. The core engine is not system or genre specific

A game system is built on top of the core engine. The engine knows about properties, modifications, states,
actions, time and objects. It knows nothing about stamina, cyborgs or sidearms.

What this demands:

- No game concepts in the core engine.
- A game system is a separate assembly that declares its objects, states and actions.
- Tools built on the engine, such as the content editor and the character sheet, work for any system from
  its declared meta data.

### 7. No server needed during play

The system works without networking. Each character sheet is free-standing, like a paper one.

What this demands:

- The rules engine runs on the player's device. Play needs no server and no connection between sheets.
- Rules and content are fetched before the session. Nothing is fetched during it.
- A character can be exported to a file and imported from one, to back it up or move it to another device.
- Everything that passes between people at the table can pass verbally or by a device-to-device handover
  that needs no server.

#### What a character sheet models

A sheet models the character and their gear, and nothing else. Rooms, vehicles, loot and other shared
things are not on a player's sheet.

#### The GM app

The GM needs more than a character sheet.

- It holds a sheet for each NPC, and can advance all of them together.
- It holds locations. A location is a container that items can be placed in, for example when a player
  drops something.
- It is **not** a campaign or adventure tool. There is no requirement that the adventure exists in a
  digital format the app understands.

#### Handing things over

An item with all its components and modifications is a small, self-contained piece of data. It can move
between sheets without being retyped.

- **QR code.** One sheet shows it and the other scans it. Works between phones and laptops, since laptops
  have cameras. A heavily modified weapon fits in a single code.
- **Pasted text.** The sheet shows the item as a block of text to copy. The players send it by whatever
  they already use.
- **File.** For large things, including a whole character.
- **Short typed code.** Possible only for items built entirely from the shared content library, which can
  be described by reference. Lower priority than the others.

The same handover can carry the result of an action, such as an attack with its damage, type and location,
so that the defender does not have to type several values.

Both sheets need the same version of the rules and content for a handover to work.

Measured with the current save format, compressed: a plain weapon is about 430 bytes, a weapon with twenty
modifications about 750 bytes, and a whole character about 7 KB.

## Open questions

Decisions not yet made. Do not assume an answer.

- **Where the engine runs.** The engine is C#. Running it on the device most likely means compiling it to
  run in the browser. Its reliance on reflection and assembly scanning needs proving there. Rewriting it in
  another language is the fallback.
- **Rules and content versions.** How a saved character survives a rules change, and how a table makes
  sure everyone has the same version.

## How the solution maps to the vision

| Part | Role |
|---|---|
| `Rpg.Experimental` | The core engine: objects, properties, mods, mod sets, states, actions, activities, time, and the meta system that describes a game system. |
| `Rpg.Cyborgs` | One game system built on the engine. A testbed: a very simple but realistic example that proves the engine works. It is not meant to be a complete or realistic game. |
| `Rpg.Cms` | An Umbraco server that reads a system's meta data, creates the matching content types, and lets an editor author items, characters and so on. Under feature 7 it is used before a session, not during one. |
| `apps/rpg-cyborgs` | The digital character sheet the players use. |
| `Rpg.ModObjects` | The previous engine, being replaced by `Rpg.Experimental`. |
| GM app | Not started. |

## Where the current code falls short

Assessed against the new engine after the Cyborgs port. This list should shrink over time, and should be
updated when a gap is closed or a new one is found.

### 1. Flexibility

- Works: override mods on any property, and manual on, off and reset for states and mod sets.
- Works: an action step can be reset and redone, and a completed action can be undone. What the step added
  is removed.
- Works: going back to the start of a turn undoes everything since, including changes made by hand.
- Nothing stops a value being changed for a step that has already run, and nothing reconciles it either.
  A cost paid with one value stays paid when the value changes.
- Manual changes carry no record of being manual, so they cannot be listed or undone as a group.
- Transferring an item to a property that does not exist fails. That is a refusal.

### 2. Constraints known but not enforced

- Works in spirit: whether an action can be performed is reported, and starting the action does not check it.
- There is no general way to express a constraint. Limits declared on properties, such as minimum, maximum
  and maximum items, are neither enforced nor reported.
- Performability is a bare true or false with no reason attached.
- Some actions can never report as performable, because their check needs values that only exist once the
  action has started.
- A wrong object type passed as an argument is ignored without saying why.

### 3. Real dice or app dice

See [DICE_PLAN.md](DICE_PLAN.md) for the design and what was built.

- Works: reading a value never rolls. Nothing in the engine or in Cyborgs rolls on its own, and a turn
  tick does not change a roll.
- Works: a roll that is needed is pending. The sheet lists pending rolls, with the action and step that
  need them. A step with a pending roll does not run.
- Works: the app rolls on request, or the player supplies the result. Either way the result is stored
  once, with who supplied it, and can be redone, replaced or cleared.
- Works: the dice are stored separately from the bonuses. A bonus that changes later moves the total and
  keeps the dice. Changed dice make the roll pending again.
- Works: each sheet has a default for who rolls. Auto completing an action lets the app roll, and the
  rolls can be seen afterwards.
- Works: stored and pending rolls are saved with the sheet and restored when going back a turn.
- Works: rules code can request a roll in reaction to a turn or a time event, and time is not held up.
  Nothing happens yet when such a roll is settled. That belongs with effects that recur each turn.
- A dice expression is only ever a sum. Individual dice cannot be entered, and rules that need each die
  cannot be written.
- Redoing a roll after a step has used it does not redo the step. This is the same gap as under
  flexibility.
- A pending roll names its action and step but gives no reason in words.
- A roll requested by rules code stays on the object after it is settled. Nothing tidies it away.

### 4. Transparency

- Works: every value is the sum of its mods, each mod knows its source property, and base values are tracked.
- Describe has not been ported to the new engine. There is no way to ask for the derivation tree.
- Mods have no meaningful names or origins. Most are anonymous.
- A value calculation function on a mod is opaque. It can be named but not explained.
- Rules code often computes a number and stores the result as a constant, which discards the derivation.
  Reduced damage after an armour check is an example.
- Mods whose source is a nested path do not resolve.
- Works: a cost skipped outside turn tracking is kept on the action. Nothing records the reason, and no
  other kind of skipped rule is recorded.

### 5. Fuzzy time

See [TIME_PLAN.md](TIME_PLAN.md) for the design and what was built.

- Works: turn tracking starts when declared, at any turn number, or when a turn-based effect is applied or
  a state that needs it switches on. The engine never ends it, and reports what still needs turns counted.
- Works: advancing several turns passes through each one. Renumbering changes the turn number with no
  time passing.
- Works: costs and effects are separate. Turn-based costs are skipped outside turn tracking and kept on
  the action. Trivial effects can be marked so they do not start turn tracking.
- Works: going back to the start of any turn since turn tracking began restores everything, and survives
  a save and reload.
- Works: a game system declares named time events. A lifespan can start or end at one, rules can react to
  one, and one can occur during turn tracking without ending it.
- Outside an encounter time only moves forwards and expired things are destroyed. That is intended.
- There are no effects that recur each turn. A bleeding character gets turn tracking but loses nothing
  each turn. This is a planned later feature.
- Cyborgs declares sunrise and sunset so the feature can be tested, but none of its own rules use them.
- Time can still be moved backwards directly on the time object, which bypasses the snapshots.
- The rules for pending, active and expired are still a set of special cases over an ordered list of time
  types. They work, but they are hard to reason about.
- Beginning an encounter while one is running ends it and starts a new one.

### 6. Core engine is not system specific

- Works: Cyborgs is a separate assembly and the engine has no game concepts.
- The engine assumes one system. The first one discovered is used.
- Works: the encounter and turn model is built into the engine's time, and the non-encounter time events
  belong to the game system.
- There is no generic location or container object for the GM app to use.

### 7. No server needed during play

- Works: a whole sheet can be saved to text and restored, and remains fully functional.
- The engine runs on the server. The old engine's character sheet calls it for every operation. The
  equivalent for the new engine has not been built, so it can be designed for the device from the start.
- Whether the engine runs in a browser is unproven. It relies on reflection and on scanning assemblies.
- The save format records .NET type names, so a rules change may stop an old save loading.
- A single item cannot be exported or imported. Moving an item only works within one graph.
- A graph has a separate context object and actor. Tests use a room as the context. Under this feature the
  context of a player's sheet is the character.
- There is no GM app, and no way to hold several sheets and advance them together.
