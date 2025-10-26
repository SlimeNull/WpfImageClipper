using LibBezierCurve.Internal;

namespace LibBezierCurve
{
    public struct CubicBezierCurve : IBezierCurve
    {
        public CubicBezierCurve(
            double controlPoint1X,
            double controlPoint1Y,
            double controlPoint2X,
            double controlPoint2Y,
            double controlPoint3X,
            double controlPoint3Y,
            double controlPoint4X,
            double controlPoint4Y)
        {
            StartPointX = controlPoint1X;
            StartPointY = controlPoint1Y;
            ControlPoint1X = controlPoint2X;
            ControlPoint1Y = controlPoint2Y;
            ControlPoint2X = controlPoint3X;
            ControlPoint2Y = controlPoint3Y;
            EndPointX = controlPoint4X;
            EndPointY = controlPoint4Y;
        }

        public double StartPointX { get; }
        public double StartPointY { get; }
        public double ControlPoint1X { get; }
        public double ControlPoint1Y { get; }
        public double ControlPoint2X { get; }
        public double ControlPoint2Y { get; }
        public double EndPointX { get; }
        public double EndPointY { get; }

        private IEnumerable<double> ResolveT(double controlPoint1, double controlPoint2, double controlPoint3, double controlPoint4, double value)
        {
            // 三元一次方程标准形式

            var a = -controlPoint1 + 3 * controlPoint2 - 3 * controlPoint3 + controlPoint4;
            var b = 3 * (controlPoint1 - 2 * controlPoint2 + controlPoint3);
            var c = -3 * controlPoint1 + 3 * controlPoint2;
            var d = controlPoint1;

            foreach (var t in MathUtils.ResolveEquation(a, b, c, d - value))
            {
                if (t >= 0 && t <= 1)
                {
                    yield return t;
                }
            }
        }

        public void Sample(double t, out double x, out double y)
        {
            var iterate1Point1X = MathUtils.Lerp(StartPointX, ControlPoint1X, t);
            var iterate1Point2X = MathUtils.Lerp(ControlPoint1X, ControlPoint2X, t);
            var iterate1Point3X = MathUtils.Lerp(ControlPoint2X, EndPointX, t);
            var iterate1Point1Y = MathUtils.Lerp(StartPointY, ControlPoint1Y, t);
            var iterate1Point2Y = MathUtils.Lerp(ControlPoint1Y, ControlPoint2Y, t);
            var iterate1Point3Y = MathUtils.Lerp(ControlPoint2Y, EndPointY, t);

            var iterate2Point1X = MathUtils.Lerp(iterate1Point1X, iterate1Point2X, t);
            var iterate2Point1Y = MathUtils.Lerp(iterate1Point1Y, iterate1Point2Y, t);
            var iterate2Point2X = MathUtils.Lerp(iterate1Point2X, iterate1Point3X, t);
            var iterate2Point2Y = MathUtils.Lerp(iterate1Point2Y, iterate1Point3Y, t);

            x = MathUtils.Lerp(iterate2Point1X, iterate2Point2X, t);
            y = MathUtils.Lerp(iterate2Point1Y, iterate2Point2Y, t);
        }

        public bool HitTest(double x, double y, double threshold, out double t)
        {
            // 使用牛顿迭代法找到曲线上距离 (x, y) 最近的点
            // 目标是最小化距离的平方: D(t) = (Bx(t) - x)^2 + (By(t) - y)^2
            // 其导数为零时取得最小值: D'(t) = 2(Bx(t) - x)B'x(t) + 2(By(t) - y)B'y(t) = 0

            // 尝试多个初始点以避免局部最小值
            double bestT = 0;
            double minDistance = double.PositiveInfinity;

            // 采样多个初始点
            for (int i = 0; i <= 10; i++)
            {
                double tCandidate = i / 10.0;

                // 牛顿迭代法
                for (int iter = 0; iter < 10; iter++)
                {
                    Sample(tCandidate, out var px, out var py);

                    // 计算一阶导数 B'(t)
                    var t2 = tCandidate * tCandidate;
                    var mt = 1 - tCandidate;
                    var mt2 = mt * mt;

                    var bx = 3 * mt2 * (ControlPoint1X - StartPointX) +
                             6 * mt * tCandidate * (ControlPoint2X - ControlPoint1X) +
                             3 * t2 * (EndPointX - ControlPoint2X);

                    var by = 3 * mt2 * (ControlPoint1Y - StartPointY) +
                             6 * mt * tCandidate * (ControlPoint2Y - ControlPoint1Y) +
                             3 * t2 * (EndPointY - ControlPoint2Y);

                    // D'(t) = 2(Bx(t) - x)B'x(t) + 2(By(t) - y)B'y(t)
                    var derivative = 2 * ((px - x) * bx + (py - y) * by);

                    if (Math.Abs(derivative) < 1e-6)
                    {
                        break;
                    }

                    // 计算二阶导数 B''(t)
                    var bxx = 6 * mt * (ControlPoint2X - 2 * ControlPoint1X + StartPointX) +
                              6 * tCandidate * (EndPointX - 2 * ControlPoint2X + ControlPoint1X);
                    var byy = 6 * mt * (ControlPoint2Y - 2 * ControlPoint1Y + StartPointY) +
                              6 * tCandidate * (EndPointY - 2 * ControlPoint2Y + ControlPoint1Y);

                    // D''(t) = 2[B'x(t)^2 + B'y(t)^2 + (Bx(t) - x)B''x(t) + (By(t) - y)B''y(t)]
                    var secondDerivative = 2 * (bx * bx + by * by + (px - x) * bxx + (py - y) * byy);

                    if (Math.Abs(secondDerivative) < 1e-6)
                    {
                        break;
                    }

                    // 牛顿迭代步骤
                    var tNext = tCandidate - derivative / secondDerivative;
                    tNext = Math.Max(0, Math.Min(1, tNext)); // 限制在 [0, 1] 范围内

                    if (Math.Abs(tNext - tCandidate) < 1e-6)
                    {
                        tCandidate = tNext;
                        break;
                    }

                    tCandidate = tNext;
                }

                // 检查这个候选点的距离
                Sample(tCandidate, out var candidatePx, out var candidatePy);
                var dx = candidatePx - x;
                var dy = candidatePy - y;
                var dist = dx * dx + dy * dy;

                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestT = tCandidate;
                }
            }

            // 也检查端点
            Sample(0, out var startX, out var startY);
            var distStart = (startX - x) * (startX - x) + (startY - y) * (startY - y);

            Sample(1, out var endX, out var endY);
            var distEnd = (endX - x) * (endX - x) + (endY - y) * (endY - y);

            if (distStart < minDistance)
            {
                minDistance = distStart;
                bestT = 0;
            }

            if (distEnd < minDistance)
            {
                minDistance = distEnd;
                bestT = 1;
            }

            t = bestT;
            return Math.Sqrt(minDistance) <= threshold;
        }

        public IEnumerable<(double, double)> EnumerateControlPoints()
        {
            yield return (StartPointX, StartPointY);
            yield return (ControlPoint1X, ControlPoint1Y);
            yield return (ControlPoint2X, ControlPoint2Y);
            yield return (EndPointX, EndPointY);
        }

        /// <summary>
        /// 在参数 t 处细分三次贝塞尔曲线，返回两段新的三次贝塞尔曲线
        /// 使用 De Casteljau 算法保证细分后的曲线形状与原曲线完全一致
        /// </summary>
        /// <param name="t">细分参数，范围 [0, 1]</param>
        /// <param name="leftCurve">左侧曲线 [0, t]</param>
        /// <param name="rightCurve">右侧曲线 [t, 1]</param>
        public void Subdivide(double t, out CubicBezierCurve leftCurve, out CubicBezierCurve rightCurve)
        {
            // De Casteljau 算法
            // 第一层插值
            var p01X = MathUtils.Lerp(StartPointX, ControlPoint1X, t);
            var p01Y = MathUtils.Lerp(StartPointY, ControlPoint1Y, t);
            
            var p12X = MathUtils.Lerp(ControlPoint1X, ControlPoint2X, t);
            var p12Y = MathUtils.Lerp(ControlPoint1Y, ControlPoint2Y, t);
        
            var p23X = MathUtils.Lerp(ControlPoint2X, EndPointX, t);
            var p23Y = MathUtils.Lerp(ControlPoint2Y, EndPointY, t);

            // 第二层插值
            var p012X = MathUtils.Lerp(p01X, p12X, t);
            var p012Y = MathUtils.Lerp(p01Y, p12Y, t);
            
            var p123X = MathUtils.Lerp(p12X, p23X, t);
            var p123Y = MathUtils.Lerp(p12Y, p23Y, t);

            // 第三层插值 - 曲线上的点
            var p0123X = MathUtils.Lerp(p012X, p123X, t);
            var p0123Y = MathUtils.Lerp(p012Y, p123Y, t);

            // 左侧曲线: P0, P01, P012, P0123
            leftCurve = new CubicBezierCurve(
                StartPointX, StartPointY,      // 起点
                p01X, p01Y,// 控制点1
                p012X, p012Y,       // 控制点2
                p0123X, p0123Y);     // 终点

            // 右侧曲线: P0123, P123, P23, P3
            rightCurve = new CubicBezierCurve(
                p0123X, p0123Y,        // 起点
                p123X, p123Y,  // 控制点1
                p23X, p23Y,            // 控制点2
                EndPointX, EndPointY);          // 终点
     }
    }

}
