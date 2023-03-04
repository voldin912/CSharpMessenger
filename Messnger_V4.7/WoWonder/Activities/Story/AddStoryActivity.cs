using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Aghajari.Emojiview.View;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Story.Service;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.JobWorker;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AddStoryActivity : BaseActivity
    {
        #region Variables Basic

        private ImageView StoryImageView;
        private ImageView EmojisView;
        private CircleButton AddStoryButton;
        private AXEmojiEditText EmojisIconEditText;
        private string PathStory = "", Type = "", Thumbnail = UserDetails.Avatar;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.AddStoryLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                if (Intent != null)
                {
                    Thumbnail = Intent.GetStringExtra("Thumbnail") ?? UserDetails.Avatar;

                    var dataUri = Intent.GetStringExtra("Uri") ?? "Data not available";
                    if (dataUri != "Data not available" && !string.IsNullOrEmpty(dataUri))
                        PathStory = dataUri; // Uri file 
                    var dataType = Intent.GetStringExtra("Type") ?? "Data not available";
                    if (dataType != "Data not available" && !string.IsNullOrEmpty(dataType))
                        Type = dataType; // Type file  
                }

                switch (Type)
                {
                    case "image":
                        SetImageStory(PathStory);
                        break;
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                DestroyBasic();

                base.OnDestroy();
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
                StoryImageView = FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                EmojisView = FindViewById<ImageView>(Resource.Id.emojiicon);
                EmojisIconEditText = FindViewById<AXEmojiEditText>(Resource.Id.EmojiconEditText5);
                AddStoryButton = FindViewById<CircleButton>(Resource.Id.sendButton);

                InitEmojisView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Methods.SetColorEditText(EmojisIconEditText, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(AppSettings.MainColor);

                    EmojisViewTools.MStickerView = false;
                    EmojisViewTools.LoadView(this, EmojisIconEditText, "", EmojisView);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_Addnewstory);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        AddStoryButton.Click += AddStoryButtonOnClick;
                        break;
                    default:
                        AddStoryButton.Click -= AddStoryButtonOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetImageStory(string url)
        {
            try
            {
                StoryImageView.Visibility = StoryImageView.Visibility switch
                {
                    ViewStates.Gone => ViewStates.Visible,
                    _ => StoryImageView.Visibility
                };

                var file = Uri.FromFile(new File(url));

                Glide.With(this).Load(file?.Path).Apply(new RequestOptions()).Into(StoryImageView);

                // GlideImageLoader.LoadImage(this, file.Path, StoryImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
                StoryImageView = null!;
                EmojisView = null!;
                EmojisIconEditText = null!;
                AddStoryButton = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //add
        private void AddStoryButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var item = new FileModel
                    {
                        StoryFileType = Type,
                        StoryFilePath = PathStory,
                        StoryDescription = EmojisIconEditText.Text,
                        StoryTitle = EmojisIconEditText.Text,
                        StoryThumbnail = Thumbnail,
                    };

                    Intent intent = new Intent(this, typeof(StoryService));
                    intent.SetAction(StoryService.ActionStory);
                    intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                    StartService(intent);

                    Finish();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}