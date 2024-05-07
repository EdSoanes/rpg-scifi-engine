using Newtonsoft.Json;
using Rpg.Sys.GraphOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Moddable
{
    public class ModGraph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [JsonProperty] protected ModObject? Context { get; private set; }
        [JsonProperty] Dictionary<Guid, ModObject> ModObjectStore = new Dictionary<Guid, ModObject>();
        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public static ModGraph Current { get; } = new ModGraph();
        public EvaluateOp Evaluate { get; } = new EvaluateOp();
        public ModObject GetContext() 
            => Context!;

        public T GetContext<T>()
            where T : ModObject
                => (T)Context!;

        public void SetContext(ModObject context)
        {
            Context = context;
            ModObjectStore.Clear();
            foreach (var entity in context.Traverse())
            {
                if (!ModObjectStore.ContainsKey(entity.Id))
                    ModObjectStore.Add(entity.Id, entity);
            }
        }

        public T? GetEntity<T>(Guid? entityId)
            where T : ModObject
                => entityId != null && ModObjectStore.ContainsKey(entityId.Value)
                    ? ModObjectStore[entityId.Value] as T
                    : null;

        public List<ModObjectPropRef> PropsAffectedBy(Guid entityId, string prop)
        {
            var res = new List<ModObjectPropRef>();

            var propsAffectedBy = ModObjectStore.Values
                .SelectMany(x => x.PropStore)
                .Where(x => x.IsAffectedBy(entityId, prop))
                .Select(x => new ModObjectPropRef(x.EntityId, x.Prop));

            res.Merge(propsAffectedBy);

            foreach (var affect in propsAffectedBy)
            {
                var parentAffects = PropsAffectedBy(affect.EntityId, affect.Prop);
                res.Merge(parentAffects);
            }

            return res;
        }

        public List<ModObjectPropRef> AffectedByProps(Guid entityId, string prop)
        {
            var res = new List<ModObjectPropRef>();

            var affectedByProps = GetEntity<ModObject>(entityId)
                !.PropStore[prop]
                !.AffectedBy();

            foreach (var affectedByProp in affectedByProps)
            {
                var childAffectedByProps = AffectedByProps(affectedByProp.EntityId, affectedByProp.Prop);
                res.Merge(childAffectedByProps);
            }

            res.Merge(affectedByProps);

            return res;
        }

        public void NewEncounter()
        {
            Turn = 1;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void EndEncounter()
        {
            Turn = 0;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void NewTurn()
        {
            Turn++;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void PrevTurn()
        {
            Turn--;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }
    }
}
