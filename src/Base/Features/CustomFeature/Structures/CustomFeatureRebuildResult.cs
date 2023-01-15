﻿//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features.CustomFeature.Structures
{
    /// <summary>
    /// Rebuild result of the <see cref="IXCustomFeature"/>
    /// </summary>
    public class CustomFeatureRebuildResult
    {
        /// <summary>
        /// Creates rebuild result from bodies
        /// </summary>
        /// <param name="bodies">Bodies</param>
        /// <returns>Rebuild result</returns>
        public static CustomFeatureBodyRebuildResult FromBodies(params IXBody[] bodies)
            => new CustomFeatureBodyRebuildResult()
            {
                Bodies = bodies
            };

        /// <summary>
        /// Creates rebuild result from status
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="errMsg">Error if failed or warning</param>
        /// <returns>Rebuild result</returns>
        public static CustomFeatureRebuildResult FromStatus(bool result, string errMsg = "")
            => new CustomFeatureRebuildResult()
            {
                Result = result,
                ErrorMessage = errMsg
            };

        /// <summary>
        /// Status result
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Bodies result of the <see cref="IXCustomFeature"/>
    /// </summary>
    public class CustomFeatureBodyRebuildResult : CustomFeatureRebuildResult
    {
        /// <summary>
        /// Bodies generated by macro feature
        /// </summary>
        public IXBody[] Bodies { get; set; }
    }
}