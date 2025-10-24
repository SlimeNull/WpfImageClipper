using System.Windows;

namespace WpfImageClipper.Controls
{
    public class ImageClipAreaBezierPoint2 : ImageClipAreaBezierPoint1
    {
        /// <summary>
        /// 形成曲线的入点
        /// </summary>
        public Point ControlPoint1 { get; set; }

        public override void MoveTo(Point newPosition)
        {
            var offsetToCP1 = ControlPoint1 - Position;

            base.MoveTo(newPosition);
            ControlPoint1 = Position + offsetToCP1;
        }
    }
}
