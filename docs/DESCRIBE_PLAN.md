# Describe: implementation plan

The plan for porting describe to the new engine, in line with feature 4 of [VISION.md](VISION.md).

Status: built and tested. See "What was built" at the end.

## What describe is for

At any point a player can ask why a value is what it is, and get the tree of modifications that produce
it, down to the original sources. The same goes for an action in progress, a state and a mod set.

## What the old engine had

The old engine, `Rpg.ModObjects`, had three operations:

- **Describe a property.** The value, its active mods, and for each mod the property it came from,
  described the same way. This gave a tree.
- **Describe a mod set.** The value the set gives each property it targets. If the set was not applied,
  the old code applied it, read the values, and unapplied it again.
- **Describe a state.** The same as a mod set, using the state's set.

Its limits:

- A mod showed its type and its value, but not where it came from in words.
- Only active mods were shown. A mod that was overridden, expired or switched off was invisible.
- A calculation on a mod showed only the function's name.
- Describing a mod set changed the graph and triggered time events, then changed it back.
- The tree was never returned in full: the code that collected the mods of a property discarded them.

The two Cyborgs describe tests are commented out. They were written against the old stat objects, which
the port replaced with plain whole numbers.

## Where the new engine was before this plan

- A value is the sum of its mods. Each mod has a type, a target, a source and a lifespan.
- A mod's source is either a fixed value or another property, with an optional calculation function.
- A mod has an owner id. The owner can be the object itself, a mod set, a state or an activity action.
- A mod has a name, but almost nothing sets it.
- A property now also has an expression, a stored roll and a pending flag. See [DICE_PLAN.md](DICE_PLAN.md).
- An activity action has a cost set, a result set, skipped costs, pending rolls and follow-on actions.
- Nothing marks a change as made by hand.
- An effect too trivial to start turn tracking is dropped outside it, and nothing records that.
- A mod whose source is a nested path, such as a property of a child object, is listed in the vision as
  not resolving. This needs checking before describe can follow such a source.

## Confirmed decisions

These were assumptions in the first draft of the plan. They are now confirmed and the engine follows them.

### A. Describe only reads

- Describe never changes the sheet, never triggers a time event and never rolls.
- A mod set or state that is not applied is described from its mods as they stand. Nothing is applied and
  unapplied to find out.

### B. The result is plain data

- Describe returns simple objects that can be turned into text and sent to a user interface.
- Each one can also print itself as indented text. Tests and debugging use that.
- The words are English and built by the engine. Translation and nicer wording belong to the user
  interface, and are out of scope.

### C. Everything on the property is shown, not only what counts

- A mod that does not count is still listed, with the reason: expired, not started yet, switched off by
  hand, its set is not applied, replaced by a newer version, or set aside by an override.
- Inside turn tracking the engine keeps expired mods, so "this was +2 from the stimulant until turn 3" can
  be shown. Outside it they are destroyed, so they cannot.

### D. Where a mod came from is worked out from its owner

- A mod's origin is one of: starting value, derived from another stat, a state, a mod set, the cost of an
  action, the effect of an action, an input of an action, a roll requested by the rules, a change made by
  hand, or the rules with no further detail.
- The origin carries the names needed to show it: the state, the item, the action and its step.
- No new information is stored for this, except for changes made by hand. See E.

### E. Changes made by hand are marked

- A mod can be marked as made by hand. The sheet gets one operation for overriding a value by hand, which
  sets the mark.
- Describe shows such a mod as a manual change.
- This also lets manual changes be listed, which closes a gap under flexibility. Undoing them as a group
  is out of scope.
- Rules code that adds a mod directly, without a set, shows as "the rules" unless the mod is given a name.

### F. A calculation shows its input, its output and a description

- For a mod with a calculation function, describe shows what went in and what came out.
- The rules author can put a short description on the function, such as "half of the score above ten,
  rounded down". Without one, the function's name is shown.
- Describe does not try to explain the code of a function.

### G. Numbers worked out in rules code need a name

- The engine cannot recover how rules code arrived at a number it stores as a fixed value.
- So a mod can be given a name saying what it is, and describe shows it. This already exists and is
  simply not used.
- Cyborgs gets names on the mods where the reason matters, such as damage halved by armour. It is a
  testbed, so not every mod.

### H. What was skipped is recorded with a reason

- A skipped cost gets a reason: turns were not being tracked.
- A trivial effect dropped outside turn tracking is recorded on the action in the same way, instead of
  vanishing.
- Describe shows both under the action.

### I. The tree stops at a sensible depth

- Describe follows sources until it reaches a fixed value. It stops if it meets a property it is already
  describing, and marks the loop.
- A caller can ask for one level only, for a user interface that opens the tree a step at a time.

## Design

### 1. Describing a property

For one property of one object:

| Part | Meaning |
|---|---|
| Object | Its id, name and type |
| Property | Its name and its display name from the game system's meta data |
| Value | What the property is now |
| Expression | What the mods add up to, before any roll |
| Roll | The stored roll, who supplied it, and whether it is out of range or out of date |
| Pending | A number is wanted and the dice are not rolled |
| Base value | The value without temporary changes |
| Mods | Every mod on the property. See below |

For each mod:

| Part | Meaning |
|---|---|
| Name | The mod's name, if it has one |
| Kind | Starting value, base, override, standard, combine, replace, temporary |
| Contribution | What the mod adds |
| Counts | Whether it is part of the value now |
| Why not | The reason, when it does not count |
| Lifespan | When it starts and ends, in turns or time events |
| Origin | Where it came from. See assumption D |
| Fixed value | The value, when the source is a fixed value |
| Source property | The description of the property it comes from, when there is one |
| Calculation | The function's description, its input and its output |

### 2. Describing an action in progress

For one activity action:

- The action, its owner, and whether it can be performed.
- Each step: whether it is done, and its inputs. Each input shows its value, and its property description
  when it is a number or dice.
- The rolls still pending and the rolls already made.
- The costs: each mod in the cost set, with its target and what it changes.
- The skipped costs, with the reason.
- The effects: each mod in the result set, with its target, what it changes and how long it lasts.
- The effects that were dropped, with the reason.
- The actions nominated to follow.

### 3. Describing a state or a mod set

- Its name, its owner, and whether it is applied.
- For a state: whether it is on, and why. The reasons are: its condition is met, it was switched on or
  off by hand, or an action switched it on until a given turn or event.
- Each property it changes, and by how much. This is worked out from its mods without applying anything.
- Whether the state needs turn tracking.

### 4. Describing an object

- A summary for a whole object: each property with its value, whether it differs from its base value, and
  whether a roll is pending. The states that are on. No trees.
- This is what a sheet shows before the player asks about one value.

### 5. Where it lives

- A describer in the engine, used through the sheet: describe a property, an action, a state, a mod set
  and an object.
- It uses only what the sheet already exposes, plus the origin lookup.
- The device-side library, when it is built, passes these results to the user interface unchanged.

## Phases

| Phase | What | Status |
|---|---|---|
| 1 | Check how nested source paths resolve, and fix what describe needs | Done. See below |
| 2 | Describe a property: value, expression, roll, and the mods that count, as a tree | Done |
| 3 | Mods that do not count, with the reason, and lifespans in words | Done |
| 4 | Origins: work out where each mod came from | Done |
| 5 | Changes made by hand: the mark, the sheet operation, and listing them | Done |
| 6 | Calculations: input, output and a description on the function | Done |
| 7 | Describe a state, a mod set and an object | Done |
| 8 | Record reasons for skipped costs and dropped effects | Done |
| 9 | Describe an action in progress | Done |
| 10 | Text output for every description | Done |
| 11 | Cyborgs: names on the mods that matter, restore and extend the describe tests | Done |
| 12 | Update the vision document's gap list | Done |

### Tests to add

- A stat derived from another stat describes as a tree down to the starting values.
- A value with a calculation shows the input, the output and the description.
- An override shows the mods it sets aside, with the reason.
- An expired effect inside turn tracking is shown as expired, with the turn it ended.
- An effect from a state names the state. An effect from an action names the action and the step.
- A change made by hand shows as manual, and all manual changes can be listed.
- A property with a stored roll shows the expression, the dice result, the bonus and who rolled.
- A property with a pending roll shows that it is pending.
- A state that is on shows why. A state forced off by hand shows that.
- A mod set that is not applied describes what it would change, and the sheet is unchanged afterwards.
- An action in progress shows inputs, pending rolls, costs, skipped costs with the reason, and effects.
- A loop between two properties is marked and does not hang.
- Describing never rolls and never changes the sheet. The saved sheet is identical before and after.
- A description survives being turned into text and back.
- Cyborgs: defence, focus points, an attack roll in progress, and damage reduced by a parry and armour.

## What was built

### Differences from the plan

- **Nested source paths were broken and are fixed.** A mod whose source was a path through a child
  object, such as "Child.Strength", found nothing and added nothing. The source is now resolved to the
  object that has the property when the mod is added to the sheet, the same way targets already were.
- **Two operations for changes by hand, not one.** Override sets a value. Adjust adds to it. Both mark the
  change as made by hand. A single change can also be undone.
- **The owner of a mod is found even when it was not recorded.** Mods added to an action's effect set
  carry no owner. Describe finds the set by looking through the sets, so these still name their action.
- **Plumbing inputs are left out of an action's description.** Inputs that hand rules code its own
  activity or action say nothing to a player.
- **The armour check in Cyborgs now builds its damage in steps.** It used to store one worked-out number.
  It now stores the damage before armour and the reduction as two named mods. This also fixed a fault:
  when armour stopped all the damage, the next action picked up the damage from before the armour check.

### Engine API

- On the sheet: describe a property, an action, a state, a mod set and an object. Override by hand, adjust
  by hand, list the changes made by hand, and undo one.
- On a mod: the mark for a change made by hand.
- On an activity action: the effects that were dropped.
- For rules authors: a describe attribute for calculation functions, and names on mods.
- Each description prints itself as indented text, and can be turned into JSON and back.

### Example output

```
Benny.CurrentStaminaPoints = 14
  +14 Base: From Benny.StaminaPoints
      Benny.StaminaPoints = 14
        +12 Initial: Starting value
        +2 Base: From Benny.Health
            calculation: Twice health (1 gives 2)
            Benny.Health = 1
              +1 Initial: Starting value
```

```
MeleeAttack.diceRoll = 10 (2d6 + 1: rolled 9 on 2d6 by the player)
  +2d6 Initial: Starting value
  +0 Ability and focus: Set during MeleeAttack
  +1 Weapon hit bonus: From Excalibur.HitBonus
      Excalibur.HitBonus = 1
        +1 Initial: Starting value
```

### Tests

- `Describe_Tests` covers properties, mods that do not count, changes by hand, rolls, states and mod
  sets, actions, object summaries, reading without changing, and text and data output.
- `DescribeTests` in the Cyborgs tests restores the two old tests and adds stamina, an attack roll in
  progress, damage reduced by a parry and armour, a state that is off, and a change made by hand.

### Known limits

- A lasting change loses its origin once turn tracking is not running. Such changes are merged into one
  total, and the total has no owner. It shows as set by the rules.
- An unnamed mod added directly by rules code shows only as set by the rules, or set during an action.
- An input carried over from an earlier action in the same activity does not say which action it came from.
- Switching a state or mod set on or off by hand shows in its description, but is not in the list of
  changes made by hand.
- The loop guard only matters for mods that do not count. Two live mods that feed each other would send
  the engine itself round the loop before describe was ever asked.

## Out of scope

- The user interface that shows a description.
- Translation, and wording beyond plain English built by the engine.
- Reasons why an action cannot be performed. That belongs with constraints, feature 2.
- Undoing manual changes as a group.
- Explaining the code inside a calculation function.
- History outside turn tracking. Expired mods are destroyed there.
- Limits such as minimum and maximum. The engine does not apply them yet, so there is nothing to describe.
