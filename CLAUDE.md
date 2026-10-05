# rpg-scifi-engine

A rules engine for tabletop role-playing games. The goal is a digital character sheet that runs the rules,
so the players and the GM do not need to understand them.

## Read first

- `docs/VISION.md` is the reference for every design decision. Read it before proposing or reviewing engine
  work. It lists seven design features, the open questions, and a gap list of where the code falls short.
- Each finished piece of work has a plan document that records the confirmed decisions, what was built and
  the known limits:
  - `docs/TIME_PLAN.md`: turn tracking, time events, costs and effects, going back a turn.
  - `docs/DICE_PLAN.md`: pending rolls, stored rolls, who rolls.
  - `docs/DESCRIBE_PLAN.md`: describing values, actions, states and mod sets.
  - `docs/BROWSER_SPIKE.md`: the engine running in a browser, with sizes and timings.
  - `docs/CMS_PORT.md`: the CMS on Umbraco 18 and the new engine, and the command layer.
- `docs/system/` is the design of the cyberpunk game system being built on the engine. Fifteen linked HTML
  documents and a shared `_style.css`; start at `index.html`. Read the relevant document before writing
  any rules code for that system. The engine documents above still govern the engine.

## The game system documents

`docs/system/` holds design intent, not implementation plans. One concern per document: `index`, `rolls`,
`pace`, `tempo`, `cooperation`, `gear`, `scenes`, `engine`, `sheet`, `targets`, `character`, `signature`, `items`, `damage`, `values`. The `engine` document is the only one that
talks about code: it maps each rule to an engine construct and lists what the engine does not do yet.

House rules for that folder:

- Every document in a folder that has a `_style.css` is styled HTML, formatted to that sheet. Link the
  folder's `_style.css`, reuse its classes (`docnav`, `.hdr`, `.plate`, `.rule`, `.why`, `.open`, `.chip`,
  `.cards`) rather than inventing new ones, and add a new page to every document's `docnav` strip and to
  the folder's `index.html`. Folders with no `_style.css` stay markdown.
- Decisions are bullets and tables. Prose only where a rule needs a sentence.
- Rationale goes in a `.why` note beside the decision it explains, never inline. Undecided things go in an
  `.open` note. Do not assume an answer to an open note.
- No cross-referencing between documents. The nav strip handles navigation. If two documents keep pointing
  at each other, move the content.
- Every probability in those documents is computed from the full distribution of the expression, not
  estimated. If a rule's numbers change, recompute the tables, including the simulated scenes.
- The system has no name. `System` in those documents is a placeholder. Do not promote it to a name.
- Nothing in `docs/system/` goes into the engine. Game concepts belong in the game system assembly. The
  `engine` document lists the genre-free additions the engine needs; each gets its own plan document in
  `docs` before it is built.

## Project map

Everything is under `src`. Projects target .NET 10.

| Project | What it is |
|---|---|
| `Rpg.Experimental` | The engine. All new engine work goes here. |
| `Rpg.Experimental.Tests` | Engine tests, NUnit. |
| `Rpg.Cyborgs` | A game system built on the engine. A testbed, not a real game. |
| `Rpg.Cyborgs.Tests` | Cyborgs tests. |
| `Rpg.Experimental.Server` | The operations a character sheet app performs on a sheet. No web dependency, so it can sit behind a web api or run on the device. |
| `Rpg.Experimental.Server.Tests` | Tests for the command layer, driven through JSON. |
| `Rpg.Cms` | Umbraco 18 site. Syncs a game system's meta data into document types, serves authored content as character sheets, and exposes the command layer over HTTP. |
| `Rpg.Cms.Tests` | Tests for the document type and data type models. |
| `Rpg.BrowserSpike` | Blazor WebAssembly app proving the engine runs in a browser. Not in the solution. |
| `Rpg.BrowserSpike.Baseline` | The same steps as a console app, for timings. Not in the solution. |
| `Rpg.ModObjects`, `Rpg.ModObjects.Server` | The old engine. Nothing uses it any more. Kept as reference until removed. Do not extend it. |
| `Rpg.Core.Tests`, `Rpg.Core.Tests.Models`, `Rpg.ModObjects.Server.Tests` | Tests for the old engine. |

`apps/rpg-cyborgs` is the character sheet web app. It was written against the old engine's server api and
does not work with the new one yet.

## Build and test

Run the suites that cover what you changed. The first two cover the engine and must always pass.

```bash
dotnet test src/Rpg.Experimental.Tests
dotnet test src/Rpg.Cyborgs.Tests
dotnet test src/Rpg.Experimental.Server.Tests
dotnet test src/Rpg.Cms.Tests
```

The whole solution builds. The CMS can be run on a throwaway SQLite database without touching the real
one, by overriding the connection string and content root. `docs/CMS_PORT.md` describes how it was checked.
Do not start the CMS against the SQL Server database in `appsettings.json` without asking: Umbraco upgrades
a database when it starts.

The browser spike and its desktop baseline are described in `docs/BROWSER_SPIKE.md`. `.claude/launch.json`
has two entries for starting the spike in the app's browser.

## Rules the engine must keep

These come from the vision. Breaking one is a defect even if the tests pass.

- **The engine never refuses.** Limits and prerequisites are reported, never enforced. Any value can be
  changed by hand.
- **Reading a value never rolls.** A roll that is needed is pending until the app rolls it on request or
  the player supplies the result. Rules code never rolls. A roll is a declared whole-number input of an
  action step.
- **A supplied number is the result of the dice, not the total.** Bonuses are added to it.
- **Describe only reads.** It changes nothing, triggers no time event and rolls nothing.
- **Turn counting is not "time passes".** Turns and time events are two separate clocks. "Time passes" is
  the event for when nothing in particular is happening. It gives effects a fuzzy duration.
- **The engine never ends turn tracking by itself.** It starts when declared, or when a turn-based effect
  or a state that needs turns is applied.
- **Costs measured in turns are skipped outside turn tracking** and kept on the action as skipped.
- **No game concepts in the engine.** Anything specific to a game belongs in the game system.
- **No server during play.** Do not design anything that needs a connection between sheets.

## Things that are easy to get wrong

- **The engine has a namespace called `Rpg.Experimental.System`.** Inside the engine, `System.Text` and
  the like resolve to it. Put `using System.Text;` at the top of the file, or write `global::System`.
- **Two types are called `Temporal`.** `Rpg.Experimental.Time.Temporal` is the clock.
  `Rpg.Experimental.Mods.Temporal` is a mod with a lifespan. Files that need both use aliases.
- **Going back a turn replaces every object in the sheet.** Fetch objects again afterwards, for example
  the actor from the sheet.
- **The save format leaves out default values.** A property is not written when its value matches what a
  newly created object of that type has. This is done by comparing with an object created the way a
  restore creates it, in `RpgGraphStateContractResolver`. Do not switch to the JSON library's own
  "ignore defaults" setting: it would lose `false` and zero where the starting value is `true` or non-zero.
- **A restore uses the constructor marked for JSON.** A value set only in the public constructor is not
  set on restore, so it has to be saved.
- **Child objects are not saved inside their parents.** They are saved as references and joined up again
  on restore.
- **`Dice.Roll()` no longer exists.** Read `Number` for a constant. Use dice arithmetic to scale an
  expression. Tests inject a dice roller to fix results.
- **An object that can be authored needs a template constructor.** A public constructor taking one plain
  class, such as `new MeleeWeapon(MeleeWeaponTemplate template)`. The settable properties of that class
  are what an author fills in. Without one the object gets no document type and cannot be created from
  content.
- **The dice parser reads text it does not understand as zero.** Check a value typed by a person before
  parsing it. The command layer does.
- **Do not change the site-wide JSON settings in the CMS.** The rpg controllers read and write their own
  JSON through `RpgServerJson`. Umbraco's own api must keep its settings.
- **A mod added straight to a property by rules code has no origin.** Give it a name with `SetName` so
  describe can show what it is. Prefer the action's cost set and result set.
- **Lasting changes are merged outside turn tracking**, and the merged total loses its origin.
- **A sheet grows during turn tracking**, because expired things are kept so turns can be gone back to.
  This makes long fights slower. It is a known open problem.

## How Cyborgs is used

Cyborgs exists to prove the engine. It should work, but it does not need to be complete or realistic.
When an engine feature needs exercising, add a small plausible example to Cyborgs. Do not stop to ask
what the game's rules are. Say what you chose.

## How work is done here

- For anything beyond a small fix, write a plan document in `docs` first. State the assumptions plainly so
  they can be confirmed or changed. Build after the plan is agreed.
- When the work is built, update the plan: mark it built, record how it differed from the plan, and list
  the known limits.
- Update the gap list in `docs/VISION.md` whenever a gap is closed or a new one is found.
- Add tests for every behaviour. Engine behaviour is tested in the engine tests with the test models
  there. Cyborgs tests show the feature in a game system.
- Do not commit. The owner commits.
- Report honestly. Say what failed, what was not tested, and what is only an estimate.
