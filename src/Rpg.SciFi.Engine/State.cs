namespace Rpg.SciFi.Engine
{
    public class State
    {
        public virtual string Name { get; set; } = string.Empty;

        public virtual string Description { get; set; } = string.Empty;

        public virtual Condition[] Conditions { get; set; } = new Condition[0];

    }
}
