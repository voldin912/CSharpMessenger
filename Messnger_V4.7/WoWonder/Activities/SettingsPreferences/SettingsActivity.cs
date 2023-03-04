using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Play.Core.Install.Model;
using Google.Android.Material.Dialog;
using WoWonder.Activities.Authentication;
using WoWonder.Activities.Base;
using WoWonder.Activities.DefaultUser;
using WoWonder.Activities.SettingsPreferences.Adapters;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.InviteFriends;
using WoWonder.Activities.Tab;
using WoWonder.Frameworks.Floating;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.SettingsPreferences
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SettingsActivity : BaseActivity, IDialogListCallBack 
    {
        #region Variables Basic

        private ImageView ImageUser, IconMore;
        private TextView TxtName, TxtUsername, BtToggleAccount;
        private FrameLayout TopLayoutAccount;
        private LinearLayout ExpandAccountLayout;
        private LinearLayoutManager LayoutManager;
        private RecyclerView MRecycler;
        private SelectAccountAdapter MAdapter;

        private LinearLayout EditProfileLayout, MyAccountLayout, BlockUserLayout, DeleteAccountLayout, LogoutLayout;
         
        private LinearLayout WallpaperLayout, ThemeLayout, ChangePasswordLayout, TwoFactorLayout, ManageSessionsLayout, FingerprintLockLayout;
        private TextView TxtSummaryTheme;
        private SwitchCompat SwitchPip;

        private LinearLayout StorageConnectedMobileLayout, StorageConnectedWiFiLayout;
        private TextView TxtSummaryStorageConnectedMobile, TxtSummaryStorageConnectedWiFi;

        private LinearLayout WhoCanFollowMeLayout, WhoCanMessageMeLayout, WhoCanSeeMyBirthdayLayout;
        private TextView TxtSummaryWhoCanFollowMe, TxtSummaryWhoCanMessageMe, TxtSummaryWhoCanSeeMyBirthday;

        private LinearLayout OnlineUsersLayout, ChatHeadsLayout, NotificationPopupLayout, PlaySoundsLayout;
        private SwitchCompat SwitchOnlineUsers, SwitchChatHeads;
        private CheckBox CheckBoxNotificationPopup, CheckBoxPlaySounds;

        private TextView InviteFriendsLayout, RateOurAppLayout, ContactUsLayout, PrivacyPolicyLayout, TermsOfUseLayout;
        private TextView VersionValue;

        private PopupWindow popupWindow;
        private string TypeDialog, SOnlineUsersPref, SWhoCanFollowMe = "0", SWhoCanMessageMe = "0", SWhoCanSeeMyBirthday = "0";

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
                SetContentView(Resource.Layout.SettingsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GetMyInfoData();
                InAppUpdate();

                AdsGoogle.Ad_Interstitial(this);
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
                base.OnDestroy();
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
                IconMore = FindViewById<ImageView>(Resource.Id.IconMore);
                 
                ImageUser = FindViewById<ImageView>(Resource.Id.imageUser);
                TxtName = FindViewById<TextView>(Resource.Id.name);
                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                BtToggleAccount = FindViewById<TextView>(Resource.Id.bt_toggle_account);

                TopLayoutAccount = FindViewById<FrameLayout>(Resource.Id.top_layout_account);
                ExpandAccountLayout = FindViewById<LinearLayout>(Resource.Id.lyt_expand_account);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);

                EditProfileLayout = FindViewById<LinearLayout>(Resource.Id.EditProfileLayout);

                MyAccountLayout = FindViewById<LinearLayout>(Resource.Id.MyAccountLayout);

                BlockUserLayout = FindViewById<LinearLayout>(Resource.Id.BlockUserLayout);

                DeleteAccountLayout = FindViewById<LinearLayout>(Resource.Id.DeleteAccountLayout);

                LogoutLayout = FindViewById<LinearLayout>(Resource.Id.LogoutLayout);

                WallpaperLayout = FindViewById<LinearLayout>(Resource.Id.WallpaperLayout);

                ThemeLayout = FindViewById<LinearLayout>(Resource.Id.ThemeLayout);
                TxtSummaryTheme = FindViewById<TextView>(Resource.Id.summary_Theme);

                ChangePasswordLayout = FindViewById<LinearLayout>(Resource.Id.ChangePasswordLayout);

                TwoFactorLayout = FindViewById<LinearLayout>(Resource.Id.TwoFactorLayout);

                ManageSessionsLayout = FindViewById<LinearLayout>(Resource.Id.ManageSessionsLayout);

                FingerprintLockLayout = FindViewById<LinearLayout>(Resource.Id.FingerprintLockLayout);
                SwitchPip = FindViewById<SwitchCompat>(Resource.Id.SwitchPIP);

                StorageConnectedMobileLayout = FindViewById<LinearLayout>(Resource.Id.StorageConnectedMobileLayout);
                TxtSummaryStorageConnectedMobile = FindViewById<TextView>(Resource.Id.summary_StorageConnectedMobile);

                StorageConnectedWiFiLayout = FindViewById<LinearLayout>(Resource.Id.StorageConnectedWiFiLayout);
                TxtSummaryStorageConnectedWiFi = FindViewById<TextView>(Resource.Id.summary_StorageConnectedWiFi);

                WhoCanFollowMeLayout = FindViewById<LinearLayout>(Resource.Id.WhoCanFollowMeLayout);
                TxtSummaryWhoCanFollowMe = FindViewById<TextView>(Resource.Id.summary_WhoCanFollowMe);
                 
                WhoCanMessageMeLayout = FindViewById<LinearLayout>(Resource.Id.WhoCanMessageMeLayout);
                TxtSummaryWhoCanMessageMe = FindViewById<TextView>(Resource.Id.summary_WhoCanMessageMe);

                WhoCanSeeMyBirthdayLayout = FindViewById<LinearLayout>(Resource.Id.WhoCanSeeMyBirthdayLayout);
                TxtSummaryWhoCanSeeMyBirthday = FindViewById<TextView>(Resource.Id.summary_WhoCanSeeMyBirthday);

                OnlineUsersLayout = FindViewById<LinearLayout>(Resource.Id.OnlineUsersLayout);
                SwitchOnlineUsers = FindViewById<SwitchCompat>(Resource.Id.SwitchOnlineUsers);

                ChatHeadsLayout = FindViewById<LinearLayout>(Resource.Id.ChatHeadsLayout);
                SwitchChatHeads = FindViewById<SwitchCompat>(Resource.Id.SwitchChatHeads);

                NotificationPopupLayout = FindViewById<LinearLayout>(Resource.Id.NotificationPopupLayout);
                CheckBoxNotificationPopup = FindViewById<CheckBox>(Resource.Id.CheckBoxNotificationPopup);

                PlaySoundsLayout = FindViewById<LinearLayout>(Resource.Id.PlaySoundsLayout);
                CheckBoxPlaySounds = FindViewById<CheckBox>(Resource.Id.CheckBoxPlaySounds);
                 
                VersionValue = (TextView)FindViewById(Resource.Id.VersionValue);

                AdsGoogle.Ad_AdMobNative(this);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, BtToggleAccount, FontAwesomeIcon.AngleDown);

                if (!AppSettings.ShowSettingsAccount)
                    MyAccountLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsBlockedUsers)
                    BlockUserLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowSettingsDeleteAccount)
                    DeleteAccountLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowSettingsWallpaper)
                    WallpaperLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowSettingsPassword)
                    ChangePasswordLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsTwoFactor)
                    TwoFactorLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsManageSessions)
                    ManageSessionsLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsFingerprintLock)
                    FingerprintLockLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowChatHeads)
                    ChatHeadsLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.InvitationSystem)
                    InviteFriendsLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowSettingsRateApp)
                    RateOurAppLayout.Visibility = ViewStates.Gone; 
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
                MAdapter = new SelectAccountAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Settings);
                    toolbar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    IconMore.Click += IconMoreOnClick;
                    TopLayoutAccount.Click += TopLayoutAccountOnClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                    EditProfileLayout.Click += EditProfileLayoutOnClick;
                    MyAccountLayout.Click += MyAccountLayoutOnClick;
                    BlockUserLayout.Click += BlockUserLayoutOnClick;
                    DeleteAccountLayout.Click += DeleteAccountLayoutOnClick;
                    LogoutLayout.Click += LogoutLayoutOnClick;
                    WallpaperLayout.Click += WallpaperLayoutOnClick;
                    ThemeLayout.Click += ThemeLayoutOnClick;
                    ChangePasswordLayout.Click += ChangePasswordLayoutOnClick;
                    TwoFactorLayout.Click += TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click += ManageSessionsLayoutOnClick;
                    SwitchPip.CheckedChange += SwitchPipOnCheckedChange;
                    StorageConnectedMobileLayout.Click += StorageConnectedMobileLayoutOnClick;
                    StorageConnectedWiFiLayout.Click += StorageConnectedWiFiLayoutOnClick;
                    WhoCanFollowMeLayout.Click += WhoCanFollowMeLayoutOnClick;
                    WhoCanMessageMeLayout.Click += WhoCanMessageMeLayoutOnClick;
                    WhoCanSeeMyBirthdayLayout.Click += WhoCanSeeMyBirthdayLayoutOnClick;
                    SwitchOnlineUsers.CheckedChange += SwitchOnlineUsersOnCheckedChange;
                    SwitchChatHeads.CheckedChange += SwitchChatHeadsOnCheckedChange;
                    CheckBoxNotificationPopup.CheckedChange += CheckBoxNotificationPopupOnCheckedChange;
                    CheckBoxPlaySounds.CheckedChange += CheckBoxPlaySoundsOnCheckedChange;
                   
                    VersionValue.Click += VersionValueOnClick;
                }
                else
                {
                    IconMore.Click -= IconMoreOnClick;
                    TopLayoutAccount.Click -= TopLayoutAccountOnClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                    EditProfileLayout.Click -= EditProfileLayoutOnClick;
                    MyAccountLayout.Click -= MyAccountLayoutOnClick;
                    BlockUserLayout.Click -= BlockUserLayoutOnClick;
                    DeleteAccountLayout.Click -= DeleteAccountLayoutOnClick;
                    LogoutLayout.Click -= LogoutLayoutOnClick;
                    WallpaperLayout.Click -= WallpaperLayoutOnClick;
                    ThemeLayout.Click -= ThemeLayoutOnClick;
                    ChangePasswordLayout.Click -= ChangePasswordLayoutOnClick;
                    TwoFactorLayout.Click -= TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click -= ManageSessionsLayoutOnClick;
                    SwitchPip.CheckedChange -= SwitchPipOnCheckedChange;
                    StorageConnectedMobileLayout.Click -= StorageConnectedMobileLayoutOnClick;
                    StorageConnectedWiFiLayout.Click -= StorageConnectedWiFiLayoutOnClick;
                    WhoCanFollowMeLayout.Click -= WhoCanFollowMeLayoutOnClick;
                    WhoCanMessageMeLayout.Click -= WhoCanMessageMeLayoutOnClick;
                    WhoCanSeeMyBirthdayLayout.Click -= WhoCanSeeMyBirthdayLayoutOnClick;
                    SwitchOnlineUsers.CheckedChange -= SwitchOnlineUsersOnCheckedChange;
                    SwitchChatHeads.CheckedChange -= SwitchChatHeadsOnCheckedChange;
                    CheckBoxNotificationPopup.CheckedChange -= CheckBoxNotificationPopupOnCheckedChange;
                    CheckBoxPlaySounds.CheckedChange -= CheckBoxPlaySoundsOnCheckedChange;
                    VersionValue.Click -= VersionValueOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        #region Events

        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
                View popupView = layoutInflater?.Inflate(Resource.Layout.MoreSettingsLayout, null);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 180, Resources.DisplayMetrics);
                popupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent);

                InviteFriendsLayout = popupView.FindViewById<TextView>(Resource.Id.InviteFriendsLayout);
                RateOurAppLayout = popupView.FindViewById<TextView>(Resource.Id.RateOurAppLayout);
                ContactUsLayout = popupView.FindViewById<TextView>(Resource.Id.ContactUsLayout);
                PrivacyPolicyLayout = popupView.FindViewById<TextView>(Resource.Id.PrivacyPolicyLayout);
                TermsOfUseLayout = popupView.FindViewById<TextView>(Resource.Id.TermsOfUseLayout);

                InviteFriendsLayout.Click += InviteFriendsLayoutOnClick;
                RateOurAppLayout.Click += RateOurAppLayoutOnClick;
                ContactUsLayout.Click += ContactUsLayoutOnClick;
                PrivacyPolicyLayout.Click += PrivacyPolicyLayoutOnClick;
                TermsOfUseLayout.Click += TermsOfUseLayoutOnClick;

                popupWindow.SetBackgroundDrawable(new ColorDrawable());
                popupWindow.Focusable = true;
                popupWindow.ClippingEnabled = true;
                popupWindow.OutsideTouchable = false;
                popupWindow.DismissEvent += delegate (object sender, EventArgs args) {
                    try
                    {
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.ShowAsDropDown(IconMore);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemLongClick(object sender, SelectAccountAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        if (item.Status == "Active" || item.Status == "Add")
                        {
                            return;
                        } 
                        else
                        {
                            TypeDialog = "LogoutAccount";

                            var dialog = new MaterialAlertDialogBuilder(this);

                            dialog.SetTitle(Resource.String.Lbl_Warning);
                            dialog.SetMessage(GetText(Resource.String.Lbl_Are_you_logout));
                            dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                            {
                                try
                                {
                                    ListUtils.DataUserLoginList.Remove(item);

                                    MAdapter.AccountList.Remove(item);
                                    MAdapter.NotifyItemRemoved(e.Position);

                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.DeleteInfoLogin_Credentials(item.UserId);

                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Auth.DeleteTokenAsync(item.AccessToken) });
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception); 
                                }
                            });
                            dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                            dialog.Show();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, SelectAccountAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        if (item.Status == "Active")
                        {
                            return;
                        }
                        else if (item.Status == "Add")
                        {
                            StartActivity(new Intent(this, typeof(LoginActivity)));
                        }
                        else
                        {
                            item.Status = "Active";

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateLogin_Credentials(item);

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_SwitchingTo) + " " + item.FullName,
                                ToastLength.Long);
                            ApiRequest.SwitchAccount(this);

                            dbDatabase.Get_data_Login_Credentials();

                            Intent intent = new Intent(this, typeof(SplashScreenActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            StartActivity(intent);
                            FinishAffinity();
                            Finish();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open PackageName on Google play
        private void VersionValueOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenAppOnGooglePlay(PackageName);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopLayoutAccountOnClick(object sender, EventArgs e)
        {
            ToggleSection(BtToggleAccount, ExpandAccountLayout);
        }

        private void TermsOfUseLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenBrowserFromApp(InitializeWoWonder.WebsiteUrl + "/terms/terms");
                popupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PrivacyPolicyLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenBrowserFromApp(InitializeWoWonder.WebsiteUrl + "/terms/privacy-policy");
                popupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ContactUsLayoutOnClick(object sender, EventArgs e)
        {
            try
            { 
                new IntentController(this).OpenBrowserFromApp(InitializeWoWonder.WebsiteUrl + "/contact-us");
                popupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RateOurAppLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StoreReviewApp store = new StoreReviewApp();
                store.OpenStoreReviewPage(PackageName);
                popupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InviteFriendsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartActivity(new Intent(this, typeof(InviteFriendsActivity)));
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadPhoneNumbers) == Permission.Granted)
                        StartActivity(new Intent(this, typeof(InviteFriendsActivity)));
                    else
                        new PermissionsController(this).RequestPermission(101);
                }

                popupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CheckBoxPlaySoundsOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    UserDetails.SoundControl = true;
                }
                else
                {
                    UserDetails.SoundControl = false;
                }
                MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_PlaySound_key", UserDetails.SoundControl)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CheckBoxNotificationPopupOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }
                else
                {
                    OneSignalNotification.Instance.UnRegisterNotificationDevice();
                }
                MainSettings.SharedData?.Edit()?.PutBoolean("notifications_key", e.IsChecked)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchChatHeadsOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.ChatHead = e.IsChecked;

                OpenChatHead();
                MainSettings.SharedData?.Edit()?.PutBoolean("chatheads_key", e.IsChecked)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchOnlineUsersOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.OnlineUsers = e.IsChecked;

                MainSettings.SharedData?.Edit()?.PutBoolean("onlineuser_key", e.IsChecked)?.Commit();
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                switch (e.IsChecked)
                {
                    //Online >> value = 0
                    case true:
                    {
                        SOnlineUsersPref = "0";

                        if (dataUser != null)
                        {
                            dataUser.Status = "0";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                        }

                        break;
                    }
                    //Offline >> value = 1
                    default:
                    {
                        SOnlineUsersPref = "1";

                        if (dataUser != null)
                        {
                            dataUser.Status = "1";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                        }

                        break;
                    }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"status", SOnlineUsersPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (UserDetails.OnlineUsers)
                        UserDetails.Socket?.Emit_loggedintEvent(UserDetails.AccessToken);
                    else
                        UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WhoCanSeeMyBirthdayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Birthday";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_Who_can_see_my_birthday);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WhoCanMessageMeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Message";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_Who_can_message_me);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WhoCanFollowMeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "WhoCanFollow";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_Who_can_follow_me);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StorageConnectedWiFiLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "StorageConnectedWiFi";

                MainSettings.DataStorageConnected();

                var listItems = ListUtils.StorageTypeMobileSelect.Select(selectClass => selectClass.Text).ToList();

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                for (int i = 0; i < checkedItems.Length; i++)
                {
                    var typeValue = ListUtils.StorageTypeWiFiSelect[i].Value;
                    checkedItems[i] = typeValue;
                }

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_StorageConnectedMobile);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        UserDetails.PhotoWifi = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("photoWifi_key", false)?.Commit();

                        UserDetails.VideoWifi = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("videoWifi_key", false)?.Commit();

                        UserDetails.AudioWifi = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("audioWifi_key", false)?.Commit();

                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            var type = ListUtils.StorageTypeWiFiSelect[i];
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];

                                switch (type.Id)
                                {
                                    case 0:
                                        UserDetails.PhotoWifi = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("photoWifi_key", true)?.Commit();
                                        break;
                                    case 1:
                                        UserDetails.VideoWifi = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("videoWifi_key", true)?.Commit();
                                        break;
                                    case 2:
                                        UserDetails.AudioWifi = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("audioWifi_key", true)?.Commit();
                                        break;
                                }
                            }
                            else
                            {
                                type.Value = false;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StorageConnectedMobileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "StorageConnectedMobile";

                MainSettings.DataStorageConnected();

                var listItems = ListUtils.StorageTypeMobileSelect.Select(selectClass => selectClass.Text).ToList();

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                for (int i = 0; i < checkedItems.Length; i++)
                {
                    var typeValue = ListUtils.StorageTypeMobileSelect[i].Value;
                    checkedItems[i] = typeValue;
                }

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_StorageConnectedMobile);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems.ToArray(), (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        UserDetails.PhotoMobile = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("photoMobile_key", false)?.Commit();

                        UserDetails.VideoMobile = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("videoMobile_key", false)?.Commit();

                        UserDetails.AudioMobile = false;
                        MainSettings.SharedData?.Edit()?.PutBoolean("audioMobile_key", false)?.Commit();

                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            var type = ListUtils.StorageTypeMobileSelect[i];
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];

                                switch (type.Id)
                                {
                                    case 0:
                                        UserDetails.PhotoMobile = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("photoMobile_key", true)?.Commit();
                                        break;
                                    case 1:
                                        UserDetails.VideoMobile = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("videoMobile_key", true)?.Commit();
                                        break;
                                    case 2:
                                        UserDetails.AudioMobile = true;
                                        type.Value = true;
                                        MainSettings.SharedData?.Edit()?.PutBoolean("audioMobile_key", true)?.Commit();
                                        break;
                                }
                            }
                            else
                            {
                                type.Value = false;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchPipOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.FingerprintLock = e.IsChecked;
                MainSettings.SharedData?.Edit()?.PutBoolean("FingerprintLock_key", e.IsChecked)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ManageSessionsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(ManageSessionsActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TwoFactorLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(TwoFactorAuthActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ChangePasswordLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PasswordActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ThemeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "NightMode";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_Theme);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                if ((int)Build.VERSION.SdkInt >= 29)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WallpaperLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(WallpaperActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LogoutLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Logout";

                var dialog = new MaterialAlertDialogBuilder(this);

                dialog.SetTitle(Resource.String.Lbl_Warning);
                dialog.SetMessage(GetText(Resource.String.Lbl_Are_you_logout));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (o, args) =>
                {
                    try
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long);
                        ApiRequest.Logout(this);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                dialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DeleteAccountLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(DeleteAccountActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BlockUserLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(BlockedUsersActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MyAccountLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyAccountActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditProfileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyProfileActivity));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Result & Permissions

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == InitFloating.ChatHeadDataRequestCode && InitFloating.CanDrawOverlays(this))
                {
                    UserDetails.ChatHead = true;
                    MainSettings.SharedData?.Edit()?.PutBoolean("chatheads_key", UserDetails.ChatHead)?.Commit();
                }
                else if (requestCode == 4711)
                {
                    switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                    {
                        case Result.Ok:
                            // In app update success
                            if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                ToastUtils.ShowToast(this, "App updated", ToastLength.Short);
                            break;
                        case Result.Canceled:
                            ToastUtils.ShowToast(this, "In app update cancelled", ToastLength.Short);
                            break;
                        case (Result)ActivityResult.ResultInAppUpdateFailed:
                            ToastUtils.ShowToast(this, "In app update failed", ToastLength.Short);
                            break;
                    }
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
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StartActivity(new Intent(this, typeof(InviteFriendsActivity)));
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
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
                string text = itemString;
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();

                if (TypeDialog == "NightMode")
                {
                    string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);

                    if (text == GetString(Resource.String.Lbl_Light) && getValue != MainSettings.LightMode)
                    {
                        //Set Light Mode   
                        TxtSummaryTheme.Text = GetString(Resource.String.Lbl_Light);

                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(this, typeof(ChatTabbedMainActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        intent.AddFlags(ActivityFlags.NoAnimation);
                        FinishAffinity();
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                    else if (text == GetString(Resource.String.Lbl_Dark) && getValue != MainSettings.DarkMode)
                    {
                        TxtSummaryTheme.Text = GetString(Resource.String.Lbl_Dark);

                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(this, typeof(ChatTabbedMainActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        intent.AddFlags(ActivityFlags.NoAnimation);
                        FinishAffinity();
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                    else if (text == GetString(Resource.String.Lbl_SetByBattery) && getValue != MainSettings.DefaultMode)
                    {
                        TxtSummaryTheme.Text = GetString(Resource.String.Lbl_SetByBattery);
                        MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

                        if ((int) Build.VERSION.SdkInt >= 29)
                        {
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                            var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                            if (currentNightMode == UiMode.NightNo) // Night mode is not active, we're using the light theme
                                MainSettings.ApplyTheme(MainSettings.LightMode);
                            else if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
                                MainSettings.ApplyTheme(MainSettings.DarkMode);
                        }
                        else
                        {
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                            var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                            if (currentNightMode == UiMode.NightNo) // Night mode is not active, we're using the light theme
                                MainSettings.ApplyTheme(MainSettings.LightMode);
                            else if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
                                MainSettings.ApplyTheme(MainSettings.DarkMode);

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                            {
                                Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                            }

                            Intent intent = new Intent(this, typeof(ChatTabbedMainActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            intent.AddFlags(ActivityFlags.NoAnimation);
                            FinishAffinity();
                            OverridePendingTransition(0, 0);
                            StartActivity(intent);
                        }
                    }
                }
                else if (TypeDialog == "WhoCanFollow")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("WhoCanFollow", "0")?.Commit();
                        TxtSummaryWhoCanFollowMe.Text = text;
                        SWhoCanFollowMe = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("WhoCanFollow", "1")?.Commit();
                        TxtSummaryWhoCanFollowMe.Text = text;
                        SWhoCanFollowMe = "1";
                    }

                    if (dataUser != null)
                    {
                        dataUser.FollowPrivacy = SWhoCanFollowMe;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"follow_privacy", SWhoCanFollowMe},
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy)});
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (TypeDialog == "Birthday")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "0")?.Commit();
                        TxtSummaryWhoCanSeeMyBirthday.Text = text;
                        SWhoCanSeeMyBirthday = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "1")?.Commit();
                        TxtSummaryWhoCanSeeMyBirthday.Text = text;
                        SWhoCanSeeMyBirthday = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "2")?.Commit();
                        TxtSummaryWhoCanSeeMyBirthday.Text = text;
                        SWhoCanSeeMyBirthday = "2";
                    }

                    if (dataUser != null)
                    {
                        dataUser.BirthPrivacy = SWhoCanSeeMyBirthday;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"birth_privacy", SWhoCanSeeMyBirthday},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy)});
                    }
                    else
                    {
                        ToastUtils.ShowToast(this,
                            GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (TypeDialog == "Message")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "0")?.Commit();
                        TxtSummaryWhoCanMessageMe.Text = text;
                        SWhoCanMessageMe = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "1")?.Commit();
                        TxtSummaryWhoCanMessageMe.Text = text;
                        SWhoCanMessageMe = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "2")?.Commit();
                        TxtSummaryWhoCanMessageMe.Text = text;
                        SWhoCanMessageMe = "2";
                    }

                    if (dataUser != null)
                    {
                        dataUser.MessagePrivacy = SWhoCanMessageMe;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"message_privacy", SWhoCanMessageMe},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy)});
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion
         
        #region Get Data User

        //Get Data User From Database 
        private void GetMyInfoData()
        {
            try
            {
                UserDataObject dataUser = ListUtils.MyProfileList.FirstOrDefault();
                LoadDataUser(dataUser);

                Task.Factory.StartNew(StartApiService);
                 
                PackageInfo info = null;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    info = PackageManager?.GetPackageInfo(PackageName!, PackageManager.PackageInfoFlags.Of((long)PackageInfoFlags.Signatures));
                else
#pragma warning disable CS0618
                    info = PackageManager?.GetPackageInfo(PackageName!, PackageInfoFlags.Signatures);
#pragma warning restore CS0618
                string versionName = info?.VersionName;

                VersionValue.Text = GetText(Resource.String.Lbl_BuildVersion) + " " + versionName;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Data My Profile API
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data,followers,following");

            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() => LoadDataUser(result.UserData));
            }
        }
         
        private void LoadDataUser(UserDataObject data)
        {
            try
            { 
                //profile_picture
                GlideImageLoader.LoadImage(this, data.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                TxtName.Text = WoWonderTools.GetNameFinal(data);
                TxtName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, data.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                TxtUsername.Text = "@" + data.Username;
                 
                string getValueFollow = MainSettings.SharedData?.GetString("whocanfollow_key", data.FollowPrivacy ?? "");
                switch (getValueFollow)
                {
                    case "0":
                        TxtSummaryWhoCanFollowMe.Text = GetText(Resource.String.Lbl_Everyone);
                        SWhoCanFollowMe = "0";
                        break;
                    case "1":
                        TxtSummaryWhoCanFollowMe.Text = GetText(Resource.String.Lbl_People_i_Follow);
                        SWhoCanFollowMe = "1";
                        break;
                    default:
                        TxtSummaryWhoCanFollowMe.Text = getValueFollow;
                        break;
                }

                string getValueMessage = MainSettings.SharedData?.GetString("whocanMessage_key", data.MessagePrivacy ?? "");
                switch (getValueMessage)
                {
                    case "0":
                        TxtSummaryWhoCanMessageMe.Text = GetText(Resource.String.Lbl_Everyone);
                        SWhoCanMessageMe = "0";
                        break;
                    case "1":
                        TxtSummaryWhoCanMessageMe.Text = GetText(Resource.String.Lbl_People_i_Follow);
                        SWhoCanMessageMe = "1";
                        break;
                    case "2":
                        TxtSummaryWhoCanMessageMe.Text = GetText(Resource.String.Lbl_No_body);
                        SWhoCanMessageMe = "2";
                        break;
                    default:
                        TxtSummaryWhoCanMessageMe.Text = getValueMessage;
                        break;
                }

                string getValueBirth = MainSettings.SharedData?.GetString("whocanseemybirthday_key", data.BirthPrivacy ?? "");
                switch (getValueBirth)
                {
                    case "0":
                        TxtSummaryWhoCanSeeMyBirthday.Text = GetText(Resource.String.Lbl_Everyone);
                        SWhoCanSeeMyBirthday = "0";
                        break;
                    case "1":
                        TxtSummaryWhoCanSeeMyBirthday.Text = GetText(Resource.String.Lbl_People_i_Follow);
                        SWhoCanSeeMyBirthday = "1";
                        break;
                    case "2":
                        TxtSummaryWhoCanSeeMyBirthday.Text = GetText(Resource.String.Lbl_No_body);
                        SWhoCanSeeMyBirthday = "1";
                        break;
                    default:
                        TxtSummaryWhoCanSeeMyBirthday.Text = getValueBirth;
                        break;
                }

                string getValueTheme = MainSettings.SharedData?.GetString("Night_Mode_key", GetString(Resource.String.Lbl_Light)); 
                if (getValueTheme == MainSettings.LightMode)
                {
                    TxtSummaryTheme.Text = GetString(Resource.String.Lbl_Light);
                }
                else if (getValueTheme == MainSettings.DarkMode)
                {
                    TxtSummaryTheme.Text = GetString(Resource.String.Lbl_Dark);
                }
                else if (getValueTheme == MainSettings.DefaultMode)
                {
                    TxtSummaryTheme.Text = GetString(Resource.String.Lbl_SetByBattery);
                }
                else
                {
                    TxtSummaryTheme.Text = getValueTheme;
                }

                CheckBoxNotificationPopup.Checked = MainSettings.SharedData?.GetBoolean("notifications_key", true) ?? true;
                SwitchOnlineUsers.Checked = UserDetails.OnlineUsers = MainSettings.SharedData?.GetBoolean("onlineuser_key", true) ?? true;
                SwitchChatHeads.Checked = UserDetails.ChatHead = MainSettings.SharedData?.GetBoolean("chatheads_key", InitFloating.CanDrawOverlays(this)) ?? false;
                CheckBoxPlaySounds.Checked = UserDetails.SoundControl = MainSettings.SharedData?.GetBoolean("checkBox_PlaySound_key", UserDetails.SoundControl) ?? UserDetails.SoundControl;
                SwitchPip.Checked = UserDetails.FingerprintLock = MainSettings.SharedData?.GetBoolean("FingerprintLock_key", false) ?? false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Animate

        private void ToggleSection(View bt, View lyt)
        {
            try
            {
                if (lyt == null)
                    return;
                 
                bool show = ToggleArrow(bt);
                if (show)
                {
                    lyt.Measure(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    int targtetHeight = lyt.MeasuredHeight;

                    lyt.LayoutParameters.Height = 0;
                    lyt.Visibility = ViewStates.Visible;

                    ActionAnimation animation = new ActionAnimation(lyt, targtetHeight, "Expand");
                    lyt.StartAnimation(animation);
                }
                else
                {
                    int targtetHeight = lyt.MeasuredHeight;
                    ActionAnimation animation = new ActionAnimation(lyt, targtetHeight, "Collapse");
                    animation.Duration = ((int)(targtetHeight / lyt.Context.Resources.DisplayMetrics.Density));
                    lyt.StartAnimation(animation);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool ToggleArrow(View view)
        {
            try
            {
                if (view.Rotation == 0)
                {
                    view?.Animate()?.SetDuration(200)?.Rotation(180);
                    return true;
                }
                else
                {
                    view?.Animate()?.SetDuration(200)?.Rotation(0);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private class ActionAnimation : Animation
        {
            private readonly View View;
            private readonly int TargtetHeight;
            private readonly string Type;
            public ActionAnimation(View v, int targtetHeight, string type)
            {
                View = v;
                TargtetHeight = targtetHeight;
                Type = type;
            }

            protected override void ApplyTransformation(float interpolatedTime, Transformation t)
            {
                try
                {
                    if (Type == "Expand")
                    {
                        View.LayoutParameters.Height = interpolatedTime == 1 ? ViewGroup.LayoutParams.WrapContent : (int)(TargtetHeight * interpolatedTime);
                        View.RequestLayout();
                    }
                    else
                    {
                        if (interpolatedTime == 1)
                        {
                            View.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            View.LayoutParameters.Height = TargtetHeight - (int)(TargtetHeight * interpolatedTime);
                            View.RequestLayout();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    base.ApplyTransformation(interpolatedTime, t);
                }
            }

            public override bool WillChangeBounds()
            {
                return true;
            }
        }

        #endregion

        public void OpenChatHead()
        {
            try
            {
                if (UserDetails.ChatHead && !InitFloating.CanDrawOverlays(this))
                {
                    Intent intent = new Intent(Settings.ActionManageOverlayPermission, Uri.Parse("package:" + Application.Context.PackageName));
                    StartActivityForResult(intent, InitFloating.ChatHeadDataRequestCode);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InAppUpdate()
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
        }

    }
}