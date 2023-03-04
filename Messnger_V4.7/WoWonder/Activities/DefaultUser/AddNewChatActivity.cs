using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using WoWonder.Activities.Base;
using WoWonder.Activities.Call;
using WoWonder.Activities.DefaultUser.Adapters;
using WoWonder.Activities.GroupChat;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SQLite;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.DefaultUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AddNewChatActivity : BaseActivity, TextView.IOnEditorActionListener
    {
        #region Variables Basic

        private AddNewChatAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AutoCompleteTextView SearchView;
        private string SearchText = "";
        private AdView MAdView;
        private Xamarin.Facebook.Ads.InterstitialAd InterstitialAd;

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
                SetContentView(Resource.Layout.AddNewChatLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadContacts();

                if (AppSettings.ShowFbInterstitialAds)
                    InterstitialAd = AdsFacebook.InitInterstitial(this);
                else
                    AdsColony.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                MAdView?.Resume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                MAdView?.Pause();
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                SearchView = FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchView?.SetOnEditorActionListener(this);
                SearchView?.ClearFocus();

                //Change text colors 
                SearchView?.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                SearchView?.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchView.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon?.Parent;
                linearLayoutSearchView?.RemoveView(searchViewIcon);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolBar.Title = " ";

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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new AddNewChatAdapter(this)
                {
                    UserList = new ObservableCollection<Classes.AddNewChatObject>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

                if (AppSettings.EnableChatGroup)
                {
                    MAdapter.UserList.Add(new Classes.AddNewChatObject()
                    {
                        Id = "001",
                        Text = GetText(Resource.String.Lbl_CreateNewGroup),
                        Type = Classes.ItemType.AddGroup,
                    });
                }

                var videoCall = WoWonderTools.CheckAllowedCall(TypeCall.Video);
                if (videoCall)
                {
                    MAdapter.UserList.Add(new Classes.AddNewChatObject()
                    {
                        Id = "002",
                        Text = GetText(Resource.String.Lbl_CreateNewVideoCall),
                        Type = Classes.ItemType.AddCall,
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                if (addEvent)
                {
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                MAdView?.Destroy();
                InterstitialAd?.Destroy();

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                MainScrollEvent = null!;
                SearchText = null!;
                MAdView = null!;
                InterstitialAd = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, AddNewChatAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.AddGroup:
                            StartActivity(new Intent(this, typeof(CreateGroupChatActivity)));
                            break;
                        case Classes.ItemType.AddCall:
                            StartActivity(new Intent(this, typeof(AddNewCallActivity)));
                            break;
                        case Classes.ItemType.User:
                            WoWonderTools.OpenProfile(this, item.User.UserId, item.User);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.UserList.Clear();

                if (AppSettings.EnableChatGroup)
                {
                    MAdapter.UserList.Add(new Classes.AddNewChatObject()
                    {
                        Id = "001",
                        Text = GetText(Resource.String.Lbl_CreateNewGroup),
                        Type = Classes.ItemType.AddGroup,
                    });
                }

                var videoCall = WoWonderTools.CheckAllowedCall(TypeCall.Video);
                if (videoCall)
                {
                    MAdapter.UserList.Add(new Classes.AddNewChatObject()
                    {
                        Id = "002",
                        Text = GetText(Resource.String.Lbl_CreateNewVideoCall),
                        Type = Classes.ItemType.AddCall,
                    });
                }

                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.User?.UserId) && !MainScrollEvent.IsLoading)
                {
                    if (string.IsNullOrEmpty(SearchText))
                    {
                        Task.Factory.StartNew(() => StartApiService(item.User?.UserId));
                    }
                    else
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => StartSearchRequest(item.User?.UserId) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Contacts 

        private void LoadContacts()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                var userList = sqlEntity.Get_MyContact();

                foreach (var data in userList)
                {
                    MAdapter.UserList.Add(new Classes.AddNewChatObject()
                    {
                        Id = data.UserId,
                        User = data,
                        Type = Classes.ItemType.User,
                    });
                }

                MAdapter.NotifyDataSetChanged();

                SwipeRefreshLayout.Refreshing = false;

                var lastIdUser = MAdapter.UserList.LastOrDefault()?.User?.UserId ?? "0";
                StartApiService(lastIdUser);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadContactsAsync(offset) });
        }

        private async Task LoadContactsAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsAsync(UserDetails.UserId, "following", "10", offset);
                if (apiStatus != 200 || respond is not GetFriendsObject result || result.DataFriends == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.DataFriends.Following.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.DataFriends.Following let check = MAdapter.UserList.FirstOrDefault(a => a.User?.UserId == item.UserId) where check == null select item)
                        {
                            MAdapter.UserList.Add(new Classes.AddNewChatObject()
                            {
                                Id = item.UserId,
                                User = item,
                                Type = Classes.ItemType.User,
                            });
                        }

                        if (countList > 0)
                        {
                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                    if (!x.EmptyStateButton.HasOnClickListeners) x.EmptyStateButton.Click += null!;

                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Search)
            {
                try
                {
                    SearchText = v.Text;

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    Search();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

                return true;
            }

            return false;
        }

        private void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        MAdapter.UserList.Clear();
                        MAdapter.NotifyDataSetChanged();

                        SwipeRefreshLayout.Refreshing = false;

                        if (EmptyStateLayout != null)
                            EmptyStateLayout.Visibility = ViewStates.Gone;

                        if (AppSettings.EnableChatGroup)
                        {
                            MAdapter.UserList.Add(new Classes.AddNewChatObject()
                            {
                                Id = "001",
                                Text = GetText(Resource.String.Lbl_CreateNewGroup),
                                Type = Classes.ItemType.AddGroup,
                            });
                        }

                        var videoCall = WoWonderTools.CheckAllowedCall(TypeCall.Video);
                        if (videoCall)
                        {
                            MAdapter.UserList.Add(new Classes.AddNewChatObject()
                            {
                                Id = "002",
                                Text = GetText(Resource.String.Lbl_CreateNewVideoCall),
                                Type = Classes.ItemType.AddCall,
                            });
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => StartSearchRequest() });
                    }
                }
                else
                {
                    Inflated ??= EmptyStateLayout?.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    if (EmptyStateLayout != null)
                    {
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                    SwipeRefreshLayout.Refreshing = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task StartSearchRequest(string offset = "0")
        {
            int countList = MAdapter.UserList.Count;

            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "10"},
                {"user_offset", offset},
                {"gender", UserDetails.SearchGender},
                {"search_key", SearchText},
            };

            var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            if (apiStatus == 200)
            {
                if (respond is GetSearchObject result)
                {
                    var respondList = result.Users.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Users let check = MAdapter.UserList.FirstOrDefault(a => a.User?.UserId == item.UserId) where check == null select item)
                        {
                            MAdapter.UserList.Add(new Classes.AddNewChatObject
                            {
                                Id = item.UserId,
                                User = item,
                                Type = Classes.ItemType.User,
                            });
                        }

                        if (countList > 0)
                        {
                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }
                }
            }
            else
                Methods.DisplayReportResult(this, respond);

            MainScrollEvent.IsLoading = false;

            RunOnUiThread(ShowEmptyPage);
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";

                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}