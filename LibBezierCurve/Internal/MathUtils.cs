using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("TestConsole")]

namespace LibBezierCurve.Internal
{
    internal static class MathUtils
    {
        /// <summary>
        /// 解一元一次方程
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static IEnumerable<double> CoreResolveLinearEquation(double a, double b)
        {
            if (b == 0)
            {
                yield break;
            }

            yield return -b / a;
        }

        /// <summary>
        /// 解二元一次方程
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IEnumerable<double> CoreResolveQuadraticEquation(double a, double b, double c)
        {
            var deltaT = Math.Pow(b, 2) - 4 * a * c;

            if (deltaT < 0)
            {
                // no real number root
                yield break;
            }

            var sqrtDeltaT = Math.Sqrt(deltaT);
            yield return (-b + sqrtDeltaT) / (2 * a);

            if (deltaT != 0)
            {
                yield return (-b - sqrtDeltaT) / (2 * a);
            }
        }

        /// <summary>
        /// 解三元一次方程
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static IEnumerable<double> CoreResolveCubicEquation(double a, double b, double c, double d)
        {
            static double CubeRoot(double x)
            {
                if (x < 0)
                {
                    return -Math.Pow(-x, 1.0 / 3);
                }
                else
                {
                    return Math.Pow(x, 1.0 / 3);
                }
            }

            double s = b / (3 * a);
            double p = (3 * a * c - b * b) / (3 * a * a);
            double q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);
            double delta = (q / 2) * (q / 2) + (p / 3) * (p / 3) * (p / 3);

            if (delta > double.Epsilon)
            {
                // 一个实根和两个复数根，只返回实根
                double sqrtDelta = Math.Sqrt(delta);
                double u = CubeRoot(-q / 2 + sqrtDelta);
                double v = CubeRoot(-q / 2 - sqrtDelta);
                double y = u + v;
                yield return y - s;
            }
            else if (delta < -double.Epsilon)
            {
                // 三个不同的实根，使用三角函数形式
                double sqrt_p_3 = Math.Sqrt(-p / 3);
                double denominator = sqrt_p_3 * sqrt_p_3 * sqrt_p_3;
                double cosTheta = (-q / 2) / denominator;
                cosTheta = Math.Max(Math.Min(cosTheta, 1.0), -1.0); // 确保acos参数有效
                double theta = Math.Acos(cosTheta);
                double temp = 2 * sqrt_p_3;

                yield return temp * Math.Cos(theta / 3) - s;
                yield return temp * Math.Cos((theta + 2 * Math.PI) / 3) - s;
                yield return temp * Math.Cos((theta + 4 * Math.PI) / 3) - s;
            }
            else
            {
                // Delta近似为0的情况
                bool pIsZero = Math.Abs(p) < double.Epsilon;
                bool qIsZero = Math.Abs(q) < double.Epsilon;

                if (pIsZero && qIsZero)
                {
                    // 三重根
                    yield return -s;
                }
                else
                {
                    // 一个单根和一个双重根
                    double u = CubeRoot(-q / 2);
                    yield return 2 * u - s;
                    yield return -u - s;
                }
            }
        }

        public static IEnumerable<double> ResolveEquation(double a, double b)
        {
            return CoreResolveLinearEquation(a, b);
        }

        public static IEnumerable<double> ResolveEquation(double a, double b, double c)
        {
            if (a != 0)
            {
                return CoreResolveQuadraticEquation(a, b, c);
            }
            else
            {
                return ResolveEquation(b, c);
            }
        }

        public static IEnumerable<double> ResolveEquation(double a, double b, double c, double d)
        {
            if (a != 0)
            {
                return CoreResolveCubicEquation(a, b, c, d);
            }
            else
            {
                return ResolveEquation(b, c, d);
            }
        }

        /// <summary>
        /// 计算点到直线的垂足（投影点）。
        /// </summary>
        /// <param name="linePoint1x">直线上第一个点的X坐标</param>
        /// <param name="linePoint1y">直线上第一个点的Y坐标</param>
        /// <param name="linePoint2x">直线上第二个点的X坐标</param>
        /// <param name="linePoint2y">直线上第二个点的Y坐标</param>
        /// <param name="pointX">线外点的X坐标</param>
        /// <param name="pointY">线外点的Y坐标</param>
        /// <param name="resultX">计算出的垂足的X坐标</param>
        /// <param name="resultY">计算出的垂足的Y坐标</param>
        public static void GetPerpendicularProjection(
            double linePoint1x, double linePoint1y,
            double linePoint2x, double linePoint2y,
            double pointX, double pointY,
            out double resultX, out double resultY)
        {
            // 向量表示：
            // P1: linePoint1
            // P2: linePoint2
            // A: a (the point)
            // v: 向量 P1 -> P2
            // w: 向量 P1 -> A
            double vx = linePoint2x - linePoint1x;
            double vy = linePoint2y - linePoint1y;
            double wx = pointX - linePoint1x;
            double wy = pointY - linePoint1y;
            // 计算点积 (dot product)
            double dot_wv = wx * vx + wy * vy;
            double dot_vv = vx * vx + vy * vy;
            // 处理特殊情况：线段的两个端点重合
            // 此时无法定义一条唯一的线，我们将垂足定为这个唯一的点
            if (dot_vv == 0)
            {
                resultX = linePoint1x;
                resultY = linePoint1y;
                return;
            }
            // 计算投影系数 t
            // t = (w · v) / |v|^2
            // |v|^2 就是 v 和自身的点积 dot_vv
            double t = dot_wv / dot_vv;
            // 计算垂足 C 的坐标
            // C = P1 + t * v
            resultX = linePoint1x + t * vx;
            resultY = linePoint1y + t * vy;
        }

        public static double Lerp(double x, double y, double t)
        {
            return x * (1 - t) + y * t;
        }

        public static double Length(double vectorX, double vectorY)
        {
            double squaredDistance = (vectorX * vectorX) + (vectorY * vectorY);

            double distance = Math.Sqrt(squaredDistance);
            return distance;
        }

        public static double Distance(
            double point1X, double point1Y,
            double point2X, double point2Y)
        {
            return Length(point1X - point2X, point1Y - point2Y);
        }

        /// <summary>
        /// 从 <paramref name="sortedValues1"/> 中和 <paramref name="sortedValues2"/> 中分别取一个值, 并且保证这两个值之间的差是所有组合中的最小值. 最后返回它们的平均值
        /// </summary>
        /// <param name="sortedValues1"></param>
        /// <param name="sortedValues2"></param>
        /// <returns></returns>
        public static double GetAverage(IList<double> sortedValues1, List<double> sortedValues2)
        {
            int i = 0, j = 0;
            double minDiff = double.MaxValue;
            double result = 0;

            // 双指针法遍历
            while (i < sortedValues1.Count && j < sortedValues2.Count)
            {
                double val1 = sortedValues1[i];
                double val2 = sortedValues2[j];

                // 计算差值
                double diff = Math.Abs(val1 - val2);

                // 如果当前差值比之前记录的最小差值还小，更新最小差值和结果
                if (diff < minDiff)
                {
                    minDiff = diff;
                    result = (val1 + val2) / 2.0;
                }

                // 移动指针
                if (val1 < val2)
                    i++;  // val1 较小，移动 sortedValues1 的指针
                else
                    j++;  // val2 较小或相等，移动 sortedValues2 的指针
            }

            return result;
        }
    }
}
