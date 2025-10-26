using LibBezierCurve.Internal;

namespace LibBezierCurve
{
    public struct QuadraticBezierCurve : IBezierCurve
    {
        public QuadraticBezierCurve(
            double controlPoint1X,
            double controlPoint1Y,
            double controlPoint2X,
            double controlPoint2Y,
            double controlPoint3X,
            double controlPoint3Y)
        {
            StartPointX = controlPoint1X;
            StartPointY = controlPoint1Y;
            ControlPointX = controlPoint2X;
            ControlPointY = controlPoint2Y;
            EndPointX = controlPoint3X;
            EndPointY = controlPoint3Y;
        }

        public double StartPointX { get; }
        public double StartPointY { get; }
        public double ControlPointX { get; }
        public double ControlPointY { get; }
        public double EndPointX { get; }
        public double EndPointY { get; }

        private IEnumerable<double> ResolveT(double controlPoint1, double controlPoint2, double controlPoint3, double value)
        {
            // 二元一次方程标准形式

            var a = controlPoint1 - 2 * controlPoint2 + controlPoint3;
            var b = -2 * controlPoint1 + 2 * controlPoint2;
            var c = controlPoint1;

            foreach (var t in MathUtils.ResolveEquation(a, b, c - value))
            {
                if (t >= 0 && t <= 1)
                {
                    yield return t;
                }
            }
        }

        public void Sample(double t, out double x, out double y)
        {
            var iterate1Point1X = MathUtils.Lerp(StartPointX, ControlPointX, t);
            var iterate1Point2X = MathUtils.Lerp(ControlPointX, EndPointX, t);
            var iterate1Point1Y = MathUtils.Lerp(StartPointY, ControlPointY, t);
            var iterate1Point2Y = MathUtils.Lerp(ControlPointY, EndPointY, t);

            x = MathUtils.Lerp(iterate1Point1X, iterate1Point2X, t);
            y = MathUtils.Lerp(iterate1Point1Y, iterate1Point2Y, t);
        }

        public bool HitTest(double x, double y, double threshold, out double t)
        {
            var rootFromX = double.NaN;
            var rootFromY = double.NaN;

            var minDiffFromX = double.PositiveInfinity;
            var minDiffFromY = double.PositiveInfinity;

            foreach (var tMaybe in ResolveT(StartPointX, ControlPointX, EndPointX, x))
            {
                Sample(tMaybe, out _, out var yFromT);
                var diff = Math.Abs(y - yFromT);

                if (diff <= threshold &&
                    diff < minDiffFromX)
                {
                    minDiffFromX = diff;
                    rootFromX = tMaybe;
                }
            }

            foreach (var tMaybe in ResolveT(StartPointY, ControlPointY, EndPointY, y))
            {
                Sample(tMaybe, out var xFromT, out _);
                var diff = Math.Abs(x - xFromT);

                if (diff <= threshold &&
                    diff < minDiffFromY)
                {
                    minDiffFromY = diff;
                    rootFromY = tMaybe;
                }
            }

            if (double.IsNaN(rootFromX))
            {
                t = rootFromY;
                return !double.IsNaN(rootFromY);
            }
            else if (double.IsNaN(rootFromY))
            {
                t = rootFromX;
                return true;
            }

            t = (rootFromX + rootFromY) / 2;
            return true;
        }

        public IEnumerable<(double, double)> EnumerateControlPoints()
        {
            yield return (StartPointX, StartPointY);
            yield return (ControlPointX, ControlPointY);
            yield return (EndPointX, EndPointY);
        }

        /// <summary>
        /// 在参数 t 处细分二次贝塞尔曲线，返回两段新的二次贝塞尔曲线
        /// 使用 De Casteljau 算法保证细分后的曲线形状与原曲线完全一致
        /// </summary>
        /// <param name="t">细分参数，范围 [0, 1]</param>
        /// <param name="leftCurve">左侧曲线 [0, t]</param>
        /// <param name="rightCurve">右侧曲线 [t, 1]</param>
        public void Subdivide(double t, out QuadraticBezierCurve leftCurve, out QuadraticBezierCurve rightCurve)
        {
            // De Casteljau 算法
            // 第一层插值
            var p01X = MathUtils.Lerp(StartPointX, ControlPointX, t);
            var p01Y = MathUtils.Lerp(StartPointY, ControlPointY, t);

            var p12X = MathUtils.Lerp(ControlPointX, EndPointX, t);
            var p12Y = MathUtils.Lerp(ControlPointY, EndPointY, t);

            // 第二层插值 - 曲线上的点
            var p012X = MathUtils.Lerp(p01X, p12X, t);
            var p012Y = MathUtils.Lerp(p01Y, p12Y, t);

            // 左侧曲线: P0, P01, P012
            leftCurve = new QuadraticBezierCurve(
                StartPointX, StartPointY,    // 起点
                p01X, p01Y,         // 控制点
                p012X, p012Y);       // 终点

            // 右侧曲线: P012, P12, P2
            rightCurve = new QuadraticBezierCurve(
                p012X, p012Y,      // 起点
                p12X, p12Y,       // 控制点
                EndPointX, EndPointY);       // 终点
        }
    }

}
