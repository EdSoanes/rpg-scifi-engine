using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Actions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class EntityGraph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Reuse
        };

        [JsonProperty] public ModdableObject? Context { get; private set; }
        [JsonProperty] public EntityStore Entities { get; private set; }
        [JsonProperty] public ModStore Mods { get; private set; }
        [JsonProperty] public ActionManager Actions { get; private set; }

        public PropEvaluator Evaluator { get; private set; }

        public EntityGraph()
        {
            Mods = new ModStore();
            Entities = new EntityStore();
            Actions = new ActionManager();
            Evaluator = new PropEvaluator();

            InitContext();
        }

        public void Initialize(ModdableObject context, ModStore? modStore = null)
        {
            Mods.Clear();
            Entities.Clear();
            Actions.EndEncounter();
            Entities.Add(context);

            if (modStore != null)
                Mods.Restore(modStore);

            Context = context;
        }

        public string[] Describe() => Entities.Values.OrderBy(x => x.Meta.Path).Select(x => x.ToString()).ToArray();

        public string Serialize<T>() where T : ModdableObject
        {
            var state = new EntityGraphState<T>
            {
                Context = Context as T,
                Mods = Mods
            };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            return json;
        }

        public static EntityGraph? Deserialize<T>(string json) where T : ModdableObject
        {
            var state = JsonConvert.DeserializeObject<EntityGraphState<T>>(json, JsonSettings)!;
            var graph = new EntityGraph();

            graph.Initialize(state.Context!, state.Mods);

            return graph;
        }

        private void InitContext()
        {
            Evaluator.Initialize(this);
            Mods.Initialize(this);
            Entities.Initialize(this);
            Actions.Initialize(this);
        }
    }
}
