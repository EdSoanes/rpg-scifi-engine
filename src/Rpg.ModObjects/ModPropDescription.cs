using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class ModPropDescription
    {
        public ModObject RootEntity { get; set; }
        public ModObject Entity { get; private set; }
        public string Prop { get; private set; }
        public string Path { get; private set; }

        public Dice InitialValue { get; private set; }
        public Dice BaseValue { get; private set; }
        public Dice Value { get; private set; }

        public ModPropDescription(ModGraph graph, ModObject rootEntity, ModPropRef propRef)
        {
            RootEntity = rootEntity;
            Entity = graph.GetEntity(propRef.EntityId)!;
            Path = string.Join('.', rootEntity.PathTo(Entity));
            Prop = propRef.Prop;

            InitialValue = Entity.CalculateInitialValue(propRef.Prop) ?? Dice.Zero;
            BaseValue = Entity.CalculateBaseValue(propRef.Prop) ?? Dice.Zero;
            Value = Entity.GetPropValue(propRef.Prop) ?? Dice.Zero;
        }

        public string PropertyString()
        {
            var parts = new List<string>
            {
                string.IsNullOrEmpty(Path) ? Entity.Name : RootEntity.Name,
                Path,
                Prop
            };

            var prop = string.Join('.', parts.Where(x => !string.IsNullOrEmpty(x)));
            return prop;
        }

        public override string ToString()
            => $"{PropertyString()} = {Value}";
    }

    public class ModObjectPropDescription : ModPropDescription
    {
        public ModDescription[] Mods { get; private set; } = new ModDescription[0];

        public ModObjectPropDescription(ModGraph graph, ModObject rootEntity, string prop)
            : base(graph, rootEntity, new ModPropRef(rootEntity.Id, prop))
        {
            Mods = Entity.GetMods(prop)
                .Select(x => new ModDescription(graph, rootEntity, x))
                .ToArray();
        }
    }


    public class ModDescription
    {
        public ModPropDescription TargetProp { get; private set; }
        public ModPropDescription? SourceProp { get; private set; }
        public ModType ModType { get; private set; }
        public Dice SourceValue { get; private set; }
        public Dice Value { get; private set; }
        public string? ValueFunction { get; private set; }
        public ModDescription[] Mods { get; private set; } = new ModDescription[0];

        public ModDescription(ModGraph graph, ModObject rootEntity, ModPropDescription targetProp, Mod[] baseMods)
        {
            TargetProp = targetProp;
            ModType = ModType.BaseOverride;
            Value = TargetProp.Entity.CalculateBaseValue(targetProp.Prop) ?? Dice.Zero;

            Mods = baseMods
                .Where(x => !x.IsBaseMod)
                .Select(x => new ModDescription(graph, rootEntity, x))
                .ToArray();
        }

        public ModDescription(ModGraph graph, ModObject rootEntity, Mod mod)
        {
            TargetProp = new ModPropDescription(graph, rootEntity, mod);
            ModType = mod.ModifierType;
            Value = mod.Source.CalculatePropValue(graph);
            ValueFunction = mod.Source.ValueFunc.FullName;

            var sourcePropRef = mod.Source.PropRef;
            if (sourcePropRef != null)
            {
                SourceProp = new ModPropDescription(graph, rootEntity, sourcePropRef);
                SourceValue = SourceProp.Value;

                var mods = new List<ModDescription>();
                var sourceMods = SourceProp.Entity.GetMods(sourcePropRef.Prop);
                if (sourceMods.Any(x => x.IsBaseOverrideMod))
                {
                    mods.Add(new ModDescription(graph, rootEntity, TargetProp, sourceMods.Where(x => x.IsBaseMod && !x.IsBaseOverrideMod).ToArray()));
                    mods.AddRange(sourceMods
                        .Where(x => !x.IsBaseMod)
                        .Select(x => new ModDescription(graph, rootEntity, x)));
                }
                else
                {
                    mods.AddRange(sourceMods
                        .Select(x => new ModDescription(graph, rootEntity, x)));
                }

                Mods = mods.ToArray();
            }
            else
                SourceValue = mod.Source.Value ?? Dice.Zero;
        }

        public override string ToString()
        {
            var res = $"[{ModType}] {(ModType == ModType.BaseInit ? TargetProp.InitialValue : Value)}";
            if (!string.IsNullOrEmpty(ValueFunction))
                res += $" ({ValueFunction} {SourceValue})";

            var srcProp = SourceProp?.PropertyString();
            if (!string.IsNullOrEmpty(srcProp))
                res += $" from {srcProp}";

            return res;
        }
    }
}
