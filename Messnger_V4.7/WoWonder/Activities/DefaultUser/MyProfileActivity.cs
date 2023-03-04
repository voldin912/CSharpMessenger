using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.DefaultUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyProfileActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView ImageUser, MoreButton;
        private TextView TxtName, TxtUsername;

        private AppCompatButton EditButton;
        private LinearLayout ChatButton;
        private SuperTextView TxtAboutUser;

        private LinearLayout FollowersLayout, FollowingLayout;
        private TextView TxtCountFollowers, TxtFollowers, TxtCountFollowing, TxtFollowing;

        private LinearLayout SocialLiner;
        private TextView TextSocialLinks;
        private LinearLayout FacebookButton, TwitterButton, InstegramButton, VkButton, YoutubeButton;

        private LinearLayout MediaLinear;
        private RecyclerView MRecycler;

        private LinearLayout WorkLiner, StudyLiner, CountryLiner, MobileLiner, GenderLiner, WebsiteLiner, BirthdayLiner, RelationshipLiner;
        private TextView TextWork, TextStudy, TextCountry, TextMobile, TextGender, TextWebsite, TextBirthday, TextRelationship;
         
        private UserDataObject UserData;

        private PublisherAdView PublisherAdView;

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
                SetContentView(Resource.Layout.UserProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 
                GetMyInfoData();
                 
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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                PublisherAdView?.Destroy();

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
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
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
                MoreButton = FindViewById<ImageView>(Resource.Id.IconMore);

                ImageUser = FindViewById<ImageView>(Resource.Id.imageUser);
                TxtName = FindViewById<TextView>(Resource.Id.name);
                TxtUsername = FindViewById<TextView>(Resource.Id.username);

                EditButton = FindViewById<AppCompatButton>(Resource.Id.FollowButton);
                EditButton.SetBackgroundResource(Resource.Drawable.round_button_outline);
                EditButton.Text = GetText(Resource.String.Lbl_Edit);

                ChatButton = FindViewById<LinearLayout>(Resource.Id.ChatButton);
                ChatButton.Visibility = ViewStates.Gone;

                TxtAboutUser = FindViewById<SuperTextView>(Resource.Id.Txt_AboutUser);

                FollowersLayout = FindViewById<LinearLayout>(Resource.Id.FollowersLayout);
                TxtCountFollowers = FindViewById<TextView>(Resource.Id.countFollowers);
                TxtFollowers = FindViewById<TextView>(Resource.Id.txtFollowers);

                FollowingLayout = FindViewById<LinearLayout>(Resource.Id.FollowingLayout);
                TxtCountFollowing = FindViewById<TextView>(Resource.Id.countFollowing);
                TxtFollowing = FindViewById<TextView>(Resource.Id.txtFollowing);

                TextSocialLinks = FindViewById<TextView>(Resource.Id.textSocialLinks);
                SocialLiner = FindViewById<LinearLayout>(Resource.Id.socialLiner);
                FacebookButton = FindViewById<LinearLayout>(Resource.Id.FacebookLiner);
                TwitterButton = FindViewById<LinearLayout>(Resource.Id.TwitterLiner);
                InstegramButton = FindViewById<LinearLayout>(Resource.Id.InstagramLiner);
                VkButton = FindViewById<LinearLayout>(Resource.Id.VkLiner);
                YoutubeButton = FindViewById<LinearLayout>(Resource.Id.YoutubeLiner);

                MediaLinear = FindViewById<LinearLayout>(Resource.Id.MediaLinear);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);

                WorkLiner = FindViewById<LinearLayout>(Resource.Id.WorkLiner);
                TextWork = FindViewById<TextView>(Resource.Id.textWork);

                StudyLiner = FindViewById<LinearLayout>(Resource.Id.StudyLiner);
                TextStudy = FindViewById<TextView>(Resource.Id.textStudy);

                CountryLiner = FindViewById<LinearLayout>(Resource.Id.CountryLiner);
                TextCountry = FindViewById<TextView>(Resource.Id.textCountry);

                MobileLiner = FindViewById<LinearLayout>(Resource.Id.MobileLiner);
                TextMobile = FindViewById<TextView>(Resource.Id.textMobile);

                GenderLiner = FindViewById<LinearLayout>(Resource.Id.GenderLiner);
                TextGender = FindViewById<TextView>(Resource.Id.textGender);

                WebsiteLiner = FindViewById<LinearLayout>(Resource.Id.WebsiteLiner);
                TextWebsite = FindViewById<TextView>(Resource.Id.textWebsite);

                BirthdayLiner = FindViewById<LinearLayout>(Resource.Id.BirthdayLiner);
                TextBirthday = FindViewById<TextView>(Resource.Id.textBirthday);

                RelationshipLiner = FindViewById<LinearLayout>(Resource.Id.RelationshipLiner);
                TextRelationship = FindViewById<TextView>(Resource.Id.textRelationship);

                MRecycler.Visibility = ViewStates.Gone;
                MediaLinear.Visibility = ViewStates.Gone;

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolbar.Title = " ";
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
                    MoreButton.Click += MoreButtonOnClick;
                    EditButton.Click += EditButtonOnClick; 
                    FacebookButton.Click += FacebookButtonOnClick;
                    TwitterButton.Click += TwitterButtonOnClick;
                    InstegramButton.Click += InstegramButtonOnClick;
                    VkButton.Click += VkButtonOnClick;
                    YoutubeButton.Click += YoutubeButtonOnClick; 
                }
                else
                {
                    MoreButton.Click -= MoreButtonOnClick;
                    EditButton.Click -= EditButtonOnClick; 
                    FacebookButton.Click -= FacebookButtonOnClick;
                    TwitterButton.Click -= TwitterButtonOnClick;
                    InstegramButton.Click -= InstegramButtonOnClick;
                    VkButton.Click -= VkButtonOnClick;
                    YoutubeButton.Click -= YoutubeButtonOnClick; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void YoutubeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenYoutubeIntent(UserData.Youtube);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VkButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenVkontakteIntent(UserData.Vk);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InstegramButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenInstagramIntent(UserData.Instagram);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TwitterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenTwitterIntent(UserData.Twitter);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FacebookButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenFacebookIntent(this, UserData.Facebook);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);
                 
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));

                dialogList.SetTitle(GetString(Resource.String.Lbl_Menu_More));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void EditButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditMyProfileActivity));
                StartActivityForResult(intent, 230);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 230 && resultCode == Result.Ok)
                {
                    GetMyInfoData();
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
                UserData = ListUtils.MyProfileList.FirstOrDefault();
                LoadDataUser(UserData);

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Data user Profile API
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data");

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

                TxtUsername.Text = data.Username;

                string followers = Methods.FunString.FormatPriceValue(Convert.ToInt32(data.Details.DetailsClass.FollowersCount));
                string following = Methods.FunString.FormatPriceValue(Convert.ToInt32(data.Details.DetailsClass.FollowingCount));

                TxtCountFollowers.Text = followers;
                TxtCountFollowing.Text = following;

                if (AppSettings.ConnectivitySystem == 0)// Friend
                {
                    TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                    FollowingLayout.Visibility = ViewStates.Gone;
                }

                TextSanitizer sanitizer = new TextSanitizer(TxtAboutUser, this);
                sanitizer.Load(WoWonderTools.GetAboutFinal(data));
                 
                FacebookButton.Visibility = !string.IsNullOrEmpty(data.Facebook) ? ViewStates.Visible : ViewStates.Gone;
                TwitterButton.Visibility = !string.IsNullOrEmpty(data.Twitter) ? ViewStates.Visible : ViewStates.Gone;
                InstegramButton.Visibility = !string.IsNullOrEmpty(data.Instagram) ? ViewStates.Visible : ViewStates.Gone;
                VkButton.Visibility = !string.IsNullOrEmpty(data.Vk) ? ViewStates.Visible : ViewStates.Gone;
                YoutubeButton.Visibility = !string.IsNullOrEmpty(data.Youtube) ? ViewStates.Visible : ViewStates.Gone;

                if (string.IsNullOrEmpty(data.Facebook) && string.IsNullOrEmpty(data.Twitter) && string.IsNullOrEmpty(data.Instagram) && string.IsNullOrEmpty(data.Vk) && string.IsNullOrEmpty(data.Youtube))
                {
                    SocialLiner.Visibility = ViewStates.Gone;
                    TextSocialLinks.Visibility = ViewStates.Gone;
                }

                if (!string.IsNullOrEmpty(data.Working))
                {
                    WorkLiner.Visibility = ViewStates.Visible;
                    TextWork.Text = data.Working;
                }
                else
                    WorkLiner.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(data.School))
                {
                    StudyLiner.Visibility = ViewStates.Visible;
                    TextStudy.Text = data.School;
                }
                else
                    StudyLiner.Visibility = ViewStates.Gone;
                 
                if (!string.IsNullOrEmpty(data.CountryId) && data.CountryId != "0")
                {
                    CountryLiner.Visibility = ViewStates.Visible;
                    var countryName = WoWonderTools.GetCountryList(this).FirstOrDefault(a => a.Key == data.CountryId).Value;
                    TextCountry.Text = countryName;
                }
                else
                    CountryLiner.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(data.PhoneNumber))
                {
                    MobileLiner.Visibility = ViewStates.Visible;
                    TextMobile.Text = data.PhoneNumber;
                }
                else
                    MobileLiner.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(data.Website))
                {
                    WebsiteLiner.Visibility = ViewStates.Visible;
                    TextWebsite.Text = data.Website;
                }
                else
                    WebsiteLiner.Visibility = ViewStates.Gone;

                if (data.BirthPrivacy == "0") // Everyone
                {
                    BirthdayLiner.Visibility = ViewStates.Visible;
                }
                else if (data.BirthPrivacy == "1") // People i Follow
                {
                    if (data.IsFollowingMe == "0")
                    {
                        BirthdayLiner.Visibility = data.IsFollowing == "0" ? ViewStates.Gone : ViewStates.Visible;
                    }
                    else if (data.IsFollowingMe == "1")
                    {
                        BirthdayLiner.Visibility = ViewStates.Visible;
                    }
                }
                else
                {
                    BirthdayLiner.Visibility = ViewStates.Visible;
                }

                try
                {
                    if (data.Birthday != "0000-00-00" && !string.IsNullOrEmpty(data.Birthday))
                    {
                        DateTime date = DateTime.Parse(data.Birthday);
                        string newFormat = date.Day + "/" + date.Month + "/" + date.Year;
                        TextBirthday.Text = newFormat;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data.Birthday))
                            TextBirthday.Text = data.Birthday;
                        else
                        {
                            BirthdayLiner.Visibility = ViewStates.Gone;
                        }
                    } 
                }
                catch
                {
                    if (!string.IsNullOrEmpty(data.Birthday))
                        TextBirthday.Text = data.Birthday;
                    else
                    {
                        BirthdayLiner.Visibility = ViewStates.Gone;
                    }
                }

                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    var value = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Key == data.Gender).Value;
                    TextGender.Text = value ?? GetText(Resource.String.Radio_Male);
                }
                else
                {
                    TextGender.Text = data.Gender switch
                    {
                        "male" => GetText(Resource.String.Radio_Male),
                        "female" => GetText(Resource.String.Radio_Female),
                        _ => GetText(Resource.String.Radio_Male)
                    };
                }

                string relationship = WoWonderTools.GetRelationship(Convert.ToInt32(data.RelationshipId));
                if (Methods.FunString.StringNullRemover(relationship) != "Empty")
                {
                    RelationshipLiner.Visibility = ViewStates.Visible;
                    TextRelationship.Text = relationship;
                }
                else
                    RelationshipLiner.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(IDialogInterface dialog, int position, string text)
        {
            try
            { 
                if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as video
                    if (!CrossShare.IsSupported) return;

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = WoWonderTools.GetNameFinal(UserData),
                        Text = "",
                        Url = UserData.Url
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion
         
    }
}