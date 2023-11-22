using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.SciFi.Engine.Artifacts.Core;

namespace Rpg.SciFi.Engine.Artifacts.Expressions.Parsers
{
    internal class PropInfo
    {
        internal Guid ContextId { get; set; }
        internal string Prop { get; set; }
    }

    internal static class PropertyParser
    {
        internal static PropInfo Parse(Guid? id, string? path)
        {
            if (string.IsNullOrEmpty(path) && id == null)
                throw new ArgumentException("Path and contextId are empty");

            var res = new PropInfo();

            if (!string.IsNullOrEmpty(path))
            {
                var parts = path.Split('.').Reverse();

                var propParts = new List<string>();
                foreach (var part in parts)
                {
                    var isId = Guid.TryParse(part, out var contextId);
                    if (isId)
                    {
                        if (!Nexus.Contexts.TryGetValue(contextId, out var context))
                            throw new ArgumentException($"PropertyParser.Parse path contains invalid contextId {part}");

                        res.ContextId = contextId;
                        break;
                    }

                    if (!isId)
                        propParts.Add(part);
                }

                propParts.Reverse();
                res.Prop = string.Join(".", propParts);
            }

            return res;
        }
    }
}
