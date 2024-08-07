﻿using Rpg.ModObjects.Meta;

namespace Rpg.Cms
{
    public class MetaSystems
    {
        private static IMetaSystem[]? _metaSystems;
        private static object _lock = new object();

        public static bool IsMetaSystemType(Type type)
        {
            lock (_lock)
            {
                if (type.Namespace == null)
                    return false;

                var nspcs = GetNamespaces();
                foreach (var nspc in nspcs)
                {
                    if (type.Namespace.StartsWith(nspc))
                        return true;
                }

                return false;
            }
        }

        public static string MetaSystemTypeName(Type type)
        {
            lock (_lock)
            {
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(type.Namespace))
                    parts.Add(type.Namespace);

                parts.Add(type.Name.Split('`').First());
                if (type.IsGenericType)
                {
                    foreach (var genType in type.GetGenericArguments())
                        parts.Add(MetaSystemTypeName(genType));
                }

                return string.Join('.', parts);
            }
        }

        public static IMetaSystem[] Get()
        {
            lock (_lock)
            {
                if (_metaSystems == null)
                {
                    var systems = MetaGraph.DiscoverMetaSystems();
                    var res = new List<IMetaSystem>();
                    foreach (var system in systems)
                    {
                        var meta = new MetaGraph();
                        res.Add(meta.Build(system));
                    }

                    _metaSystems = res.ToArray();
                }

                return _metaSystems!;
            }
        }

        public static IMetaSystem? Get(string identifier)
        {
            lock (_lock)
                return Get().FirstOrDefault(x => x.Identifier == identifier);
        }

        public static string[] GetNamespaces()
        {
            lock( _lock)
            {
                var nspcs = Get()
                    .Where(x => x?.Namespaces != null)
                    .SelectMany(x => x.Namespaces!)
                    .ToList();
                nspcs.Add(typeof(MetaSystems).Namespace!);

                return nspcs.ToArray();
            }
        }
    }
}