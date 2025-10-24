using System.Windows;

namespace WpfImageClipper.Controls
{
    /// <summary>
    /// 裁剪区域的一个点
    /// </summary>
    public abstract class ImageClipAreaPoint
    {
        public Point Position { get; set; }

        public virtual void MoveTo(Point newPosition)
        {
            Position = newPosition;
        }
    }
}
