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

        public void Initialize(ModdableObject context)
        {
            Mods.Clear();
            Entities.Clear();
            Actions.EndEncounter();
            Entities.Add(context);

            Context = context;
        }

        public string[] Describe() => Entities.Values.OrderBy(x => x.Meta.Path).Select(x => x.ToString()).ToArray();

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(this, JsonSettings);
            return json;
        }

        public static EntityGraph? Deserialize(string json)
        {
            var meta = JsonConvert.DeserializeObject<EntityGraph>(json, JsonSettings)!;
            meta.InitContext();
            return meta;
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
