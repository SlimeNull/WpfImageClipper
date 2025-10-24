
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
            ControlPoint1X = controlPoint1X;
            ControlPoint1Y = controlPoint1Y;
            ControlPoint2X = controlPoint2X;
            ControlPoint2Y = controlPoint2Y;
            ControlPoint3X = controlPoint3X;
            ControlPoint3Y = controlPoint3Y;
        }

        public double ControlPoint1X { get; }
        public double ControlPoint1Y { get; }
        public double ControlPoint2X { get; }
        public double ControlPoint2Y { get; }
        public double ControlPoint3X { get; }
        public double ControlPoint3Y { get; }

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
            var iterate1Point1X = MathUtils.Lerp(ControlPoint1X, ControlPoint2X, t);
            var iterate1Point2X = MathUtils.Lerp(ControlPoint2X, ControlPoint3X, t);
            var iterate1Point1Y = MathUtils.Lerp(ControlPoint1Y, ControlPoint2Y, t);
            var iterate1Point2Y = MathUtils.Lerp(ControlPoint2Y, ControlPoint3Y, t);

            x = MathUtils.Lerp(iterate1Point1X, iterate1Point2X, t);
            y = MathUtils.Lerp(iterate1Point1Y, iterate1Point2Y, t);
        }

        public bool HitTest(double x, double y, double threshold, out double t)
        {
            var rootFromX = double.NaN;
            var rootFromY = double.NaN;

            var minDiffFromX = double.PositiveInfinity;
            var minDiffFromY = double.PositiveInfinity;

            foreach (var tMaybe in ResolveT(ControlPoint1X, ControlPoint2X, ControlPoint3X, x))
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

            foreach (var tMaybe in ResolveT(ControlPoint1Y, ControlPoint2Y, ControlPoint3Y, y))
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
            yield return (ControlPoint1X, ControlPoint1Y);
            yield return (ControlPoint2X, ControlPoint2Y);
            yield return (ControlPoint3X, ControlPoint3Y);
        }
    }

}
