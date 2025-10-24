using System.Windows;

namespace WpfImageClipper.Controls
{
    /// <summary>
    /// 裁剪区域的一个贝塞尔点 (只有一个出点)
    /// </summary>
    public class ImageClipAreaBezierPoint1 : ImageClipAreaPoint
    {
        /// <summary>
        /// 形成曲线的出点
        /// </summary>
        public Point ControlPoint2 { get; set; }

        public override void MoveTo(Point newPosition)
        {
            var offsetToCP2 = ControlPoint2 - Position;

            base.MoveTo(newPosition);
            ControlPoint2 = Position + offsetToCP2;
        }
    }
}
