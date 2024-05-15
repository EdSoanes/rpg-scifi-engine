using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModObjectActionTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void ModdableEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Actions.Count(), Is.EqualTo(1));

            var testAction = entity.Actions.Single();
            Assert.That(testAction.ActionName, Is.EqualTo("TestAction"));
            Assert.That(testAction.EntityId, Is.EqualTo(entity.Id));
        }
    }
}
