using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public interface IRpgEntityTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
