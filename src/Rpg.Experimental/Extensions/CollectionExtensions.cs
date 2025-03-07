using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Get<T>(this ICollection<RpgObject> collection) 
            where T : RpgLifecycleObject
                => collection.Where(x => x is T).Cast<T>();
    }
}
