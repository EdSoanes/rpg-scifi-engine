﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public enum MetaObjectType
    {
        None,
        Entity,
        Component,
        EntityTemplate,
        ComponentTemplate
    }
    public enum EditorType
    {
        Int32,
        Text,
        RichText,
        Boolean,
        Select,
        CheckBoxList,
        Container,
        LongText
    }
}
