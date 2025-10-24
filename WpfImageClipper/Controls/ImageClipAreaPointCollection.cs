using System.Collections.ObjectModel;

namespace WpfImageClipper.Controls
{
    partial class ImageClipper
    {
        public class ImageClipAreaPointCollection : Collection<ImageClipAreaPoint>
        {
            public ImageClipAreaPointCollection(ImageClipper owner)
            {
                Owner = owner;
            }

            public ImageClipper Owner { get; }

            private void OnCollectionChanged()
            {
                Owner._currentPointIndex = Math.Min(Owner._currentPointIndex, Count - 1);
                Owner.InvalidateVisual();
            }

            protected override void InsertItem(int index, ImageClipAreaPoint item)
            {
                base.InsertItem(index, item);
                OnCollectionChanged();
            }

            protected override void SetItem(int index, ImageClipAreaPoint item)
            {
                base.SetItem(index, item);
                OnCollectionChanged();
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                OnCollectionChanged();
            }

            protected override void ClearItems()
            {
                base.ClearItems();
                OnCollectionChanged();
            }
        }
    }
}
