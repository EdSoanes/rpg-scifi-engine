using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaModifierStore_Tests
    {
        [TestMethod]
        public void Add_2BaseMods_EnsureStored()
        {
            var character = new Character();

            var mod1 = character.Mod(x => x.BaseSize, x => x.Size).IsBase();
            var mod2 = character.Mod(x => x.BaseSpeed, x => x.Speed).IsBase();
            var store = new MetaModifierStore
            {
                mod1,
                mod2
            };

            Assert.AreEqual(2, store.Count);

            var sizeMods = store.Get(character.Id, nameof(character.Size));
            Assert.IsNotNull(sizeMods);
            Assert.AreEqual(1, sizeMods.Count);
            Assert.AreEqual(character.Id, sizeMods[0].Source?.Id);
            Assert.AreEqual(character.Id, sizeMods[0].Target.Id);
            Assert.AreEqual(nameof(character.BaseSize), sizeMods[0].Source?.Prop);
            Assert.AreEqual(nameof(character.Size), sizeMods[0].Target?.Prop);

            Assert.AreEqual(mod1.Id, sizeMods[0].Id);
            Assert.IsFalse(sizeMods[0].CanBeCleared());
            Assert.IsFalse(sizeMods[0].ShouldBeRemoved(1));

            var speedMods = store.Get(character.Id, nameof(character.Speed));
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(1, speedMods.Count);
            Assert.AreEqual(character.Id, speedMods[0].Source?.Id);
            Assert.AreEqual(character.Id, speedMods[0].Target.Id);
            Assert.AreEqual(nameof(character.BaseSpeed), speedMods[0].Source?.Prop);
            Assert.AreEqual(nameof(character.Speed), speedMods[0].Target?.Prop);

            Assert.AreEqual(mod2.Id, speedMods[0].Id);
            Assert.IsFalse(speedMods[0].CanBeCleared());
            Assert.IsFalse(speedMods[0].ShouldBeRemoved(1));

            var baseSizeMods = store.Get(character.Id, nameof(character.BaseSize));
            Assert.IsNull(baseSizeMods);

            var baseSpeedMods = store.Get(character.Id, nameof(character.BaseSpeed));
            Assert.IsNull(baseSpeedMods);
        }

        [TestMethod]
        public void Add_ManyModTypes_ModRemove()
        {
            var character = new Character();

            var mod1 = character.Mod(x => x.BaseSpeed, x => x.Speed).IsBase();
            var mod2 = character.Mod("Custom", "3", x => x.Speed).IsCustom();
            var mod3 = character.Mod("Boost", "2", x => x.Speed).UntilTurn(3);
            var mod4 = character.Mod("Boost", "2", x => x.Speed).UntilEncounterEnds();

            var store = new MetaModifierStore
            {
                mod1,
                mod2,
                mod3,
                mod4
            };

            Assert.AreEqual(4, store.Count);

            var speedMods = store.Get(character.Id, nameof(character.Speed));
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(4, speedMods.Count);

            Assert.IsFalse(store.Remove(1));
            Assert.IsFalse(store.Remove(2));
            Assert.IsTrue(store.Remove(3));

            speedMods = store.Get(character.Id, nameof(character.Speed));
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(3, speedMods.Count);
            Assert.IsFalse(store.Contains(mod3));

            Assert.IsFalse(store.Remove(1000000));
            Assert.IsTrue(store.Remove(0));
        }

        [TestMethod]
        public void Add_2BaseMods_Serialize()
        {
            var character = new Character();

            var mod1 = character.Mod(x => x.BaseSize, x => x.Size).IsBase();
            var mod2 = character.Mod(x => x.BaseSpeed, x => x.Speed).IsBase();
            var srcStore = new MetaModifierStore
            {
                mod1,
                mod2
            };

            var json = JsonConvert.SerializeObject(srcStore, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            var store = JsonConvert.DeserializeObject<MetaModifierStore>(json);

            Assert.IsNotNull(store);
            Assert.AreEqual(2, store.Count);

            var sizeMods = store.Get(character.Id, nameof(character.Size));
            Assert.IsNotNull(sizeMods);
            Assert.AreEqual(1, sizeMods.Count);
            Assert.AreEqual(character.Id, sizeMods[0].Source?.Id);
            Assert.AreEqual(character.Id, sizeMods[0].Target.Id);
            Assert.AreEqual(nameof(character.BaseSize), sizeMods[0].Source?.Prop);
            Assert.AreEqual(nameof(character.Size), sizeMods[0].Target?.Prop);

            Assert.AreEqual(mod1.Id, sizeMods[0].Id);
            Assert.IsFalse(sizeMods[0].CanBeCleared());
            Assert.IsFalse(sizeMods[0].ShouldBeRemoved(1));

            var speedMods = store.Get(character.Id, nameof(character.Speed));
            Assert.IsNotNull(speedMods);
            Assert.AreEqual(1, speedMods.Count);
            Assert.AreEqual(character.Id, speedMods[0].Source?.Id);
            Assert.AreEqual(character.Id, speedMods[0].Target.Id);
            Assert.AreEqual(nameof(character.BaseSpeed), speedMods[0].Source?.Prop);
            Assert.AreEqual(nameof(character.Speed), speedMods[0].Target?.Prop);

            Assert.AreEqual(mod2.Id, speedMods[0].Id);
            Assert.IsFalse(speedMods[0].CanBeCleared());
            Assert.IsFalse(speedMods[0].ShouldBeRemoved(1));

            var baseSizeMods = store.Get(character.Id, nameof(character.BaseSize));
            Assert.IsNull(baseSizeMods);

            var baseSpeedMods = store.Get(character.Id, nameof(character.BaseSpeed));
            Assert.IsNull(baseSpeedMods);
        }
    }
}
