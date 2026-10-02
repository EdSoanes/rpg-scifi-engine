# Time keeping: implementation plan

The plan for bringing the core engine's time keeping in line with feature 5 of [VISION.md](VISION.md),
including time events defined by the game system.

Status: built and tested. See "What was built" at the end.

## Confirmed decisions

These were assumptions in the first draft of the plan. They are now confirmed and the engine follows them.

### A. Ending turn tracking while effects are still running

- An effect with a fixed number of turns left is **dropped** when turn tracking is ended. A stimulant with
  two turns remaining simply ends. The engine's report lists such effects beforehand, so the players can
  see what is about to go.
- A state that is on because of a condition is **not** switched off. Bleeding caused by an injury stays on
  until the injury is dealt with. Ending turn tracking only means turns are no longer counted.
- A state can declare that it needs turn tracking. When it switches on, turn tracking starts. While it is
  on, the report says turns still need counting. The players can end turn tracking anyway, and it does not
  start itself again.
- Anything that began during turn tracking and lasts until a time event carries on afterwards.

### B. Trivial effects

- An effect that lasts a number of turns starts turn tracking when it is applied outside it.
- The game system can mark an effect as too trivial for that. Outside turn tracking a trivial effect is
  dropped: it does not start turn tracking and it does not wait for the next encounter. Inside turn
  tracking it runs normally.

### C. Going back a turn

- Going back uses snapshots. The sheet stores its whole state at the start of each turn.
- Going back to turn N restores the start of turn N and discards the later turns. Replaying a turn means
  doing it again.

### D. Named events also count as time passing

- Triggering "sunrise" also ends anything that lasts "until time passes".

### F. What "time passes" means

- "Time passes" is the time event for when the story has moved on and nothing in particular is happening.
  It gives effects a fuzzy duration: they last for as long as something is going on, then end.
- The turn counter and time events are two separate clocks. A turn tick ends things measured in turns.
  A time event ends things that last until an event. Neither affects the other.
- So starting turn tracking, a turn tick and ending turn tracking are not time events. Something lasting
  "until time passes" carries on through all three.
- This replaced an older rule that destroyed an activity begun outside an encounter, with its mod sets,
  the moment an encounter began. That rule would have destroyed an action whose own outcome starts turn
  tracking, part way through completing it.

### E. No undo within a turn

- Going back is only to the start of a turn. Going back to the current turn undoes the turn so far.

### Recurring effects are a later feature

- Something that happens every turn, such as losing a point each turn while bleeding, is not part of this
  plan. It will be developed as a feature of its own.
- Until then, a bleeding character gets turn tracking but loses nothing each turn unless the player applies
  the loss by hand.
- Advancing several turns steps through each turn in order, which is what recurring effects will need.

## Where the engine was before this plan

- `Temporal` held the current time as a `TimePoint`: a type and a count. The types are an ordered enum:
  before time, time begins, waiting, encounter begins, turn, encounter ends, time passes, time ends.
- Whether something is pending, active or expired was worked out by comparing its start and end points with
  the current time.
- Inside an encounter, expired things were kept. Outside an encounter, expired things were destroyed, and
  combining mods merged.
- An encounter started only when told to. Any "time passes" ended it.
- "Time passes" with a count was the only non-encounter event, and the engine defined it.
- An action had one result mod set, holding both its costs and its effects.
- Flags were not time-aware, so moving back a turn did not undo them.

## Design

### 1. The time model

Current time is:

- **Mode**: not started, non-encounter, or turn tracking.
- **Turn**: the turn number, when turn tracking.
- **Event sequence**: a counter that goes up by one for every time event.
- **Last event**: the name of the most recent time event.

A point in a lifespan is one of:

- **Always**: from the beginning, or to the end.
- **Turn N**: an actual turn number.
- **Relative turn N**: N turns from when the lifespan begins. Anchored to an actual turn on first use.
- **Event**: the next occurrence of a named time event, or of any time event.

### 2. Turn tracking

- **Start**: explicitly at a given turn number, defaulting to one. Also automatically, see section 4.
- **Advance**: to the next turn, or to turn N. Advancing several turns steps through each turn in order,
  so the result always equals advancing one at a time.
- **Renumber**: change the current turn number with no time passing. Every turn point on every object,
  mod set, mod and child reference shifts by the same amount.
- **End**: only when told to. A time event does not end it.
- **Report**: the graph exposes whether anything turn-based is still running and what.
- **Settle**: ending clears the turn history, drops what still had turns to run, destroys what has
  expired, and merges combining mods.

### 3. Time events defined by the game system

- A game system declares its time events by name, for example sunrise and sunset. They are part of the
  system's meta data, so the character sheet and the GM app can list them.
- The engine keeps one built-in event, "time passes", for systems that declare none.
- The GM triggers any event at any time. Events have no fixed order. An event the system did not declare
  is accepted too.
- Every named event also counts as time passing.
- A lifespan can start or end at a named event. Something waiting for its start event is not ended by an
  earlier end event.
- Every object receives every time event. The event name is available to it, so rules code can react.
- An event triggered during turn tracking is delivered without ending turn tracking. Something that ends
  "at sunrise" is expired at the current turn, so going back a turn brings it back.

### 4. Costs and effects

- An activity action has two mod sets: one filled by the cost step and one by the outcome step.
- A mod is turn-based if its lifespan ends on a turn.
- **Starting turn tracking**: when a turn-based effect is applied while the sheet is outside turn
  tracking, the graph starts turn tracking at turn one.
- **Skipping costs**: when an action completes, the effects are applied first. If the sheet is still
  outside turn tracking, turn-based costs are not applied. They are taken out of the cost set and kept on
  the action as skipped costs, so they can be shown and applied by hand.
- Costs that are permanent or last until an event are always applied.
- Turn-based mods never lie dormant waiting for the next encounter.

### 5. Going back a turn

- At the start of each turn, the sheet stores a compressed snapshot of itself.
- Going back to turn N restores that snapshot and discards the later ones.
- An action that starts turn tracking is part of turn one. The snapshot of turn one is retaken when the
  action completes.
- Snapshots are saved with the sheet, so going back still works after a save and reload.
- Ending turn tracking discards the snapshots.
- Renumbering shifts the snapshot turn numbers. A snapshot restored after a renumber is shifted to match.

Consequence: going back replaces the objects in memory. Anything holding a reference to an object must
look it up again.

## Phases

| Phase | What | Status |
|---|---|---|
| 1 | Tests that pin down existing behaviour | Not done as a separate phase. See below |
| 2 | The time model | Done, as an extension of the existing types. See below |
| 3 | Turn tracking control | Done |
| 4 | System time events | Done |
| 5 | Costs and effects | Done |
| 6 | Going back | Done |
| 7 | Adoption in Cyborgs | Done, to the extent a testbed needs. See below |

## What was built

### Differences from the plan

- **The time model was extended, not replaced.** The plan called for new types. Instead `TimePoint` gained
  an event kind and `Temporal` gained the turn tracking mode, event sequence and last event. The existing
  ordered enum and its comparison rules remain. This was far less risky, and every existing test kept
  passing. The cost is that the rules for pending, active and expired are still spread across special
  cases. The relative-lifespan marker stays on the lifecycle object, not in the point.
- **Phase 1 was not done separately.** The existing time tests served as the safety net. One of them
  changed, deliberately: something that lasts "until time passes" used to be destroyed the moment an
  encounter began. It now carries on through turn tracking and ends on a time event.
- **Resetting an action step now removes what the step added.** This was not in the plan. The separate cost
  and outcome sets made it straightforward, and it closes a gap listed under flexibility in the vision.
- **A wrong object type passed as an argument no longer throws.** The argument is left without a value.
  This came up while testing two actions in one activity.

### Engine API

On the graph: begin turn tracking, next turn, advance to turn, renumber turn, end turn tracking, trigger
time event, and get the turn tracking report. On the character sheet: the turns that can be gone back to,
and rewind to turn. On a mod and a mod set: lasts until a named event. On a mod: too trivial to start turn
tracking. On a state: needs turn tracking. On an activity action: the cost set, the skipped costs, and
apply skipped costs.

### Tests

- `Time_Tests` covers turn tracking, renumbering, time events, and costs and effects.
- `Rewind_Tests` covers going back: completed actions, hand-edited values, states switched on by hand,
  permanent changes, moved items, time events, save and restore, and renumbering.
- `TurnTrackingTests` in the Cyborgs tests covers the Cyborgs actions inside and outside turn tracking.

### Phase 7: what remains in Cyborgs

Done:

- Action costs moved to the cost step's mod set.
- The "moving" state from running is marked as too trivial to start turn tracking.
- Tests for dropping an item, running, aiming, taking damage and going back a turn.

- Cyborgs declares two time events, sunrise and sunset, so the feature can be tested. `TimeEventTests`
  covers effects that last until sunrise, effects active only between sunset and sunrise, sunrise during
  a fight, and save and restore.

Cyborgs is a testbed, not a complete game, so the rest is left as it is:

- None of the Cyborgs rules use the time events themselves. Only the tests do.
- Focus spent on an attack is a one-turn cost, so it returns next turn and is free outside turn tracking.

### Known limits

- Moving time backwards directly through the time object still exists and does not use snapshots. Use
  rewind on the character sheet.
- Beginning an encounter while one is running ends it and starts a new one. This is existing behaviour.
- A snapshot is the size of the whole sheet, taken every turn. For a character that is several kilobytes
  compressed per turn. They are discarded when turn tracking ends.
- An activity begun outside turn tracking, whose outcome then starts turn tracking, becomes the activity of
  turn one.

## Out of scope

- Effects that recur every turn. See the confirmed decisions.
- Time of day, calendars and measured durations outside an encounter.
- Anything about where the engine runs, or the character sheet and GM app user interfaces.
- Describe. Skipped costs are recorded so that describe can show them later.
