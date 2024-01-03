using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class ModdableObject : INotifyPropertyChanged
    {
        protected PropEvaluator? Evaluator;
        protected ModStore? ModStore;
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }

        [JsonProperty] public MetaEntity MetaData { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ModdableObject()
        {
            Name = GetType().Name;
            MetaData = this.GetMeta();
        }

        public Dice Evaluate([CallerMemberName] string prop = "") => Evaluator?.Evaluate(Id, prop) ?? 0;
        public int Resolve([CallerMemberName] string prop = "") => Evaluator?.Evaluate(Id, prop).Roll() ?? 0;
        public string[] Describe(string prop) => Evaluator?.Describe(this, prop, true) ?? new string[0];

        internal void PropChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void Initialize(ModStore modStore, PropEvaluator evaluator)
        {
            ModStore = modStore;
            Evaluator = evaluator;   
        }

        public virtual Modifier[] Setup() => new Modifier[0];
    }

    public static class ModdableObjectExtensions
    {
        public static string[] Describe<T, TResult>(this T entity, Expression<Func<T, TResult>> expression)
            where T : ModdableObject
        {
            var moddableProperty = PropRef.FromPath(entity, expression);
            return entity.Describe(moddableProperty.Prop);
        }
    }

}
