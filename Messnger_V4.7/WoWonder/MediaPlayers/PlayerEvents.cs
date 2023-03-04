using Android.App;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Metadata;
using Com.Google.Android.Exoplayer2.Text;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Video;
using System;
using WoWonder.Helpers.Utils;
using Object = Java.Lang.Object;

namespace WoWonder.MediaPlayers
{
    public class PlayerEvents : Object, IPlayer.IListener, StyledPlayerView.IControllerVisibilityListener, StyledPlayerControlView.IProgressUpdateListener
    {
        private readonly Activity ActContext;
        private readonly ImageButton VideoPlayButton;

        public PlayerEvents(Activity act, StyledPlayerControlView controlView)
        {
            try
            {
                ActContext = act;

                if (controlView != null)
                {
                    VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play_pause);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnAudioAttributesChanged(AudioAttributes audioAttributes)
        {

        }

        public void OnAudioSessionIdChanged(int audioSessionId)
        {

        }

        public void OnAvailableCommandsChanged(IPlayer.Commands availableCommands)
        {

        }

        public void OnCues(CueGroup cueGroup)
        {

        }

        public void OnDeviceInfoChanged(DeviceInfo deviceInfo)
        {

        }

        public void OnDeviceVolumeChanged(int volume, bool muted)
        {

        }

        public void OnEvents(IPlayer player, IPlayer.Events events)
        {

        }

        public void OnIsLoadingChanged(bool isLoading)
        {

        }

        public void OnIsPlayingChanged(bool isPlaying)
        {

        }

        public void OnLoadingChanged(bool p0)
        {

        }

        public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
        {

        }

        public void OnMediaItemTransition(MediaItem mediaItem, int reason)
        {

        }

        public void OnMediaMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnMetadata(Metadata metadata)
        {

        }

        public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
        {

        }

        public void OnPlaybackParametersChanged(PlaybackParameters p0)
        {

        }

        public void OnPlaybackStateChanged(int playbackState)
        {

        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {

        }

        public void OnPlayerError(PlaybackException error)
        {

        }

        public void OnPlayerErrorChanged(PlaybackException error)
        {

        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                if (VideoPlayButton == null)
                    return;

                switch (playbackState)
                {
                    case IPlayer.StateEnded:
                        {
                            switch (playWhenReady)
                            {
                                case false:
                                    VideoPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                                    break;
                                default:
                                    VideoPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                    break;
                            }
                            VideoPlayButton.Visibility = ViewStates.Visible;
                            break;
                        }
                    case IPlayer.StateReady:
                        {
                            switch (playWhenReady)
                            {
                                case false:
                                    VideoPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                    break;
                                default:
                                    VideoPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                                    break;
                            }
                            VideoPlayButton.Visibility = ViewStates.Visible;
                            break;
                        }
                    case IPlayer.StateBuffering:
                        VideoPlayButton.Visibility = ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnPositionDiscontinuity(int p0)
        {

        }

        public void OnRenderedFirstFrame()
        {

        }

        public void OnRepeatModeChanged(int p0)
        {

        }

        public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
        {

        }

        public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
        {

        }

        public void OnSeekProcessed()
        {

        }

        public void OnShuffleModeEnabledChanged(bool p0)
        {

        }

        public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
        {

        }

        public void OnSurfaceSizeChanged(int width, int height)
        {

        }

        public void OnTimelineChanged(Timeline timeline, int reason)
        {

        }

        public void OnTrackSelectionParametersChanged(TrackSelectionParameters parameters)
        {

        }

        public void OnTracksChanged(Tracks tracks)
        {

        }

        public void OnVideoSizeChanged(VideoSize videoSize)
        {

        }

        public void OnVolumeChanged(float volume)
        {

        }

        public void OnProgressUpdate(long position, long bufferedPosition)
        {

        }

        public void OnVisibilityChanged(int visibility)
        {

        }
    }
}