using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public interface IPropEvaluator
    {
        Dice Evaluate(IEnumerable<Modifier> mods);
        Dice Evaluate(Guid entityId, string prop);
        Dice Evaluate<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression);

        string[] Describe(Guid id, string prop, bool addEntityInfo = false);
        string[] Describe(Modifier mod, bool addEntityInfo = false);
    }
}
