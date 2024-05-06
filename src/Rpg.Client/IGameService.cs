using Rpg.Sys;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Moddable;

namespace Rpg.Client
{
    public interface IGameService<T> where T : ModObject
    {
        Graph Graph { get; }
        ActorTemplate[] Humans { get; }

        void SetContext(T context);
    }
}