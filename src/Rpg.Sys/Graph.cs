using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class Graph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [JsonProperty] public ModdableObject? Context { get; private set; }
        [JsonProperty] public EntityStore Entities { get; private set; }
        [JsonProperty] public ModStore Mods { get; private set; }

        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public Graph()
        {
            Mods = new ModStore();
            Entities = new EntityStore();

            Mods.Initialize(this);
            Entities.Initialize(this);
        }

        public void Initialize(ModdableObject context, ModStore? modStore = null)
        {
            Mods.Clear();
            Entities.Clear();
            Entities.Add(context, modStore == null);

            Context = context;

            if (modStore != null)
                Mods.Restore(modStore);
        }

        public string Serialize<T>() where T : ModdableObject
        {
            var state = new GraphState<T>
            {
                Context = Context as T,
                Mods = Mods
            };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            return json;
        }

        public static Graph Deserialize<T>(string json) where T : ModdableObject
        {
            var state = JsonConvert.DeserializeObject<GraphState<T>>(json, JsonSettings)!;
            var graph = new Graph();

            graph.Initialize(state.Context!, state.Mods);

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
