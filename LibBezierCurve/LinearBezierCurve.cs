using LibBezierCurve.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBezierCurve
{
    public struct LinearBezierCurve : IBezierCurve
    {
        public LinearBezierCurve(
            double startPointX, double startPointY,
            double endPointX, double endPointY)
        {
            StartPointX = startPointX;
            StartPointY = startPointY;
            EndPointX = endPointX;
            EndPointY = endPointY;
        }

        public double StartPointX { get; }
        public double StartPointY { get; }
        public double EndPointX { get; }
        public double EndPointY { get; }


        public IEnumerable<(double, double)> EnumerateControlPoints()
        {
            yield break;
        }

        public bool HitTest(double x, double y, double maxDistance, out double t)
        {
            MathUtils.GetPerpendicularProjection(StartPointX, StartPointY, EndPointX, EndPointY, x, y, out var resultX, out var resultY);
            var distanceToPoint = MathUtils.Distance(x, y, resultX, resultY);
            if (distanceToPoint > maxDistance)
            {
                t = default;
                return false;
            }

            var distanceToStart = MathUtils.Distance(StartPointX, StartPointY, resultX, resultY);
            var length = MathUtils.Distance(StartPointX, StartPointY, EndPointX, EndPointY);

            t = distanceToStart / length;
            return true;
        }

        public void Sample(double t, out double x, out double y)
        {
            x = MathUtils.Lerp(StartPointX, EndPointX, t);
            y = MathUtils.Lerp(StartPointY, EndPointY, t);
        }
    }
}
