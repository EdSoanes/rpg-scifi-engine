using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Server
{
    public class RpgRequest<T>
    {
        public RpgGraphState GraphState { get; init; }
        public T Op { get; init; }
    }
}
