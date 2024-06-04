using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public interface IGraphEvents
    {
        void OnGraphCreating(RpgGraph graph, RpgObject? entity = null);
        void OnObjectsCreating();
    }
}
