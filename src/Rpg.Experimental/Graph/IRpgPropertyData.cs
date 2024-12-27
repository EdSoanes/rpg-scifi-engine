using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public interface IRpgPropertyData : ILifecycle
    {
        string ObjectId { get; }
        string Prop { get; }
        RpgPropertyType PropType { get; }
        bool IsNullable { get; }
        bool ExpireRefsTo(string objectId, TimePoint now);
        void OnSyncProperty(RpgGraph graph);
    }
}
