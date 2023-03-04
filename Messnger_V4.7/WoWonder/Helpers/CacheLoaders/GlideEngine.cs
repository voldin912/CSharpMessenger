using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Bumptech.Glide;
using Com.Zhihu.Matisse.Engine;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace WoWonder.Helpers.CacheLoaders
{
    public class GlideEngine : Object, IImageEngine
    {
        public void LoadGifImage(Context context, int resizeX, int resizeY, ImageView imageView, Uri uri)
        {
            Glide.With(context)
                .AsGif()
                .Load(uri)
                .Override(resizeX, resizeY)
                .SetPriority(Priority.High)
                .Into(imageView);
        }

        public void LoadGifThumbnail(Context context, int resize, Drawable placeholder, ImageView imageView, Uri uri)
        {
            Glide.With(context)
                .AsBitmap()
                .Load(uri)
                .Override(resize, resize)
                .Placeholder(placeholder)
                .CenterCrop()
                .Into(imageView);
        }

        public void LoadImage(Context context, int resizeX, int resizeY, ImageView imageView, Uri uri)
        {
            Glide.With(context)
                .Load(uri)
                .Override(resizeX, resizeY)
                .SetPriority(Priority.High)
                .FitCenter()
                .Into(imageView);
        }

        public void LoadThumbnail(Context context, int resize, Drawable placeholder, ImageView imageView, Uri uri)
        {
            Glide.With(context)
                .AsBitmap() // some .jpeg files are actually gif
                .Load(uri)
                .Override(resize, resize)
                .Placeholder(placeholder)
                .CenterCrop()
                .Into(imageView);
        }

        public bool SupportAnimatedGif()
        {
            return true;
        }
    }
}