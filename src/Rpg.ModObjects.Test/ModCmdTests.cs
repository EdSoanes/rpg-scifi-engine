using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModCmdTests
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

            Assert.That(entity.Commands.Count(), Is.EqualTo(1));

            var testAction = entity.Commands.Single();
            Assert.That(testAction.CmdName, Is.EqualTo("TestCommand"));
            Assert.That(testAction.EntityId, Is.EqualTo(entity.Id));

            Assert.That(testAction.Args.Count(), Is.EqualTo(2));
            Assert.That(testAction.Args[0].Name, Is.EqualTo("initiator"));
            Assert.That(testAction.Args[0].ArgType, Is.EqualTo(ModCmdArgType.Actor));
            Assert.That(testAction.Args[0].DataType, Is.EqualTo("ModObject"));

            Assert.That(testAction.Args[1].Name, Is.EqualTo("target"));
            Assert.That(testAction.Args[1].ArgType, Is.EqualTo(ModCmdArgType.Any));
            Assert.That(testAction.Args[1].DataType, Is.EqualTo("Int32"));

        }
    }
}
