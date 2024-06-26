﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.ModObjects.Meta
{
    public interface IMetaSystem
    {
        string Identifier { get; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
        MetaObj[] Objects { get; set; }
        MetaAction[] Actions { get; set; }
        MetaState[] States { get; set; }
        MetaPropUIAttribute[] PropUIs { get; set; }
    }
}
