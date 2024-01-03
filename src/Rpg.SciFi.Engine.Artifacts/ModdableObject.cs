using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class ModdableObject : INotifyPropertyChanged
    {
        protected EntityGraph? Graph;

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public MetaEntity Meta { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModdableObject()
        {
            Name = GetType().Name;
            Meta = MetaEntity.Create(this);
        }

        public Dice Evaluate([CallerMemberName] string prop = "") => Graph?.Evaluator?.Evaluate(Id, prop) ?? 0;
        public int Resolve([CallerMemberName] string prop = "") => Graph?.Evaluator?.Evaluate(Id, prop).Roll() ?? 0;
        public string[] Describe(string prop) => Graph?.Evaluator?.Describe(this, prop, true) ?? new string[0];

        internal void PropChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void Initialize(EntityGraph entityGraph)
        {
            Graph = entityGraph;  
        }

        public virtual Modifier[] Setup() => new Modifier[0];
    }
}
