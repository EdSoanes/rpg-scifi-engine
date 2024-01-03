namespace Rpg.SciFi.Engine.Artifacts
{
    public class MetaAbility
    {
        public string Name { get; set; }
        public List<MetaAbilityInput> Inputs { get; set; } = new List<MetaAbilityInput>();
    }
}
