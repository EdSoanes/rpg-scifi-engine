# Dice rolls: implementation plan

The plan for bringing the core engine's dice handling in line with feature 3 of [VISION.md](VISION.md).

Status: plan only. Nothing here is built yet.

## The rule being implemented

Reading a value never rolls. A roll that is needed becomes a pending roll on the sheet. The player settles
it with real dice or lets the app roll. The result is stored once and stays until it is redone or replaced.

## Where the engine is today

- A property value is a dice expression built by adding up its mods. `2d6` from one mod and `1` from another
  give `2d6 + 1`.
- The whole-number and dice inputs of an action step are properties of the activity action, so their values
  are built from mods like any other property.
- A step input declared as a whole number is filled by converting the property's expression to a number.
  That conversion rolls. It happens every time the inputs are refreshed, which includes every time event
  and every turn tick. Nothing is stored. This is the re-rolling.
- An input that holds an unrolled expression counts as filled in, so a step runs without anyone rolling.
- A result supplied with a step call is added as an override mod. It replaces the whole expression,
  bonuses included. Nothing records that it was a roll or who made it.
- `Dice.Roll()` is public and is also the only way to read the number out of a constant expression. So
  rules code and tests call it everywhere, mostly on constants. In Cyborgs: the focus points in a parry, a
  skill rating, an injury's severity, and the stamina calculation.
- The random number generator is a private static. Tests cannot control it.

## Assumptions

These are my choices where the vision leaves room. Each one is easy to change before the build starts.

### A. What makes a roll "needed"

- A step input declared as a whole number, whose property holds an expression with dice in it, needs a roll.
- A step input declared as dice wants the expression itself and never needs a roll.
- A stat that holds dice, such as a weapon's damage of `2d6`, is not a pending roll. It becomes one when it
  flows into a whole-number input.
- Rules code can also ask for a roll directly, for rolls caused by time. See section 5 of the design.

### B. A supplied number is the result of the dice, not the total

- Today `("diceRoll", 14)` against `2d6 + 1` makes the value 14. Under this plan it makes the value 15.
- Forcing a total stays possible with an override, which is the general way to overrule any value.
- Existing tests that supply a roll will change their expected numbers.

### C. A typed result outside the possible range is accepted

- Typing 14 for `2d6` is stored and used. The stored roll is flagged as out of range so the sheet can show
  it. The engine does not refuse.

### D. Reading an unrolled value as a number is a programming error

- A step never runs while one of its rolls is pending, so a player cannot cause this.
- If rules code asks for a number from an expression that still has dice, the engine throws an error that
  names the property. Returning zero or a silent roll would hide the mistake.
- This is the one place the engine fails on purpose. It fails for the rules author, not for the player.

### E. One stored roll per property, held as one total

- An expression such as `2d6 + 1d4 + 3` has a dice part, `2d6 + 1d4`, and a bonus, `3`. One number is stored
  for the whole dice part.
- When the app rolls, it also stores each die, for display. The player types only the total.
- Entering each die, and rules that need each die, wait until a game system needs them.

### F. When the dice part changes, the stored roll no longer applies

- The stored roll remembers the dice part it was rolled for. If a bonus changes, the roll stays and the
  total follows.
- If the dice part itself changes, for example `2d6` becomes `3d6`, the roll is pending again. The old
  result is kept, marked as out of date, so the sheet can show what happened.

### G. Who rolls by default

- Each sheet has a setting: ask, or the app rolls. It is saved with the sheet. The engine's default is ask.
- With "ask", a step with a pending roll does not run. It reports the pending rolls.
- With "the app rolls", running a step rolls and stores its pending rolls first. The player pressed the
  button, so the roll is prompted.
- Auto completing an action always lets the app roll whatever is pending, whatever the setting.

### H. Redoing a roll does not redo a step that already used it

- This is the existing gap under flexibility: a value changed after a step has run is not reconciled. The
  player resets the step and runs it again. This plan does not change that.

## Design

### 1. The dice type

- `Dice` gains: whether it is a constant, its number when it is, its dice part, and its bonus part.
- `Dice` gains multiplication by a whole number, so a calculation can double an expression without rolling.
- Rolling moves out of `Dice` into a roller owned by the graph. The roller can be replaced, so tests can
  fix the results. The roller is not saved.
- `Dice.Roll()` stops being the way to read a constant. Rules code and tests use the number instead.

### 2. Stored rolls

A stored roll is a small record held on the property it belongs to:

| Field | Meaning |
|---|---|
| Dice part | The expression that was rolled, for example `2d6` |
| Result | The total of the dice |
| Each die | The individual dice, when the app rolled |
| Supplied by | The app or the player |
| Out of range | The result is not possible on those dice |

A property then has three readings:

- **Expression**: what its mods add up to. Never affected by a roll.
- **Roll**: the stored roll, if there is one.
- **Value**: the expression with its dice part replaced by the roll's result. If there is no roll that
  applies, the value is the expression.

Because the roll is part of the property, it is saved with the sheet, restored with it, and rewound with it.
No extra work is needed for going back a turn.

### 3. Operations on the sheet

- **Pending rolls**: list every roll that is needed and not settled. Each entry gives the object, the
  property, the expression, the dice part, and the action and step that need it.
- **Roll**: the app rolls a property's dice part and stores the result.
- **Set roll**: store a result the player typed.
- **Clear roll**: remove the stored roll, making it pending again.
- Rolling or setting again replaces the stored roll.

### 4. Actions

- A whole-number input is filled only when its property's value is a constant. Otherwise the input is empty
  and the step's inputs are incomplete, which the engine already handles by not running the step.
- An activity action reports the pending rolls for each step.
- A number passed with a step call for a property with a dice part is stored as that property's roll,
  supplied by the player.
- Refreshing inputs on a time event no longer rolls, because a settled value is a constant and an unsettled
  one is left empty.
- Resetting the whole action clears its rolls. Resetting one step keeps them, since the dice were rolled
  whatever the bonuses turn out to be.
- An override with a constant value means there is nothing to roll. Spending luck in the armour check
  already works this way.

### 5. Rolls caused by time

- Rules code reacting to a turn tick or a time event can request a roll on an object: a name and an
  expression. It appears in the pending rolls like any other.
- Time is never held up. Advancing five turns can leave five pending rolls.
- Nothing in the engine uses this yet. Effects that recur each turn are a later feature and will build on
  it, including what happens when such a roll is settled.

## Phases

| Phase | What | Depends on |
|---|---|---|
| 1 | The dice type: constants, dice part, bonus part, multiplication, replaceable roller | |
| 2 | Stored rolls on properties, the three readings, save, restore and rewind | 1 |
| 3 | Sheet operations: pending rolls, roll, set roll, clear roll | 2 |
| 4 | Actions: whole-number inputs wait for a roll, per-step pending rolls, supplied number is the dice result, the sheet's default, auto complete | 3 |
| 5 | Remove every silent roll from the engine and make an unrolled read an error | 4 |
| 6 | Requesting a roll from rules code, for time-caused rolls | 3 |
| 7 | Cyborgs: remove rolling from rules code, update tests, add roll tests | 5 |
| 8 | Update the vision document's gap list | 7 |

Each phase ends with both test suites passing.

### Tests to add

- A dice value read many times, and across several turn ticks, keeps the same expression and never rolls.
- An app roll is stored once and survives turn ticks, a save and reload, and going back a turn.
- A typed result and an app roll give the same behaviour afterwards.
- A bonus added after the roll changes the total and keeps the dice.
- A changed dice part makes the roll pending again and keeps the old result marked out of date.
- A step with a pending roll does not run under "ask", and reports the roll.
- The same step under "the app rolls" rolls, stores and runs.
- Auto complete rolls what is pending and every roll can be seen afterwards.
- An out of range typed result is accepted and flagged.
- A roll requested on a turn tick is pending, and advancing several turns queues several.
- Reading an unrolled value as a number raises the error.
- Cyborgs: an attack, a parry and an armour check with real and app dice.

## What changes for existing code

- Tests that call `Roll()` to read a number switch to reading the number.
- Tests that supply a roll and assert the total change by the size of the bonus. See assumption B.
- Cyborgs rules code that calls `Roll()` reads a number or uses dice arithmetic instead.

## Out of scope

- Entering each die, exploding dice, keeping the highest, counting successes and doubles.
- Effects that recur each turn.
- Reconciling a step that already ran when its roll is redone.
- The user interface for choosing who rolls.
