namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public class Game : Entity
    {
        public Character Character { get; set; }
        public Environment Environment { get; set; } = new Environment();
        public List<Character> Players { get; set; } = new List<Character>();
    }
}
