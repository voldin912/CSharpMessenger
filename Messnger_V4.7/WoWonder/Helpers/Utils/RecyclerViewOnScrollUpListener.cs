using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using System;
using WoWonder.Adapters;

namespace WoWonder.Helpers.Utils
{
    public class RecyclerViewOnScrollUpListener : RecyclerView.OnScrollListener
    {
        public delegate void LoadMoreEventHandler(object sender, EventArgs e);

        public event LoadMoreEventHandler LoadMoreEvent;

        private readonly Holders.MsgPreCachingLayoutManager LayoutManager;
        private readonly FloatingActionButton FabScrollDown;
        private static readonly int HideThreshold = 20;
        private int ScrolledDistance;
        private bool ControlsVisible = true;
         
        private int firstVisibleInListview;

        public RecyclerViewOnScrollUpListener(Holders.MsgPreCachingLayoutManager layoutManager, FloatingActionButton fabScrollDown)
        {
            LayoutManager = layoutManager;
            FabScrollDown = fabScrollDown;
            firstVisibleInListview = LayoutManager.FindFirstVisibleItemPosition();
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            try
            {
                base.OnScrolled(recyclerView, dx, dy);

                var visibleItemCount = recyclerView.ChildCount;
                var totalItemCount = recyclerView.GetAdapter().ItemCount;

                if (ScrolledDistance > HideThreshold && ControlsVisible)
                {
                    FabScrollDown.Visibility = ViewStates.Gone;
                    ControlsVisible = false;
                    ScrolledDistance = 0;
                }
                else if (ScrolledDistance < -HideThreshold && !ControlsVisible)
                {
                    FabScrollDown.Visibility = ViewStates.Visible;
                    ControlsVisible = true;
                    ScrolledDistance = 0;
                }

                if (ControlsVisible && dy > 0 || !ControlsVisible && dy < 0)
                {
                    ScrolledDistance += dy;
                }
                 
                //int currentFirstVisible = LayoutManager.FindFirstVisibleItemPosition();
                //if (currentFirstVisible > firstVisibleInListview)
                //{
                //    //scroll up 
                //    //Load More  from API Request
                //    LoadMoreEvent?.Invoke(this, null);
                //}
                //else
                //{
                //    //scroll down 
                //}
                //firstVisibleInListview = currentFirstVisible;

                var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
                if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
                {
                    //Load More  from API Request
                    LoadMoreEvent?.Invoke(this, null);
                    //Start Load More messages From Database
                }
                else
                {
                    //if (SwipeRefreshLayout.Refreshing)
                    //{
                    //    SwipeRefreshLayout.Refreshing = false;
                    //    SwipeRefreshLayout.Enabled = false;
                    //}
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}