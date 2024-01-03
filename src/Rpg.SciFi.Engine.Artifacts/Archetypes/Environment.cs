using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public class Environment : Artifact
    {
        public Environment() 
        {
            Name = nameof(Environment);
            Containers.Add(Container.Environment, new Container());
        }
    }
}
