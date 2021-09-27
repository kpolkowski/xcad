﻿using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// IMassProperty API in SOLIDOWRKS 2019 failed to correctly calculate the Moment Of Intertia for the components
    /// </summary>
    public class MomentOfInertiaOverridenException : NotSupportedException, IUserException
    {
        internal MomentOfInertiaOverridenException(string reason)
            : base($"Failed to calculate Moment Of Intertia for in SOLIDWORKS 2019 for the overriden mass properties: {reason}") 
        {
        }
    }
}
