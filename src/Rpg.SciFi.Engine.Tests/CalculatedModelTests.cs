using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    public sealed class BaseModel
    {
        public BaseModel(Guid? id = null, string? name = null, int? val = null)
        {
            Id = id ?? Guid.NewGuid();
            Name = name ?? GetType().Name;
            Val = val ?? 10;
        }

        public Guid Id { get; }
        public string Name { get; }
        public int Val { get; }
    }

    public class CalculatedModel
    {
        [JsonProperty]
        protected List<Condition> Conditions { get; }

        [JsonProperty]
        protected BaseModel BaseModel { get; }

        public CalculatedModel(Guid? id = null, string? name = null, BaseModel? baseModel = null, IEnumerable<Condition>? conditions = null)
        {
            Id = id ?? Guid.NewGuid();
            Name = name ?? GetType().Name;
            BaseModel = baseModel ?? new BaseModel();
            Conditions = conditions?.ToList() ?? new List<Condition>();
        }

        public Guid Id { get; }
        public string Name { get; }

        public int Val
        {
            get
            {
                return BaseModel.Val + Conditions.Sum(x => x.Val);
            }
        }

        public void ClearConditions()
        {
            Conditions.Clear();
        }

        public void AddCondition(Condition condition)
        {
            if (!Conditions.Any(x => x.Id == condition.Id))
                Conditions.Add(condition);
        }

        public void RemoveCondition(Guid id)
        {
            var existing = Conditions.SingleOrDefault(x => x.Id == id);
            if (existing != null)
                Conditions.Remove(existing);
        }
    }

    public class Condition
    {
        public Condition(Guid? id = null, string? name = null, string? target = null, string? prop = null, int? val = null)
        {
            Id = id ?? Guid.NewGuid();
            Name = name ?? GetType().Name;
            Target = target;
            Prop = prop;
            Val = val ?? 0;
        }

        public Guid Id { get; }
        public string Name { get; set; }
        public string Target { get; set; }
        public string Prop { get; set; }
        public int Val { set; get; }
    }

    [TestClass]
    public class CalculatedModelTests
    {
        [TestMethod]
        public void CalculatedModelTest()
        {
            var model = new CalculatedModel();

            Assert.AreEqual("CalculatedModel", model.Name);
            Assert.AreNotEqual(Guid.Empty, model.Id);
            Assert.AreEqual(10, model.Val);

            var c1 = new Condition { Name = "Increase by 5", Target = "CalculatedModel", Prop = "Val", Val = 5 };
            model.AddCondition(c1);

            Assert.AreEqual(15, model.Val);

            var c2 = new Condition { Name = "Increase by 5", Target = "CalculatedModel", Prop = "Val", Val = -6 };
            model.AddCondition(c2);

            Assert.AreEqual(9, model.Val);

            var json = JsonConvert.SerializeObject(model);

            var model2 = JsonConvert.DeserializeObject<CalculatedModel>(json);

        }

        [TestMethod]
        public void CalculatedModelTest_Serialized()
        {
            var model = new CalculatedModel();

            var c1 = new Condition { Name = "Increase by 5", Target = "CalculatedModel", Prop = "Val", Val = 5 };
            model.AddCondition(c1);

            var c2 = new Condition { Name = "Increase by 5", Target = "CalculatedModel", Prop = "Val", Val = -6 };
            model.AddCondition(c2);

            var json = JsonConvert.SerializeObject(model);
            var model2 = JsonConvert.DeserializeObject<CalculatedModel>(json);

            Assert.AreEqual("CalculatedModel", model2.Name);
            Assert.AreEqual(model.Id, model2.Id);
            Assert.AreEqual(9, model2.Val);
        }
    }
}
