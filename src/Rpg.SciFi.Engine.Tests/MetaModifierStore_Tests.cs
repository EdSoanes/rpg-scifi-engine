using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaModifierStore_Tests
    {
        private EntityManager<Character> SetupMeta()
        {
            var meta = new EntityManager<Character>();
            var character = new Character();
            meta.Initialize(character);
            meta.StartEncounter();

            var mod1 = TurnModifier.Create("Boost", 3, character, x => x.Size);
            var mod2 = TurnModifier.Create("Boost", 3, character, x => x.Speed);

            meta.AddMods(mod1, mod2);
            
            return meta;
        }

        [TestMethod]
        public void Speed_2TurnMods_IncrementTurn_EnsureValues()
        {
            var meta = SetupMeta();
            var character = meta.Context;

            Assert.IsNotNull(meta);
            Assert.IsNotNull(character);

            Assert.AreEqual(1, meta.CurrentTurn);
            Assert.AreEqual(3, character.Speed);

            meta.AddMods(
                TurnModifier.Create("Buff", "10", character, x => x.Speed)
            );

            Assert.AreEqual(13, character.Speed);

            meta.IncrementTurn();

            Assert.AreEqual(2, meta.CurrentTurn);
            Assert.AreEqual(0, character.Speed);
        }


        [TestMethod]
        public void Size_2TurnMods_EnsureValues()
        {
            var meta = SetupMeta();
            var character = meta.Context;

            Assert.IsNotNull(meta);
            Assert.IsNotNull(character);

            Assert.AreEqual(4, character.Size);
 
            meta.AddMods(
                TurnModifier.Create("Buff", "4", character, x => x.Size)
            );

            Assert.AreEqual(8, character.Size);
        }

        [TestMethod]
        public void SpeedAndSize_2TurnMods_Serialize()
        {
            var meta = SetupMeta();
            var character = meta.Context;

            Assert.IsNotNull(meta);
            Assert.IsNotNull(character);

            var json = meta.Serialize();
            var meta2 = EntityManager<Character>.Deserialize(json);
            var character2 = meta.Context;

            Assert.IsNotNull(meta2);
            Assert.IsNotNull(character2);

            var sizeMods = meta2.GetMods(character2, x => x.Size);
            Assert.IsNotNull(sizeMods);
            Assert.AreEqual(2, sizeMods.Count);

            //Assert base mod
            var baseSizeMod = sizeMods.Single(x => x.ModifierType == ModifierType.Base);
            Assert.AreEqual(character2.Id, baseSizeMod.Target.Id);
            Assert.AreEqual(nameof(character2.BaseSize), baseSizeMod.Source.Prop);
            Assert.AreEqual(nameof(character2.Size), baseSizeMod.Target.Prop);
            Assert.IsFalse(baseSizeMod.CanBeCleared());
            Assert.IsFalse(baseSizeMod.ShouldBeRemoved(1));

            //Assert boost mod
            var sizeBoostMod = sizeMods.Single(x => x.ModifierType == ModifierType.Transient);
            Assert.AreEqual("Boost", sizeBoostMod.Name);
            Assert.AreEqual(character2.Id, sizeBoostMod.Target.Id);
            Assert.AreEqual(nameof(character2.Size), sizeBoostMod.Target.Prop);

            var speedMods = meta.GetMods(character2, x => x.Speed);
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(2, speedMods.Count);

            //Assert base mod
            var baseSpeedMod = speedMods.Single(x => x.ModifierType == ModifierType.Base);
            Assert.AreEqual(character2.Id, baseSpeedMod.Target.Id);
            Assert.AreEqual(nameof(character2.BaseSpeed), baseSpeedMod.Source.Prop);
            Assert.AreEqual(nameof(character2.Speed), baseSpeedMod.Target.Prop);
            Assert.IsFalse(baseSpeedMod.CanBeCleared());
            Assert.IsFalse(baseSpeedMod.ShouldBeRemoved(1));

            //Assert boost mod
            var speedBoostMod = speedMods.Single(x => x.ModifierType == ModifierType.Transient);
            Assert.AreEqual("Boost", speedBoostMod.Name);
            Assert.AreEqual(character2.Id, speedBoostMod.Target.Id);
            Assert.AreEqual(nameof(character2.Speed), speedBoostMod.Target.Prop);
        }
    }
}
