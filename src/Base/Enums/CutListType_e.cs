﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Type of cut-list item
    /// </summary>
    public enum CutListType_e
    {
        /// <summary>
        /// Solid body cut-list
        /// </summary>
        SolidBody,
        
        /// <summary>
        /// Sheet metal cut-list
        /// </summary>
        SheetMetal,

        /// <summary>
        /// Weldment cut-list
        /// </summary>
        Weldment
    }
}
