# CMS port: what was done

The content management site, `Rpg.Cms`, moved from Umbraco 14 and the old engine to Umbraco 18 and the new
engine. This records the decisions, what was built, how it was checked and what is left.

Status: built and tested. Checked end to end on a throwaway database. Not yet run against the real one.

## What the CMS is for

Under feature 7 of [VISION.md](VISION.md) the CMS is used before a session, not during one. It does three
things:

1. It turns a game system's meta data into document types, so an author can create the system's
   characters and items as content.
2. It serves that content as new character sheets.
3. For now, it also serves the operations a character sheet app performs on a sheet, over HTTP.

The third is a stopgap. The same operations are meant to run on the device with no server.

## Decisions

No plan was agreed before this work, so these were my choices. Each can be changed.

### A. The operations on a sheet live outside the CMS

- A new project, `Rpg.Experimental.Server`, holds every operation a character sheet app performs:
  describe, changes by hand, states, time, actions, rolls and settings.
- It has no dependency on a web framework. The CMS exposes it over HTTP. A browser build can call the same
  class directly, which is the device-side library the vision needs.
- It keeps nothing between calls. A request carries the whole sheet as text, and the response carries it
  back.

### B. An object says what an author fills in by having a template

- A game system object that can be authored has a public constructor taking one template class, such as
  `new MeleeWeapon(MeleeWeaponTemplate template)`. Cyborgs already worked this way.
- The engine's meta data now records the template and its properties. Those properties are what an author
  fills in. Everything else on the object is worked out by the rules.
- An object with no template, such as a body part, gets no document type.
- The engine has a factory that builds an object from values given by name. The CMS uses it, and so can
  anything else that receives authored values, such as an item handed over from another sheet.

### C. One data type per kind of value

- Each system gets six data types: integer, dice, text, long text, boolean and a picker for child objects.
- The old code made a data type for each property attribute, with minimum and maximum as validation. That
  is gone. The engine reports limits and never enforces them, so the CMS should not refuse a value either.
- Dice are entered as text, for example `2d6 + 1`.

### D. Descriptions use a plain text area, not the rich text editor

- Umbraco 16 replaced its rich text editor. The new one needs its toolbar and extensions configured by
  name, and I could not confirm those names. A text area is certain to work.

### E. Property aliases are made safe

- The alias of a property in the CMS is the property name with only letters and digits and a lower case
  first letter, such as `hitBonus`. The old code used the raw name, which could contain dots and spaces.
- The description of the property holds the real property name. That is what turns content back into an
  object.

### F. The CMS's JSON settings are left alone

- The old code changed the JSON settings for every controller in the site, including Umbraco's own.
- The rpg controllers now read and write their own JSON. Enums and dice travel as text, and there are no
  .NET type names.

### G. The HTTP api changed shape

- The old api passed the engine's objects back and forth. The new one passes the saved sheet text, and
  plain descriptions built for a user interface.
- The character sheet app in `apps/rpg-cyborgs` was written for the old api. It will not work until its
  client is regenerated and its screens are updated. That is not part of this work.

## What was built

### Engine, `Rpg.Experimental`

- A system's meta data records, for each object, its .NET type and its template.
- `RpgObjectFactory` creates an object from values given by name, and puts a child into a parent's
  property.

### Command layer, `Rpg.Experimental.Server`

- `RpgSessionlessServer`: the operations.
- `RpgSystems`: the game systems a host has registered.
- `IContentFactory`: where authored objects come from. The host supplies it.
- `RpgServerJson`: the JSON a host exchanges with a user interface.
- A sheet view: everything a user interface needs to draw a sheet in one response.

### CMS, `Rpg.Cms`

- Moved to Umbraco 18.2 on .NET 10.
- Sync of data types, document types and library content from the new meta data.
- Content to object conversion through the engine's factory.
- `RpgSheetController`: 22 endpoints under `/api/rpg`.
- `RpgManagementController`: sync endpoints for someone signed in to the backoffice. The old one was
  commented out, so there was no way to run a sync.
- An option, `Rpg:SyncOnStartup`, to sync every system when the site starts.
- OpenAPI documents through Microsoft's generator, which Umbraco 18 now uses in place of Swashbuckle.
  They are served from `/umbraco/openapi`.

### Umbraco 18 changes that were needed

| Change | Why |
|---|---|
| Swagger setup rewritten | Swashbuckle is gone in Umbraco 18 |
| Content create, update and publish calls rewritten | The models for these changed |
| Models builder mode set to "Nothing" | The old default now needs an extra package, and the site would not start |
| Razor compile flags removed from the project file | Umbraco's guidance from version 17 |
| Two stock partial views updated | They used a block property removed after version 14 |

## How it was checked

### Tests

| Suite | Tests |
|---|---|
| `Rpg.Experimental.Tests` | 193 |
| `Rpg.Cyborgs.Tests` | 57 |
| `Rpg.Experimental.Server.Tests`, new | 24 |
| `Rpg.Cms.Tests`, rewritten | 8 |

The whole solution now builds with no errors.

### End to end, on a throwaway database

The site was run on a new, empty SQLite database in a temporary folder, with sync on start-up. A script
then did the following over HTTP. Every step passed.

1. Signed in to the backoffice.
2. Listed the systems through the admin api, and ran the sync a second time to show it can be repeated.
3. Confirmed the document types, the three libraries, and the action library content.
4. Authored a sword and a character holding it, as content.
5. Listed the content through the open api.
6. Created a character sheet from the character. The stats, the derived stats and the sword in hand were
   correct.
7. Played an attack: started turn tracking, ran the steps, was stopped by the pending roll, supplied the
   roll, and completed the action.
8. Described the attack roll.
9. Sent a bad request and got a clear error.

## What was not checked

- **The real database.** The site has not been run against the SQL Server database in `appsettings.json`.
  Umbraco will upgrade that database from 14 to 18 the first time it starts. Whether a direct jump is
  supported is not something I could confirm. The content can be recreated by a sync, so an empty database
  is a safe fallback.
- **The backoffice by hand.** Content was authored through Umbraco's api, not by clicking through the
  editor. The picker for child objects, in particular, has not been used by a person.
- **Document types left by the old sync.** A database synced by the old code has document types with the
  old property aliases. The new sync will add its own properties beside them. Starting from an empty
  database avoids this.

## Known limits

- The character sheet app does not work with the new api. See decision G.
- Only list properties get a picker, such as hands and wearing. A property holding a single child object
  does not.
- Four calls to an Umbraco method are marked obsolete and will be removed in Umbraco 19.
- The meta data sent by the systems endpoint is large, because every action includes its method
  signatures.
- A sheet created from content has the character as its context. There is no room or scene.

## The old engine

Nothing that builds now uses `Rpg.ModObjects` except its own tests. These five projects can be deleted:

- `Rpg.ModObjects`
- `Rpg.ModObjects.Server`
- `Rpg.ModObjects.Server.Tests`
- `Rpg.Core.Tests`
- `Rpg.Core.Tests.Models`

They have been left in place for now. The old server operations and the old CMS code were the reference
for this port, and are worth keeping until the site has run against the real database.
