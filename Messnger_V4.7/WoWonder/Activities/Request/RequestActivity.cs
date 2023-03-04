using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using System;
using Android.Graphics;
using AndroidX.AppCompat.Content.Res;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using WoWonder.Activities.Base;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using WoWonder.Activities.Request.Fragment;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WoWonder.Helpers.Model;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using System.Collections.Generic;
using Android.Widget;
using WoWonder.Helpers.Controller;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Request
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RequestActivity : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private ViewPager2 ViewPager;
        private FriendRequestFragment FriendRequestTab;
        private GroupRequestFragment GroupRequestTab;
        private TabLayout TabLayout;
       
        private static RequestActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.RequestMainLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 

                AdsGoogle.Ad_Interstitial(this);
                StartApiService(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);
                 
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_Request);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.ParseColor("#060606"));
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static RequestActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        private void DestroyBasic()
        {
            try
            {
                ViewPager = null!;
                TabLayout = null!; 
                Instance = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                FriendRequestTab = new FriendRequestFragment();
                GroupRequestTab = new GroupRequestFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(FriendRequestTab, GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_FollowRequest : Resource.String.Lbl_FriendRequest));
                Adapter.AddFragment(GroupRequestTab, GetText(Resource.String.Lbl_GroupRequest));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
   
        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadGeneralData });
        }

        //Get General Data Using Api >> Friend Requests and Group Chat Requests
        private async Task LoadGeneralData()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var fetch = "friend_requests";

                    if (AppSettings.EnableChatGroup)
                        fetch += ",group_chat_requests";

                    var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(false, UserDetails.OnlineUsers, UserDetails.DeviceId, "1", fetch);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            // FriendRequests
                            var respondListFriendRequests = result?.FriendRequests?.Count;
                            if (respondListFriendRequests > 0)
                            {
                                ListUtils.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);
                                FriendRequestTab?.MAdapter?.NotifyDataSetChanged();
                            }
                            FriendRequestTab?.ShowEmptyPage();

                            // Group Requests
                            if (AppSettings.EnableChatGroup)
                            {
                                var respondListGroupRequests = result?.GroupChatRequests?.Count;
                                if (respondListGroupRequests > 0)
                                {
                                    ListUtils.GroupRequestsList = new ObservableCollection<GroupChatRequest>(result.GroupChatRequests);
                                    GroupRequestTab?.MAdapter?.NotifyDataSetChanged();
                                }
                                GroupRequestTab?.ShowEmptyPage();
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}