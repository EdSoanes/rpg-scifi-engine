using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModifierStore
    {
        [JsonProperty] public Dictionary<string, List<Modifier>> Modifiers { get; private set; } = new Dictionary<string, List<Modifier>>();
        [JsonProperty] public List<string> ModdableProperties { get; private set; } = new List<string>();


        public Dice Evaluate(string prop)
        {
            var mods = Get(prop);
            if (!mods?.Any() ?? true)
                return "0";

            Dice dice = Dice.Sum(mods!.Select(x => x.Evaluate()));
            return dice;
        }

        public List<Modifier> Get(string prop)
        {
            if (!ModdableProperties.Any() || ModdableProperties.Contains(prop))
            {
                if (!Modifiers.ContainsKey(prop))
                    Modifiers.Add(prop, new List<Modifier>());

                return Modifiers[prop];
            }

            return new List<Modifier>();
        }

        public Modifier Create(string name, Dice dice, ModdableProperty target, string? diceCalc = null)
        {
            return new Modifier(name, dice, target, diceCalc);
        }

        public Modifier Create(string name, ModdableProperty source, ModdableProperty target, string? diceCalc = null)
        {
            return new Modifier(name, source, target, diceCalc);
        }

        public bool Add(Modifier mod)
        {
            var mods = Get(mod.Target.Prop);

            var existing = mods.SingleOrDefault(x => x.Name == mod.Name);
            if (existing != null)
                mods.Remove(existing);

            mods.Add(mod);

            return true;
        }

        public bool Store(string prop, Modifier mod)
        {
            var mods = Get(prop);

            var existing = mods.SingleOrDefault(x => x.Name == mod.Name);
            if (existing != null)
                mods.Remove(existing);

            mods.Add(mod);

            return true;
        }

        public void RemoveNamed(string name)
            => RemoveByCondition(mod => mod.Name == name);

        public void RemoveExpired(int currentTurn)
            => RemoveByCondition((mod) => mod.ShouldBeRemoved(currentTurn));

        public void Clear()
            => RemoveByCondition(mod => mod.CanBeCleared());

        public void Remove(Modifier modifier)
        {
            if (Modifiers.ContainsKey(modifier.Target.Prop))
            {
                var propModifiers = Modifiers[modifier.Target.Prop];
                if (propModifiers.Contains(modifier))
                    propModifiers.Remove(modifier);
            }
        }

        private void RemoveByCondition(Func<Modifier, bool> condition)
        {
            foreach (var mods in Modifiers.Values)
            {
                var toRemove = new List<Modifier>();
                foreach (var mod in mods)
                {
                    if (condition(mod))
                        toRemove.Add(mod);
                }

                foreach (var mod in toRemove)
                    mods.Remove(mod);
            }
        }
    }
}
