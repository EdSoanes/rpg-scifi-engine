using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Meta
{
    public static class MetaEngine
    {
        public static Entity? Context { get; private set; }
        public static MetaEntity[]? MetaEntities { get; private set; }

        public static List<object> _setups = new List<object>();

        public static void Initialize(Entity context)
        {
            Context = context;
            MetaEntities = Discover(context)
                .OrderBy(x => x.Path)
                .ToArray();
        }

        public static MetaEntity? Meta(this Entity entity)
        {
            return MetaEntities?.SingleOrDefault(x => x.Id == entity.Id && x.Type == entity.GetType().Name);
        }

        public static MetaEntity Meta(this Guid id)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException($"{nameof(MetaEngine)} not initialized");

            return MetaEntities.Single(x => x.Id == id);
        }

        public static MetaEntity? Meta<T>(Guid id)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException($"{nameof(MetaEngine)} not initialized");

            return MetaEntities.SingleOrDefault(x => x.Id == id && x.Type == typeof(T).Name);
        }

        public static MetaEntity Find(string path, out string property)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException($"{nameof(MetaEngine)} not initialized");

            var parts = path.Split('.');
            var entityPath = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var metaEntity = MetaEntities.SingleOrDefault(x => x.Path == entityPath && x.ModifiableProperties.Any(x => x == prop));
            if (metaEntity == null)
                throw new ArgumentException($"{path} is invalid");

            property = prop;
            return metaEntity;
        }

        private static List<MetaEntity> Discover(Entity context)
        {
            var meta = context.TraverseMetaGraph((metaEntity, path, propertyInfo) =>
            {
                if (propertyInfo.IsModdableProperty())
                    metaEntity.ModifiableProperties?.Add(propertyInfo.Name);
            });

            return meta;
        }
    }
}
