using System.Diagnostics;
using Rpg.Cyborgs;
using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Json;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;

namespace Rpg.BrowserSpike
{
    public sealed class SpikeStep
    {
        public string Name { get; set; } = string.Empty;
        public bool Ok { get; set; }
        public double Ms { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    /// <summary>
    /// Runs the engine and the Cyborgs system through the things a character sheet has to do, with no
    /// server. Each step is timed and checked. A step that throws is recorded as failed and the rest carry on
    /// where they can.
    /// </summary>
    public sealed class SpikeRunner
    {
        public List<SpikeStep> Steps { get; } = new();
        public bool AllOk => Steps.Count > 0 && Steps.All(x => x.Ok);

        private RpgSystem? _system;
        private RpgCharacterSheet? _sheet;
        private PlayerCharacter? _pc;
        private MeleeWeapon? _sword;
        private string? _saved;

        public void Run()
        {
            Step("Build the game system by scanning assemblies", () =>
            {
                RpgTypeUtilities.RegisterAssembly(typeof(CyborgsSystem).Assembly);
                _system = RpgSystemFactory.Build(new CyborgsSystem());

                Check(_system.Objects.Any(x => x.Archetypes.Contains(nameof(PlayerCharacter))), "PlayerCharacter not found in the system");
                Check(_system.Actions.Length > 0, "no actions found");
                Check(_system.TimeEvents.SequenceEqual(new[] { "Sunrise", "Sunset" }), "time events missing");

                return $"{_system.Objects.Length} object types, {_system.Actions.Length} actions, {_system.States.Length} states";
            });

            Step("Create a character sheet", () =>
            {
                _sword = new MeleeWeapon(new MeleeWeaponTemplate { Name = "Excalibur", Damage = "d6", HitBonus = 1 });
                _pc = new PlayerCharacter(new PlayerCharacterTemplate
                {
                    Name = "Benny",
                    Strength = -1,
                    Agility = 0,
                    Health = 1,
                    Brains = 1,
                    Insight = 0,
                    Charisma = 1
                });
                _pc.Hands.Add(_sword);

                _sheet = new RpgCharacterSheet(_pc, _system);

                return $"{_sheet.GetObjectCount()} objects in the sheet";
            });

            Step("Derived stats", () =>
            {
                Check(_pc!.Defence == 7, $"Defence is {_pc.Defence}, expected 7");
                Check(_pc.StaminaPoints == 14, $"StaminaPoints is {_pc.StaminaPoints}, expected 14");
                Check(_pc.LifePoints == 5, $"LifePoints is {_pc.LifePoints}, expected 5");

                return $"Defence {_pc.Defence}, stamina {_pc.StaminaPoints}, life {_pc.LifePoints}";
            });

            RpgActivityAction? attack = null;

            Step("An attack with real dice", () =>
            {
                _sheet!.BeginTurnTracking();

                attack = _sheet.CreateActivity(_pc!.Id, _sword!.Id, nameof(MeleeAttack)).CurrentActivityAction!;
                Check(attack.Cost(_sheet, ("actionPoints", 1), ("focusPoints", 0)), "cost step did not run");
                Check(attack.Perform(_sheet, ("targetDefence", 12)), "perform step did not run");
                Check(!attack.Outcome(_sheet), "outcome ran without a roll");
                Check(_sheet.GetPendingRolls().Length == 1, "no pending roll");

                _sheet.SetRoll(attack, "diceRoll", 9);
                Check(attack.Outcome(_sheet), "outcome did not run after the roll");
                attack.Complete(_sheet);

                var total = _sheet.GetPropertyValue<Dice>(attack.Id, "diceRoll");
                Check(total == new Dice(10), $"attack total is {total}, expected 10");
                Check(_pc.CurrentActionPoints == 0, "action point not spent");
                Check(_pc.IsStateOn(nameof(MeleeAttacking)), "MeleeAttacking state is not on");

                return $"rolled 9, total {total}, action points {_pc.CurrentActionPoints}";
            });

            Step("A parry with app dice", () =>
            {
                _sheet!.NextTurn();
                _sheet.RollMode = RpgRollMode.App;

                var parry = _sheet.CreateActivity(_pc!.Id, _pc.Id, nameof(MeleeParry)).CurrentActivityAction!;
                Check(parry.Cost(_sheet, ("focusPoints", 0), ("damage", 10)), "cost step did not run");
                Check(parry.Perform(_sheet, ("parryTarget", 8)), "perform step did not run");
                Check(parry.Outcome(_sheet), "outcome did not run");
                parry.Complete(_sheet);

                var roll = _sheet.GetRoll(parry.Id, "diceRoll");
                Check(roll != null && roll.Result >= 2 && roll.Result <= 12, "no stored roll in range");
                Check(roll!.SuppliedBy == RpgRollSource.App, "roll not marked as an app roll");

                _sheet.RollMode = RpgRollMode.Ask;
                return $"app rolled {roll.Result} ({string.Join(" + ", roll.Dice ?? [])})";
            });

            Step("Describe a value and an action", () =>
            {
                var stamina = _sheet!.Describe(_pc!, x => x.CurrentStaminaPoints)!.ToText();
                Check(stamina.Contains("Twice health (1 gives 2)"), "calculation description missing");

                var roll = _sheet.Describe(attack!, "diceRoll")!.ToText();
                Check(roll.Contains("rolled 9 on 2d6 by the player"), "roll description missing");
                Check(roll.Contains("From Excalibur.HitBonus"), "source through another object missing");

                var action = _sheet.DescribeAction(attack!).ToText();
                Check(action.Contains("MeleeAttack (Excalibur): completed"), "action description wrong");

                return roll.Split('\n')[0].Trim();
            });

            Step("A change made by hand", () =>
            {
                var change = _sheet!.OverrideByHand(_pc!, x => x.Strength, 2);
                Check(_pc!.Strength == 2 && _pc.LifePoints == 8, "override did not reach the derived stat");

                _sheet.RemoveManualChange(change);
                Check(_pc.Strength == -1 && _pc.LifePoints == 5, "undoing the override did not restore the stats");

                return "strength overridden to 2 and back";
            });

            Step("A time event defined by the game system", () =>
            {
                _sheet!.Add(new Rpg.Experimental.Mods.Standard().Until("Sunrise"), _pc!, x => x.Agility, 4);
                _sheet.Time.Refresh();
                Check(_pc!.IsStateOn(nameof(VeryFast)), "VeryFast is not on");

                _sheet.TriggerTimeEvent("Sunrise");
                Check(_pc.Agility == 0 && !_pc.IsStateOn(nameof(VeryFast)), "the effect did not end at sunrise");
                Check(_sheet.Time.IsTurnTracking, "the time event ended turn tracking");

                return "effect ended at Sunrise, turn tracking carried on";
            });

            Step("Save the sheet to text", () =>
            {
                _saved = _sheet!.Save();
                Check(_saved.Length > 1000, "saved sheet is suspiciously small");

                var json = RpgJson.SerializeGraphState(_sheet.GetState());
                return $"{_saved.Length / 1024.0:0.0} KB saved ({json.Length / 1024.0:0.0} KB before compression), {_sheet.GetObjectCount()} objects";
            });

            Step("Restore the sheet from text", () =>
            {
                var sheet2 = RpgCharacterSheet.Load(_saved!, _system!);
                var pc2 = (PlayerCharacter)sheet2.Actor;
                var restoredAtTurn = sheet2.Time.Turn;

                Check(pc2.Name == "Benny", "actor not restored");
                Check(pc2.Defence == 7 && pc2.StaminaPoints == 14, "stats not restored");
                Check(sheet2.Time.Turn == _sheet!.Time.Turn, "turn not restored");
                Check(sheet2.GetRoll(attack!.Id, "diceRoll")?.Result == 9, "stored roll not restored");
                Check(sheet2.Save() == _saved, "saving the restored sheet gives different text");

                //The restored sheet has to work, not just hold values
                sheet2.NextTurn();
                var attack2 = sheet2.CreateActivity(pc2.Id, pc2.Hands.Get<MeleeWeapon>().First().Id, nameof(MeleeAttack)).CurrentActivityAction!;
                attack2.AutoComplete(sheet2, ("actionPoints", 1), ("focusPoints", 0), ("targetDefence", 12));
                Check(attack2.IsComplete, "an attack on the restored sheet did not complete");

                return $"restored at turn {restoredAtTurn} and attacked again";
            });

            Step("Go back a turn", () =>
            {
                var turn = _sheet!.Time.Turn;
                _sheet.NextTurn();
                _sheet.OverrideByHand(_pc!, x => x.Strength, 3);

                Check(_sheet.RewindToTurn(turn + 1), "rewind refused");
                _pc = (PlayerCharacter)_sheet.Actor;
                Check(_pc.Strength == -1, "the override was not undone by going back");

                Check(_sheet.RewindToTurn(1), "rewind to turn 1 refused");
                _pc = (PlayerCharacter)_sheet.Actor;
                //The start of turn 1 is before the attack was made
                Check(_sheet.Time.Turn == 1 && _pc.CurrentActionPoints == 1, "turn 1 not restored as it was");

                return $"turns that can be gone back to: {string.Join(", ", _sheet.GetRewindableTurns())}";
            });

            Step("Speed: 50 attacks over 50 turns", () =>
            {
                var sword = _pc!.Hands.Get<MeleeWeapon>().First();
                var lap = Stopwatch.StartNew();
                double first10 = 0, last10 = 0;
                for (var i = 0; i < 50; i++)
                {
                    lap.Restart();
                    _sheet!.NextTurn();
                    var action = _sheet.CreateActivity(_pc.Id, sword.Id, nameof(MeleeAttack)).CurrentActivityAction!;
                    action.AutoComplete(_sheet, ("actionPoints", 1), ("focusPoints", 0), ("targetDefence", 12));
                    Check(action.IsComplete, $"attack {i + 1} did not complete");

                    if (i < 10) first10 += lap.Elapsed.TotalMilliseconds;
                    if (i >= 40) last10 += lap.Elapsed.TotalMilliseconds;
                }

                //The sheet keeps every turn while turns are tracked, so each turn costs more than the last
                return $"now at turn {_sheet!.Time.Turn}, {_sheet.GetObjectCount()} objects. First 10 turns {first10 / 10:0} ms each, last 10 turns {last10 / 10:0} ms each";
            });

            Step("Speed: save and restore 10 times", () =>
            {
                var text = string.Empty;
                for (var i = 0; i < 10; i++)
                {
                    text = _sheet!.Save();
                    _sheet = RpgCharacterSheet.Load(text, _system!);
                }

                _pc = (PlayerCharacter)_sheet!.Actor;
                return $"{text.Length / 1024.0:0.0} KB each time, including {_sheet.GetRewindableTurns().Length} turn snapshots";
            });

            Step("End turn tracking", () =>
            {
                _sheet!.EndTurnTracking();
                Check(!_sheet.Time.IsTurnTracking, "turn tracking did not end");
                Check(_sheet.GetRewindableTurns().Length == 0, "snapshots were not discarded");

                var text = _sheet.Save();
                return $"{text.Length / 1024.0:0.0} KB once the turn history is discarded";
            });
        }

        private void Step(string name, Func<string> action)
        {
            var step = new SpikeStep { Name = name };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                step.Detail = action();
                step.Ok = true;
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null)
                    inner = inner.InnerException;

                step.Ok = false;
                step.Detail = $"{inner.GetType().Name}: {inner.Message}";
                Console.WriteLine($"SPIKE STEP FAILED: {name}\n{ex}");
            }

            stopwatch.Stop();
            step.Ms = stopwatch.Elapsed.TotalMilliseconds;
            Steps.Add(step);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
