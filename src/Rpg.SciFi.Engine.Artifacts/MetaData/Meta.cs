using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaState<T> where T : Entity
    {
        public T? Context { get; set; }
        public MetaEntity[]? MetaEntities { get; set; }
    }

    public static class Meta
    {
        public static Entity? Context { get; private set; }

        public static MetaEntity[]? MetaEntities { get; private set; }

        public static void Initialize<T>(T context) where T : Entity
        {
            Context = context;

            var meta = TraverseMetaGraph(Context, (metaEntity, path, propertyInfo) =>
            {
                if (propertyInfo.IsModdableProperty())
                    metaEntity.ModifiableProperties?.Add(propertyInfo.Name);
            });

            MetaEntities = meta
                .OrderBy(x => x.Path)
                .ToArray();

            foreach (var metaEntity in MetaEntities)
            {
                foreach (var setup in metaEntity.SetupMethods)
                    metaEntity.Entity.ExecuteAction(setup);
            }
        }

        public static void Clear()
        {
            Context = null;
            MetaEntities = null;
        }

        public static string Serialize<T>() where T : Entity
        {
            var json = JsonConvert.SerializeObject(new MetaState<T> { Context = Context as T, MetaEntities = MetaEntities }, Formatting.None);
            return json;
        }

        public static void Deserialize<T>(string state) where T : Entity
        {
            var metaState = JsonConvert.DeserializeObject<MetaState<T>>(state);
            Context = metaState!.Context;
            MetaEntities = metaState!.MetaEntities;

            TraverseMetaGraph(Context!, (e, _, _) =>
            {
                var metaEntity = MetaEntities!.Single(x => x.Id == e.Id);
                metaEntity.SetEntity(e.Entity!);
            });
        }

        private static List<MetaEntity> TraverseMetaGraph(object context, Action<MetaEntity, string, PropertyInfo> processContext, string basePath = "{}")
        {
            var entities = new List<MetaEntity>();

            var metaEntity = new MetaEntity(context, basePath);
            entities.Add(metaEntity);

            foreach (var propertyInfo in context.MetaProperties())
            {
                var items = context.PropertyObjects(propertyInfo, out var isEnumerable);
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? "[]" : "")}";

                processContext.Invoke(metaEntity, path, propertyInfo);

                foreach (var item in items)
                {
                    var childEntities = TraverseMetaGraph(item, processContext, path);
                    entities.AddRange(childEntities);
                }
            }

            return entities;
        }
    }
}
