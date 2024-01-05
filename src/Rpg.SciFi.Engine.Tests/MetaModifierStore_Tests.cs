using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaModifierStore_Tests
    {
        private EntityGraph SetupGraph()
        {
            var graph = new EntityGraph();
            var character = new Character();
            graph.Initialize(character);
            graph.Actions.StartEncounter();

            var mod1 = TurnModifier.Create(character, "Boost", 3, x => x.Size);
            var mod2 = TurnModifier.Create(character, "Boost", 3, x => x.CurrentSpeed);

            graph.Mods.Add(mod1, mod2);
            
            return graph;
        }

        [TestMethod]
        public void Speed_2TurnMods_IncrementTurn_EnsureValues()
        {
            var meta = SetupGraph();
            var character = meta.Context as Character;

            Assert.IsNotNull(meta);
            Assert.IsNotNull(character);

            Assert.AreEqual(1, meta.Actions.Current);
            Assert.AreEqual(3, character.CurrentSpeed);

            meta.Mods.Add(
                TurnModifier.Create(character, "Buff", "10", x => x.CurrentSpeed)
            );

            Assert.AreEqual(13, character.CurrentSpeed);

            meta.Actions.Increment();

            Assert.AreEqual(2, meta.Actions.Current);
            Assert.AreEqual(0, character.CurrentSpeed);
        }


        [TestMethod]
        public void Size_2TurnMods_EnsureValues()
        {
            var meta = SetupGraph();
            var character = meta.Context as Character;

            Assert.IsNotNull(meta);
            Assert.IsNotNull(character);

            Assert.AreEqual(4, character.Size);
 
            meta.Mods.Add(
                TurnModifier.Create(character, "Buff", "4", x => x.Size)
            );

            Assert.AreEqual(8, character.Size);
        }

        [TestMethod]
        public void SpeedAndSize_2TurnMods_Serialize()
        {
            var graph = SetupGraph();
            var character = graph.Context;

            Assert.IsNotNull(graph);
            Assert.IsNotNull(character);

            var json = graph.Serialize<Character>();
            var graph2 = EntityGraph.Deserialize<Character>(json);
            var character2 = graph.Context as Character;

            Assert.IsNotNull(graph2);
            Assert.IsNotNull(character2);

            var sizeMods = graph2.Mods.GetMods(character2, x => x.Size);
            Assert.IsNotNull(sizeMods);
            Assert.AreEqual(2, sizeMods.Count);

            //Assert base mod
            var baseSizeMod = sizeMods.Single(x => x.ModifierType == ModifierType.Base);
            Assert.AreEqual(character2.Id, baseSizeMod.Target.Id);
            Assert.AreEqual(nameof(character2.Size), baseSizeMod.Target.Prop);
            Assert.IsFalse(baseSizeMod.CanBeCleared());
            Assert.IsFalse(baseSizeMod.ShouldBeRemoved(1));

            //Assert boost mod
            var sizeBoostMod = sizeMods.Single(x => x.ModifierType == ModifierType.Transient);
            Assert.AreEqual("Boost", sizeBoostMod.Name);
            Assert.AreEqual(character2.Id, sizeBoostMod.Target.Id);
            Assert.AreEqual(nameof(character2.Size), sizeBoostMod.Target.Prop);

            var speedMods = graph.Mods.GetMods(character2, x => x.CurrentSpeed);
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(2, speedMods.Count);

            //Assert base mod
            var baseSpeedMod = speedMods.SingleOrDefault(x => x.ModifierType == ModifierType.Base);

            //Assert boost mod
            var speedBoostMod = speedMods.Single(x => x.ModifierType == ModifierType.Transient);
            Assert.AreEqual("Boost", speedBoostMod.Name);
            Assert.AreEqual(character2.Id, speedBoostMod.Target.Id);
            Assert.AreEqual(nameof(character2.CurrentSpeed), speedBoostMod.Target.Prop);
        }
    }
}
