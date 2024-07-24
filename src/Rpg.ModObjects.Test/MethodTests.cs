using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    internal class TestMethodClass
    {
        public int GetInt(int i) { return i; }
        public int GetIntNullable(int? i) { return i ?? 0; }

        public Dice GetDice(Dice dice) { return dice; }

    }

    public class MethodTests
    {
        private RpgMethodFactory _methodFactory;
        private TestMethodClass _testMethodClass;

        [SetUp]
        public void Setup()
        {
            _methodFactory = new RpgMethodFactory();
            _testMethodClass = new TestMethodClass();
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
        }


        [Test]
        public void Method_GetInt_EnsureMethodModel()
        {

            var method = _methodFactory.Create<TestMethodClass, int>(_testMethodClass, nameof(TestMethodClass.GetInt));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(TestMethodClass.GetInt)));
            Assert.That(method.ClassName, Is.Null);
            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args[0].Name, Is.EqualTo("i"));
            Assert.That(method.Args[0].TypeName, Is.EqualTo("Int32"));
            Assert.That(method.Args[0].IsNullable, Is.EqualTo(false));
        }
        [Test]
        public void Method_GetIntNullable_EnsureMethodModel()
        {

            var method = _methodFactory.Create<TestMethodClass, int>(_testMethodClass, nameof(TestMethodClass.GetIntNullable));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(TestMethodClass.GetIntNullable)));
            Assert.That(method.ClassName, Is.Null);
            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args[0].Name, Is.EqualTo("i"));
            Assert.That(method.Args[0].TypeName, Is.EqualTo("Int32"));
            Assert.That(method.Args[0].IsNullable, Is.EqualTo(true));
        }


        [Test]
        public void MetaGraph_Serialize_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            var json = RpgSerializer.Serialize(system);

            Assert.That(json, Is.Not.Null);
        }
    }
}
