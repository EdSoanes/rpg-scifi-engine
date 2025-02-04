namespace Rpg.Experimental
{
    public interface IRpgSystem
    {
        string Identifier { get;  }
        string[]? Namespaces { get; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
    }
}
