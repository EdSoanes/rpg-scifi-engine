using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class EntityManager<T> where T : ModdableObject
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Reuse
        };

        [JsonProperty] public T? Context { get; private set; }
        [JsonProperty] public EntityStore Entities { get; private set; }
        [JsonProperty] public ModStore Mods { get; private set; }
        [JsonProperty] public TurnManager Turns { get; private set; }

        public PropEvaluator Evaluator { get; private set; }

        public EntityManager()
        {
            Mods = new ModStore();
            Entities = new EntityStore();
            Turns = new TurnManager();
            Evaluator = new PropEvaluator();

            InitContext();
        }

        public void Initialize(T context)
        {
            Mods.Clear();
            Entities.Clear();
            Turns.EndEncounter();

            Context = context;

            Entities.Add(Context);
        }

        public string[] Describe() => Entities.Values.OrderBy(x => x.MetaData.Path).Select(x => x.ToString()).ToArray();

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(this, JsonSettings);
            return json;
        }

        public static EntityManager<T>? Deserialize(string json)
        {
            var meta = JsonConvert.DeserializeObject<EntityManager<T>>(json, JsonSettings)!;
            meta.InitContext();
            meta.Context = meta.Entities.Get(meta.Context!.Id) as T;
            return meta;
        }

        private void InitContext()
        {
            Evaluator.Initialize(Mods, Entities);
            Mods.Initialize(Entities, Evaluator);
            Entities.Initialize(Mods, Evaluator, Turns);
            Turns.Initialize(Mods, Entities);
        }
    }
}
