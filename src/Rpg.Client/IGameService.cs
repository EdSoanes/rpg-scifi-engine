using Rpg.Sys;
using Rpg.Sys.Archetypes;

namespace Rpg.Client
{
    public interface IGameService<T> where T : ModdableObject
    {
        Graph Graph { get; }
        ActorTemplate[] Humans { get; }

        void SetContext(T context);
    }
}