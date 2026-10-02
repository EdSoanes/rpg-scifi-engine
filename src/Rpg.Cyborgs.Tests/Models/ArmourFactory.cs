namespace Rpg.Cyborgs.Tests.Models
{
    internal static class ArmourFactory
    {
        public static ArmourTemplate VestTemplate
        {
            get => new ArmourTemplate
            {
                Name = "Vest",
                ArmourRating = 3,
                DefenceModifier = 0
            };
        }
    }
}
