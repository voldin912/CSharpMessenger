using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Com.Google.Android.Exoplayer2.UI;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Utils;
using WoWonder.MediaPlayers.Exo;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Viewer
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class VideoFullScreenActivity : BaseActivity
    {
        #region Variables Basic

        public static VideoFullScreenActivity Instance;
        private string VideoUrl;
        //private int VideoDuration;

        public StyledPlayerView PlayerView;
        public ExoController ExoController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                //Set Full screen 
                Methods.App.FullScreenApp(this, true);

                //newUiOptions |= (int)SystemUiFlags.LowProfile;
                //newUiOptions |= (int)SystemUiFlags.Immersive;

                //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                //RequestedOrientation = ScreenOrientation.Landscape;

                SetContentView(Resource.Layout.VideoFullScreenLayout);

                Instance = this;

                VideoUrl = Intent?.GetStringExtra("videoUrl") ?? "";
                //VideoDuration = Intent?.GetIntExtra("videoDuration", 0) ?? 0;

                //Get Value And Set Toolbar
                InitComponent();
                InitBackPressed("VideoFullScreenActivity");
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
                StopVideo();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                ReleaseVideo();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
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
                PlayerView = FindViewById<StyledPlayerView>(Resource.Id.videoView);

                //===================== Exo Player ======================== 
                ExoController = new ExoController(this, PlayerView);
                ExoController.SetPlayer();
                ExoController.SetPlayerControl(true, true);

                ExoController.MFullScreenButton.Tag = "FullScreenOpen";

                // Uri
                Uri uri = Uri.Parse(VideoUrl);
                ExoController?.FirstPlayVideo(uri);

                ChatTabbedMainActivity.GetInstance()?.SetOnWakeLock();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void StopVideo()
        {
            try
            {
                ExoController?.StopVideo();

                ChatTabbedMainActivity.GetInstance()?.SetOffWakeLock();

                //GC Collect
                //GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ReleaseVideo()
        {
            try
            {
                ExoController?.ReleaseVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BackPressed()
        {
            try
            {
                ChatTabbedMainActivity.GetInstance()?.SetOffWakeLock();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                Finish();
            }
        }
    }
}