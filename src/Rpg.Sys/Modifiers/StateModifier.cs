using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class StateModifier : Modifier
    {
        [JsonProperty] public string StateName { get; protected set; }
        [JsonProperty] public Guid ArtifactId { get; protected set; }

        public StateModifier()
        {
            ModifierType = ModifierType.State;
            ModifierAction = ModifierAction.Replace;
        }

        public override void Expire(int turn)
        {
            EndTurn = turn;

            //State modifiers always expire immediately, whether within an encounter or not
            Expiry = ModifierExpiry.Expired;
        }

        public static Modifier Create<TEntity, T1>(Guid artifactId, string stateName, TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
        {
            var mod = _Create<StateModifier, TEntity, T1, TEntity, T1>(null, stateName, dice, null, entity, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifactId;
            mod.StateName = stateName;
            return mod;
        }

        public static Modifier Create<TEntity, T1, T2>(Guid artifactId, string stateName, TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
        {
            var mod = _Create<StateModifier, TEntity, T1, TEntity, T2>(entity, stateName, null, sourceExpr, entity, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifactId;
            mod.StateName = stateName;
            return mod;
        }

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity? entity, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<StateModifier, TEntity, T1, TEntity2, T2>(entity, null, null, sourceExpr, target, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity2, T2>(entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);
    }
}
