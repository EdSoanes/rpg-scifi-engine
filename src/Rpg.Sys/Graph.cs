using Newtonsoft.Json;
using Rpg.Sys.Components;
using Rpg.Sys.GraphOperations;
using Rpg.Sys.Moddable;

namespace Rpg.Sys
{
    public class Graph
    {
        public readonly GetOp Get;
        public readonly AddOp Add;
        public readonly UpdateOp Update;
        public readonly RemoveOp Remove;
        public readonly ExpireOp Expire;
        public readonly RestoreOp Restore;
        public readonly NotifyOp Notify;
        public readonly CountOp Count;
        public readonly EvaluateOp Evaluate;
        public readonly DescribeOp Describe;

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        public static Graph Current { get; } = new Graph();

        [JsonProperty] protected ModObject? Context { get; private set; }
        [JsonProperty] protected EntityStore Entities { get; set; }
        [JsonProperty] protected ModStore Mods { get; set; }
        [JsonProperty] protected List<Condition> Conditions { get; set; }

        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public Graph()
        {
            GraphExtensions.RegisterAssembly(GetType().Assembly);

            Mods = new ModStore();
            Entities = new EntityStore();
            Conditions = new List<Condition>();

            Get = new GetOp(this, Mods, Entities, Conditions);
            Add = new AddOp(this, Mods, Entities, Conditions);
            Remove = new RemoveOp(this, Mods, Entities, Conditions);
            Expire = new ExpireOp(this, Mods, Entities, Conditions);
            Update = new UpdateOp(this, Mods, Entities, Conditions);
            Restore = new RestoreOp(this, Mods, Entities, Conditions);
            Notify = new NotifyOp(this);
            Count = new CountOp(this, Mods, Entities, Conditions);
            Evaluate = new EvaluateOp(this, Mods, Entities, Conditions);
            Describe = new DescribeOp(this, Mods, Entities, Conditions);
        }

        public T GetContext<T>() 
            where T : ModObject
                => (T)Context!;

        public void SetContext(ModObject context)
        {
            //Mods.Clear();
            Entities.Clear();
            //Conditions.Clear();

            Add.Entities(context);

            Context = context;
        }

        public string Serialize<T>() where T : ModObject
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

        public static Graph Deserialize<T>(string json) where T : ModObject
        {
            var state = JsonConvert.DeserializeObject<GraphState<T>>(json, JsonSettings)!;
            var graph = new Graph();

            graph.Restore.Entities(state!.Context!);
            graph.Restore.Mods(state!.Mods!);
            graph.Restore.Conditions(state!.Conditions!);
            graph.Context = graph.Get.Entity<T>(state!.Context!.Id);

            return graph;
        }

        public void NewEncounter()
        {
            Turn = 1;
            Update.Conditions();
            Update.Mods();
        }

        public void EndEncounter()
        {
            Turn = 0;
            Update.Conditions();
            Update.Mods();
        }

        public void NewTurn()
        {
            Turn++;
            Update.Conditions();
            Update.Mods();
        }

        public void PrevTurn()
        {
            Turn--;
            Update.Conditions();
            Update.Mods();
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            Update.Conditions();
            Update.Mods();
        }
    }
}
