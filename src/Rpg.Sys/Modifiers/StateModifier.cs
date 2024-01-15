using Newtonsoft.Json;
using Rpg.Sys.Archetypes;
using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    //TODO: State manager for:
    // State groups where only one can be active (E.g, FireMode = (Single, Burst, Grenade, etc...)
    // Permitted States. E.g. Scanning state only allowed when device is PoweredUp
    // Conditional states. Can only be active when certain conditions are met. E.g. PoweredUp only when power supply > threshold
    public class StateModifier : Modifier
    {
        [JsonProperty] public Guid ArtifactId { get; protected set; }

        public StateModifier()
        {
            ModifierType = ModifierType.State;
            ModifierAction = ModifierAction.Replace;
        }

        public static Modifier Create<TArtifact, T1>(string stateName, TArtifact artifact, Dice dice, Expression<Func<TArtifact, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TArtifact : Artifact
        {
            var mod = _Create<StateModifier, TArtifact, T1, TArtifact, T1>(null, stateName, dice, null, artifact, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifact.Id;
            return mod;
        }

        public static Modifier Create<TArtifact, T1, T2>(string stateName, TArtifact artifact, Expression<Func<TArtifact, T1>> sourceExpr, Expression<Func<TArtifact, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TArtifact : Artifact
        {
            var mod = _Create<StateModifier, TArtifact, T1, TArtifact, T2>(artifact, stateName, null, sourceExpr, artifact, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifact.Id;
            return mod;
        }

        public static Modifier Create<TEntity, T1>(string stateName, Guid artifactId, TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
        {
            var mod = _Create<StateModifier, TEntity, T1, TEntity, T1>(null, stateName, dice, null, entity, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifactId;
            return mod;
        }

        public static Modifier Create<TEntity, T1, T2>(string stateName, Guid artifactId, TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
        {
            var mod = _Create<StateModifier, TEntity, T1, TEntity, T2>(entity, stateName, null, sourceExpr, entity, targetExpr, diceCalcExpr);
            mod.ArtifactId = artifactId;
            return mod;
        }

        public static Modifier Create<TEntity, T1, TEntity2, T2>(string stateName, TEntity? entity, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<StateModifier, TEntity, T1, TEntity2, T2>(entity, stateName, null, sourceExpr, target, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(string stateName, TEntity? entity, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity2, T2>(entity, stateName, dice, sourceExpr, target, targetExpr, diceCalcExpr);
    }
}
