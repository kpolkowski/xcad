﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of configurations in <see cref="IXPart"/>
    /// </summary>
    public interface IXPartConfigurationRepository : IXConfigurationRepository, IXRepository<IXPartConfiguration> 
    {
        /// <inheritdoc/>
        new IXPartConfiguration Active { get; set; }

        /// <inheritdoc/>
        new IXPartConfiguration PreCreate();
    }
}
