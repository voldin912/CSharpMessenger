using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.ViewPager2.Widget;
using Com.Google.Android.Play.Core.Install.Model;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.Editor;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Request;
using WoWonder.Activities.Search;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Story;
using WoWonder.Activities.Tab.Fragment;
using WoWonder.Adapters;
using WoWonder.Frameworks.Floating;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using static Com.Canhub.Cropper.CropImageView;
using ActivityResult = Com.Google.Android.Play.Core.Install.Model.ActivityResult;
using Console = System.Console;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ChatTabbedMainActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables

        private static ChatTabbedMainActivity Instance;

        private LinearLayout AppBarLayout;

        public ViewPager2 ViewPager;
        private MainTabAdapter TabAdapter;
        public LastChatFragment ChatTab2;
        public TabChatFragment ChatTab;
        public LastStoriesFragment LastStoriesTab;
        public LastCallsFragment LastCallsTab;

        private TextView TxtAppName;
        private ImageView DiscoverImageView, SearchImageView, FriendsImageView;

        private BottomNavigationTab BottomNavigationTab;

        public static bool RunCall = false;
        private PowerManager.WakeLock Wl;
        private Handler ExitHandler;
        private bool RecentlyBackPressed;
        private string ImageType;

        public InitFloating Floating;
        private DialogGalleryController GalleryController;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Task.Factory.StartNew(() => MainApplication.GetInstance().SecondRunExcite());
                base.OnCreate(savedInstanceState);
                Delegate.SetLocalNightMode(WoWonderTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                Instance = this;
                Floating = new InitFloating();
                RunCall = false;

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainPage);

                //Get Value And Set Toolbar
                InitComponent();
                AddFragmentsTabs();
                InitBackPressed("ChatTabbedMainActivity");
                GalleryController = new DialogGalleryController(this, this);

                Task.Factory.StartNew(GetGeneralAppData);

                SetService();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }
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
                LastStoriesTab?.PublisherAdView?.Resume();
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
                LastStoriesTab?.PublisherAdView?.Pause();
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
                LastStoriesTab?.PublisherAdView?.Destroy();

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken);

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Back Pressed

        public void BackPressed()
        {
            try
            {
                ExitHandler ??= new Handler(Looper.MainLooper);
                if (RecentlyBackPressed)
                {
                    ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                    RecentlyBackPressed = false;
                    MoveTaskToBack(true);
                    Finish();
                }
                else
                {
                    RecentlyBackPressed = true;
                    ToastUtils.ShowToast(this, GetString(Resource.String.press_again_exit), ToastLength.Long);
                    ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TabAdapter = new MainTabAdapter(this);
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                AppBarLayout = FindViewById<LinearLayout>(Resource.Id.appbar);
                AppBarLayout.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.Black : Color.White);

                TxtAppName = FindViewById<TextView>(Resource.Id.appName);
                TxtAppName.Text = AppSettings.ApplicationName;

                DiscoverImageView = FindViewById<ImageView>(Resource.Id.discoverButton);
                FriendsImageView = FindViewById<ImageView>(Resource.Id.friendsButton);
                SearchImageView = FindViewById<ImageView>(Resource.Id.searchButton);

                BottomNavigationTab = new BottomNavigationTab(this);
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
                // true +=  // false -=
                if (addEvent)
                {
                    DiscoverImageView.Click += DiscoverImageViewOnClick;
                    SearchImageView.Click += SearchImageViewOnClick;
                    FriendsImageView.Click += FriendsImageViewOnClick;
                }
                else
                {
                    DiscoverImageView.Click -= DiscoverImageViewOnClick;
                    SearchImageView.Click -= SearchImageViewOnClick;
                    FriendsImageView.Click -= FriendsImageViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ChatTabbedMainActivity GetInstance()
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

        #endregion

        #region Events

        private void FriendsImageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RequestActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SearchImageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchActivity));
                intent.PutExtra("Key", "");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DiscoverImageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(PeopleNearByActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Set Tab

        private void AddFragmentsTabs()
        {
            try
            {
                TabAdapter.ClaerFragment();

                ChatTab = new TabChatFragment();
                LastStoriesTab = new LastStoriesFragment(this);
                LastCallsTab = new LastCallsFragment();

                if (TabAdapter is { ItemCount: <= 0 })
                {
                    TabAdapter.AddFragment(ChatTab, GetText(Resource.String.Lbl_Tab_Chats));
                    TabAdapter.AddFragment(LastStoriesTab, GetText(Resource.String.Lbl_Tab_Stories));
                    TabAdapter.AddFragment(LastCallsTab, GetText(Resource.String.Lbl_Tab_Calls));

                    ViewPager.UserInputEnabled = false;
                    ViewPager.CurrentItem = TabAdapter.ItemCount;
                    ViewPager.OffscreenPageLimit = TabAdapter.ItemCount;

                    ViewPager.Orientation = ViewPager2.OrientationHorizontal;
                    ViewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                    ViewPager.Adapter = TabAdapter;
                    ViewPager.Adapter.NotifyDataSetChanged();
                }

                BottomNavigationTab.SelectItem(0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly ChatTabbedMainActivity Activity;

            public MyOnPageChangeCallback(ChatTabbedMainActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    switch (position)
                    {
                        case < 0:
                            return;
                        // Chats
                        case 0:
                            {
                                Activity.BottomNavigationTab.SelectItem(0);
                                break;
                            }
                        // Stories
                        case 1:
                            {
                                Activity.BottomNavigationTab.SelectItem(1);

                                break;
                            }
                        // Calls
                        case 2:
                            {
                                Activity.BottomNavigationTab.SelectItem(2);

                                break;
                            }
                        // More_Tab
                        case 3:
                            {

                                break;
                            }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 501 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            Intent intent = new Intent(this, typeof(VideoEditorActivity));
                            intent.PutExtra("Uri", filepath);
                            intent.PutExtra("Thumbnail", filepath);
                            intent.PutExtra("Type", "Story");
                            StartActivity(intent);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // video_camera
                {
                    if (IntentController.CurrentVideoPath != null)
                    {
                        Intent intent = new Intent(this, typeof(VideoEditorActivity));
                        intent.PutExtra("Uri", IntentController.CurrentVideoPath);
                        intent.PutExtra("Thumbnail", IntentController.CurrentVideoPath);
                        intent.PutExtra("Type", "Story");
                        StartActivity(intent);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Image")
                        {
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                //Do something with your Uri
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", filepath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                            }
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                    }
                }
                else if (requestCode == 2200 && resultCode == Result.Ok)
                {
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                            if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                            {
                                //Do something with your Uri
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", imagePath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
                else if (requestCode == 4711)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            {
                                // In app update success
                                if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate) ToastUtils.ShowToast(this, "App updated", ToastLength.Short);
                                break;
                            }
                        case Result.Canceled:
                            ToastUtils.ShowToast(this, "In app update cancelled", ToastLength.Short);
                            break;
                        case (Result)ActivityResult.ResultInAppUpdateFailed:
                            ToastUtils.ShowToast(this, "In app update failed", ToastLength.Short);
                            break;
                    }
                }
                else if (requestCode == InitFloating.ChatHeadDataRequestCode && InitFloating.CanDrawOverlays(this))
                {
                    Floating.FloatingShow(InitFloating.FloatingObject);

                    UserDetails.ChatHead = true;
                    MainSettings.SharedData?.Edit()?.PutBoolean("chatheads_key", UserDetails.ChatHead)?.Commit();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        switch (ImageType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image":
                                GalleryController?.OpenDialogGallery();
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "Video_camera":
                                //requestCode >> 513 => video camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                        }

                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 110 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    case 110:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 100:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 102 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        LastCallsTab.StartCall(TypeCall.Audio, LastCallsTab.DataUser);
                        break;
                    case 102:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 103 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        LastCallsTab.StartCall(TypeCall.Video, LastCallsTab.DataUser);
                        break;
                    case 103:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                        break;
                    case 16248:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service

        private void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this);
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReceiveResult(string resultData)
        {
            try
            {
                //ToastUtils.ShowToast(Application.Context, "Result got ", ToastLength.Short);

                var result = JsonConvert.DeserializeObject<LastChatObject>(resultData);
                if (result != null)
                {
                    ChatTab?.LastChatTab?.LoadDataLastChatNewV(result.Data);
                    RunOnUiThread(() => { LastChatFragment.LoadCall(result); });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region General App Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                RunOnUiThread(() =>
                {
                    if (!InitFloating.CanDrawOverlays(this))
                        DisplayChatHeadDialog();
                });

                LoadConfigSettings();

                sqlEntity.Get_MyProfile();

                ListUtils.StickersList = sqlEntity.Get_From_StickersTb();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPinChats, ApiRequest.GetArchivedChats, ApiRequest.Get_MyProfileData_Api });

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void GetOneSignalNotification()
        {
            try
            {
                string userId = Intent?.GetStringExtra("userId") ?? "";
                string pageId = Intent?.GetStringExtra("PageId") ?? "";
                string groupId = Intent?.GetStringExtra("GroupId") ?? "";
                string type = Intent?.GetStringExtra("type") ?? "";

                Intent intent = null!;
                switch (type)
                {
                    case "user":
                        {
                            var item = ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == userId && a.LastChat?.ChatType == "user");
                            string mainChatColor = AppSettings.MainColor;

                            intent = new Intent(this, typeof(ChatWindowActivity));
                            intent.PutExtra("UserID", userId);
                            intent.PutExtra("ShowEmpty", "no");

                            if (item?.LastChat != null)
                            {
                                if (!WoWonderTools.ChatIsAllowed(item.LastChat))
                                    return;

                                if (item.LastChat.LastMessage.LastMessageClass != null)
                                    mainChatColor = item.LastChat.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(item.LastChat.LastMessage.LastMessageClass.ChatColor) : item.LastChat.LastMessage.LastMessageClass.ChatColor ?? AppSettings.MainColor;

                                intent.PutExtra("TypeChat", "LastMessenger");
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("ColorChat", mainChatColor);
                                intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.LastChat));
                            }
                            else
                            {
                                intent.PutExtra("TypeChat", "OneSignalNotification");
                                intent.PutExtra("ColorChat", mainChatColor);
                            }
                            break;
                        }
                    case "page":
                        {
                            Classes.LastChatsClass item = ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.PageId == pageId && a.LastChat?.ChatType == "page");

                            intent = new Intent(this, typeof(PageChatWindowActivity));
                            intent.PutExtra("ShowEmpty", "no");
                            intent.PutExtra("PageId", pageId);

                            if (item?.LastChat != null)
                            {
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("PageObject", JsonConvert.SerializeObject(item.LastChat));
                                intent.PutExtra("TypeChat", "");
                            }
                            break;
                        }
                    case "group":
                        {
                            Classes.LastChatsClass item = ChatTab?.LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == groupId);

                            intent = new Intent(this, typeof(GroupChatWindowActivity));
                            intent.PutExtra("ShowEmpty", "no");
                            intent.PutExtra("GroupId", groupId);

                            if (item?.LastChat != null)
                            {
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item.LastChat));
                            }

                            break;
                        }
                    case "call_audio":
                    case "call_video":
                        {
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync(true) });

                            break;
                        }
                }

                if (intent != null)
                    StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InAppUpdate()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (AppSettings.ShowSettingsUpdateManagerApp)
                        UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview?.GetBoolean(MainSettings.PrefKeyInAppReview, false) ?? false;
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetTitle(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.SetMessage(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_Rate), (materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                        dialog.Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Story

        public void OnVideo_Button_Click()
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectVideoFrom));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnImage_Button_Click()
        {
            try
            {
                ImageType = "Image";
                GalleryController?.OpenDialogGallery();
            }
            catch (Exception exe)
            {
                Methods.DisplayReportResultTrack(exe);
            }
        }


        #region Gallery
         
        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                            intent.PutExtra("Uri", filepath);
                            intent.PutExtra("Type", "image");
                            StartActivity(intent);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OpenEditColor()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditColorActivity));
                StartActivityForResult(intent, 2200);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_VideoGallery)) // video  
                {
                    ImageType = "Video";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("video"))
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "video");
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera)) // video camera
                {
                    ImageType = "Video_camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("video"))
                        {
                            //requestCode >> 513 => video camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "video");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region WakeLock System

        public void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm?.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl?.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm?.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm?.NewWakeLock(WakeLockFlags.ProximityScreenOff, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Chat Head

        private Dialog ChatHeadWindow;
        private void DisplayChatHeadDialog()
        {
            try
            {
                if (AppSettings.ShowChatHeads)
                {
                    UserDetails.OpenDialog = MainSettings.SharedData.GetBoolean("OpenDialogChatHead_key", false);

                    if (UserDetails.OpenDialog) return;
                    if (InitFloating.CanDrawOverlays(this)) return;

                    ChatHeadWindow = new Dialog(this, WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                    ChatHeadWindow.SetContentView(Resource.Layout.ChatHeadDialogLayout);

                    var subTitle1 = ChatHeadWindow.FindViewById<TextView>(Resource.Id.subTitle1);
                    var btnNotNow = ChatHeadWindow.FindViewById<TextView>(Resource.Id.notNowButton);
                    var btnGoToSettings = ChatHeadWindow.FindViewById<AppCompatButton>(Resource.Id.goToSettingsButton);

                    subTitle1.Text = GetText(Resource.String.Lbl_EnableChatHead_SubTitle1) + " " + AppSettings.ApplicationName + ", " + GetText(Resource.String.Lbl_EnableChatHead_SubTitle2);

                    btnNotNow.Click += BtnNotNowOnClick;
                    btnGoToSettings.Click += BtnGoToSettingsOnClick;

                    ChatHeadWindow.Show();

                    UserDetails.OpenDialog = true;
                    MainSettings.SharedData?.Edit()?.PutBoolean("OpenDialogChatHead_key", true)?.Commit();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnGoToSettingsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Floating.CheckPermission())
                {
                    Floating.OpenManagePermission();
                }

                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnNotNowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Call

        public static void AddCallToListAndSend(string type, string typeStatus, TypeCall typeCall, CallUserObject callUserObject)
        {
            try
            {
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Classes.CallUser cv = new Classes.CallUser
                {
                    Id = callUserObject.Data.Id,
                    UserId = callUserObject.UserId,
                    Avatar = callUserObject.Avatar,
                    Name = callUserObject.Name,
                    FromId = callUserObject.Data.FromId,
                    Active = callUserObject.Data.Active,
                    Time = typeStatus + " � " + Methods.Time.TimeAgo(unixTimestamp, false),
                    Status = typeStatus,
                    RoomName = callUserObject.Data.RoomName,
                    Type = typeCall,
                    TypeIcon = type,
                    TypeColor = "#008000"
                };

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.Insert_CallUser(cv);

                var ckd = Instance?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == callUserObject.Data.Id); // id >> Call_Id
                if (ckd == null)
                {
                    Instance?.LastCallsTab?.MAdapter?.MCallUser?.Insert(0, cv);
                    Instance?.LastCallsTab?.MAdapter?.NotifyDataSetChanged();
                }
                else
                {
                    ckd = cv;
                    ckd.Id = cv.Id;
                    Instance?.LastCallsTab?.MAdapter?.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}