namespace Rpg.SciFi.Engine.Artifacts
{
    public class MetaAction
    {
        public string Name { get; set; }
        public List<MetaActionInput> Inputs { get; set; } = new List<MetaActionInput>();
    }
}
