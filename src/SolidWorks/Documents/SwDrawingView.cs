﻿//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawingView : IXDrawingView, ISwSelObject
    {
        IView DrawingView { get; }
    }

    public interface ISwModelBasedDrawingView : ISwDrawingView, IXModelViewBasedDrawingView
    {
    }

    public interface ISwProjectedDrawingView : ISwDrawingView, IXProjectedDrawingView
    {
    }

    public interface ISwAuxiliaryDrawingView : ISwDrawingView, IXAuxiliaryDrawingView
    {
    }

    public interface ISwSectionDrawingView : ISwDrawingView, IXSectionDrawingView
    {
    }

    public interface ISwDetailDrawingView : ISwDrawingView, IXDetailedDrawingView
    {
    }

    public interface ISwFlatPatternDrawingView : ISwDrawingView, IXFlatPatternDrawingView
    {
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDrawingView : SwSelObject, ISwDrawingView
    {
        protected readonly SwDrawing m_Drawing;

        public IView DrawingView => m_Creator.Element;

        protected readonly IElementCreator<IView> m_Creator;

        protected SwSheet Sheet 
        {
            get 
            {
                if (IsCommitted)
                {
                    return OwnerDocument.CreateObjectFromDispatch<SwSheet>(DrawingView.Sheet);
                }
                else 
                {
                    return m_ParentSheet;
                }
            }
        }

        internal SwDrawingView(IView drwView, SwDrawing drw)
            : this(drwView, drw, null)
        {
        }

        public override object Dispatch => DrawingView;

        private readonly SwSheet m_ParentSheet;

        protected SwDrawingView(IView drwView, SwDrawing drw, SwSheet sheet)
            : base(drwView, drw, drw?.OwnerApplication)
        {
            m_Drawing = drw;
            m_Creator = new ElementCreator<IView>(CreateDrawingViewElement, drwView, drwView != null);

            m_ParentSheet = sheet;

            Dimensions = new SwDrawingViewDimensionRepository(drw, this);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override bool IsCommitted => m_Creator.IsCreated;

        internal override void Select(bool append, ISelectData selData)
        {
            const string DRW_VIEW_TYPE_NAME = "DRAWINGVIEW";

            if (!OwnerModelDoc.Extension.SelectByID2(DrawingView.Name, DRW_VIEW_TYPE_NAME, 0, 0, 0, append, selData?.Mark ?? 0, selData?.Callout, 0))
            {
                throw new Exception("Failed to select drawing view");
            }
        }

        private IView CreateDrawingViewElement(CancellationToken cancellationToken)
        {
            using (var sheetActivator = new SheetActivator(Sheet)) 
            {
                var drwView = CreateDrawingView(cancellationToken);

                if (drwView == null)
                {
                    throw new Exception("Failed to create drawing view");
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    drwView.SetName2(Name);
                }

                var refConf = ReferencedConfiguration;

                if (refConf != null) 
                {
                    drwView.ReferencedConfiguration = refConf.Name;
                }

                var loc = Location;

                if (loc != null) 
                {
                    drwView.Position = new double[] { loc.X, loc.Y };
                }

                return drwView;
            }
        }

        protected virtual IView CreateDrawingView(CancellationToken cancellationToken)
            => throw new NotSupportedException("Creation of this drawing view is not supported");

        public string Name
        {
            get
            {
                if (IsCommitted)
                {
                    return DrawingView.Name;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    DrawingView.SetName2(value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Point Location
        {
            get
            {
                if (IsCommitted)
                {
                    var pos = DrawingView.Position as double[];

                    return new Point(pos[0], pos[1], 0);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    DrawingView.Position = new double[] { value.X, value.Y };
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXDocument3D ReferencedDocument
        {
            get
            {
                if (IsCommitted)
                {
                    var refDoc = DrawingView.ReferencedDocument;

                    if (refDoc != null)
                    {
                        return (IXDocument3D)((SwDocumentCollection)OwnerApplication.Documents)[refDoc];
                    }
                    else
                    {
                        var refDocPath = DrawingView.GetReferencedModelName();

                        if (!string.IsNullOrEmpty(refDocPath))
                        {

                            if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(refDocPath, out SwDocument doc))
                            {
                                return (ISwDocument3D)doc;
                            }
                            else
                            {
                                return (ISwDocument3D)((SwDocumentCollection)OwnerApplication.Documents).PreCreateFromPath(refDocPath);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXDocument3D>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    //TODO: implement model document replacement
                    throw new CommitedElementReadOnlyParameterException();
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXConfiguration ReferencedConfiguration
        {
            get
            {
                if (IsCommitted)
                {
                    var refConfName = DrawingView.ReferencedConfiguration;

                    if (!string.IsNullOrEmpty(refConfName))
                    {
                        return ReferencedDocument.Configurations.First(
                            c => string.Equals(c.Name, refConfName, StringComparison.CurrentCultureIgnoreCase));
                    }
                    else
                    {
                        return null;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXConfiguration>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    DrawingView.ReferencedConfiguration = value.Name;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Scale Scale
        {
            get
            {
                var scale = (double[])DrawingView.ScaleRatio;
                return new Scale(scale[0], scale[1]);
            }
            set => DrawingView.ScaleRatio = new double[] { value.Numerator, value.Denominator };
        }

        public Rect2D Boundary
        {
            get
            {
                var outline = (double[])DrawingView.GetOutline();
                var width = outline[2] - outline[0];
                var height = outline[3] - outline[1];

                var centerPt = new Point((outline[0] + outline[2]) / 2, (outline[1] + outline[3]) / 2, 0);

                return new Rect2D(width, height, centerPt);
            }
        }

        public Thickness Padding
        {
            get
            {
                const double VIEW_BORDER_RATIO = 0.02;

                var width = -1d;
                var height = -1d;

                DrawingView.Sheet.GetSize(ref width, ref height);

                var minSize = Math.Min(width, height);

                return new Thickness(minSize * VIEW_BORDER_RATIO);
            }
        }

        public IXDimensionRepository Dimensions { get; }

        public IXDrawingView BaseView
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXDrawingView>();
                }
                else
                {
                    var baseView = DrawingView.IGetBaseView();

                    if (baseView != null)
                    {
                        return m_Drawing.CreateObjectFromDispatch<ISwDrawingView>(baseView);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public IEnumerable<IXDrawingView> DependentViews 
        {
            get 
            {
                var views = (object[])DrawingView.GetDependentViews(true, -1) ?? new object[0];

                return views.Select(v => m_Drawing.CreateObjectFromDispatch<ISwDrawingView>((IView)v));
            }
        }

        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private ISwSelObject ConvertObjectBoxed(object obj)
        {
            if (obj is ISwSelObject)
            {
                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
                {
                    var disp = (obj as ISwSelObject).Dispatch;
                    var corrDisp = DrawingView.GetCorresponding(disp);

                    if (corrDisp != null)
                    {
                        return m_Drawing.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
                    }
                    else
                    {
                        throw new Exception("Failed to convert the pointer of the object");
                    }
                }
                else 
                {
                    throw new NotSupportedException("This API only available in SOLIDWORKS 2018 onwards");
                }
            }
            else
            {
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }
    }

    internal class SwDrawingViewDimensionRepository : IXDimensionRepository
    {
        private readonly SwDrawing m_Drw;
        private readonly SwDrawingView m_View;
        
        internal SwDrawingViewDimensionRepository(SwDrawing drw, SwDrawingView view) 
        {
            m_Drw = drw;
            m_View = view;
        }

        public IXDimension this[string name] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXDimension> GetEnumerator()
            => IterateDimensions().GetEnumerator();

        public T PreCreate<T>() where T : IXDimension
            => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXDimension ent)
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<IXDimension> IterateDimensions() 
        {
            var dispDim = m_View.DrawingView.GetFirstDisplayDimension5();

            while (dispDim != null) 
            {
                yield return m_Drw.CreateObjectFromDispatch<ISwDimension>(dispDim);
                dispDim = dispDim.GetNext5();
            }
        }
    }

    internal class SwModelBasedDrawingView : SwDrawingView, ISwModelBasedDrawingView
    {
        internal SwModelBasedDrawingView(IView drwView, SwDrawing drw)
            : base(drwView, drw)
        {
        }

        internal SwModelBasedDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {   
            if (SourceModelView is SwNamedView)
            {
                var namedView = (SwNamedView)SourceModelView;

                var drwView = m_Drawing.Drawing.CreateDrawViewFromModelView3(
                    namedView.Owner.GetPathName(), namedView.Name, 0, 0, 0);

                return drwView;
            }
            else
            {
                throw new InvalidCastException("Only named views are supported");
            }
        }

        public IXModelView SourceModelView 
        {
            get 
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXModelView>();
                }
                else 
                {
                    throw new NotSupportedException();
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }
    }

    internal class SwProjectedDrawingView : SwDrawingView, ISwProjectedDrawingView
    {
        internal SwProjectedDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwProjectedDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public ProjectedViewDirection_e Direction
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<ProjectedViewDirection_e>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            const double DEFAULT_OFFSET = 0.1;

            var offsetDir = CalculateViewOffsetDirection(Direction);

            BaseView.Select(false);

            var srcPos = (double[])((ISwDrawingView)BaseView).DrawingView.Position;

            var view = m_Drawing.Drawing.CreateUnfoldedViewAt3(
                            srcPos[0] + offsetDir.X * DEFAULT_OFFSET,
                            srcPos[1] + offsetDir.Y * DEFAULT_OFFSET,
                            0, false);

            if (view != null)
            {
                var srcViewOutline = (double[])((ISwDrawingView)BaseView).DrawingView.GetOutline();

                var srcWidth = srcViewOutline[2] - srcViewOutline[0];
                var srcHeight = srcViewOutline[3] - srcViewOutline[1];

                var destViewOutline = (double[])view.GetOutline();

                var destWidth = destViewOutline[2] - destViewOutline[0];
                var destHeight = destViewOutline[3] - destViewOutline[1];

                var offset = Math.Max(srcWidth, srcHeight) * 0.1;

                double margin;

                switch (Direction)
                {
                    case ProjectedViewDirection_e.Left:
                    case ProjectedViewDirection_e.Right:
                        margin = srcWidth / 2 + destWidth / 2 + offset;
                        break;

                    case ProjectedViewDirection_e.Top:
                    case ProjectedViewDirection_e.Bottom:
                        margin = srcHeight / 2 + destHeight / 2 + offset;
                        break;

                    case ProjectedViewDirection_e.IsoBottomLeft:
                    case ProjectedViewDirection_e.IsoBottomRight:
                    case ProjectedViewDirection_e.IsoTopLeft:
                    case ProjectedViewDirection_e.IsoTopRight:
                        margin = Math.Max(srcHeight, srcWidth) / 2 + Math.Max(destHeight, destWidth) / 2 + offset;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                var newPos = new double[]
                {
                        srcPos[0] + offsetDir.X * margin,
                        srcPos[1] + offsetDir.Y * margin
                };

                view.Position = newPos;
            }

            return view;
        }

        private Vector CalculateViewOffsetDirection(ProjectedViewDirection_e projection)
        {
            double dirX;
            double dirY;

            switch (projection)
            {
                case ProjectedViewDirection_e.Left:
                    dirX = -1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Top:
                    dirX = 0;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.Right:
                    dirX = 1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Bottom:
                    dirX = 0;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoTopLeft:
                    dirX = -1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoTopRight:
                    dirX = 1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoBottomLeft:
                    dirX = -1;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoBottomRight:
                    dirX = 1;
                    dirY = -1;
                    break;

                default:
                    throw new NotSupportedException($"'{projection}' is not supported");
            }

            return new Vector(dirX, dirY, 0);
        }
    }

    internal class SwAuxiliaryDrawingView : SwDrawingView, ISwAuxiliaryDrawingView
    {
        internal SwAuxiliaryDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwAuxiliaryDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }
    }

    internal class SwSectionDrawingView : SwDrawingView, ISwSectionDrawingView
    {
        internal SwSectionDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwSectionDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public Line SectionLine
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Line>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var drwModel = m_Drawing.Model;
            var skMgr = drwModel.SketchManager;
            var mathUtils = OwnerApplication.Sw.IGetMathUtility();

            var srcView = ((ISwDrawingView)BaseView).DrawingView;

            if (m_Drawing.Drawing.ActivateView(srcView.Name))
            {
                var transform = srcView.ModelToViewTransform.IMultiply(srcView.IGetSketch().ModelToSketchTransform);

                var startPt = (IMathPoint)mathUtils.CreatePoint(new double[] { SectionLine.StartPoint.X, SectionLine.StartPoint.Y, SectionLine.StartPoint.Z });
                startPt = startPt.IMultiplyTransform(transform);

                var endPt = (IMathPoint)mathUtils.CreatePoint(new double[] { SectionLine.EndPoint.X, SectionLine.EndPoint.Y, SectionLine.EndPoint.Z });
                endPt = endPt.IMultiplyTransform(transform);

                var startCoord = (double[])startPt.ArrayData;
                var endCoord = (double[])endPt.ArrayData;

                var sectionLine = skMgr.CreateLine(startCoord[0], startCoord[1], startCoord[2], endCoord[0], endCoord[1], endCoord[2]);

                using (var selGrp = new SelectionGroup(drwModel, false))
                {
                    selGrp.Add(sectionLine);

                    var sectionView = m_Drawing.Drawing.CreateSectionViewAt5(0, 0, 0, "", (int)swCreateSectionViewAtOptions_e.swCreateSectionView_NotAligned, null, 0);
                    var section = sectionView.IGetSection();

                    section.SetAutoHatch(true);
                    section.SetDisplayOnlySurfaceCut(false);
                    section.SetPartialSection(false);
                    section.SetReversedCutDirection(false);
                    section.SetScaleWithModelChanges(true);
                    section.CutSurfaceBodies = false;
                    section.DisplaySurfaceBodies = false;
                    section.ExcludeSliceSectionBodies = false;

                    return sectionView;
                }
            }
            else 
            {
                throw new Exception("Failed to activate a view");
            }
        }
    }

    internal class SwDetailDrawingView : SwDrawingView, ISwDetailDrawingView
    {
        internal SwDetailDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwDetailDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public Circle DetailCircle
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Circle>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var drwModel = m_Drawing.Model;
            var skMgr = drwModel.SketchManager;
            var mathUtils = OwnerApplication.Sw.IGetMathUtility();

            var srcView = ((ISwDrawingView)BaseView).DrawingView;

            if (m_Drawing.Drawing.ActivateView(srcView.Name))
            {
                var transform = srcView.ModelToViewTransform.IMultiply(srcView.IGetSketch().ModelToSketchTransform);

                var centerMathPt = (IMathPoint)mathUtils.CreatePoint(new double[] { DetailCircle.CenterAxis.Point.X, DetailCircle.CenterAxis.Point.Y, DetailCircle.CenterAxis.Point.Z });
                centerMathPt = centerMathPt.IMultiplyTransform(transform);

                var centerCoord = (double[])centerMathPt.ArrayData;

                var circle = skMgr.CreateCircleByRadius(centerCoord[0], centerCoord[1], centerCoord[2], DetailCircle.Diameter / 2);

                using (var selGrp = new SelectionGroup(drwModel, false))
                {
                    selGrp.Add(circle);

                    var detailView = (IView)m_Drawing.Drawing.CreateDetailViewAt4(0, 0, 0, (int)swDetViewStyle_e.swDetViewSTANDARD, 1, 1,
                        "", (int)swDetCircleShowType_e.swDetCircleCIRCLE, true, true, false, 5);

                    var detailCircle = detailView.IGetDetail();

                    detailCircle.ScaleHatchPattern = true;

                    return detailView;
                }
            }
            else
            {
                throw new Exception($"Failed to activate '{srcView.Name}'");
            }
        }
    }

    internal class SwFlatPatternDrawingView : SwDrawingView, ISwFlatPatternDrawingView
    {
        internal SwFlatPatternDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwFlatPatternDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public IXSolidBody SheetMetalBody
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXSolidBody>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var sheetMetalBody = (SwSolidBody)SheetMetalBody;

            SwPart sheetMetalPart;

            if (sheetMetalBody != null)
            {
                if (!sheetMetalBody.Body.IsSheetMetal())
                {
                    throw new InvalidCastException("Specified body is not a sheet metal");
                }

                sheetMetalPart = (SwPart)sheetMetalBody.OwnerDocument;
            }
            else 
            {
                sheetMetalPart = (SwPart)ReferencedDocument;
            }

            if (sheetMetalPart == null) 
            {
                throw new NotSupportedException($"Sheet metal part is not specified. Either set the {nameof(SheetMetalBody)} or {nameof(ReferencedDocument)}");
            }

            if (!sheetMetalPart.IsCommitted)
            {
                if (!sheetMetalPart.State.HasFlag(DocumentState_e.Hidden)) 
                {
                    sheetMetalPart.State |= DocumentState_e.Hidden;
                }

                sheetMetalPart.Commit(cancellationToken);
            }

            var sheetMetalBodiesCount = sheetMetalPart.Bodies.Count(b => ((ISwBody)b).Body.IsSheetMetal());

            if (sheetMetalBodiesCount > 0)
            {
                if (sheetMetalBodiesCount > 1 && sheetMetalBody == null) 
                {
                    throw new Exception($"Set the body to create flat pattern for via {nameof(SheetMetalBody)} for multi body sheet metal part");
                }

                //NOTE: part docu must be activated, otherwise the flat pattern will be invalid
                var wasPartHidden = !sheetMetalPart.Model.Visible;

                var activeDoc = OwnerApplication.Documents.Active;
                OwnerApplication.Documents.Active = sheetMetalPart;

                //var flatPatternFeat = sheetMetalPart.Features.OfType<ISwFlatPattern>().FirstOrDefault(f => sheetMetalBody == null || f.FixedEntity.Body.Equals(sheetMetalBody));

                var flatPatternFeat = EnumerateFlatPatternFeatures(sheetMetalPart).FirstOrDefault(f => sheetMetalBody == null || f.FixedEntity.Body.Equals(sheetMetalBody));

                if (flatPatternFeat == null)
                {
                    throw new Exception("Failed to find the flat pattern feature");
                }

                //flat pattern feature point may become invalid as the result of suppressing - storing feature name to restore the pointer
                var flatPatternFeatName = flatPatternFeat.Name;

                var isFlattened = flatPatternFeat.IsFlattened;

                if (!isFlattened)
                {
                    flatPatternFeat.IsFlattened = true;
                }

                try
                {
                    return CreateFlatPatternView(sheetMetalPart);
                }
                finally
                {
                    ((ISwFlatPattern)sheetMetalPart.Features[flatPatternFeatName]).IsFlattened = isFlattened;
                    
                    if (wasPartHidden)
                    {
                        sheetMetalPart.Model.Visible = false;
                    }

                    OwnerApplication.Documents.Active = activeDoc;
                }
            }
            else 
            {
                throw new Exception("No sheet metal bodies found in the part");
            }
        }

        private IView CreateFlatPatternView(SwPart sheetMetalPart) 
            => m_Drawing.Drawing.CreateFlatPatternViewFromModelView3(sheetMetalPart.Path, "", 0, 0, 0, false, false);

        private IEnumerable<ISwFlatPattern> EnumerateFlatPatternFeatures(SwPart sheetMetalPart) 
        {
            int pos = 0;

            IFeature feat;

            do
            {
                feat = (IFeature)sheetMetalPart.Model.FeatureByPositionReverse(pos++);

                if (feat?.GetTypeName2() == "FlatPattern")
                {
                    yield return sheetMetalPart.CreateObjectFromDispatch<ISwFlatPattern>(feat);
                }

                if (feat?.GetTypeName2() == "OriginProfileFeature") 
                {
                    yield break;
                }

            } while (feat != null);
        }
    }
}
