using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public class MetaEntity
    {
        [JsonProperty] public Guid? Id { get; private set; }
        [JsonProperty] public string? Name { get; private set; }
        [JsonProperty] public string Path { get; private set; }
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public string Class { get; private set; }
        [JsonProperty] public Entity? Entity { get; private set; }
        [JsonProperty] public string[] SetupMethods { get; private set; }
        [JsonProperty] public MetaAction[] AbilityMethods { get; private set; }
        [JsonProperty] public List<string> ModifiableProperties { get; private set; } = new List<string>();
        [JsonProperty] public Dictionary<string, List<Modifier>> Modifiers { get; private set; } = new Dictionary<string, List<Modifier>>();

        public MetaEntity(object obj, string path)
        {

            Id = (obj as Entity)?.Id;
            Entity = obj as Entity;
            Type = obj.GetType().Name;
            Class = obj.GetEntityClass();
            SetupMethods = obj.GetSetupMethods();
            AbilityMethods = obj.GetAbilityMethods();
            Path = path;
        }

        public override string ToString()
        {
            return $"{Type}[{Name}]={Path}";
        }

        public Dice Evaluate(string prop)
        {
            var mods = GetMods(prop);
            if (mods == null)
                return Entity?.GetValue(prop)?.ToString() ?? "0";

            if (!mods.Any())
                return "0";

            Dice dice = string.Concat(mods.Select(x => x.Evaluate()));
            return dice;
        }

        public List<Modifier>? GetMods(string prop)
        {
            if (ModifiableProperties.Contains(prop))
            {
                if (!Modifiers.ContainsKey(prop))
                    Modifiers.Add(prop, new List<Modifier>());

                return Modifiers[prop];
            }

            return null;
        }

        public bool AddMod(Modifier mod)
        {
            var mods = GetMods(mod.Target.Prop);
            if (mods == null)
                return false;

            var existing = mods.SingleOrDefault(x => x.Name == mod.Name);
            if (existing != null)
                mods.Remove(existing);

            mods.Add(mod);

            return true;
        }

        public Modifier[] RemoveMods()
        {
            var res = new List<Modifier>();
            foreach (var prop in Modifiers.Keys)
                res.AddRange(RemoveMods(prop));

            return res.ToArray();
        }

        public Modifier[] RemoveMods(string prop)
        {
            var removed = Modifiers.ContainsKey(prop)
                ? Modifiers[prop].ToArray()
                : new Modifier[0];

            Modifiers[prop].Clear();

            return removed;
        }

        public Modifier[] RemoveModsByName(string name)
        {
            var removed = Modifiers.ContainsKey(name)
                ? Modifiers[name].ToArray()
                : new Modifier[0];

            Modifiers[name].Clear();

            return removed;
        }
    }
}
