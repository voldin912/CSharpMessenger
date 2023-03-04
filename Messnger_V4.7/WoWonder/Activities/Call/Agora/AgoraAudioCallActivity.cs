using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using AgoraIO.Media;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.OS;
using IO.Agora.Rtc2;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Call.Agora
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class AgoraAudioCallActivity : AppCompatActivity, ISensorEventListener
    {
        #region Variables Basic

        private string CallType = "0", Token = "";
        private CallUserObject CallUserObject;

        private RtcEngine AgoraEngine;
        private Tools.AgoraRtcAudioCallHandler AgoraHandler;

        private ImageView IconBack;
        private LinearLayout EndCallButton, SpeakerAudioButton, MuteAudioButton;
        private ImageView IconEndCall, IconSpeaker, IconMute;
        private ImageView UserImageView;
        private TextView UserNameTextView, DurationTextView;
        private TextView IconSignal;

        private int CountSecondsOfOutGoingCall;
        private Timer TimerRequestWaiter, TimerSound;

        private ChatTabbedMainActivity GlobalContext;

        private SensorManager SensorManager;
        private Sensor Proximity;
        private readonly int SensorSensitivity = 4;

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
                SetContentView(Resource.Layout.CallAudioLayout);

                SensorManager = (SensorManager)GetSystemService(SensorService);
                Proximity = SensorManager?.GetDefaultSensor(SensorType.Proximity);

                GlobalContext = ChatTabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitAgoraCall();
                InitBackPressed();
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
                SensorManager.RegisterListener(this, Proximity, SensorDelay.Normal);
                AddOrRemoveEvent(true);
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
                SensorManager.UnregisterListener(this);
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

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                FinishCall();
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
                IconBack = FindViewById<ImageView>(Resource.Id.icon_back);
                EndCallButton = FindViewById<LinearLayout>(Resource.Id.EndCallButtonLayout);
                IconEndCall = FindViewById<ImageView>(Resource.Id.iconEndCall);

                SpeakerAudioButton = FindViewById<LinearLayout>(Resource.Id.SpeakerButtonLayout);
                IconSpeaker = FindViewById<ImageView>(Resource.Id.iconSpeaker);

                MuteAudioButton = FindViewById<LinearLayout>(Resource.Id.MuteButtonLayout);
                IconMute = FindViewById<ImageView>(Resource.Id.iconMute);

                UserImageView = FindViewById<ImageView>(Resource.Id.userImage);
                UserNameTextView = FindViewById<TextView>(Resource.Id.name);

                IconSignal = FindViewById<TextView>(Resource.Id.icon_signal);
                DurationTextView = FindViewById<TextView>(Resource.Id.time);

                IconBack.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);

                SpeakerAudioButton.Selected = false;
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
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "AgoraAudioCallActivity"));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "AgoraAudioCallActivity", true));
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
                    SpeakerAudioButton.Click += SpeakerAudioButtonOnClick;
                    EndCallButton.Click += EndCallButtonOnClick;
                    MuteAudioButton.Click += MuteAudioButtonOnClick;
                }
                else
                {
                    SpeakerAudioButton.Click -= SpeakerAudioButtonOnClick;
                    EndCallButton.Click -= EndCallButtonOnClick;
                    MuteAudioButton.Click -= MuteAudioButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

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
                AgoraEngine?.MuteLocalAudioStream(MuteAudioButton.Selected);
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

        private void SpeakerAudioButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                //Speaker
                if (SpeakerAudioButton.Selected)
                {
                    SpeakerAudioButton.Selected = false;
                    IconMute.SetImageResource(Resource.Drawable.icon_volume_off_vector);
                }
                else
                {
                    SpeakerAudioButton.Selected = true;
                    IconMute.SetImageResource(Resource.Drawable.icon_volume_up_vector);
                }

                AgoraEngine?.SetEnableSpeakerphone(SpeakerAudioButton.Selected);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Sensor System

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            try
            {
                // Do something here if sensor accuracy changes.
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e.Sensor.Type == SensorType.Proximity)
                {
                    if (e.Values[0] >= -SensorSensitivity && e.Values[0] <= SensorSensitivity)
                    {
                        //near 
                        GlobalContext?.SetOffWakeLock();
                    }
                    else
                    {
                        //far 
                        GlobalContext?.SetOnWakeLock();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Agora  

        private async void InitAgoraCall()
        {
            try
            {
                CallType = Intent?.GetStringExtra("type") ?? ""; // Agora_audio_call_recieve , Agora_audio_calling_start

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                InitializeAgoraEngine();

                if (CallType == "Agora_audio_call_recieve")
                {
                    if (!string.IsNullOrEmpty(CallUserObject.UserId))
                        Load_userWhenCall();

                    Token = CallUserObject.Data.AccessToken;

                    DurationTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

                    var (apiStatus, respond) = await RequestsAsync.Call.AnswerCallAgoraAsync(CallUserObject.Data.Id);
                    if (apiStatus == 200)
                    {
                        JoinChannel(Token, CallUserObject.Data.RoomName);

                        ChatTabbedMainActivity.AddCallToListAndSend("Answered", GetText(Resource.String.Lbl_Incoming), TypeCall.Audio, CallUserObject);
                    }
                    //else Methods.DisplayReportResult(this, respond);
                }
                else if (CallType == "Agora_audio_calling_start")
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_Calling);

                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("outgoin_call.mp3", "left");

                    if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AgoraChatAppCertificate))
                    {
                        string channelName = "room_" + Methods.Time.CurrentTimeMillis();
                        int uid = 0;
                        int expirationTimeInSeconds = 3600;

                        RtcTokenBuilder tokenBuilder = new RtcTokenBuilder();
                        int timestamp = (int)(Methods.Time.CurrentTimeMillis() / 1000 + expirationTimeInSeconds);

                        Token = tokenBuilder.BuildTokenWithUid(ListUtils.SettingsSiteList?.AgoraChatAppId,
                            ListUtils.SettingsSiteList?.AgoraChatAppCertificate, channelName, uid,
                            RtcTokenBuilder.Role.RolePublisher, timestamp);
                    }

                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_Create_callEvent(CallUserObject.UserId);

                    StartApiService();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializeAgoraEngine()
        {
            try
            {
                AgoraHandler = new Tools.AgoraRtcAudioCallHandler(this);
                AgoraEngine = RtcEngine.Create(this, ListUtils.SettingsSiteList?.AgoraChatAppId, AgoraHandler);
                AgoraEngine?.EnableWebSdkInteroperability(true);
                AgoraEngine?.SetChannelProfile(Constants.ChannelProfileCommunication);
                AgoraEngine?.EnableAudio();
                AgoraEngine?.DisableVideo();
            }
            catch (Exception e)
            {
                //Colud not create RtcEngine
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void JoinChannel(string accessToken, string channelName)
        {
            try
            {
                AgoraEngine?.JoinChannel(accessToken, channelName, string.Empty, 0);
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
                GlideImageLoader.LoadImage(this, CallUserObject.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { CreateNewCall });
        }

        private async Task CreateNewCall()
        {

            if (!Methods.CheckConnectivity())
                return;

            Load_userWhenCall();
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallAgoraAsync(CallUserObject.UserId, Token, TypeCall.Audio);
            if (apiStatus == 200)
            {
                if (respond is CreateNewCallAgoraObject result)
                {
                    CallUserObject.Data.Id = result.Id;
                    Token = CallUserObject.Data.AccessToken = result.Token;
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
            var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerAgoraAsync(CallUserObject.Data.Id, TypeCall.Audio);
            RunOnUiThread(() =>
            {
                try
                {
                    if (apiStatus == 200)
                    {
                        if (respond is CheckForAnswerAgoraObject agoraObject)
                        {
                            if (string.IsNullOrEmpty(agoraObject.CallStatus))
                                return;

                            RunOnUiThread(Methods.AudioRecorderAndPlayer.StopAudioFromAsset);

                            if (agoraObject.CallStatus == "answered")
                            {
                                JoinChannel(Token, CallUserObject.Data.RoomName);

                                if (TimerRequestWaiter != null)
                                {
                                    TimerRequestWaiter.Enabled = false;
                                    TimerRequestWaiter.Stop();
                                    TimerRequestWaiter.Close();
                                }

                                ChatTabbedMainActivity.AddCallToListAndSend("Answered", GetText(Resource.String.Lbl_Outgoing), TypeCall.Audio, CallUserObject);
                            }
                            else if (agoraObject.CallStatus == "calling" && CountSecondsOfOutGoingCall < 80)
                                CountSecondsOfOutGoingCall += 10;
                            else if (agoraObject.CallStatus == "calling")
                            {
                                //Call Is inactive 
                                if (TimerRequestWaiter != null)
                                {
                                    TimerRequestWaiter.Enabled = false;
                                    TimerRequestWaiter.Stop();
                                    TimerRequestWaiter.Close();
                                }

                                ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);

                                FinishCall();
                            }
                            else if (agoraObject.CallStatus == "declined")
                            {
                                //Call Is inactive 
                                TimerRequestWaiter.Enabled = false;
                                TimerRequestWaiter.Stop();
                                TimerRequestWaiter.Close();

                                ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);

                                FinishCall();
                            }
                            else if (agoraObject.CallStatus == "no_answer")
                            {
                                //Call Is inactive 
                                TimerRequestWaiter.Enabled = false;
                                TimerRequestWaiter.Stop();
                                TimerRequestWaiter.Close();

                                ChatTabbedMainActivity.AddCallToListAndSend("NoAnswer", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);

                                FinishCall();
                                //Methods.DisplayReportResult(this, respond);
                            }
                        }
                    }

                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        #endregion

        #region Agora Rtc Handler

        public void OnConnectionLost()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Lost_Connection), ToastLength.Short);
                    FinishCall();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    FinishCall();
                }
            });
        }

        public void OnUserOffline(int uid, int reason)
        {
            RunOnUiThread(async () =>
            {
                try
                {
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                    //Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                    DurationTextView.Text = GetText(Resource.String.Lbl_Lost_his_connection);
                    await Task.Delay(2000);
                    FinishCall();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    FinishCall();
                }
            });
        }

        public void OnNetworkQuality(int uid, int txQuality, int rxQuality)
        {

        }

        public void OnUserJoined(int uid, int elapsed)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_Please_wait);
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();

                    TimerSound = new Timer();
                    TimerSound.Interval = 1000;
                    TimerSound.Elapsed += TimerSoundOnElapsed;
                    TimerSound.Start();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private string TimeCall;
        private bool IsMuted;
        private void TimerSoundOnElapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (!IsMuted)
                    {
                        //Write your own duration function here 
                        TimeCall = TimeSpan.FromSeconds(e.SignalTime.Second).ToString(@"hh\:mm\:ss");
                        DurationTextView.Text = TimeCall;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        public void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {

        }

        public void OnUserMuteAudio(int uid, bool muted)
        {
            try
            {
                IsMuted = muted;
                if (muted)
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_Muted_his_video);
                }
                else
                {
                    DurationTextView.Text = TimeCall;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnLastmileQuality(int quality)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    IconSignal.Visibility = ViewStates.Visible;
                    string icon = string.Empty;
                    switch (quality)
                    {
                        case Constants.QualityExcellent:
                            icon = "Excellent";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.Signal);
                            break;
                        case Constants.QualityGood:
                            icon = "Good";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.Signal4);
                            break;
                        case Constants.QualityPoor:
                            icon = "Poor";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.SignalAlt3);
                            break;
                        case Constants.QualityBad:
                            icon = "Bad";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.SignalAlt2);
                            break;
                        case Constants.QualityVbad:
                            icon = "Very Bad";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.SignalAlt1);
                            break;
                        case Constants.QualityDown:
                            icon = "Down";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.SignalAltSlash);
                            break;
                        default:
                            icon = "Unknown";
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconSignal, FontAwesomeIcon.SignalAltSlash);
                            break;
                    }

                    Console.WriteLine("Quality : " + icon);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        #endregion

        public void FinishCall()
        {
            try
            {
                //Close Api Starts here >> 

                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.CloseCallAgoraAsync(CallUserObject.Data.Id) });

                if (AgoraEngine != null)
                {
                    AgoraEngine.LeaveChannel();
                    AgoraEngine.RemoveHandler(AgoraHandler);
                    AgoraEngine.Dispose();
                }

                AgoraEngine = null!;

                ChatTabbedMainActivity.RunCall = false;
                Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                ChatTabbedMainActivity.RunCall = false;
                Finish();
            }
        }

    }
}