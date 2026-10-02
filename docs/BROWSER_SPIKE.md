# Browser spike: findings

Does the C# rules engine run in a browser with no server? This answers the first open question in
[VISION.md](VISION.md), which feature 7 depends on.

Date: 2026-10-02.

## Answer

Yes. The engine and the Cyborgs system run in a browser as they are, with no changes to either. Every
step of the spike passed in a published build. A rewrite in another language is not needed.

It is slower than on the desktop, by about seven times. A single action is fast enough to use. Two things
in the engine itself, not the browser, make long fights slow and saved sheets large. They are listed
under "What needs work".

## What was built

- `src/Rpg.BrowserSpike`: a Blazor WebAssembly app. It references `Rpg.Experimental` and `Rpg.Cyborgs`
  directly. On load it runs a fixed list of steps, checks each result, times it, and shows a table.
- `src/Rpg.BrowserSpike.Baseline`: a console app that runs the same steps on the desktop, for comparison.
- `src/Rpg.BrowserSpike/serve.js`: a small static file server for the published build.

Neither project is in the solution file, so they do not affect the normal build or the test suites.

The steps:

| Step | What it proves |
|---|---|
| Build the game system | Scanning assemblies and reflection work |
| Create a character sheet | The meta system and property data work |
| Derived stats | Mods and calculations work |
| An attack with real dice | Actions, pending rolls and supplied rolls work |
| A parry with app dice | App rolls work |
| Describe a value and an action | Describe works, including a source on another object |
| A change made by hand | Overrides work and can be undone |
| A time event | Game system time events work during turn tracking |
| Save and restore | The sheet survives being turned to text and back, and still works afterwards |
| Go back a turn | Compressed snapshots work |
| 50 attacks over 50 turns | How speed holds up in a long fight |
| Save and restore 10 times | How speed holds up with a large sheet |
| End turn tracking | The turn history is discarded |

## Results

Measured on this development machine, in the app's built-in browser.

### Does it work

| Build | Result |
|---|---|
| Debug, run from the SDK | All steps pass |
| Release, published and trimmed | All steps pass |

One step failed on the first run. That was a wrong check in the spike, not a fault in the engine.

### Download size

Published build, compressed as a host would serve it:

| Part | Size |
|---|---|
| Whole app, compressed | 4.05 MB |
| Whole app, uncompressed | 13.3 MB |
| .NET runtime | 954 KB |
| Newtonsoft.Json | 208 KB |
| The engine | 71 KB |
| Cyborgs | 11 KB |

The engine and the game system are a small part of the download. The rest is the .NET runtime and its
libraries. It is downloaded once and then cached by the browser.

### Start-up

About half a second from page start to the engine beginning its first step, with the files cached. A first
visit also has to download the 4 MB.

### Speed

| Step | Desktop | Browser, published | Slower by |
|---|---|---|---|
| Build the game system | 201 ms | 37 ms | |
| Create a character sheet | 75 ms | 126 ms | 1.7 |
| First attack, cold | 244 ms | 551 ms | 2.3 |
| Parry | 32 ms | 125 ms | 3.9 |
| Describe | 11 ms | 24 ms | 2.1 |
| Save the sheet | 28 ms | 41 ms | 1.5 |
| Restore the sheet and attack | 233 ms | 399 ms | 1.7 |
| Go back a turn, twice | 41 ms | 232 ms | 5.7 |
| 50 attacks over 50 turns | 1391 ms | 7405 ms | 5.3 |
| Save and restore 10 times | 846 ms | 6545 ms | 7.7 |

The debug build was three to four times slower again. It is not what players would run.

Per turn, in the 50 turn fight:

| | Desktop | Browser |
|---|---|---|
| Each of the first 10 turns | 10 ms | 71 ms |
| Each of the last 10 turns | 27 ms | 236 ms |

## What needs work

### 1. Each turn costs more than the last

During turn tracking the engine keeps everything that has expired, so that turns can be gone back to. After
50 turns the sheet held 286 objects, up from 94. Every refresh walks all of them. A turn late in a long
fight took three times as long as an early one. This is in the engine, and shows on the desktop too.

A phone is several times slower than this machine. A turn that takes a quarter of a second here could take
a second there.

### 2. A saved sheet is large

One character with one sword saves as 357 KB of text. The text is indented and repeats long .NET type
names. Each turn also stores a compressed copy of the whole sheet, so after 50 turns the saved sheet was
3.1 MB. Saving and restoring that took over half a second in the browser.

This matters for two things in the vision: saving on the device, and handing a character or an item to
another device by code or file.

### 3. The engine blocks the page while it works

The spike runs the engine on the page's own thread. While a step runs, the page cannot respond. For single
actions this is not noticeable. For anything long it is. A real app would run the engine on a background
thread.

## Update: saved sheet size

Three changes were made to the engine after the first run, to shrink a saved sheet.

- **Compression.** The sheet has a save operation that returns compressed text, and a load operation that
  reads it. Loading also accepts text that is not compressed.
- **Less text.** No indentation. A property is left out when its value is what a newly created object of
  that type already has, because a restore creates the object the same way.
- **A cap on turn history.** A sheet keeps the last five turns by default. The number is a setting of the
  sheet. It is saved with it, and zero keeps none.

Size of the saved sheet:

| Sheet | Before | After |
|---|---|---|
| A new character with one sword | 297 KB | 8 KB |
| After an attack and a parry, two turns of history | 357 KB | 28 KB |
| After 50 turns of attacks | 3,130 KB | 214 KB |
| After turn tracking ends | 281 KB | 8 KB |

Speed, published build in the browser:

| Step | Before | After |
|---|---|---|
| Save the sheet | 41 ms | 64 ms |
| Restore the sheet and attack | 399 ms | 247 ms |
| Go back a turn, twice | 232 ms | 147 ms |
| 50 attacks over 50 turns | 7,405 ms | 6,035 ms |
| Save and restore 10 times | 6,545 ms | 4,053 ms |
| Each of the last 10 turns | 236 ms | 179 ms |

Saving one sheet is a little slower because it now compresses. Everything that reads a sheet is faster.

What remains of the two problems under "What needs work":

- The sheet still grows each turn during turn tracking, so each turn still costs more than the last. After
  50 turns the saved sheet is 214 KB because the sheet itself holds 286 objects, and each of the five
  turns of history is a copy of it.
- Every action and every untouched state is still saved in full. For a new character that is most of
  what is left. It grows with the number of items.

## What was not tested

- **A phone.** Everything was measured on a development machine. A real phone is the test that matters.
- **Compiling ahead of time.** .NET can compile the app to WebAssembly in advance instead of interpreting
  it. That usually makes code like this several times faster and the download about twice as large. It
  needs an extra SDK component, the `wasm-tools` workload, which is not installed here. I did not install
  it, because that changes the machine's SDK.
- **Other browsers.** Only the app's built-in browser, which is Chromium.
- **Working offline.** The app was served from a local server. Installing it so it loads with no network
  was not tried. The template supports it.
- **Every code path.** The published build removes unused code from the .NET libraries. The engine and
  Cyborgs assemblies themselves are left whole, which is why reflection kept working. The steps here cover
  the main paths, but a path that was not exercised could still hit something removed.

## How to run it

The desktop baseline:

```bash
dotnet run -c Release --project src/Rpg.BrowserSpike.Baseline
```

The debug build in a browser, at http://localhost:5019:

```bash
dotnet run --project src/Rpg.BrowserSpike --no-launch-profile --urls http://localhost:5019
```

The published build in a browser, at http://localhost:5020:

```bash
dotnet publish src/Rpg.BrowserSpike -c Release -o src/Rpg.BrowserSpike/bin/publish
```

```bash
node src/Rpg.BrowserSpike/serve.js 5020
```

## Recommendation

1. Keep the engine in C#. Design the device-side library to run in the browser.
2. Test on a real phone before building much more on this. It is one published folder and a phone on the
   same network.
3. Install the `wasm-tools` workload and measure a build compiled ahead of time.
4. Fix the two engine costs: the growth per turn, and the size of a saved sheet. Both help the desktop and
   the tests as well.
