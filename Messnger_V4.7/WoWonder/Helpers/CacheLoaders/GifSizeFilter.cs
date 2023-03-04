using Android.Content;
using Com.Zhihu.Matisse;
using Com.Zhihu.Matisse.Filter;
using Com.Zhihu.Matisse.Internal.Entity;
using System.Collections.Generic;

namespace WoWonder.Helpers.CacheLoaders
{
    public class GifSizeFilter : Filter
    {
        private int mMinWidth;
        private int mMinHeight;
        private int mMaxSize;

        public GifSizeFilter(int minWidth, int minHeight, int maxSizeInBytes)
        {
            mMinWidth = minWidth;
            mMinHeight = minHeight;
            mMaxSize = maxSizeInBytes;
        }

        protected override ICollection<MimeType> ConstraintTypes()
        {
            //return new List<MimeType>()
            //{
            //    MimeType.Gif
            //};
            return null;
        }

        public override IncapableCause InvokeFilter(Context context, Item item)
        {
            return null;
            //if (!NeedFiltering(context, item))
            //    return null;

            //var size = PhotoMetadataUtils.GetBitmapBound(context.ContentResolver, item.ContentUri);
            //if (size.X < mMinWidth || size.Y < mMinHeight || item.Size > mMaxSize)
            //{
            //    return new IncapableCause(IncapableCause.Dialog, "error_gif");
            //}
            //return null;
        }
    }
}