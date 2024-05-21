using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Actions
{
    public abstract class ModCmd<T>
        where T : ModObject
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string CmdName { get; private set; }
        [JsonProperty] public ModCmdArg[] Args { get; private set; } = new ModCmdArg[0];

        public ModCmd(T entity, string cmdName)
        {
            EntityId = entity.Id;
            CmdName = cmdName;
        }

        public bool CanRun() => true;

        public ModSet<T> Initiate<TI>(TI initiator)
            where TI : ModObject
        {
            var setupModSet = new ModSet<T>(CmdName);

            //Find a method called OnInitiate in the subclass with params that match
            OnInitiate(setupModSet, initiator);
            return setupModSet;
        }

        protected abstract void OnInitiate<TI>(ModSet<T> modSet, TI initiator)
            where TI : ModObject;

        public virtual ModSet<T> GetOutcome<TI>(int roll, TI initiator)
            where TI : ModObject
        {
            return new ModSet<T>($"{CmdName}.Outcome");
        }
    
        public Dice? DiceRoll<TI>(ModGraph graph, TI initiator)
            where TI : ModObject
        {
            var entity = graph.GetEntity<T>(EntityId)!;
            return entity.GetPropValue(CmdName);
        }

        public ModObjectPropDescription DescribeDiceRoll(ModGraph graph)
        {
            var entity = graph.GetEntity<T>(EntityId)!;
            return entity.Describe(CmdName);
        }

    }
}
