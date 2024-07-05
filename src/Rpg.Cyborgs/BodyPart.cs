using Newtonsoft.Json;
using Rpg.ModObjects;

namespace Rpg.Cyborgs
{
    public class BodyPart : RpgComponent
    {
        [JsonProperty] private List<string> Injuries {  get; set; } = new List<string>();

        [JsonConstructor] private BodyPart() { }

        public BodyPart(string entityId, string name)
            : base(entityId, name) { }

        public void AddInjury(string injury)
            => Injuries.Add(injury);

        public void RemoveInjury(string injury)
            => Injuries.Remove(injury);

        public bool HasInjury(string injury)
            => Injuries.Contains(injury);

        public bool HasInjuries()
            => Injuries.Any();

        public void RemoveAllInjuries()
            => Injuries.Clear();
    }
}
