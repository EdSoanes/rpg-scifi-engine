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

            var testCmd = entity.Commands.Single();
            Assert.That(testCmd.CommandName, Is.EqualTo("TestCommand"));
            Assert.That(testCmd.EntityId, Is.EqualTo(entity.Id));

            Assert.That(testCmd.Args.Count(), Is.EqualTo(2));
            Assert.That(testCmd.Args[0].Name, Is.EqualTo("initiator"));
            Assert.That(testCmd.Args[0].ArgType, Is.EqualTo(ModCmdArgType.Actor));
            Assert.That(testCmd.Args[0].DataType, Is.EqualTo("Rpg.ModObjects.ModObject"));

            Assert.That(testCmd.Args[1].Name, Is.EqualTo("target"));
            Assert.That(testCmd.Args[1].ArgType, Is.EqualTo(ModCmdArgType.Any));
            Assert.That(testCmd.Args[1].DataType, Is.EqualTo("System.Int32"));
        }
    }
}
