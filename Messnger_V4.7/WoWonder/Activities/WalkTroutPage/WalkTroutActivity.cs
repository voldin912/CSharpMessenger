using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide;
using WoWonder.Activities.Base;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient.Requests;
using WoWonder.Activities.SuggestedUsers;
 
namespace WoWonder.Activities.WalkTroutPage
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class WalkTroutActivity : BaseActivity  
    {
        #region Variables Basic

        private static int MaxStep = 4;

        private ViewPager ViewPager;
        private WalkTroutPagerAdapter MAdapter;
        private AppCompatButton BtnNext, BtnDone;
        private TextView BtnSkip;
        private List<Classes.ModelsWalkTroutPager> ListPage = new List<Classes.ModelsWalkTroutPager>();

        private int Count = 0;
        private string Caller = "";
        private RequestBuilder FullGlideRequestBuilder;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this, true);
                 
                // Create your application here
                SetContentView(Resource.Layout.WalkTroutLayout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                ListPage = new List<Classes.ModelsWalkTroutPager>
                {
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page1),
                        Description = GetText(Resource.String.Lbl_Description_page1),
                        Image = Resource.Drawable.icon_WalkTroutPage1,
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page2),
                        Description = GetText(Resource.String.Lbl_Description_page2),
                        Image = Resource.Drawable.icon_WalkTroutPage2_vector
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page3),
                        Description = GetText(Resource.String.Lbl_Description_page3),
                        Image = Resource.Drawable.icon_WalkTroutPage3_vector
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page4),
                        Description = GetText(Resource.String.Lbl_Description_page4),
                        Image = Resource.Drawable.icon_WalkTroutPage4
                    }
                };

                MaxStep = ListPage.Count;

                //Get Value And Set Toolbar
                InitComponent();
                Pressed();
                LoadData();
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
                base.OnTrimMemory(level);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
                base.OnLowMemory();
                GC.Collect(GC.MaxGeneration);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = (ViewPager)FindViewById(Resource.Id.view_pager);
                BtnNext = (AppCompatButton)FindViewById(Resource.Id.btn_next);
                BtnSkip = (TextView)FindViewById(Resource.Id.btn_skip);
                BtnDone = (AppCompatButton)FindViewById(Resource.Id.btn_done);

                BtnNext.Visibility = ViewStates.Visible;
                BtnSkip.Visibility = ViewStates.Visible;
                BtnDone.Visibility = ViewStates.Gone;

                // adding bottom dots
                BottomProgressDots(0);

                MAdapter = new WalkTroutPagerAdapter(this, ListPage); 
                ViewPager.Adapter = MAdapter;
                ViewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });

                FullGlideRequestBuilder = Glide.With(this).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(200);

                List<string> stickerList = new List<string>();
                stickerList.AddRange(StickersModel.Locally.StickerList1);
                stickerList.AddRange(StickersModel.Locally.StickerList2);
                stickerList.AddRange(StickersModel.Locally.StickerList3);
                stickerList.AddRange(StickersModel.Locally.StickerList4);
                stickerList.AddRange(StickersModel.Locally.StickerList5);
                stickerList.AddRange(StickersModel.Locally.StickerList6);
                stickerList.AddRange(StickersModel.Locally.StickerList7);
                stickerList.AddRange(StickersModel.Locally.StickerList8);
                stickerList.AddRange(StickersModel.Locally.StickerList9);
                stickerList.AddRange(StickersModel.Locally.StickerList10);
                stickerList.AddRange(StickersModel.Locally.StickerList11);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (var item in stickerList)
                        {
                            FullGlideRequestBuilder.Load(item).Preload();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
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
                    BtnSkip.Click += BtnSkipOnClick;
                    BtnNext.Click += BtnNextOnClick;
                    BtnDone.Click += BtnDoneOnClick;
                    ViewPager.PageSelected += ViewPagerOnPageSelected;
                }
                else
                {
                    BtnSkip.Click -= BtnSkipOnClick;
                    BtnNext.Click -= BtnNextOnClick;
                    BtnDone.Click -= BtnDoneOnClick;
                    ViewPager.PageSelected -= ViewPagerOnPageSelected;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void BtnDoneOnClick(object sender, EventArgs e)
        {
            try
            {
                int current = ViewPager.CurrentItem + 1;
                if (current < MaxStep)
                {
                    // move to next screen
                    ViewPager.CurrentItem = current;
                }
                else
                {
                    if (Caller.Contains("register"))
                    {
                        if (AppSettings.ShowSuggestedUsersOnRegister)
                        {
                            Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                            newIntent.PutExtra("class", "register");
                            StartActivity(newIntent);
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                        }
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                    }

                    Finish();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                int current = ViewPager.CurrentItem + 1;
                if (current < MaxStep)
                {
                    // move to next screen
                    ViewPager.CurrentItem = current; 
                }
                else
                {
                    BtnNext.Visibility = ViewStates.Gone;
                    BtnSkip.Visibility = ViewStates.Gone;
                    BtnDone.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSkipOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    if (AppSettings.ShowSuggestedUsersOnRegister)
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                    }
                }
                else
                {
                    StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                }
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            try
            {
                BottomProgressDots(e.Position);
                Pressed();

                if (e.Position == ListPage.Count - 1)
                {
                    BtnNext.Visibility = ViewStates.Gone;
                    BtnSkip.Visibility = ViewStates.Gone;
                    BtnDone.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    Dictionary<string, string> dictionaryProfile = new Dictionary<string, string>();

                                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dictionaryProfile = new Dictionary<string, string>();

                                        dataUser.Lat = UserDetails.Lat;
                                        dataUser.Lat = UserDetails.Lat;

                                        var sqLiteDatabase = new SqLiteDatabase();
                                        sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    }

                                    dictionaryProfile.Add("lat", UserDetails.Lat);
                                    dictionaryProfile.Add("lng", UserDetails.Lng);

                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dictionaryProfile) });
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted && PermissionsController.CheckPermissionStorage("file"):
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 108:
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
         
        private void Pressed()
        {
            try
            {
                if (Count == 0) //Location
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(105);
                }
                else if (Count == 1) //Contacts
                {
                    if ((int)Build.VERSION.SdkInt > 23 && AppSettings.InvitationSystem)
                        new PermissionsController(this).RequestPermission(101);
                }
                else if (Count == 2) // Record
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(102);
                }
                else if (Count == 3) // Storage & Camera
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(108,"file");
                }
                Count++;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BottomProgressDots(int currentIndex)
        {
            try
            {
                LinearLayout dotsLayout = (LinearLayout)FindViewById(Resource.Id.layoutDots);
                ImageView[] dots = new ImageView[MaxStep];

                dotsLayout?.RemoveAllViews();
                for (int i = 0; i < dots.Length; i++)
                {
                    dots[i] = new ImageView(this);
                    int widthHeight = 15;
                    LinearLayout.LayoutParams paramsParams = new LinearLayout.LayoutParams(new ViewGroup.LayoutParams(widthHeight, widthHeight));
                    paramsParams.SetMargins(10, 10, 10, 10);
                    dots[i].LayoutParameters = paramsParams;
                    dots[i].SetImageResource(Resource.Drawable.Shape_Radius_Btn);
                    dots[i].SetColorFilter(Color.ParseColor("#cccccc"), PorterDuff.Mode.SrcIn);
                    dotsLayout?.AddView(dots[i]);
                }

                if (dots.Length > 0)
                {
                    dots[currentIndex].SetImageResource(Resource.Drawable.Shape_Radius_Btn);
                    dots[currentIndex].SetColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        } 
    }
}