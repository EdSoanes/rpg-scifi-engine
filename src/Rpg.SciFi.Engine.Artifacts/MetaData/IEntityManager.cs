using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public interface IEntityManager : IPropEvaluator
    {
        Entity? Root { get; }
        void Add(Entity entity);
        void AddRange(IEnumerable<Entity> entities);
        Entity? Get(Guid id);
        Entity? Get(PropRef? moddableProperty);
        bool Remove(Entity entity);
        void AddMod(Modifier mod);
        void AddMods(params Modifier[] mods);

        MetaModdableProperty? GetModProp<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression) where TEntity : Entity;
        List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression) where TEntity : Entity;

        void ClearMods(Guid entityId);

        bool RemoveMods(int currentTurn);
        bool RemoveMods(Guid entityId, string prop);
        bool RemoveMods(Entity entity, string prop);
        bool RemoveMods<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression);

        int CurrentTurn { get; }
        void StartEncounter();
        void EndEncounter();
        void IncrementTurn();
        Turns.Action CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost);
    }
}
