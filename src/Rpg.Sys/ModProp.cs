using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys
{
    public class ModProp
    {
        public Guid Id { get; set; }
        public string Prop { get; set; }
        public string ReturnType {  get; set; }

        public List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore]
        public Modifier[] FilteredModifiers
        {
            get
            {
                var activeModifiers = Modifiers.Where(x => x.Expiry == ModifierExpiry.Active);

                var res = activeModifiers
                    .Where(x => x.ModifierType != ModifierType.Base && x.ModifierType != ModifierType.Player)
                    .ToList();

                var baseMods = activeModifiers
                    .Except(res)
                    .GroupBy(x => $"{x.Target.Id}.{x.Target.Prop}.{x.Name}");

                foreach (var group in baseMods)
                {
                    var mod = group.FirstOrDefault(x => x.ModifierType == ModifierType.Player) ?? group.FirstOrDefault(x => x.ModifierType == ModifierType.Base);
                    if (mod != null)
                        res.Add(mod);
                }

                return res.ToArray();
            }
        }

        [JsonConstructor] private ModProp() { }

        public ModProp(Guid id, string prop, string returnType)
        {
            Id = id;
            Prop = prop;
            ReturnType = returnType;
        }

        public Modifier[] MatchingMods(Modifier mod)
        {
            return Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();
        }

        public void RemoveMatchingMods(Modifier mod)
        {
            var matching = MatchingMods(mod);
            Modifiers = Modifiers.Except(matching).ToList();
        }
    }
}
