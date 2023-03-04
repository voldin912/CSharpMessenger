using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.WalkTroutPage.Adapters
{
    public class WalkTroutPagerAdapter : PagerAdapter
    {
        private readonly Activity ActivityContext;
        private readonly List<Classes.ModelsWalkTroutPager> ListPage;

        public WalkTroutPagerAdapter(Activity context, List<Classes.ModelsWalkTroutPager> listPage)
        {
            try
            {
                ActivityContext = context;
                ListPage = listPage;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            try
            {
                var layoutInflater = (LayoutInflater) ActivityContext.GetSystemService(Context.LayoutInflaterService);

                View view = layoutInflater?.Inflate(Resource.Layout.Style_WalkTroutPager, container, false);
                if (view != null)
                {
                    var title = (TextView) view.FindViewById(Resource.Id.title);
                    var description = (TextView)view.FindViewById(Resource.Id.description);
                    var image = (ImageView)view.FindViewById(Resource.Id.image);

                    var item = ListPage[position];
                    if (item != null)
                    {
                        image.SetImageResource(item.Image);
                        title.Text = item.Title;
                        description.Text = item.Description;
                    }

                    container.AddView(view);
                    return view; 
                } 
                return base.InstantiateItem(container, position); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.InstantiateItem(container, position);
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }

        public override int Count => ListPage.Count;

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                View view = (View) @object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                base.DestroyItem(container, position, @object);
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}