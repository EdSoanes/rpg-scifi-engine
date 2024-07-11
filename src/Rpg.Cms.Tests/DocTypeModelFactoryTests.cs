using Rpg.Cms.Services;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Tests.Models;
using Rpg.Cyborgs;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Umbraco.Cms.Core.Strings;

namespace Rpg.Cms.Tests
{
    public class DocTypeModelFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void DataTypes_CreateModels_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            var session = new SyncSession(Guid.Empty, system);
            var dataTypeConverter = new DataTypeModelFactory();
            var models = dataTypeConverter.CreateModels(session, new TestDataTypeRootFolder());
            session.DataTypes = TestDataType.Convert(models);

            Assert.IsNotNull(session.DataTypes);
        }

        [Test]
        public void SimpleObject_Convert_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            var session = new SyncSession(Guid.Empty, system);
            var dataTypeConverter = new DataTypeModelFactory();
            var models = dataTypeConverter.CreateModels(session, new TestDataTypeRootFolder());
            session.DataTypes = TestDataType.Convert(models);

            var converter = new DocTypeModelFactory();

            var obj = session.System.Objects.First();
            var res = converter.CreateModel(session, obj);

            Assert.IsNotNull(res);
        }

        [Test]
        public void PropAliases()
        {
            var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
            var alias = helper.CleanStringForSafeAlias(@"Some_-:|/@#^\'=¤1234å /*[](){}thing");
            var alias2 = helper.CleanStringForSafeAlias("This is a string");
            Assert.That(alias, Is.Not.Null);

        }
    }
}