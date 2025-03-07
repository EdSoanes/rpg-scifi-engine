using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public abstract class RpgObject : RpgLifecycleObject
    {
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public string[] Archetypes { get; private set; }
        [JsonProperty] public string Name { get; protected set; }

        public RpgObject()
            : base()
        {
            Archetype = GetType().Name;
            Archetypes = RpgTypeUtilities.GetArchetypes(GetType());
        }

        public RpgObject(string name)
            : this()
        {
            Name = name;
        }

        public RpgObject(string ownerId, bool syncToOwner)
            : base(ownerId, syncToOwner)
        {
            Archetype = GetType().Name;
            Archetypes = RpgTypeUtilities.GetArchetypes(GetType());
        }

        public RpgObject(string name, string ownerId, bool syncToOwner)
            : base(ownerId, syncToOwner)
        {
            Archetype = GetType().Name;
            Archetypes = RpgTypeUtilities.GetArchetypes(GetType());
        }

        public RpgObject(string name, string ownerId, TimePoint start, TimePoint end)
            : base(ownerId, start, end)
        {
            Archetype = GetType().Name;
            Archetypes = RpgTypeUtilities.GetArchetypes(GetType());
        }

        public bool IsA(string type)
            => Archetypes.Contains(type);

        public Mod CreateStateActivation(string stateName, int startsIn, int duration, bool applied)
            => CreateStateMod(stateName).Lifespan(startsIn, duration, applied);

        public Mod CreateStateActivation(string stateName, int duration, bool applied)
            => CreateStateMod(stateName).Lifespan(0, duration, applied);

        public Mod CreateStateActivation(string stateName, TimePoint start, TimePoint end, bool applied)
            => CreateStateMod(stateName).Lifespan(start, end.Count, applied);

        private Mod CreateStateMod(string stateName)
            => new Standard()
                .SetTarget(Id, RpgState.StatePropName(stateName))
                .SetSource(1);

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            var states = graph.GetObjectStates(Id);

            foreach (var state in states)
                graph.CreateVirtualProperty(Id, RpgState.StatePropName(state.Name ?? state.GetType().Name), nameof(Int32), false, 0);

            base.OnCreating(graph, owner);
        }

        public virtual RpgObject? ResolvePropertyNameToObject(RpgGraph graph, string prop)
        {
            RpgObject? obj = prop switch
            {
                ActionReservedArgs.Owner => graph.GetObject(OwnerId),
                ActionReservedArgs.Actor => (graph as RpgCharacterSheet)?.Actor,
                ActionReservedArgs.Context => graph.Context,
                _ => null
            };

            return obj;
        }

        public override string ToString()
        {
            return $"{Archetype} {base.ToString()} {Name}".Trim();
        }
    }
}
