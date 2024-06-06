using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : INotifyPropertyChanged, IGraphEvents
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string[] Is { get; private set; }
        [JsonProperty] internal PropStore PropStore { get; private set; }
        [JsonProperty] internal ModSetStore ModSetStore { get; private set; }
        [JsonProperty] internal ModStateStore StateStore { get; private set; }
        [JsonProperty] internal bool IsCreated { get; set; }
        [JsonProperty] internal RpgActionStore CmdStore { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
        {
            Id = this.NewId();
            Name = GetType().Name;
            Is = this.GetBaseTypes();

            PropStore = new PropStore(Id);
            ModSetStore = new ModSetStore(Id);
            StateStore = new ModStateStore(Id);
            CmdStore = new RpgActionStore(Id);
        }

        public void AddMods(params Mod[] mods)
            => Graph!.AddMods(mods);

        internal bool AddModSet(ModSet modSet)
            => ModSetStore.Add(modSet);

        public RpgAction? GetCommand(string commandName)
            => CmdStore.Get().FirstOrDefault(x => x.ActionName == commandName);

        public RpgAction[] GetCommands()
            => CmdStore.Get();

        public string[] StateNames { get => StateStore.StateNames; }
        public string[] ActiveStateNames { get => StateStore.ActiveStateNames; }

        public string? GetStateInstanceName(string state)
            => StateStore[state]?.InstanceName;

        public bool IsStateActive(string state)
            => StateStore[state]?.IsActive() ?? false;

        public bool ActivateState(string state)
            => StateStore.SetActive(state);

        public bool DeactivateState(string state)
            => StateStore.SetInactive(state);

        public bool IsA(string type) => Is.Contains(type);

        internal ModObjectPropDescription Describe(string prop)
            => new ModObjectPropDescription(Graph!, this, prop);

        public void OnGraphCreating(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
        }

        public void OnObjectsCreating()
        {
            if (!IsCreated)
            {
                var states = this.CreateModStates();
                StateStore.Add(states);

                var cmds = this.CreateActions();
                var cmdStates = cmds.Select(x => x.State).ToArray();
                CmdStore.Add(cmds);
                StateStore.Add(cmdStates);

                foreach (var propInfo in this.GetModdableProperties())
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

                OnCreating();
                IsCreated = true;
            }
        }

        protected virtual void OnCreating() { }
        public virtual void OnUpdating(RpgGraph graph, TimePoint time) { }
    }
}
