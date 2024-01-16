using Newtonsoft.Json;
using Rpg.Sys.Components;
using Rpg.Sys.GraphOperations;

namespace Rpg.Sys
{
    public class Graph
    {
        public readonly Add AddOp;
        public readonly Update UpdateOp;
        public readonly Remove RemoveOp;
        public readonly Expire ExpireOp;

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [JsonProperty] public ModdableObject? Context { get; private set; }
        [JsonProperty] public EntityStore Entities { get; private set; }
        [JsonProperty] public ModStore Mods { get; private set; }
        [JsonProperty] public List<Condition> Conditions { get; private set; }

        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public Graph()
        {
            Mods = new ModStore();
            Entities = new EntityStore();
            Conditions = new List<Condition>();

            AddOp = new Add(this);
            RemoveOp = new Remove(this);
            ExpireOp = new Expire(this);
            UpdateOp = new Update(this, ExpireOp, RemoveOp);

            Mods.Initialize(this);
            Entities.Initialize(this);
            
        }

        public void Initialize(ModdableObject context, ModStore? modStore = null, List<Condition>? conditions = null)
        {
            Mods.Clear();
            Entities.Clear();
            Conditions.Clear();

            Entities.RestoreMods = modStore == null;
            AddOp.Execute(context);
            //Entities.Add(context);

            Context = context;

            if (modStore != null)
                Mods.Restore(modStore);

            if (conditions != null)
                AddOp.Execute(conditions.ToArray());
        }

        public string Serialize<T>() where T : ModdableObject
        {
            var state = new GraphState<T>
            {
                Context = Context as T,
                Mods = Mods,
                Conditions = Conditions
            };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            return json;
        }

        public static Graph Deserialize<T>(string json) where T : ModdableObject
        {
            var state = JsonConvert.DeserializeObject<GraphState<T>>(json, JsonSettings)!;
            var graph = new Graph();

            graph.Initialize(state.Context!, state.Mods, state.Conditions);

            return graph;
        }

        public void NewEncounter()
        {
            Turn = 1;
            Mods.UpdateOnTurn(Turn);
        }

        public void EndEncounter()
        {
            Turn = 0;
            Mods.UpdateOnTurn(Turn);
        }

        public void NewTurn()
        {
            Turn++;
            Mods.UpdateOnTurn(Turn);
        }

        public void PrevTurn()
        {
            Turn--;
            Mods.UpdateOnTurn(Turn);
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            Mods.UpdateOnTurn(Turn);
        }
    }
}
