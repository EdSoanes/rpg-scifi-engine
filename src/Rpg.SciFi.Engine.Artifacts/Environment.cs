using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Environment : Artifact
    {
        public Environment() 
        {
            Name = nameof(Environment);
        }

        public Contains Contains { get; set; } = new Contains();
    }
}
