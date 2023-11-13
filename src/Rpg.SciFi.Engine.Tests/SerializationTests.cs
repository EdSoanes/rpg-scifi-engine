using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    internal class TestSubClass
    {
        public string Prop { get; set; }
        public string ProtectedProp { get; protected set; }
        public string PrivateProp { get; protected set; }

        public TestSubClass(string protectedProp, string privateProp)
        {
            ProtectedProp = protectedProp;
            PrivateProp = privateProp;
        }
    }

    internal class TestClass
    {
        public string Prop { get; set; }
        public string ProtectedProp { get; protected set; }
        public string PrivateProp { get; protected set; }

        public TestSubClass? SubClass { get; set; }

        public TestClass(string protectedProp, string privateProp) 
        { 
            ProtectedProp = protectedProp;
            PrivateProp = privateProp;
        }
    }

    internal class TestIdClass
    {
        public Guid Id { get; protected set; }
        public string Prop { get; set; }

        public TestIdClass(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }
    }

    internal class TestId2Class
    {
        public Guid Id { get; protected set; }
        public string Prop { get; protected set; }
        [JsonProperty]
        protected string Prop2 { get; private set; }

        public TestId2Class(Guid? id = null, string? prop = null, string prop2 = null)
        {
            Id = id ?? Guid.NewGuid();
            Prop = prop;
            Prop2 = prop2;
        }

        public string GetProp2()
        { 
            return Prop2; 
        }
    }

    internal class PrivateSetClass
    {
        public PrivateSetClass()
        {
            Id = Guid.NewGuid();
        }

        [JsonProperty]
        public Guid? Id { get; private set; }
        [JsonProperty]
        public string Val { get; private set; }
    }

    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void SerializeObject()
        {
            var obj = new TestClass("val2", "val3")
            {
                Prop = "val",
                SubClass = new TestSubClass("sval2", "sval3") { Prop = "sval" }
            };

            var json = JsonConvert.SerializeObject(obj);

            Assert.IsNotNull(json);

            var obj2 = JsonConvert.DeserializeObject<TestClass>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.AreEqual("val2", obj2.ProtectedProp);
            Assert.AreEqual("val3", obj2.PrivateProp);
        }

        [TestMethod]
        public void SerializeIdObject()
        {
            var obj = new TestIdClass()
            {
                Prop = "val",
            };

            var json = JsonConvert.SerializeObject(obj);

            Assert.IsNotNull(json);

            var obj2 = JsonConvert.DeserializeObject<TestIdClass>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeIdObject2()
        {

            var json = "{ \"Prop\": \"val\" }";
            var obj = JsonConvert.DeserializeObject<TestIdClass>(json);

            Assert.IsNotNull(obj);
            Assert.AreEqual("val", obj.Prop);
            Assert.IsNotNull(obj.Id);

            json = JsonConvert.SerializeObject(obj);

            var obj2 = JsonConvert.DeserializeObject<TestIdClass>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeId2Object()
        {
            var obj = new TestId2Class(null, "val");

            var json = JsonConvert.SerializeObject(obj);

            Assert.IsNotNull(json);

            var obj2 = JsonConvert.DeserializeObject<TestIdClass>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeId2Object_EmptyConstructor()
        {
            var obj = new TestId2Class();

            var json = JsonConvert.SerializeObject(obj);

            Assert.IsNotNull(json);

            var obj2 = JsonConvert.DeserializeObject<TestIdClass>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual(null, obj2.Prop);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeId2Object_FromJson()
        {
            var json = "{ \"Prop\": \"val\" }";
            var obj = JsonConvert.DeserializeObject<TestId2Class>(json);

            Assert.IsNotNull(obj);
            Assert.AreEqual("val", obj.Prop);
            Assert.IsNotNull(obj.Id);

            json = JsonConvert.SerializeObject(obj);

            var obj2 = JsonConvert.DeserializeObject<TestId2Class>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.IsNotNull(obj2.Id);
            Assert.IsNotNull(obj.Id);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeId2Object_ProtectedProperty()
        {
            var json = "{ \"Prop\": \"val\", \"Prop2\": \"val2\" }";
            var obj = JsonConvert.DeserializeObject<TestId2Class>(json);

            Assert.IsNotNull(obj);
            Assert.AreEqual("val", obj.Prop);
            Assert.IsNotNull(obj.Id);

            json = JsonConvert.SerializeObject(obj);

            var obj2 = JsonConvert.DeserializeObject<TestId2Class>(json);

            Assert.IsNotNull(obj2);
            Assert.AreEqual("val", obj2.Prop);
            Assert.AreEqual("val2", obj2.GetProp2());
            Assert.IsNotNull(obj2.Id);
            Assert.IsNotNull(obj.Id);
            Assert.AreEqual(obj.Id, obj2.Id);
        }

        [TestMethod]
        public void SerializeId2Object_PrivateSetClass()
        {
            var json = "{\"Id\":\"8bfe226c-ed9c-491c-8ed3-040f3a23cd11\",\"Val\":\"val\"}";
            var obj = JsonConvert.DeserializeObject<PrivateSetClass>(json);

            Assert.IsNotNull(obj);
            Assert.AreEqual("val", obj.Val);
            Assert.AreEqual("8bfe226c-ed9c-491c-8ed3-040f3a23cd11", obj.Id.ToString());
        }

        [TestMethod]
        public void SerializeId2Object_PrivateSetClass_NoId()
        {
            var json = "{\"Val\":\"val\"}";
            var obj = JsonConvert.DeserializeObject<PrivateSetClass>(json);

            Assert.IsNotNull(obj);
            Assert.AreEqual("val", obj.Val);
            Assert.AreNotEqual(Guid.Empty.ToString(), obj.Id.ToString());
        }
    }
}
