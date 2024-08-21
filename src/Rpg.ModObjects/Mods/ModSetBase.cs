using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public abstract class ModSetBase
    {
        protected RpgGraph? Graph { get; set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string Name { get; set; }

        [JsonProperty] public bool IsApplied { get; private set; } = true;
        [JsonProperty] public bool IsDisabled { get; private set; }
        public bool IsActive { get => IsApplied && !IsDisabled; }

        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }
        public LifecycleExpiry Expiry { get => Lifecycle.Expiry; protected set { } }

        [JsonConstructor] protected ModSetBase() { }

        public ModSetBase(string ownerId, ILifecycle lifecycle, string name)
        {
            Id = this.NewId();
            Lifecycle = lifecycle;
            Name = name ?? GetType().Name;
            OwnerId = ownerId;
        }

        public virtual void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
            OwnerId ??= entity?.Id;
            Lifecycle.OnBeforeTime(graph);

            Graph!.AddMods([.. Mods]);
        }

        public void AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                if (!Mods.Any(x => x.Id == mod.Id))
                {
                    mod.OwnerId = Id;
                    
                    if (IsApplied) mod.Apply();
                    else mod.Unapply();

                    if (IsDisabled) mod.Disable();
                    else mod.Enable();

                    Mods.Add(mod);
                }
            }

            Graph?.AddMods(mods);
        }

        public void Apply()
            => Graph!.ApplyMods([.. Mods]);

        public void Unapply()
            => Graph!.UnapplyMods([.. Mods]);

        public void Enable()
            => Graph!.EnableMods([.. Mods]);

        public void Disable()
            => Graph!.DisableMods([.. Mods]);
    }
}