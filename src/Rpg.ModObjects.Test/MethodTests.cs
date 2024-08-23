using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests
{
    internal class TestMethodClass
    {
        public int GetInt(int i) { return i * 2; }
        public int GetIntNullable(int? i) { return i ?? 0; }
        public Dice GetDice(Dice dice) { return dice; }
        public ModSet GetObject(RpgObject obj) { return new ModSet("owner-id", "ModSet"); }

    }

    public class MethodTests
    {
        private TestMethodClass _testMethodClass;

        [SetUp]
        public void Setup()
        {
            _testMethodClass = new TestMethodClass();
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void Method_GetInt_EnsureMethodModel()
        {

            var method = RpgMethod.Create<TestMethodClass, int>(_testMethodClass, nameof(TestMethodClass.GetInt));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(TestMethodClass.GetInt)));
            Assert.That(method.ClassName, Is.Null);
            Assert.That(method.ReturnTypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args[0].Name, Is.EqualTo("i"));
            Assert.That(method.Args[0].TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(method.Args[0].IsNullable, Is.EqualTo(false));
        }

        [Test]
        public void Method_ExecuteGetInt_EnsureMethodModel()
        {

            var method = RpgMethod.Create<TestMethodClass, int>(_testMethodClass, nameof(TestMethodClass.GetInt))!;
            var args = new Dictionary<string, object?>();
            args.Add(method.Args.First().Name, 2);
            var res = method.Execute(_testMethodClass, args);

            Assert.That(res, Is.EqualTo(4));
        }

        [Test]
        public void Method_GetIntNullable_EnsureMethodModel()
        {

            var method = RpgMethod.Create<TestMethodClass, int>(_testMethodClass, nameof(TestMethodClass.GetIntNullable));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(TestMethodClass.GetIntNullable)));
            Assert.That(method.ClassName, Is.Null);
            Assert.That(method.ReturnTypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args[0].Name, Is.EqualTo("i"));
            Assert.That(method.Args[0].TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(method.Args[0].IsNullable, Is.EqualTo(true));
        }

        [Test]
        public void Method_GetObject_EnsureMethodModel()
        {
            var method = RpgMethod.Create<TestMethodClass, ModSet>(_testMethodClass, nameof(TestMethodClass.GetObject));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(TestMethodClass.GetObject)));
            Assert.That(method.ClassName, Is.Null);
            Assert.That(method.ReturnTypeName, Is.EqualTo(nameof(ModSet)));
            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args[0].Name, Is.EqualTo("obj"));
            Assert.That(method.Args[0].TypeName, Is.EqualTo("RpgObject"));
            Assert.That(method.Args[0].IsNullable, Is.EqualTo(false));
        }
    }
}
