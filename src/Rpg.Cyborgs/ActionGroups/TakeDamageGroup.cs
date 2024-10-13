using Rpg.Cyborgs.Actions;
using Rpg.ModObjects.Actions;

namespace Rpg.Cyborgs.ActionGroups
{
    public class TakeDamageGroup : ActivityTemplate
    {
        public TakeDamageGroup() 
        {
            Name = "Take Damage";
            Add(nameof(Actor), nameof(MeleeParry));
            Add(nameof(Actor), nameof(ArmourCheck));
            Add(nameof(Actor), nameof(TakeDamage), false);
        }
    }
}
