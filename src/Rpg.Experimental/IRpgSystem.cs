namespace Rpg.Experimental
{
    public interface IRpgSystem
    {
        string Identifier { get;  }
        string[]? Namespaces { get; }
        string Name { get; }
        string Version { get; }
        string Description { get; }

        /// <summary>
        /// The names of the time events of the system that happen outside of turns, e.g. "Sunrise", "Sunset".
        /// The built in "TimePasses" event always exists and does not need to be listed.
        /// </summary>
        string[] TimeEvents { get => []; }
    }
}
