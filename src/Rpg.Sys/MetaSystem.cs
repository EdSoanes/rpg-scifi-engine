﻿using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class MetaSystem : IMetaSystem
    {
        public string Identifier { get => "RpgSys"; }

        public string Name { get => "Rpg System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "A test rpg system"; }

        public MetaObj[] Objects { get; set; } = Array.Empty<MetaObj>();

        public MetaAction[] Actions { get; set; } = Array.Empty<MetaAction>();
        
        public MetaState[] States { get; set; } = Array.Empty<MetaState>();
        
        public MetaPropUIAttribute[] PropUIs { get; set; } = Array.Empty<MetaPropUIAttribute>();
    }
}
