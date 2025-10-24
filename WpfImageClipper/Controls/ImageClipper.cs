using LibBezierCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfImageClipper.Controls
{
    public partial class ImageClipper : FrameworkElement
    {
        private static readonly SolidColorBrush _brushBlue = new SolidColorBrush(Color.FromRgb(29, 124, 223));
        private static readonly SolidColorBrush _brushWhite = new SolidColorBrush(Color.FromRgb(244, 244, 244));

        private static readonly Pen _penBlueBorder = new Pen(_brushBlue, 1);
        private static readonly Pen _lineStroke = new Pen(_brushBlue, 1);

        static ImageClipper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageClipper), new FrameworkPropertyMetadata(typeof(ImageClipper)));
        }

        // 渲染
        private Rect _actualRenderArea;
        private double _scale;

        // 设置点的控制点
        private bool _isAdjustingPoint;
        private int _adjustingPointIndex;

        // 移动点或移动点的控制点
        private bool _isMovingPoint;
        private bool _isMovingControlPoint1;
        private bool _isMovingControlPoint2;
        private int _movingPointIndex;

        // 当前点
        private int _currentPointIndex;

        public ImageClipper()
        {
            AreaPoints = new ImageClipAreaPointCollection(this);
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// 裁切区域的点
        /// </summary>
        public ImageClipAreaPointCollection AreaPoints { get; }



        public bool IsAreaClosed
        {
            get { return (bool)GetValue(IsAreaClosedProperty); }
            set { SetValue(IsAreaClosedProperty, value); }
        }


        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageClipper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsAreaClosedProperty =
            DependencyProperty.Register("IsAreaClosed", typeof(bool), typeof(ImageClipper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));


        /// <summary>
        /// 计算实际渲染区域
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <param name="imageSize"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static Rect CalculateActualRenderArea(Size canvasSize, Size imageSize, out double scale)
        {
            scale = canvasSize.Width / imageSize.Width;
            if (imageSize.Height * scale > canvasSize.Height)
            {
                scale = canvasSize.Height / imageSize.Height;
            }

            var actualRenderSize = new Size(
                imageSize.Width * scale,
                imageSize.Height * scale);

            return new Rect(
                (canvasSize.Width - actualRenderSize.Width) / 2,
                (canvasSize.Height - actualRenderSize.Height) / 2,
                actualRenderSize.Width,
                actualRenderSize.Height);
        }

        private void RenderPoint(
            DrawingContext drawingContext,
            Rect actualRenderArea,
            double scale,
            ImageClipAreaPoint point,
            bool isCurrentPoint)
        {
            var actualCenter = PointFromImageSpaceToControlSpace(point.Position);

            var rectSize = new Size(5, 5);
            var rect = new Rect(
                actualCenter.X - rectSize.Width / 2,
                actualCenter.Y - rectSize.Height / 2,
                rectSize.Width,
                rectSize.Height);

            if (isCurrentPoint)
            {
                drawingContext.DrawRectangle(_brushBlue, null, rect);
            }
            else
            {
                drawingContext.DrawRectangle(_brushWhite, _penBlueBorder, rect);
            }
        }

        private PathGeometry BuildBezierGeometry(Point startPoint, Point controlPoint1, Point controlPoint2, Point endPoint)
        {
            return new PathGeometry()
            {
                Figures =
                {
                    new PathFigure()
                    {
                        StartPoint = startPoint,
                        Segments =
                            {
                                new BezierSegment(
                                    controlPoint1,
                                    controlPoint2,
                                    endPoint,
                                    true),
                            }
                    }
                }
            };
        }

        private PathGeometry BuildBezierGeometry(Point startPoint, Point controlPoint, Point endPoint)
        {
            return new PathGeometry()
            {
                Figures =
                {
                    new PathFigure()
                    {
                        StartPoint = startPoint,
                        Segments =
                            {
                                new QuadraticBezierSegment(
                                    controlPoint,
                                    endPoint,
                                    true),
                            }
                    }
                }
            };
        }

        private bool DoSegmentHitTest(
            ImageClipAreaPoint point1,
            ImageClipAreaPoint point2,
            Point imageSpacePoint,
            double maxDistance,
            out Point target)
        {
            target = default;

            if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;

                var curve = new CubicBezierCurve(
                    point1.Position.X,
                    point1.Position.Y,
                    point1Bezier.ControlPoint2.X,
                    point1Bezier.ControlPoint2.Y,
                    point2Bezier.ControlPoint1.X,
                    point2Bezier.ControlPoint1.Y,
                    point2.Position.X,
                    point2.Position.Y);

                if (!curve.HitTest(imageSpacePoint.X, imageSpacePoint.Y, maxDistance, out var t))
                {
                    return false;
                }

                curve.Sample(t, out var x, out var y);
                target = new Point(x, y);
                return true;
            }
            else if (
                point1 is ImageClipAreaCommonPoint &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;

                var curve = new QuadraticBezierCurve(
                    point1.Position.X,
                    point1.Position.Y,
                    point2Bezier.ControlPoint1.X,
                    point2Bezier.ControlPoint1.Y,
                    point2.Position.X,
                    point2.Position.Y);

                if (!curve.HitTest(imageSpacePoint.X, imageSpacePoint.Y, maxDistance, out var t))
                {
                    return false;
                }

                curve.Sample(t, out var x, out var y);
                target = new Point(x, y);
                return true;
            }
            else if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaCommonPoint)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;

                var curve = new QuadraticBezierCurve(
                    point1.Position.X,
                    point1.Position.Y,
                    point1Bezier.ControlPoint2.X,
                    point1Bezier.ControlPoint2.Y,
                    point2.Position.X,
                    point2.Position.Y);

                if (!curve.HitTest(imageSpacePoint.X, imageSpacePoint.Y, maxDistance, out var t))
                {
                    return false;
                }

                curve.Sample(t, out var x, out var y);
                target = new Point(x, y);
                return true;
            }
            else
            {
                var curve = new LinearBezierCurve(
                    point1.Position.X,
                    point1.Position.Y,
                    point2.Position.X,
                    point2.Position.Y);

                if (!curve.HitTest(imageSpacePoint.X, imageSpacePoint.Y, maxDistance, out var t))
                {
                    return false;
                }

                curve.Sample(t, out var x, out var y);
                target = new Point(x, y);
                return true;
            }
        }

        private void RenderLine(
            DrawingContext drawingContext,
            Rect actualRenderArea,
            double scale,
            ImageClipAreaPoint point1,
            ImageClipAreaPoint point2)
        {
            if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;
                var bezierGeometry = BuildBezierGeometry(
                    PointFromImageSpaceToControlSpace(point1.Position),
                    PointFromImageSpaceToControlSpace(point1Bezier.ControlPoint2),
                    PointFromImageSpaceToControlSpace(point2Bezier.ControlPoint1),
                    PointFromImageSpaceToControlSpace(point2.Position));

                drawingContext.DrawGeometry(null, _lineStroke, bezierGeometry);
            }
            else if (
                point1 is ImageClipAreaCommonPoint &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;
                var bezierGeometry = BuildBezierGeometry(
                    PointFromImageSpaceToControlSpace(point1.Position),
                    PointFromImageSpaceToControlSpace(point2Bezier.ControlPoint1),
                    PointFromImageSpaceToControlSpace(point2.Position));

                drawingContext.DrawGeometry(null, _lineStroke, bezierGeometry);
            }
            else if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaCommonPoint)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;
                var bezierGeometry = BuildBezierGeometry(
                    PointFromImageSpaceToControlSpace(point1.Position),
                    PointFromImageSpaceToControlSpace(point1Bezier.ControlPoint2),
                    PointFromImageSpaceToControlSpace(point2.Position));

                drawingContext.DrawGeometry(null, _lineStroke, bezierGeometry);
            }
            else
            {
                drawingContext.DrawLine(
                    _lineStroke,
                    PointFromImageSpaceToControlSpace(point1.Position),
                    PointFromImageSpaceToControlSpace(point2.Position));
            }
        }

        private void RenderBezierHandleControlPoint(
            DrawingContext drawingContext,
            Rect actualRenderArea,
            double scale,
            Point imageSpacePointSelfPosition,
            Point imageSpaceControlPointPosition)
        {
            var pointSelf = PointFromImageSpaceToControlSpace(imageSpacePointSelfPosition);
            var controlPointCenter = PointFromImageSpaceToControlSpace(imageSpaceControlPointPosition);

            drawingContext.DrawLine(_lineStroke, pointSelf, controlPointCenter);
            drawingContext.DrawEllipse(_brushWhite, _penBlueBorder, controlPointCenter, 2.5f, 2.5f);
        }

        private void RenderBezierHandle(
            DrawingContext drawingContext,
            Rect actualRenderArea,
            double scale,
            ImageClipAreaBezierPoint1 bezierPoint,
            bool renderControlPoint1,
            bool renderControlPoint2)
        {
            if (renderControlPoint1 && bezierPoint is ImageClipAreaBezierPoint2 bezierPoint2)
            {
                RenderBezierHandleControlPoint(drawingContext, actualRenderArea, scale, bezierPoint.Position, bezierPoint2.ControlPoint1);
            }

            if (renderControlPoint2)
            {
                RenderBezierHandleControlPoint(drawingContext, actualRenderArea, scale, bezierPoint.Position, bezierPoint.ControlPoint2);
            }
        }

        private Point PointFromControlSpaceToImageSpace(Point point)
        {
            return new Point(
                (point.X - _actualRenderArea.X) / _scale,
                (point.Y - _actualRenderArea.Y) / _scale);
        }

        private Point PointFromImageSpaceToControlSpace(Point point)
        {
            return new Point(
                point.X * _scale + _actualRenderArea.X,
                point.Y * _scale + _actualRenderArea.Y);
        }

        private bool HitTest(
            Point controlSpaceTargetPoint,
            Point controlSpaceMousePoint)
        {
            var rect = new Rect(
                controlSpaceTargetPoint.X - 2.5,
                controlSpaceTargetPoint.Y - 2.5,
                5, 5);

            return rect.Contains(controlSpaceMousePoint);
        }

        private void HitTest(
            ImageClipAreaPoint point,
            Point controlSpacePoint,
            out bool hitPointSelf,
            out bool hitControlPoint1,
            out bool hitControlPoint2)
        {
            hitPointSelf = HitTest(
                PointFromImageSpaceToControlSpace(point.Position),
                controlSpacePoint);

            hitControlPoint1 = false;
            hitControlPoint2 = false;

            if (point is ImageClipAreaBezierPoint1 bezierPoint1)
            {
                hitControlPoint2 = HitTest(
                    PointFromImageSpaceToControlSpace(bezierPoint1.ControlPoint2),
                    controlSpacePoint);
            }

            if (point is ImageClipAreaBezierPoint2 bezierPoint2)
            {
                hitControlPoint1 = HitTest(
                    PointFromImageSpaceToControlSpace(bezierPoint2.ControlPoint1),
                    controlSpacePoint);
            }
        }

        private void BuildClipGeometryFigurePart(
            PathFigure pathFigure,
            ImageClipAreaPoint point1,
            ImageClipAreaPoint point2)
        {
            if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;

                pathFigure.Segments.Add(new BezierSegment(point1Bezier.ControlPoint2, point2Bezier.ControlPoint1, point2.Position, true));
            }
            else if (
                point1 is ImageClipAreaCommonPoint &&
                point2 is ImageClipAreaBezierPoint2)
            {
                var point2Bezier = (ImageClipAreaBezierPoint2)point2;

                pathFigure.Segments.Add(new QuadraticBezierSegment(point2Bezier.ControlPoint1, point2.Position, true));
            }
            else if (
                point1 is ImageClipAreaBezierPoint1 &&
                point2 is ImageClipAreaCommonPoint)
            {
                var point1Bezier = (ImageClipAreaBezierPoint1)point1;

                pathFigure.Segments.Add(new QuadraticBezierSegment(point1Bezier.ControlPoint2, point2.Position, true));
            }
            else
            {
                pathFigure.Segments.Add(new LineSegment(point2.Position, true));
            }
        }

        private Geometry? BuildClipGeometry()
        {
            if (AreaPoints.Count < 3)
            {
                return null;
            }

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure()
            {
                StartPoint = AreaPoints[0].Position,
            };

            for (int i = 0; i < AreaPoints.Count - 1; i++)
            {
                BuildClipGeometryFigurePart(pathFigure, AreaPoints[i], AreaPoints[(i + 1) % AreaPoints.Count]);
            }

            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        private void InsertPoint(int afterIndex, Point position)
        {
            // TODO: 插入的应该是完整的贝塞尔曲线点, 计算对现有曲线不会造成影响的两个控制点

            AreaPoints.Insert(afterIndex + 1, new ImageClipAreaCommonPoint()
            {
                Position = position
            });
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Source is null)
            {
                return;
            }

            var canvasSize = new Size(RenderSize.Width, RenderSize.Height);
            var imageSize = new Size(Source.Width, Source.Height);

            // 计算实际渲染区域
            _actualRenderArea = CalculateActualRenderArea(canvasSize, imageSize, out _scale);

            // 渲染图像
            drawingContext.DrawRectangle(Brushes.Red, null, _actualRenderArea);
            drawingContext.DrawImage(Source, _actualRenderArea);

            // 绘制线
            for (int i = 0; i < AreaPoints.Count; i++)
            {
                bool isLast = i == AreaPoints.Count - 1;
                bool isCurrent = i == _currentPointIndex;

                ImageClipAreaPoint? point = AreaPoints[i];

                // 连接线
                if (!isLast || IsAreaClosed)
                {
                    RenderLine(drawingContext, _actualRenderArea, _scale, point, AreaPoints[(i + 1) % AreaPoints.Count]);
                }
            }

            // 绘制点
            for (int i = 0; i < AreaPoints.Count; i++)
            {
                bool isCurrent = i == _currentPointIndex;

                ImageClipAreaPoint? point = AreaPoints[i];

                // 贝塞尔控制点绘制
                if (point is ImageClipAreaBezierPoint1 bezierPoint &&
                    _currentPointIndex >= 0)
                {
                    if (isCurrent)
                    {
                        RenderBezierHandle(drawingContext, _actualRenderArea, _scale, bezierPoint, true, true);
                    }
                    else if (i == _currentPointIndex - 1)
                    {
                        RenderBezierHandle(drawingContext, _actualRenderArea, _scale, bezierPoint, false, true);
                    }
                    else if (i == _currentPointIndex + 1)
                    {
                        RenderBezierHandle(drawingContext, _actualRenderArea, _scale, bezierPoint, true, false);
                    }
                }

                // 点本体
                RenderPoint(drawingContext, _actualRenderArea, _scale, point, isCurrent);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (CaptureMouse())
            {
                var controlSpaceMousePosition = e.GetPosition(this);

                // 移动点
                if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    for (int i = 0; i < AreaPoints.Count; i++)
                    {
                        ImageClipAreaPoint? point = AreaPoints[i];
                        HitTest(point, controlSpaceMousePosition, out var hitPointSelf, out var hitControlPoint1, out var hitControlPoint2);

                        if (hitPointSelf)
                        {
                            _isMovingPoint = true;
                            _movingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }

                        // 移动控制点1
                        if (hitControlPoint1 &&
                            (i == _currentPointIndex || i == _currentPointIndex + 1))
                        {
                            _isMovingPoint = true;
                            _isMovingControlPoint1 = true;
                            _movingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }
                        // 移动控制点2
                        else if (hitControlPoint2 &&
                            (i == _currentPointIndex || i == _currentPointIndex - 1))
                        {
                            _isMovingPoint = true;
                            _isMovingControlPoint2 = true;
                            _movingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }
                    }

                    e.Handled = true;
                }
                // 调整点
                else if (
                    Keyboard.IsKeyDown(Key.LeftAlt) ||
                    Keyboard.IsKeyDown(Key.RightAlt))
                {
                    for (int i = 0; i < AreaPoints.Count; i++)
                    {
                        ImageClipAreaPoint? point = AreaPoints[i];
                        HitTest(point, controlSpaceMousePosition, out var hitPointSelf, out var hitControlPoint1, out var hitControlPoint2);

                        if (hitPointSelf)
                        {
                            _isAdjustingPoint = true;
                            _adjustingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }

                        // 移动控制点1
                        if (hitControlPoint1 &&
                            (i == _currentPointIndex || i == _currentPointIndex + 1))
                        {
                            _isMovingPoint = true;
                            _isMovingControlPoint1 = true;
                            _movingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }
                        // 移动控制点2
                        else if (hitControlPoint2 &&
                            (i == _currentPointIndex || i == _currentPointIndex - 1))
                        {
                            _isMovingPoint = true;
                            _isMovingControlPoint2 = true;
                            _movingPointIndex = i;
                            _currentPointIndex = i;
                            e.Handled = true;
                            break;
                        }
                    }
                }
                // 删除点
                else
                {
                    if (AreaPoints.Count > 0 && !IsAreaClosed)
                    {
                        HitTest(AreaPoints[0], controlSpaceMousePosition, out var hitFirstPointSelf, out _, out _);
                        if (hitFirstPointSelf)
                        {
                            _currentPointIndex = -1;
                            IsAreaClosed = true;
                            e.Handled = true;
                        }
                        else
                        {
                            for (int i = 1; i < AreaPoints.Count; i++)
                            {
                                ImageClipAreaPoint? point = AreaPoints[i];
                                HitTest(point, controlSpaceMousePosition, out var hitPointSelf, out _, out _);
                                if (hitPointSelf)
                                {
                                    AreaPoints.RemoveAt(i);
                                    e.Handled = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // 创建点
                if (!e.Handled && !IsAreaClosed)
                {
                    var imageSpaceMousePosition = PointFromControlSpaceToImageSpace(controlSpaceMousePosition);
                    for (int i = 0; i < AreaPoints.Count - 1; i++)
                    {
                        if (DoSegmentHitTest(AreaPoints[i], AreaPoints[i + 1], imageSpaceMousePosition, 2.5 / _scale, out var target))
                        {
                            _isAdjustingPoint = true;
                            _currentPointIndex = i + 1;
                            _adjustingPointIndex = _currentPointIndex;
                            InsertPoint(i, target);

                            e.Handled = true;
                            break;
                        }
                    }

                    if (!e.Handled)
                    {
                        _isAdjustingPoint = true;
                        _currentPointIndex = AreaPoints.Count;
                        _adjustingPointIndex = _currentPointIndex;
                        AreaPoints.Add(new ImageClipAreaCommonPoint()
                        {
                            Position = imageSpaceMousePosition
                        });

                        InvalidateVisual();
                        e.Handled = true;
                    }
                }

                // 取消选中
                if (!e.Handled)
                {
                    _currentPointIndex = -1;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var controlSpaceMousePoint = e.GetPosition(this);

            if (_isAdjustingPoint &&
                _adjustingPointIndex >= 0 &&
                _adjustingPointIndex < AreaPoints.Count)
            {
                var adjustingPoint = AreaPoints[_adjustingPointIndex];
                var adjustingPointBezier = adjustingPoint as ImageClipAreaBezierPoint1;
                if (adjustingPointBezier is null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) ||
                        Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        adjustingPointBezier = new ImageClipAreaBezierPoint1()
                        {
                            Position = adjustingPoint.Position
                        };
                    }
                    else
                    {
                        adjustingPointBezier = new ImageClipAreaBezierPoint2()
                        {
                            Position = adjustingPoint.Position
                        };
                    }
                }

                var mousePoint = PointFromControlSpaceToImageSpace(controlSpaceMousePoint);
                var offset = mousePoint - adjustingPointBezier.Position;
                adjustingPointBezier.ControlPoint2 = adjustingPointBezier.Position + offset;
                if (adjustingPointBezier is ImageClipAreaBezierPoint2 adjustingPointBezier2)
                {
                    adjustingPointBezier2.ControlPoint1 = adjustingPointBezier.Position - offset;
                }

                AreaPoints[_adjustingPointIndex] = adjustingPointBezier;
                InvalidateVisual();
                e.Handled = true;
            }
            else if (_isMovingPoint &&
                _movingPointIndex >= 0 &&
                _movingPointIndex < AreaPoints.Count)
            {
                var movingPoint = AreaPoints[_movingPointIndex];
                if (_isMovingControlPoint1)
                {
                    var movingPointBezier = (ImageClipAreaBezierPoint2)movingPoint;
                    movingPointBezier.ControlPoint1 = PointFromControlSpaceToImageSpace(controlSpaceMousePoint);
                }
                else if (_isMovingControlPoint2)
                {
                    var movingPointBezier = (ImageClipAreaBezierPoint1)movingPoint;
                    movingPointBezier.ControlPoint2 = PointFromControlSpaceToImageSpace(controlSpaceMousePoint);
                }
                else
                {
                    movingPoint.MoveTo(PointFromControlSpaceToImageSpace(controlSpaceMousePoint));
                }

                AreaPoints[_movingPointIndex] = movingPoint;
                InvalidateVisual();
                e.Handled = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            InvalidateVisual();
            _isAdjustingPoint = false;
            _isMovingPoint = false;

            base.OnMouseUp(e);
        }

        public BitmapSource? GetClippedImage()
        {
            if (Source is null)
            {
                return null;
            }

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            drawingContext.PushClip(BuildClipGeometry());
            drawingContext.DrawImage(Source, new Rect(0, 0, Source.Width, Source.Height));
            drawingContext.Close();

            var renderTargetBitmap = new RenderTargetBitmap(Source.PixelWidth, Source.PixelHeight, Source.DpiX, Source.DpiY, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            return renderTargetBitmap;
        }
    }
}
