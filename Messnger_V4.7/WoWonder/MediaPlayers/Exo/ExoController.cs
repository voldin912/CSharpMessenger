using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.Net;
using System;
using WoWonder.Activities.Viewer;
using WoWonder.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace WoWonder.MediaPlayers.Exo
{
    public class ExoController
    {
        private readonly Activity ActivityContext;

        private IExoPlayer VideoPlayer;
        private readonly StyledPlayerView PlayerView;
        private StyledPlayerControlView ControlView;

        private PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

        private IDataSource.IFactory DataSourceFactory;
        private IDataSource.IFactory HttpDataSourceFactory;
        private PlayerEvents PlayerListener;

        private ImageView MVolumeIcon, MFullScreenIcon;
        public FrameLayout MFullScreenButton;

        public ExoController(Activity context, StyledPlayerView playerView)
        {
            try
            {
                ActivityContext = context;
                PlayerView = playerView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayer(bool useController = true)
        {
            try
            {
                PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(ActivityContext);
                DefaultTrackSelector trackSelector = new DefaultTrackSelector(ActivityContext);
                ControlView = PlayerView.FindViewById<StyledPlayerControlView>(Resource.Id.exo_controller);

                VideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(trackSelector)?.Build();
                PlayerListener = new PlayerEvents(ActivityContext, ControlView);
                VideoPlayer?.AddListener(PlayerListener);

                PlayerView.UseController = useController;
                PlayerView.Player = VideoPlayer;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayerControl(bool showFullScreen = true, bool isFullScreen = false)
        {
            try
            {
                if (ControlView != null)
                {
                    //Check All Views 
                    MVolumeIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_volume_icon);
                    MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    if (!showFullScreen)
                    {
                        MVolumeIcon.Visibility = ViewStates.Gone;
                        MFullScreenIcon.Visibility = ViewStates.Gone;
                        MFullScreenButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MFullScreenButton.Click += MFullScreenButtonOnClick;

                        if (isFullScreen)
                        {
                            MFullScreenButton.Tag = "FullScreenOpen";
                            //MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_skrink);
                        }
                        else
                        {
                            MFullScreenButton.Tag = "FullScreenClose";
                            //MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_expand);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                if (DataSourceFactory == null)
                {
                    DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, GetHttpDataSourceFactory());
                    DataSourceFactory = BuildReadOnlyCacheDataSource(upstreamFactory, PreCachingExoPlayerVideo.GetCache());
                }

                IMediaSource src = new ProgressiveMediaSource.Factory(DataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private IDataSource.IFactory GetHttpDataSourceFactory()
        {
            if (HttpDataSourceFactory == null)
            {
                CookieManager cookieManager = new CookieManager();
                cookieManager.SetCookiePolicy(ICookiePolicy.AcceptOriginalServer);
                CookieHandler.Default = cookieManager;
                HttpDataSourceFactory = new DefaultHttpDataSource.Factory();
            }

            return HttpDataSourceFactory;
        }

        private CacheDataSource.Factory BuildReadOnlyCacheDataSource(IDataSource.IFactory upstreamFactory, ICache cache)
        {
            return new CacheDataSource.Factory()?.SetCache(cache)?.SetUpstreamDataSourceFactory(upstreamFactory)?.SetCacheWriteDataSinkFactory(null)?.SetFlags(CacheDataSource.FlagIgnoreCacheOnError);
        }

        public void FirstPlayVideo(Uri uri)
        {
            try
            {
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                }

                VideoPlayer.SetMediaSource(videoSource, true);
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(0, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FirstPlayVideo(Uri uri, int videoDuration)
        {
            try
            {
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                }

                VideoPlayer.SetMediaSource(videoSource);
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(videoDuration);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void PlayVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlaybackState == IPlayer.StateReady && !PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = true;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                StopVideo();
                PlayerView?.Player?.Stop();

                if (VideoPlayer != null)
                {
                    VideoPlayer.Release();
                    VideoPlayer = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public StyledPlayerView GetPlayerView()
        {
            return PlayerView;
        }

        public IExoPlayer GetExoPlayer()
        {
            return VideoPlayer;
        }

        private void MFullScreenButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MFullScreenButton?.Tag?.ToString() == "FullScreenClose")
                {
                    Intent intent = new Intent(ActivityContext, typeof(VideoFullScreenActivity));
                    intent.PutExtra("videoDuration", VideoPlayer.Duration);
                    ActivityContext.StartActivityForResult(intent, 2000);
                }
                else if (MFullScreenButton?.Tag?.ToString() == "FullScreenOpen")
                {
                    Intent intent = new Intent();
                    VideoFullScreenActivity.Instance?.SetResult(Result.Ok, intent);
                    VideoFullScreenActivity.Instance?.Finish();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RestartPlayAfterShrinkScreen(IExoPlayer player)
        {
            try
            {
                PlayerView.Player = null!;
                PlayerView.Player = player;
                PlayerView.Player.PlayWhenReady = true;
                //MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


    }
}