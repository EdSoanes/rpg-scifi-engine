using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class ModPropDescription
    {
        private ModGraph _graph;
        private readonly ModObject _rootEntity;
        private readonly ModObject _entity;

        public string Entity { get; private set; }
        public string[]? Path { get; private set; }
        public string? Prop { get; private set; }
        public Dice Value { get; private set; }
        public string? ValueFunction { get; private set; }
        public List<ModPropDescription> Mods { get; private set; }

        public ModPropDescription(ModGraph graph, ModObject rootEntity, string prop)
            : this(graph, rootEntity, rootEntity.Id, prop)
        {
            Mods = _entity.GetMods(prop)
                .Select(x => new ModPropDescription(_graph, _rootEntity, x))
                .ToList();
        }

        public ModPropDescription(ModGraph graph, ModObject rootEntity, ModPropRef propRef)
            : this(graph, rootEntity, propRef.EntityId, propRef.Prop)
        {
            Mods = _entity.GetMods(propRef.Prop)
                .Select(x => new ModPropDescription(_graph, _rootEntity, x))
                .ToList();
        }

        public ModPropDescription(ModGraph graph, ModObject rootEntity, Mod mod)
            : this(graph, rootEntity, mod.Source.EntityId, mod.Source.Prop, mod.Source.Value)
        {
            ValueFunction = mod.Source.ValueFunc.FuncName;
            if (mod.Source.PropRef != null)
            {
                var affectedByPropRefs = _entity.GetPropsThatAffect(mod.Prop);
                Mods = affectedByPropRefs
                    .Select(x => new ModPropDescription(_graph, _rootEntity, x))
                    .ToList();
            }
            else
            {
                Mods = new List<ModPropDescription>();
            }
        }

        private ModPropDescription(ModGraph graph, ModObject rootEntity, Guid? entityId, string? prop, Dice? value = null)
        {
            _graph = graph;
            _rootEntity = rootEntity;
            _entity = _graph.GetEntity(entityId)!;

            Value = value ?? _entity?.GetPropValue(prop) ?? Dice.Zero;
            Prop = prop;
            Entity = Path != null ? _rootEntity.Name : _entity.Name;
        }

        public override string ToString()
        {
            var parts = new List<string> { Entity };
            if (Path != null)
                parts.AddRange(Path);
            parts.Add(Prop);

            return $"{string.Join('.', parts)} => {Value}";
        }
    }
}
