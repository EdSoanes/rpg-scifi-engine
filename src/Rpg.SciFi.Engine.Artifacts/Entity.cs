using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();

        public MetaEntity? MetaData()
        {
            return Meta.MetaEntities?.SingleOrDefault(x => x.Id == Id && x.Type == GetType().Name);
        }

        public List<Modifier> Mods(string prop)
        {
            var mods = MetaData()
                ?.Mods.Get(prop)
                ?? new List<Modifier>();

            return mods;
        }

        public void ClearMods() => MetaData()?.Mods.Clear();

        public Dice Evaluate(string prop)
        {
            Dice dice = Dice.Sum(Mods(prop).Select(x => x.Evaluate()));
            return dice;
        }

        public int Resolve(string prop)
        {
            //TODO: We need to store the result so we don't get different resolutions each time this is called.
            Dice dice = Evaluate(prop);
            return dice.Roll();
        }

        public virtual string[] Describe(string prop)
        {
            var description = MetaData()
                ?.Mods.Describe(prop)
                ?? new string[0];

            return description;
        }
    }
}
