namespace LibBezierCurve
{
    public interface IBezierCurve
    {
        IEnumerable<(double, double)> EnumerateControlPoints();

        void Sample(double t, out double x, out double y);
        bool HitTest(double x, double y, double maxDistance, out double t);
    }

}
