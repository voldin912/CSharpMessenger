using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.OS;
using Newtonsoft.Json;
using TwilioVideo;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using VideoView = TwilioVideo.VideoView;

namespace WoWonder.Activities.Call.Twilio
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ResizeableActivity = true, SupportsPictureInPicture = true)]
    public class TwilioVideoCallActivity : AppCompatActivity, TwilioVideoHelper.IListener
    {

        #region Variables Basic

        private TwilioVideoHelper TwilioVideo { get; set; }
        private string CallType = "0";
        private CallUserObject CallUserObject;

        private VideoView UserPrimaryVideo, ThumbnailVideo;
        private LocalVideoTrack LocalVideoTrack;
        private VideoTrack UserVideoTrack;

        //Controls
        private RelativeLayout MainVideoViewLayout;

        private FrameLayout LocalVideoOverlay;
        private ImageView IconMuteVoiceLocalVideo, IconMuteLocalVideo;
        private ImageView IconMuteVoiceRemoteVideo, IconMuteRemoteVideo;

        private LinearLayout TopControlLayout;
        private FrameLayout BottomControlLayout;
        private ImageView IconBack; //wael add call in background

        private LinearLayout SwitchButtonLayout, EndCallButton, StopVideoButton, MuteAudioButton;
        private ImageView IconEndCall, IconSwitch, IconStopVideo, IconMute;
        private ImageView UserImageView;
        private TextView UserNameTextView, DurationTextView;
        private TextView IconSignal;

        private bool DataUpdated;
        private int CountSecondsOfOutGoingCall;
        private string LocalVideoTrackId, RemoteVideoTrackId;
        private Timer TimerRequestWaiter;

        private ChatTabbedMainActivity GlobalContext;

        private PictureInPictureParams PictureInPictureParams;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);

                // Create your application here
                SetContentView(Resource.Layout.TwilioVideoCallActivityLayout);

                GlobalContext = ChatTabbedMainActivity.GetInstance();
                //Get Value And Set Toolbar
                InitComponent();
                InitBackPressed();
                InitTwilioCall();
                ChatTabbedMainActivity.RunCall = true;
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
                UpdateState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();
                UpdateState();
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
                DataUpdated = false;
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnRestart()
        {
            try
            {
                base.OnRestart();
                TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Video);
                UpdateState();
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
                ChatTabbedMainActivity.RunCall = false;
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                ChatTabbedMainActivity.RunCall = false;
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainVideoViewLayout = (RelativeLayout)FindViewById(Resource.Id.activity_video_chat_view);
                MainVideoViewLayout.Tag = "show";

                UserPrimaryVideo = FindViewById<VideoView>(Resource.Id.primary_video_view); // user video 
                ThumbnailVideo = FindViewById<VideoView>(Resource.Id.local_video_view_container); //local video 

                TopControlLayout = FindViewById<LinearLayout>(Resource.Id.top_control);
                IconBack = FindViewById<ImageView>(Resource.Id.icon_back);
                IconBack.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);

                LocalVideoOverlay = (FrameLayout)FindViewById(Resource.Id.local_video_overlay);
                IconMuteVoiceLocalVideo = FindViewById<ImageView>(Resource.Id.iconMuteVoice_local_video);
                IconMuteLocalVideo = FindViewById<ImageView>(Resource.Id.iconMute_local_video);

                IconMuteVoiceRemoteVideo = FindViewById<ImageView>(Resource.Id.iconMuteVoice_remote_video);
                IconMuteRemoteVideo = FindViewById<ImageView>(Resource.Id.iconMute_remote_video);

                BottomControlLayout = FindViewById<FrameLayout>(Resource.Id.bottom_control);
                SwitchButtonLayout = FindViewById<LinearLayout>(Resource.Id.SwitchButtonLayout);
                IconSwitch = FindViewById<ImageView>(Resource.Id.iconSwitch);

                EndCallButton = FindViewById<LinearLayout>(Resource.Id.EndCallButtonLayout);
                IconEndCall = FindViewById<ImageView>(Resource.Id.iconEndCall);

                StopVideoButton = FindViewById<LinearLayout>(Resource.Id.StopVideoButtonLayout);
                IconStopVideo = FindViewById<ImageView>(Resource.Id.iconStopVideo);

                MuteAudioButton = FindViewById<LinearLayout>(Resource.Id.MuteButtonLayout);
                IconMute = FindViewById<ImageView>(Resource.Id.iconMute);

                UserImageView = FindViewById<ImageView>(Resource.Id.userImage);
                UserNameTextView = FindViewById<TextView>(Resource.Id.name);

                IconSignal = FindViewById<TextView>(Resource.Id.icon_signal);
                DurationTextView = FindViewById<TextView>(Resource.Id.time);

                if (!PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                {
                    //PictureInToPictureButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    var pictureInPicture = new PictureInPictureParams.Builder().SetAspectRatio(new Rational(9, 16));
                    //.SetSourceRectHint(sourceRectHint)
                    if ((int)Build.VERSION.SdkInt >= 31)
                        pictureInPicture.SetAutoEnterEnabled(true);

                    PictureInPictureParams = pictureInPicture?.Build();
                    SetPictureInPictureParams(PictureInPictureParams);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitBackPressed()
        {
            try
            {
                if (BuildCompat.IsAtLeastT && Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "TwilioVideoCallActivity"));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "TwilioVideoCallActivity", true));
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
                    MainVideoViewLayout.Click += MainVideoViewLayoutOnClick;
                    SwitchButtonLayout.Click += SwitchCamButtonOnClick;
                    StopVideoButton.Click += StopVideoButtonOnClick;
                    EndCallButton.Click += EndCallButtonOnClick;
                    MuteAudioButton.Click += MuteAudioButtonOnClick;
                    IconBack.Click += PictureInToPictureButtonOnClick;
                }
                else
                {
                    MainVideoViewLayout.Click -= MainVideoViewLayoutOnClick;
                    SwitchButtonLayout.Click -= SwitchCamButtonOnClick;
                    StopVideoButton.Click -= StopVideoButtonOnClick;
                    EndCallButton.Click -= EndCallButtonOnClick;
                    MuteAudioButton.Click -= MuteAudioButtonOnClick;
                    IconBack.Click -= PictureInToPictureButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MainVideoViewLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var stat = MainVideoViewLayout.Tag?.ToString();
                if (stat == "show")
                {
                    MainVideoViewLayout.Tag = "hide";

                    TopControlLayout.Visibility = ViewStates.Gone;
                    BottomControlLayout.Visibility = ViewStates.Gone;
                }
                else if (stat == "hide")
                {
                    MainVideoViewLayout.Tag = "show";

                    TopControlLayout.Visibility = ViewStates.Visible;
                    BottomControlLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PictureInToPictureButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    EnterPictureInPictureMode(PictureInPictureParams);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCamButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                TwilioVideo.FlipCamera();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopVideoButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (StopVideoButton.Selected)
                {
                    StopVideoButton.Selected = false;
                    IconStopVideo.SetImageResource(Resource.Drawable.icon_video_camera);
                }
                else
                {
                    StopVideoButton.Selected = true;
                    IconStopVideo.SetImageResource(Resource.Drawable.icon_video_camera_mute);
                }
                LocalVideoTrack.Enable(StopVideoButton.Selected);

                var IsVideoEnabled = StopVideoButton.Selected;
                ThumbnailVideo.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Gone;
                //LocalVideoView.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Gone;

                LocalVideoOverlay.Visibility = IsVideoEnabled ? ViewStates.Gone : ViewStates.Visible;
                IconMuteLocalVideo.Visibility = IsVideoEnabled ? ViewStates.Gone : ViewStates.Visible;

                //old
                //ThumbnailVideo.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MuteAudioButtonOnClick(object sender, EventArgs e)
        {

            try
            {
                if (MuteAudioButton.Selected)
                {
                    MuteAudioButton.Selected = false;
                    IconMute.SetImageResource(Resource.Drawable.icon_mic_vector);
                }
                else
                {
                    MuteAudioButton.Selected = true;
                    IconMute.SetImageResource(Resource.Drawable.icon_microphone_mute);
                }

                TwilioVideo.Mute(MuteAudioButton.Selected);

                var visibleMutedLayers = MuteAudioButton.Selected ? ViewStates.Visible : ViewStates.Gone;
                //LocalVideoOverlay.Visibility = visibleMutedLayers;
                IconMuteLocalVideo.Visibility = visibleMutedLayers;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EndCallButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishCall();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Twilio  

        private async void InitTwilioCall()
        {
            try
            {
                CallType = Intent?.GetStringExtra("type") ?? ""; // Twilio_video_call , Twilio_audio_call,Agora_video_call_recieve,Agora_audio_call_recieve

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                switch (CallType)
                {
                    case "Twilio_video_call":
                        {
                            if (!string.IsNullOrEmpty(CallUserObject.UserId))
                                Load_userWhenCall();

                            TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Video);
                            UpdateState();
                            DurationTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

                            var (apiStatus, respond) = await RequestsAsync.Call.AnswerCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video);
                            if (apiStatus == 200)
                            {
                                ConnectToRoom();

                                ChatTabbedMainActivity.AddCallToListAndSend("Answered", GetText(Resource.String.Lbl_Incoming), TypeCall.Video, CallUserObject);
                            }
                            //else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                    case "Twilio_video_calling_start":
                        DurationTextView.Text = GetText(Resource.String.Lbl_Calling_video);
                        TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Video);

                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("outgoin_call.mp3", "Looping");

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_Create_callEvent(CallUserObject.UserId);

                        StartApiService();

                        UpdateState();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Load_userWhenCall()
        {
            try
            {
                UserNameTextView.Text = CallUserObject.Name;

                //profile_picture
                GlideImageLoader.LoadImage(this, CallUserObject.Avatar, UserImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProfileFromUserId });
        }

        private async Task LoadProfileFromUserId()
        {
            Load_userWhenCall();
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallTwilioAsync(CallUserObject.UserId, TypeCall.Video);
            if (apiStatus == 200)
            {
                if (respond is CallUserObject.DataCallUser result)
                {
                    CallUserObject.Data.Id = result.Id.ToString();
                    CallUserObject.Data.AccessToken = result.AccessToken;
                    CallUserObject.Data.AccessToken2 = result.AccessToken2;
                    CallUserObject.Data.RoomName = result.RoomName;

                    TimerRequestWaiter = new Timer { Interval = 5000 };
                    TimerRequestWaiter.Elapsed += TimerCallRequestAnswer_Waiter_Elapsed;
                    TimerRequestWaiter.Start();
                }
            }
            else
            {
                FinishCall();

                //Methods.DisplayReportResult(this, respond);
            }
        }

        private async void TimerCallRequestAnswer_Waiter_Elapsed(object sender, ElapsedEventArgs e)
        {
            var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerTwilioAsync(CallUserObject.Data.Id, TypeCall.Video);
            RunOnUiThread(() =>
            {
                try
                {
                    if (respond == "answered")
                    {
                        TwilioVideo?.UpdateToken(CallUserObject.Data.AccessToken2);
                        TwilioVideo?.JoinRoom(ApplicationContext, CallUserObject.Data.RoomName);

                        TimerRequestWaiter.Enabled = false;
                        TimerRequestWaiter.Stop();
                        TimerRequestWaiter.Close();

                        ChatTabbedMainActivity.AddCallToListAndSend("Answered", GetText(Resource.String.Lbl_Outgoing), TypeCall.Video, CallUserObject);
                    }
                    else if (respond == "calling" && CountSecondsOfOutGoingCall < 80)
                        CountSecondsOfOutGoingCall += 10;
                    else if (respond == "calling")
                    {
                        //Call Is inactive 
                        TimerRequestWaiter.Enabled = false;
                        TimerRequestWaiter.Stop();
                        TimerRequestWaiter.Close();

                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Outgoing), TypeCall.Video, CallUserObject);
                        FinishCall();
                    }
                    else if (respond == "declined")
                    {
                        //Call Is inactive 
                        TimerRequestWaiter.Enabled = false;
                        TimerRequestWaiter.Stop();
                        TimerRequestWaiter.Close();

                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);

                        FinishCall();
                    }
                    else if (respond == "no_answer")
                    {
                        //Call Is inactive 
                        TimerRequestWaiter.Enabled = false;
                        TimerRequestWaiter.Stop();
                        TimerRequestWaiter.Close();

                        ChatTabbedMainActivity.AddCallToListAndSend("NoAnswer", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);

                        FinishCall();
                        //Methods.DisplayReportResult(this, respond);
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        #endregion

        #region TwilioVideoHelper.IListener

        public void SetLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                if (LocalVideoTrack == null)
                {
                    LocalVideoTrack = track;
                    var trackId = track?.Name;
                    if (LocalVideoTrackId == trackId)
                    {
                    }
                    else
                    {
                        LocalVideoTrackId = trackId;
                        LocalVideoTrack.AddSink(ThumbnailVideo);
                        ThumbnailVideo.Visibility = LocalVideoTrack == null ? ViewStates.Invisible : ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetRemoteVideoTrack(VideoTrack track)
        {
            try
            {
                var trackId = track?.Name;

                if (RemoteVideoTrackId == trackId)
                    return;

                RemoteVideoTrackId = trackId;
                if (UserVideoTrack == null)
                {
                    UserVideoTrack = track;
                    UserVideoTrack?.AddSink(UserPrimaryVideo);
                    ThumbnailVideo.Visibility = LocalVideoTrack == null ? ViewStates.Invisible : ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                SetLocalVideoTrack(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveRemoteVideoTrack(VideoTrack track)
        {
            try
            {
                // NameControl.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRoomConnected(string roomId)
        {

        }

        public void OnRoomDisconnected(TwilioVideoHelper.StopReason reason)
        {
            try
            {
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Room_Disconnected), ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantConnected(string participantId)
        {
            try
            {
                //NameControl.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantDisconnected(string participantId)
        {
            RunOnUiThread(FinishCall);
        }

        public void SetCallTime(int seconds)
        {

        }

        #endregion

        #region picture-in-picture

        protected override void OnUserLeaveHint()
        {
            try
            {
                base.OnUserLeaveHint();
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    EnterPictureInPictureMode(PictureInPictureParams);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
        {
            try
            {
                if (isInPictureInPictureMode)
                {
                    TopControlLayout.Visibility = ViewStates.Gone;
                    EndCallButton.Visibility = ViewStates.Gone;
                    MuteAudioButton.Visibility = ViewStates.Gone;
                    StopVideoButton.Visibility = ViewStates.Gone;
                    SwitchButtonLayout.Visibility = ViewStates.Gone;
                    UserNameTextView.Visibility = ViewStates.Gone;
                    DurationTextView.Visibility = ViewStates.Gone;
                    //PictureInToPictureButton.Visibility = ViewStates.Gone;
                    ThumbnailVideo.Visibility = ViewStates.Gone;
                }
                else
                {
                    TopControlLayout.Visibility = ViewStates.Visible;
                    EndCallButton.Visibility = ViewStates.Visible;
                    MuteAudioButton.Visibility = ViewStates.Visible;
                    SwitchButtonLayout.Visibility = ViewStates.Visible;
                    UserNameTextView.Visibility = ViewStates.Visible;
                    //DurationTextView.Visibility = ViewStates.Visible;
                    StopVideoButton.Visibility = ViewStates.Visible;
                    //PictureInToPictureButton.Visibility = ViewStates.Visible;
                    ThumbnailVideo.Visibility = ViewStates.Visible;
                }

                base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void ConnectToRoom()
        {
            TwilioVideo?.UpdateToken(CallUserObject.Data.AccessToken);
            TwilioVideo?.JoinRoom(this, CallUserObject.Data.RoomName);
        }

        private void UpdateState()
        {
            try
            {
                if (DataUpdated)
                    return;
                DataUpdated = true;
                TwilioVideo?.Bind(this);
                UpdatingState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool OnSupportNavigateUp()
        {
            TryCancelCall();
            return true;
        }

        protected virtual void UpdatingState()
        {
        }

        private void TryCancelCall()
        {
            CloseScreen();
        }

        private void CloseScreen()
        {
            Finish();
        }

        public virtual void FinishCall()
        {
            try
            {
                //Close Api Starts here >> 
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.CloseCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });

                if (TwilioVideo != null && TwilioVideo.ClientIsReady)
                {
                    TwilioVideo.Unbind(this);
                    TwilioVideo.FinishCall();
                }
                Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}