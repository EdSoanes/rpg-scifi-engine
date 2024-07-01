using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Props
{
    public class PropDescription
    {
        public RpgObject RootEntity { get; set; }
        public RpgObject Entity { get; private set; }
        public string Prop { get; private set; }
        public string Path { get; private set; }

        public Dice InitialValue { get; private set; }
        public Dice BaseValue { get; private set; }
        public Dice Value { get; private set; }

        public PropDescription(RpgGraph graph, RpgObject rootEntity, PropRef propRef)
        {
            RootEntity = rootEntity;
            Entity = graph.GetEntity(propRef.EntityId)!;
            Path = string.Join('.', rootEntity.PathTo(Entity));
            Prop = propRef.Prop;

            InitialValue = graph.GetInitialPropValue(Entity, propRef.Prop) ?? Dice.Zero;
            BaseValue = graph.GetBasePropValue(Entity, propRef.Prop) ?? Dice.Zero;
            Value = graph.GetPropValue(Entity, propRef.Prop);
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

    public class ModObjectPropDescription : PropDescription
    {
        public ModDescription[] Mods { get; private set; } = new ModDescription[0];

        public ModObjectPropDescription(RpgGraph graph, RpgObject rootEntity, string prop)
            : base(graph, rootEntity, new PropRef(rootEntity.Id, prop))
        {
            Mods = graph.GetActiveMods(Entity, prop)
                .Select(x => new ModDescription(graph, rootEntity, x))
                .ToArray();
        }
    }


    public class ModDescription
    {
        public PropDescription TargetProp { get; private set; }
        public PropDescription? SourceProp { get; private set; }
        public ModType ModType { get; private set; }
        public Dice SourceValue { get; private set; }
        public Dice Value { get; private set; }
        public string? ValueFunction { get; private set; }
        public ModDescription[] Mods { get; private set; } = new ModDescription[0];

        public ModDescription(RpgGraph graph, RpgObject rootEntity, PropDescription targetProp, Mod[] baseMods)
        {
            TargetProp = targetProp;
            ModType = ModType.Override;
            Value = graph.GetBasePropValue(TargetProp.Entity, TargetProp.Prop) ?? Dice.Zero;

            Mods = baseMods
                .Where(x => !x.IsBaseMod)
                .Select(x => new ModDescription(graph, rootEntity, x))
                .ToArray();
        }

        public ModDescription(RpgGraph graph, RpgObject rootEntity, Mod mod)
        {
            TargetProp = new PropDescription(graph, rootEntity, mod);
            ModType = mod.Behavior.Type;
            Value = graph.CalculateModValue(mod);
            ValueFunction = mod.SourceValueFunc.FullName;

            var sourcePropRef = mod.SourcePropRef;
            if (sourcePropRef != null)
            {
                SourceProp = new PropDescription(graph, rootEntity, sourcePropRef);
                SourceValue = SourceProp.Value;

                var mods = new List<ModDescription>();
                var sourceMods = graph.GetActiveMods(sourcePropRef);
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
                SourceValue = mod.SourceValue ?? Dice.Zero;
        }

        public override string ToString()
        {
            var res = $"[{ModType}] {(ModType == ModType.Initial ? TargetProp.InitialValue : Value)}";
            if (!string.IsNullOrEmpty(ValueFunction))
                res += $" ({ValueFunction} {SourceValue})";

            var srcProp = SourceProp?.PropertyString();
            if (!string.IsNullOrEmpty(srcProp))
                res += $" from {srcProp}";

            return res;
        }
    }
}
