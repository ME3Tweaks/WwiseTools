﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WwiseTools.Basics;

namespace WwiseTools
{
    /// <summary>
    /// Wwise中的事件
    /// </summary>
    public class WwiseEvent : WwiseUnit
    {
        public WwiseEvent(string _name) : base(_name, "Event")
        { 
        }
    }
}
