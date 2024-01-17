using Newtonsoft.Json;
using Rpg.Sys.Components;
using Rpg.Sys.GraphOperations;

namespace Rpg.Sys
{
    public class Graph
    {
        public readonly Get GetOp;
        public readonly Add AddOp;
        public readonly Update UpdateOp;
        public readonly Remove RemoveOp;
        public readonly Expire ExpireOp;
        public readonly Restore RestoreOp;
        public readonly Notify NotifyOp;

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [JsonProperty] public ModdableObject? Context { get; private set; }
        [JsonProperty] internal EntityStore Entities { get; set; }
        [JsonProperty] internal ModStore Mods { get; set; }
        [JsonProperty] internal List<Condition> Conditions { get; set; }

        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public Graph()
        {
            Mods = new ModStore();
            Entities = new EntityStore();
            Conditions = new List<Condition>();

            GetOp = new Get(this);
            AddOp = new Add(this);
            RemoveOp = new Remove(this);
            ExpireOp = new Expire(this);
            UpdateOp = new Update(this, ExpireOp, RemoveOp);
            RestoreOp = new Restore(this);
            NotifyOp = new Notify(this);
        }

        public void Initialize(ModdableObject context)
        {
            Mods.Clear();
            Entities.Clear();
            Conditions.Clear();

            AddOp.Entities(context);

            Context = context;
        }

        public string Serialize<T>() where T : ModdableObject
        {
            var state = new GraphState<T>
            {
                Context = Context as T,
                Mods = Mods.SelectMany(x => x.Value.AllModifiers).ToArray(),
                Conditions = Conditions.ToArray(),
                Turn = Turn
            };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            return json;
        }

        public static Graph Deserialize<T>(string json) where T : ModdableObject
        {
            var state = JsonConvert.DeserializeObject<GraphState<T>>(json, JsonSettings)!;
            var graph = new Graph();

            graph.RestoreOp.Entities(state!.Context!);
            graph.RestoreOp.Mods(state!.Mods!);
            graph.RestoreOp.Conditions(state!.Conditions!);

            return graph;
        }

        public void NewEncounter()
        {
            Turn = 1;
            UpdateOp.Conditions();
            UpdateOp.Mods();
        }

        public void EndEncounter()
        {
            Turn = 0;
            UpdateOp.Conditions();
            UpdateOp.Mods();
        }

        public void NewTurn()
        {
            Turn++;
            UpdateOp.Conditions();
            UpdateOp.Mods();
        }

        public void PrevTurn()
        {
            Turn--;
            UpdateOp.Conditions();
            UpdateOp.Mods();
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            UpdateOp.Conditions();
            UpdateOp.Mods();
        }
    }
}
