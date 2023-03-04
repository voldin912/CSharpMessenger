using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AT.Markushi.UI;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.Call.Agora;
using WoWonder.Activities.Call.Twilio;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Call;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Call
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ScreenOrientation = ScreenOrientation.Portrait)]
    public class VideoAudioComingCallActivity : AppCompatActivity, CallAnswerDeclineButton.IAnswerDeclineListener, IDialogListCallBack, IDialogInputCallBack
    {
        private string CallType = "0";

        private CallUserObject CallUserObject;

        private ImageView UserImageView;
        private TextView UserNameTextView, TypeCallTextView;
        public static VideoAudioComingCallActivity CallActivity;

        private CircleButton MessageCallButton;

        public static bool IsActive = false;

        private Ringtone Ringtone;
        private Vibrator Vibrator;

        private CallAnswerDeclineButton AnswerDeclineButton;

        private string PermissionsType;
        private ChatTabbedMainActivity GlobalContext;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                SetContentView(Resource.Layout.TwilioCommingVideoCallLayout);
                Window.AddFlags(WindowManagerFlags.KeepScreenOn);

                CallActivity = this;
                GlobalContext = ChatTabbedMainActivity.GetInstance();

                CallType = Intent?.GetStringExtra("type") ?? "";

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                UserNameTextView = FindViewById<TextView>(Resource.Id.UsernameTextView);
                TypeCallTextView = FindViewById<TextView>(Resource.Id.TypecallTextView);
                UserImageView = FindViewById<ImageView>(Resource.Id.UserImageView);

                MessageCallButton = FindViewById<CircleButton>(Resource.Id.message_call_button);

                AnswerDeclineButton = FindViewById<CallAnswerDeclineButton>(Resource.Id.answer_decline_button);

                AnswerDeclineButton.SetAnswerDeclineListener(this);
                AnswerDeclineButton.Visibility = ViewStates.Visible;
                AnswerDeclineButton.StartRingingAnimation();

                MessageCallButton.Click += MessageCallButton_Click;

                if (!string.IsNullOrEmpty(CallUserObject.Name))
                    UserNameTextView.Text = CallUserObject.Name;

                if (!string.IsNullOrEmpty(CallUserObject.Avatar))
                    GlideImageLoader.LoadImage(this, CallUserObject.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (CallType == "Twilio_video_call" || CallType == "Agora_video_call_recieve")
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Video_call);
                else
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Voice_call);

                PlayAudioFromAsset("mystic_call.mp3");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    VibratorManager vibratorManager = (VibratorManager)GetSystemService(VibratorManagerService);
                    Vibrator = vibratorManager?.DefaultVibrator;
                }
                else
                {
                    Vibrator = (Vibrator)GetSystemService("vibrator");
                }

                var vibrate = new long[]
                {
                    1000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000,
                    2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000
                };

                // Vibrate for 500 milliseconds
                Vibrator?.Vibrate(VibrationEffect.CreateWaveform(vibrate, 3));

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
                IsActive = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                IsActive = false;
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
                GlobalContext?.OffWakeLock();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MessageCallButton_Click(object sender, EventArgs e)
        {
            try
            {

                if (Methods.CheckConnectivity())
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialAlertDialogBuilder(this);

                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall1));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall2));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall3));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall4));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall5));

                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                    
                    dialogList.Show();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region MaterialDialog
         
        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (text == GetString(Resource.String.Lbl_MessageCall5))
                    {
                        var dialogBuilder = new MaterialAlertDialogBuilder(this);


                        EditText input = new EditText(this);
                        input.SetHint(Resource.String.Lbl_Write_your_message);
                        input.InputType = InputTypes.TextFlagImeMultiLine;
                        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                        input.LayoutParameters = lp;

                        dialogBuilder.SetView(input);
                        
                        dialogBuilder.SetPositiveButton(GetText(Resource.String.Btn_Send), new MaterialDialogUtils(input, this));
                        dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                        dialogBuilder.Show();
                        
                    }
                    else
                    {
                        SendMess(text);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(IDialogInterface dialog, string input)
        {
            try
            {
                if (input.Length > 0)
                { 
                    SendMess(input);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void SendMess(string text)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    //Here on This function will send Selected audio file to the user 
                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(CallUserObject.UserId, time2, text);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                            Console.WriteLine(result.MessageData);

                            switch (CallType)
                            {
                                case "Twilio_video_call":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });
                                    ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);
                                    break;
                                case "Twilio_audio_call":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
                                    ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);
                                    break;
                                case "Agora_video_call_recieve":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                                    ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);
                                    break;
                                case "Agora_audio_call_recieve":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                                    ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);
                                    break;
                            }

                            ChatTabbedMainActivity.RunCall = false;
                            FinishVideoAudio();
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FinishVideoAudio()
        {
            try
            {
                StopAudioFromAsset();
                Vibrator?.Cancel();

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayAudioFromAsset(string fileName, string typeVolume = "right")
        {
            try
            {
                Ringtone = RingtoneManager.GetRingtone(this, RingtoneManager.GetDefaultUri(RingtoneType.Ringtone));
                Ringtone.Play();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopAudioFromAsset()
        {
            try
            {
                if (Ringtone != null && Ringtone.IsPlaying)
                {
                    Ringtone.Stop();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnAnswered()
        {
            try
            {
                switch (CallType)
                {
                    case "Agora_video_call_recieve":
                    case "Twilio_video_call":
                        {
                            PermissionsType = "VideoCall";
                            if ((int)Build.VERSION.SdkInt >= 23)
                            {
                                if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                                {
                                    StartCall(TypeCall.Video);
                                }
                                else
                                {
                                    new PermissionsController(this).RequestPermission(103);
                                }
                            }
                            else
                            {
                                StartCall(TypeCall.Video);
                            }
                            break;
                        }
                    case "Agora_audio_call_recieve":
                    case "Twilio_audio_call":
                        {
                            PermissionsType = "AudioCall";
                            if ((int)Build.VERSION.SdkInt >= 23)
                            {
                                if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                                {
                                    StartCall(TypeCall.Audio);
                                }
                                else
                                {
                                    new PermissionsController(this).RequestPermission(102);
                                }
                            }
                            else
                            {
                                StartCall(TypeCall.Audio);
                            }
                            break;
                        }
                }

                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnDeclined()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (CallType)
                {
                    case "Twilio_video_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });
                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);
                        break;
                    case "Twilio_audio_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);
                        break;
                    case "Agora_video_call_recieve":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Video, CallUserObject);
                        break;
                    case "Agora_audio_call_recieve":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                        ChatTabbedMainActivity.AddCallToListAndSend("Cancel", GetText(Resource.String.Lbl_Missing), TypeCall.Audio, CallUserObject);
                        break;
                }

                ChatTabbedMainActivity.RunCall = false;
                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                ChatTabbedMainActivity.RunCall = false;
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (grantResults.Length <= 0 || grantResults[0] != Permission.Granted)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    return;
                }

                if (requestCode == 102)
                {
                    if (PermissionsType == "AudioCall")
                    {
                        StartCall(TypeCall.Audio);
                    }
                }
                else if (requestCode == 103)
                {
                    if (PermissionsType == "VideoCall")
                    {
                        StartCall(TypeCall.Video);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion


        #region Call

        private void StartCall(TypeCall type)
        {
            try
            {
                Intent intentCall = null;
                if (type == TypeCall.Audio)
                {
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentCall = new Intent(this, typeof(AgoraAudioCallActivity));
                        intentCall.PutExtra("type", "Agora_audio_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentCall = new Intent(this, typeof(TwilioAudioCallActivity));
                        intentCall.PutExtra("type", "Twilio_audio_calling_start");
                    }
                }
                else if (type == TypeCall.Video)
                {
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentCall = new Intent(this, typeof(AgoraVideoCallActivity));
                        intentCall.PutExtra("type", "Agora_video_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentCall = new Intent(this, typeof(TwilioVideoCallActivity));
                        intentCall.PutExtra("type", "Twilio_video_calling_start");
                    }
                }

                if (CallUserObject != null)
                {
                    intentCall?.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                }

                if (intentCall != null)
                {
                    intentCall.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                    StartActivity(intentCall);
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