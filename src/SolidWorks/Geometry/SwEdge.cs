﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwEdge : ISwEntity, IXEdge 
    {
        IEdge Edge { get; }
        new ISwCurve Definition { get; }
    }

    internal class SwEdge : SwEntity, ISwEdge
    {
        IXSegment IXEdge.Definition => Definition;

        public IEdge Edge { get; }

        public override ISwBody Body => Application.CreateObjectFromDispatch<SwBody>(Edge.GetBody(), Document);

        public override IEnumerable<ISwEntity> AdjacentEntities 
        {
            get 
            {
                foreach (IFace2 face in (Edge.GetTwoAdjacentFaces2() as object[]).ValueOrEmpty()) 
                {
                    yield return Application.CreateObjectFromDispatch<SwFace>(face, Document);
                }

                foreach (ICoEdge coEdge in (Edge.GetCoEdges() as ICoEdge[]).ValueOrEmpty())
                {
                    var edge = coEdge.GetEdge() as IEdge;
                    yield return Application.CreateObjectFromDispatch<SwEdge>(edge, Document);
                }

                yield return Application.CreateObjectFromDispatch<ISwVertex>(Edge.IGetStartVertex(), Document);
                yield return Application.CreateObjectFromDispatch<ISwVertex>(Edge.IGetEndVertex(), Document);
            }
        }

        public ISwCurve Definition => Application.CreateObjectFromDispatch<SwCurve>(Edge.IGetCurve(), Document);

        public override Point FindClosestPoint(Point point)
            => new Point(((double[])Edge.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        internal SwEdge(IEdge edge, ISwDocument doc, ISwApplication app) : base((IEntity)edge, doc, app)
        {
            Edge = edge;
        }
    }

    public interface ISwCircularEdge : ISwEdge, IXCircularEdge
    {
        new ISwArcCurve Definition { get; }
    }

    internal class SwCircularEdge : SwEdge, ISwCircularEdge
    {
        IXArc IXCircularEdge.Definition => Definition;

        internal SwCircularEdge(IEdge edge, ISwDocument doc, ISwApplication app) : base(edge, doc, app)
        {
        }

        public new ISwArcCurve Definition => Application.CreateObjectFromDispatch<SwArcCurve>(this.Edge.IGetCurve(), Document);
    }

    public interface ISwLinearEdge : ISwEdge, IXLinearEdge
    {
        new ISwLineCurve Definition { get; }
    }

    internal class SwLinearEdge : SwEdge, ISwLinearEdge
    {
        IXLine IXLinearEdge.Definition => Definition;

        internal SwLinearEdge(IEdge edge, ISwDocument doc, ISwApplication app) : base(edge, doc, app)
        {
        }

        public new ISwLineCurve Definition => Application.CreateObjectFromDispatch<SwLineCurve>(this.Edge.IGetCurve(), Document);
    }
}