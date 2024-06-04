using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModCmdTests
    {
        [SetUp]
        public void Setup()
        {
            RpgGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void ModdableEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.GetCommands().Count(), Is.EqualTo(1));

            var testCmd = entity.GetCommands().Single();
            Assert.That(testCmd.CommandName, Is.EqualTo("TestCommand"));
            Assert.That(testCmd.EntityId, Is.EqualTo(entity.Id));

            Assert.That(testCmd.Args.Count(), Is.EqualTo(2));
            Assert.That(testCmd.Args[0].Name, Is.EqualTo("initiator"));
            Assert.That(testCmd.Args[0].ArgType, Is.EqualTo(ModCmdArgType.Actor));
            Assert.That(testCmd.Args[0].TypeName, Is.EqualTo(typeof(RpgObject).AssemblyQualifiedName));

            Assert.That(testCmd.Args[1].Name, Is.EqualTo("target"));
            Assert.That(testCmd.Args[1].ArgType, Is.EqualTo(ModCmdArgType.Any));
            Assert.That(testCmd.Args[1].TypeName, Is.EqualTo(typeof(int).AssemblyQualifiedName));
        }
    }
}
