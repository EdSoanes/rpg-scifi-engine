using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : INotifyPropertyChanged, ILifecycle
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty]
        [TextUI(Ignore = true)]
        public string Id { get; private set; }

        [JsonProperty]
        [TextUI(Ignore = true)]
        public string Archetype { get; set; }

        [JsonProperty] 
        [TextUI(Ignore = true)]
        public string Name { get; set; }

        [JsonProperty] public string[] Archetypes { get; private set; }

        [JsonProperty] public LifecycleExpiry Expiry { get; set; } = LifecycleExpiry.Pending;

        [JsonProperty] internal PropStore PropStore { get; private set; }
        [JsonProperty] internal ModSetStore ModSetStore { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
        {
            Id = this.NewId();
            Archetype = GetType().Name;
            Name = GetType().Name;
            Archetypes = this.GetType().GetArchetypes();

            PropStore = new PropStore(Id);
            ModSetStore = new ModSetStore(Id);
        }

        public void AddMods(params Mod[] mods)
            => Graph!.AddMods(mods);

        public bool AddModSet(ModSet modSet)
            => ModSetStore.Add(modSet);

        public bool IsA(string type) 
            => Archetypes.Contains(type);

        internal ModObjectPropDescription Describe(string prop)
            => new ModObjectPropDescription(Graph!, this, prop);

        protected virtual void OnLifecycleStarting() { }
        public virtual void OnUpdating(RpgGraph graph, TimePoint time) { }

        public void SetExpired(TimePoint currentTime)
        {
        }

        public virtual void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
            PropStore.OnBeforeTime(graph, entity);
            ModSetStore.OnBeforeTime(graph, entity);
        }

        public virtual void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            PropStore.OnBeginningOfTime(graph, entity);
            foreach (var propInfo in RpgReflection.ScanForModdableProperties(this))
            {
                var val = this.PropertyValue(propInfo.Name, out var propExists);
                if (val != null)
                {
                    if (val is Dice dice)
                        this.InitMod(propInfo.Name, dice);
                    else if (val is int i)
                        this.InitMod(propInfo.Name, i);
                }
            }

            OnLifecycleStarting();

            Expiry = LifecycleExpiry.Active;

            ModSetStore.OnBeginningOfTime(graph, entity);
        }

        public virtual LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            ModSetStore.OnStartLifecycle(graph, currentTime);
            PropStore.OnStartLifecycle(graph, currentTime);

            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            ModSetStore.OnUpdateLifecycle(graph, currentTime);
            PropStore.OnUpdateLifecycle(graph, currentTime);

            return Expiry;
        }
    }
}
