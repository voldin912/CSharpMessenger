using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using Com.Zhihu.Matisse;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Refractored.Controls;
using Top.Defaults.Drawabletoolbox;
using WoWonder.Activities.Base;
using WoWonder.Activities.Call.Agora;
using WoWonder.Activities.Call.Twilio;
using WoWonder.Activities.ChatWindow.Adapters;
using WoWonder.Activities.ChatWindow.Fragment;
using WoWonder.Activities.Editor;
using WoWonder.Activities.Gif;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.SharedFiles;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Activities.Viewer;
using WoWonder.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.XRecordView;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.ChatWindow
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ChatWindowActivity : BaseActivity, IOnRecordClickListener, IOnRecordListener, SwipeReply.ISwipeControllerActions, IDialogListCallBack
    {
        #region Variables Basic

        private static ChatWindowActivity Instance;
        private static ChatTabbedMainActivity GlobalContext;

        private LinearLayout MainLayout, BodyListChatLayout;
        private ImageView BackButton, IconMore;
        private CircleImageView UserProfileImage;
        public TextView TxtUsername, TxtLastTime;
        private FrameLayout VideoCallButton, AudioCallButton;
        private ImageView VideoCallIcon, AudioCallIcon;

        private RecyclerView MRecyclerSuggestions;
        private SuggestionMessageAdapter MAdapterSuggestion;
        public RecyclerView MRecycler;
        public MessageAdapter MAdapter;
        private Holders.MsgPreCachingLayoutManager LayoutManager;
        private RecyclerViewOnScrollUpListener RecyclerViewOnScrollUpListener;

        private FrameLayout TopFragmentHolder, ButtonFragmentHolder;
        private FloatingActionButton FabScrollDown;
        private LinearLayout PinMessageView, LoadingLayout, RepliedMessageView;
        private TextView ShortPinMessage;
        private TextView TxtOwnerName, TxtMessageType, TxtShortMessage;
        private ImageView MessageFileThumbnail, BtnCloseReply;

        private LinearLayout LayoutEditText;
        private ImageView MediaButton, EmojiIcon, StickerButton;
        public ImageView SendButton;
        private AXEmojiEditText TxtMessage;
        public RecordButton RecordButton;
        private RecordView RecordView;

        private LinearLayout LayoutBlocked;
        private AppCompatButton UnBlockButton;
        private bool IsBlocked;

        private Timer TimerMessages;
        public bool UpdaterRun, LoadMoreRun;
        private bool IsRecording;

        public static string MainChatColor = AppSettings.MainColor;

        public string ChatId, UserId, TypeChat, LastSeen;
        public string ReplyId, FirstMessageId;
        public ChatObject DataUser;

        private string PermissionsType;

        public AdapterModelsClassMessage SelectedItemPositions;
        public ObservableCollection<AdapterModelsClassMessage> StartedMessageList, PinnedMessageList;

        private ChatRecordSoundFragment ChatRecordSoundBoxFragment;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private FastOutSlowInInterpolator Interpolation;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                LoadDataIntent();
                Methods.App.FullScreenApp(this);

                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                // Create your application here
                if (AppSettings.ChatTheme == ChatTheme.Default)
                {
                    SetContentView(Resource.Layout.ChatWindowLayout);
                }
                else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                {
                    SetContentView(Resource.Layout.ChatWindow_Style1_Layout);
                }

                Instance = this;
                GlobalContext = ChatTabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();
                GetOneSignalNotification();

                Task.Factory.StartNew(LoadDataMessage);
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
                StartTimerMessage();
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
                StopTimerMessage();
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
                DestroyTimerMessage();
                Instance = null;
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
                MainLayout = FindViewById<LinearLayout>(Resource.Id.rootChatWindowView);
                //MainLayout.ViewTreeObserver.AddOnGlobalLayoutListener(this);

                BackButton = FindViewById<ImageView>(Resource.Id.BackButton);

                if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                    BackButton.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);

                UserProfileImage = FindViewById<CircleImageView>(Resource.Id.userProfileImage);
                TxtUsername = FindViewById<TextView>(Resource.Id.Txt_Username);
                TxtLastTime = FindViewById<TextView>(Resource.Id.Txt_last_time);
                IconMore = FindViewById<ImageView>(Resource.Id.IconMore);
                InitCall();

                BodyListChatLayout = FindViewById<LinearLayout>(Resource.Id.bodyListChatLayout);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                MRecyclerSuggestions = FindViewById<RecyclerView>(Resource.Id.RecyclerSuggestions);

                FabScrollDown = FindViewById<FloatingActionButton>(Resource.Id.fab_scroll);
                FabScrollDown.Visibility = ViewStates.Gone;

                PinMessageView = FindViewById<LinearLayout>(Resource.Id.pin_message_view);
                ShortPinMessage = FindViewById<TextView>(Resource.Id.short_pin_message);

                LoadingLayout = FindViewById<LinearLayout>(Resource.Id.Loading_LinearLayout);
                LoadingLayout.Visibility = ViewStates.Gone;

                TopFragmentHolder = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);
                ButtonFragmentHolder = FindViewById<FrameLayout>(Resource.Id.ButtomFragmentHolder);

                RepliedMessageView = FindViewById<LinearLayout>(Resource.Id.replied_message_view);
                TxtOwnerName = FindViewById<TextView>(Resource.Id.owner_name);
                TxtMessageType = FindViewById<TextView>(Resource.Id.message_type);
                TxtShortMessage = FindViewById<TextView>(Resource.Id.short_message);
                MessageFileThumbnail = FindViewById<ImageView>(Resource.Id.message_file_thumbnail);
                BtnCloseReply = FindViewById<ImageView>(Resource.Id.clear_btn_reply_view);
                BtnCloseReply.Visibility = ViewStates.Visible;
                MessageFileThumbnail.Visibility = ViewStates.Gone;

                LayoutEditText = FindViewById<LinearLayout>(Resource.Id.LayoutEditText);
                MediaButton = FindViewById<ImageView>(Resource.Id.mediaButton);
                EmojiIcon = FindViewById<ImageView>(Resource.Id.emojiicon);
                TxtMessage = FindViewById<AXEmojiEditText>(Resource.Id.EmojiconEditText5);
                Methods.SetColorEditText(TxtMessage, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                InitEmojisView();

                SendButton = FindViewById<ImageView>(Resource.Id.sendButton);

                if (AppSettings.ChatTheme == ChatTheme.Default)
                    StickerButton = FindViewById<ImageView>(Resource.Id.StickerButton);

                LayoutBlocked = FindViewById<LinearLayout>(Resource.Id.BlockedLayout);
                UnBlockButton = FindViewById<AppCompatButton>(Resource.Id.UnBlockButton);
                LayoutBlocked.Visibility = ViewStates.Gone;

                RecordButton = FindViewById<RecordButton>(Resource.Id.record_button);
                RecordView = FindViewById<RecordView>(Resource.Id.record_view);
                InitRecord();

                SetThemeView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(MainChatColor);

                    EmojisViewTools.MStickerView = AppSettings.ShowButtonStickers;
                    EmojisViewTools.LoadView(this, TxtMessage, "ChatWindowActivity", EmojiIcon);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void InitRecord()
        {
            try
            {
                if (AppSettings.ShowButtonRecordSound)
                {
                    //Audio FrameWork initialize 
                    ChatRecordSoundBoxFragment = new ChatRecordSoundFragment("ChatWindow");
                    RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                    Interpolation = new FastOutSlowInInterpolator();

                    //ChatSendButton.LongClickable = true;
                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);

                    SendButton.Visibility = ViewStates.Gone;

                    SupportFragmentManager.BeginTransaction().Add(TopFragmentHolder.Id, ChatRecordSoundBoxFragment, "Chat_Recourd_Sound_Fragment");

                    RecordButton.SetRecordView(RecordView);

                    //Cancel Bounds is when the Slide To Cancel text gets before the timer . default is 8
                    RecordView.SetCancelBounds(8);
                    RecordView.SetSmallMicColor(Color.ParseColor("#c2185b"));

                    //prevent recording under one Second
                    RecordView.SetLessThanSecondAllowed(false);
                    RecordView.SetSlideToCancelText(GetText(Resource.String.Lbl_SlideToCancelAudio));
                    RecordView.SetCustomSounds(Resource.Raw.record_start, Resource.Raw.record_finished, Resource.Raw.record_error);
                    RecordView.SetOnRecordListener(this);

                    RecordButton.SetOnRecordClickListener(this); //click on Button 
                }
                else
                {
                    RecordView.Visibility = ViewStates.Gone;
                    SendButton.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitCall()
        {
            try
            {
                if (AppSettings.ChatTheme == ChatTheme.Default)
                {
                    VideoCallIcon = FindViewById<ImageView>(Resource.Id.IconvideoCall);
                    AudioCallIcon = FindViewById<ImageView>(Resource.Id.IconCall);

                    VideoCallIcon.Visibility = WoWonderTools.CheckAllowedCall(TypeCall.Video) ? ViewStates.Visible : ViewStates.Gone;
                    AudioCallIcon.Visibility = WoWonderTools.CheckAllowedCall(TypeCall.Audio) ? ViewStates.Visible : ViewStates.Gone;

                }
                else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                {
                    VideoCallButton = FindViewById<FrameLayout>(Resource.Id.IconvideoCallLayout);
                    AudioCallButton = FindViewById<FrameLayout>(Resource.Id.IconCallLayout);

                    VideoCallButton.Visibility = WoWonderTools.CheckAllowedCall(TypeCall.Video) ? ViewStates.Visible : ViewStates.Gone;
                    AudioCallButton.Visibility = WoWonderTools.CheckAllowedCall(TypeCall.Audio) ? ViewStates.Visible : ViewStates.Gone;
                }
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
                MAdapter = new MessageAdapter(this, UserId, false) { DifferList = new ObservableCollection<AdapterModelsClassMessage>() };

                LayoutManager = new Holders.MsgPreCachingLayoutManager(this) { Orientation = LinearLayoutManager.Vertical };
                LayoutManager.SetPreloadItemCount(35);
                LayoutManager.AutoMeasureEnabled = false;
                LayoutManager.SetExtraLayoutSpace(2000);
                LayoutManager.StackFromEnd = true;

                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                ((SimpleItemAnimator)MRecycler.GetItemAnimator()).SupportsChangeAnimations = false;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<AdapterModelsClassMessage>(this, MAdapter, sizeProvider, 8);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollUpListener = new RecyclerViewOnScrollUpListener(LayoutManager, FabScrollDown);
                RecyclerViewOnScrollUpListener.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(RecyclerViewOnScrollUpListener);

                if (AppSettings.EnableReplyMessageSystem)
                {
                    SwipeReply swipeReplyController = new SwipeReply(this, this);
                    ItemTouchHelper itemTouchHelper = new ItemTouchHelper(swipeReplyController);
                    itemTouchHelper.AttachToRecyclerView(MRecycler);
                }

                if (AppSettings.EnableSuggestionMessage && !IsBlocked)
                {
                    MAdapterSuggestion = new SuggestionMessageAdapter(this);
                    MRecyclerSuggestions.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                    MRecyclerSuggestions.SetAdapter(MAdapterSuggestion);
                }
                else
                {
                    MRecyclerSuggestions.Visibility = ViewStates.Gone;
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
                    BackButton.Click += BackButtonOnClick;
                    UserProfileImage.Click += OpenUserProfileOnClick;
                    TxtUsername.Click += OpenUserProfileOnClick;
                    TxtLastTime.Click += OpenUserProfileOnClick;
                    IconMore.Click += IconMoreOnClick;

                    TxtMessage.AfterTextChanged += TxtMessageOnAfterTextChanged;

                    MAdapterSuggestion.OnItemClick += MAdapterSuggestionOnOnItemClick;

                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ErrorLoadingItemClick += MAdapterOnErrorLoadingItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                    MAdapter.DownloadItemClick += MAdapterOnDownloadItemClick;
                    FabScrollDown.Click += FabScrollDownOnClick;
                    BtnCloseReply.Click += BtnCloseReplyOnClick;
                    PinMessageView.Click += PinMessageViewOnClick;
                    SendButton.Click += SendButtonOnClick;
                    MediaButton.Click += MediaButtonOnClick;

                    UnBlockButton.Click += UnBlockButtonOnClick;

                    if (AppSettings.ChatTheme == ChatTheme.Default)
                    {
                        VideoCallIcon.Click += VideoCallButtonOnClick;
                        AudioCallIcon.Click += AudioCallButtonOnClick;

                        StickerButton.Click += StickerButtonOnClick;
                    }
                    else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                    {
                        VideoCallButton.Click += VideoCallButtonOnClick;
                        AudioCallButton.Click += AudioCallButtonOnClick;
                    }
                }
                else
                {
                    BackButton.Click -= BackButtonOnClick;
                    UserProfileImage.Click -= OpenUserProfileOnClick;
                    TxtUsername.Click -= OpenUserProfileOnClick;
                    TxtLastTime.Click -= OpenUserProfileOnClick;
                    IconMore.Click -= IconMoreOnClick;

                    TxtMessage.AfterTextChanged -= TxtMessageOnAfterTextChanged;

                    MAdapterSuggestion.OnItemClick -= MAdapterSuggestionOnOnItemClick;

                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ErrorLoadingItemClick -= MAdapterOnErrorLoadingItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                    MAdapter.DownloadItemClick -= MAdapterOnDownloadItemClick;
                    FabScrollDown.Click -= FabScrollDownOnClick;
                    BtnCloseReply.Click -= BtnCloseReplyOnClick;
                    PinMessageView.Click -= PinMessageViewOnClick;
                    SendButton.Click -= SendButtonOnClick;
                    MediaButton.Click -= MediaButtonOnClick;

                    UnBlockButton.Click -= UnBlockButtonOnClick;

                    if (AppSettings.ChatTheme == ChatTheme.Default)
                    {
                        VideoCallIcon.Click -= VideoCallButtonOnClick;
                        AudioCallIcon.Click -= AudioCallButtonOnClick;

                        StickerButton.Click -= StickerButtonOnClick;
                    }
                    else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                    {
                        VideoCallButton.Click -= VideoCallButtonOnClick;
                        AudioCallButton.Click -= AudioCallButtonOnClick;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ChatWindowActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
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

        #region Events

        //Back
        private void BackButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Audio Call
        private void AudioCallButtonOnClick(object sender, EventArgs e)
        {
            try
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Video Call
        private void VideoCallButtonOnClick(object sender, EventArgs e)
        {
            try
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show more Icon 
        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                MenuChatBottomSheet bottomSheet = new MenuChatBottomSheet();
                Bundle bundle = new Bundle();
                bundle.PutString("Page", "ChatWindow");
                bottomSheet.Arguments = bundle;
                bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Pin Message
        private void PinMessageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PinnedMessageActivity));
                intent.PutExtra("ChatId", ChatId);
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("MainChatColor", MainChatColor);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close Reply Ui
        private void BtnCloseReplyOnClick(object sender, EventArgs e)
        {
            try
            {
                CloseReplyUi();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll Down
        private void FabScrollDownOnClick(object sender, EventArgs e)
        {
            try
            {
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                FabScrollDown.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterSuggestionOnOnItemClick(object sender, Adapters.AdapterClickEvents e)
        {
            try
            {
                if (e.Position <= -1) return;

                var text = MAdapterSuggestion.GetItem(e.Position).RealMessage;
                AddMessageToListAndSend(MessageModelType.RightText, text);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click download Message 
        private async void MAdapterOnDownloadItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (e.Position <= -1) return;
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        item.MesData.BtnDownload = true;

                        var fileName = item.MesData.Media.Split('/').Last();
                        switch (e.Type)
                        {
                            case Holders.TypeClick.Sound:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimSound, fileName, item.MesData.Media, "audio", true);
                                    break;
                                }
                            case Holders.TypeClick.Video:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimVideo, fileName, item.MesData.Media, "video", true);
                                    break;
                                }
                            case Holders.TypeClick.Image:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, item.MesData.Media, "image", true);
                                    break;
                                }
                        }

                        await Task.Delay(1000);

                        UpdateOneMessage(item.MesData, true, false);
                    }
                }
                else
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Display options for the message
        private void MAdapterOnItemLongClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(e.Position);
                    if (SelectedItemPositions != null)
                    {
                        OptionsItemMessageBottomSheet bottomSheet = new OptionsItemMessageBottomSheet();
                        Bundle bundle = new Bundle();
                        bundle.PutString("Type", JsonConvert.SerializeObject(e.Type));
                        bundle.PutString("Page", "ChatWindow");
                        bundle.PutString("ItemObject", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                        bottomSheet.Arguments = bundle;
                        bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click again send item when Error Send Message 
        private void MAdapterOnErrorLoadingItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (e.Position <= -1) return;
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        item.MesData.ErrorSendMessage = false;
                        UpdateOneMessage(item.MesData);

                        Task.Factory.StartNew(() =>
                        {
                            MessageController.SendMessageTask(this, UserId, ChatId, item.MesData.Id, "", "", item.MesData.Media, "", "", "", "", "", "", "", item.MesData.ReplyId).ConfigureAwait(false);
                        });
                    }
                }
                else
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show #time Or #View message 
        private void MAdapterOnItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    switch (e.Type)
                    {
                        case Holders.TypeClick.Text:
                        case Holders.TypeClick.Contact:
                            item.MesData.ShowTimeText = !item.MesData.ShowTimeText;
                            MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(item));
                            break;
                        case Holders.TypeClick.File:
                            {
                                var fileName = item.MesData.Media.Split('/').Last();
                                string imageFile = Methods.MultiMedia.CheckFileIfExits(item.MesData.Media);
                                if (imageFile != "File Dont Exists")
                                {
                                    try
                                    {
                                        var extension = fileName.Split('.').Last();
                                        string mimeType = MimeTypeMap.GetMimeType(extension);

                                        Intent openFile = new Intent();
                                        openFile.SetFlags(ActivityFlags.NewTask);
                                        openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                                        openFile.SetAction(Intent.ActionView);
                                        openFile.SetDataAndType(Uri.Parse(imageFile), mimeType);
                                        StartActivity(openFile);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                }
                                else
                                {
                                    var extension = fileName.Split('.').Last();
                                    string mimeType = MimeTypeMap.GetMimeType(extension);

                                    Intent i = new Intent(Intent.ActionView);
                                    i.SetData(Uri.Parse(item.MesData.Media));
                                    i.SetType(mimeType);
                                    StartActivity(i);
                                }

                                break;
                            }
                        case Holders.TypeClick.Video:
                            {
                                var fileName = item.MesData.Media.Split('/').Last();
                                var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimVideo, fileName, item.MesData.Media, "video");

                                string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                                if (imageFile != "File Dont Exists")
                                {
                                    File file2 = new File(mediaFile);
                                    var mediaUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                    if (AppSettings.OpenVideoFromApp)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                        intent.PutExtra("videoUrl", mediaUri.ToString());
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent();
                                        intent.SetAction(Intent.ActionView);
                                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                        intent.SetDataAndType(mediaUri, "video/*");
                                        StartActivity(intent);
                                    }
                                }
                                else
                                {
                                    if (AppSettings.OpenVideoFromApp)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                        intent.PutExtra("videoUrl", item.MesData.Media);
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.MesData.Media));
                                        StartActivity(intent);
                                    }
                                }

                                break;
                            }
                        case Holders.TypeClick.Image:
                            {
                                if (AppSettings.OpenImageFromApp)
                                {
                                    Intent intent = new Intent(this, typeof(ImageViewerActivity));
                                    intent.PutExtra("Id", UserId);
                                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(item.MesData));
                                    StartActivity(intent);
                                }
                                else
                                {
                                    var fileName = item.MesData.Media.Split('/').Last();
                                    var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, item.MesData.Media, "image");

                                    string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                                    if (imageFile != "File Dont Exists")
                                    {
                                        File file2 = new File(mediaFile);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                        Intent intent = new Intent();
                                        intent.SetAction(Intent.ActionView);
                                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                        intent.SetDataAndType(photoUri, "image/*");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(mediaFile));
                                        StartActivity(intent);
                                    }
                                }

                                break;
                            }
                        case Holders.TypeClick.Map:
                            {
                                // Create a Uri from an intent string. Use the result to create an Intent. 
                                var uri = Uri.Parse("geo:" + item.MesData.Lat + "," + item.MesData.Lng);
                                var intent = new Intent(Intent.ActionView, uri);
                                intent.SetPackage("com.google.android.apps.maps");
                                intent.AddFlags(ActivityFlags.NewTask);
                                StartActivity(intent);
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open User Profile
        private void OpenUserProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenProfileClick();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Load More >> Up Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                if (!LoadMoreRun)
                    ScrollLoadMoreFromTop();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //UnBlocked User
        private async void UnBlockButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.BlockUserAsync(UserId, false); //false >> "Unblock" 
                    if (apiStatus == 200)
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Unblock_successfully), ToastLength.Short);

                        IsBlocked = DataUser.IsBlocked = false;

                        LayoutBlocked.Visibility = ViewStates.Gone;
                        LayoutEditText.Visibility = ViewStates.Visible;

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi)
                            SetTimerMessage();
                    }
                    else
                    {
                        Methods.DisplayReportResultTrack(respond);
                    }
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

        //Show Attachment Media 
        private void MediaButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                //wael change to popap view
                AttachmentMediaChatBottomSheet bottomSheet = new AttachmentMediaChatBottomSheet();
                Bundle bundle = new Bundle();
                bundle.PutString("Page", "ChatWindow");
                bottomSheet.Arguments = bundle;
                bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send message text
        private void SendButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (SendButton?.Tag?.ToString() == "Audio")
                {
                    SendRecordButton();
                }
                else
                {
                    if (!string.IsNullOrEmpty(TxtMessage.Text) && !string.IsNullOrWhiteSpace(TxtMessage.Text))
                    {
                        AddMessageToListAndSend(MessageModelType.RightText, TxtMessage.Text);
                        TxtMessage.Text = "";
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtMessageOnAfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                if (e.Editable?.Length() > 0)
                {
                    RecordButton.Visibility = ViewStates.Gone;
                    SendButton.Visibility = ViewStates.Visible;

                    if (AppSettings.ChatTheme == ChatTheme.Default)
                        StickerButton.Visibility = ViewStates.Gone;

                    ApiStatusChat("typing");
                }
                else
                {
                    RecordButton.Visibility = ViewStates.Visible;
                    SendButton.Visibility = ViewStates.Gone;

                    if (AppSettings.ChatTheme == ChatTheme.Default)
                        StickerButton.Visibility = ViewStates.Visible;

                    ApiStatusChat("stopped");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StickerButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                BrowseStickersFragment fragment = new BrowseStickersFragment();
                Bundle bundle = new Bundle();

                bundle.PutString("TypePage", "ChatWindowActivity");
                fragment.Arguments = bundle;
                fragment.Show(SupportFragmentManager, fragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 506 && resultCode == Result.Ok) // right_contact
                {
                    var contact = Methods.PhoneContactManager.Get_ContactInfoBy_Id(data.Data.LastPathSegment);
                    if (contact != null)
                    {
                        AddMessageToListAndSend(MessageModelType.RightContact, "", "", "", contact);
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok) // right_image 
                {
                    IList<Uri> mUris = Matisse.ObtainResult(data);
                    IList<string> mPaths = Matisse.ObtainPathResult(data);

                    //var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    foreach (var filepath in mPaths)
                    {
                        AddMessageToListAndSend(MessageModelType.RightImage, "", filepath);
                    }
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add right_image using camera   
                {
                    if (IntentController.CurrentPhotoPath != null)
                    {
                        AddMessageToListAndSend(MessageModelType.RightImage, "", IntentController.CurrentPhotoPath);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // right_video 
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDiskVideo + UserId;
                            var fullPathFile = new File(Methods.Path.FolderDiskVideo + UserId, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data?.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimVideo + UserId, false, true);
                            if (newCopyedFilepath != "Path File Dont exits")
                            {
                                Console.WriteLine(newCopyedFilepath);
                            }

                            if (AppSettings.EnableVideoEditor)
                            {
                                var intent = new Intent(this, typeof(VideoEditorActivity));
                                intent.PutExtra("Uri", filepath);
                                intent.PutExtra("Type", "Messages");
                                StartActivityForResult(intent, 2000);
                            }
                            else
                            {
                                AddMessageToListAndSend(MessageModelType.RightVideo, "", filepath);
                            }
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // right_video camera 
                {
                    if (IntentController.CurrentVideoPath != null)
                    {
                        if (AppSettings.EnableVideoEditor)
                        {
                            var intent = new Intent(this, typeof(VideoEditorActivity));
                            intent.PutExtra("Uri", IntentController.CurrentVideoPath);
                            intent.PutExtra("Type", "Messages");
                            StartActivityForResult(intent, 2000);
                        }
                        else
                            AddMessageToListAndSend(MessageModelType.RightVideo, "", IntentController.CurrentVideoPath);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 504 && resultCode == Result.Ok) // right_file
                {
                    string filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        AddMessageToListAndSend(MessageModelType.RightFile, "", filepath);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 505 && resultCode == Result.Ok) // right_audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        AddMessageToListAndSend(MessageModelType.RightAudio, "", filepath);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 300 && resultCode == Result.Ok) // right_gif
                {
                    // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                    // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                    var gifLink = data.GetStringExtra("MediaGif") ?? "Data not available";
                    if (gifLink != "Data not available" && !string.IsNullOrEmpty(gifLink))
                    {
                        var gifUrl = data.GetStringExtra("UrlGif") ?? "Data not available";

                        AddMessageToListAndSend(MessageModelType.RightGif, "", gifLink, gifUrl);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details) + " ", ToastLength.Long);
                    }
                }
                else if (requestCode == 502 && resultCode == Result.Ok) // Location
                {
                    //var placeAddress = data.GetStringExtra("Address") ?? "";
                    var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    if (!string.IsNullOrEmpty(placeLatLng))
                    {
                        string[] latLng = placeLatLng.Split(',');
                        if (latLng?.Length > 0)
                        {
                            string lat = latLng[0];
                            string lng = latLng[1];

                            AddMessageToListAndSend(MessageModelType.RightMap, "", "", "", null, lat, lng);
                        }
                    }
                }
                else if (requestCode == 2000 && resultCode == Result.Ok)
                {
                    var videoPath = data.GetStringExtra("VideoPath") ?? "";
                    if (!string.IsNullOrEmpty(videoPath))
                    {
                        AddMessageToListAndSend(MessageModelType.RightVideo, "", videoPath);
                    }
                }
                else if (requestCode == 23154 && resultCode == Result.Ok)
                {
                    GetWallpaper();
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

                if (grantResults.Length <= 0 || grantResults[0] != Permission.Granted)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    return;
                }

                if (requestCode == 111)
                {
                    Methods.Path.Chack_MyFolder(UserId);
                }
                else if (requestCode == 100)
                {
                    Methods.Path.Chack_MyFolder(UserId);

                    switch (PermissionsType)
                    {
                        case "File":
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                            break;
                        case "Music":
                            //requestCode >> 505 => Music
                            new IntentController(this).OpenIntentAudio();
                            break;
                    }
                }
                else if (requestCode == 101)
                {
                    //request code of result is 506
                    new IntentController(this).OpenIntentGetContactNumberPhone();
                }
                else if (requestCode == 102)
                {
                    if (PermissionsType == "AudioCall")
                    {
                        StartCall(TypeCall.Audio);
                    }
                    else if (PermissionsType == "Record")
                    {
                        StartRecordingVoice();
                    }
                }
                else if (requestCode == 103)
                {
                    if (PermissionsType == "VideoCall")
                    {
                        StartCall(TypeCall.Video);
                    }
                }
                else if (requestCode == 105)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else if (requestCode == 108)
                {
                    switch (PermissionsType)
                    {
                        case "Image":
                            //requestCode >> 500 => Image Gallery
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false);
                            break;
                        case "Video":
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                            break;
                        case "Video_camera":
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoCamera();
                            break;
                        case "Camera":
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Message

        private void LoadDataIntent()
        {
            try
            {
                ChatId = Intent?.GetStringExtra("ChatId") ?? "";
                UserId = Intent?.GetStringExtra("UserID") ?? "";
                TypeChat = Intent?.GetStringExtra("TypeChat") ?? "";

                if (TypeChat == "LastMessenger")
                {
                    DataUser = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("UserItem") ?? "");
                    if (DataUser?.LastMessage.LastMessageClass != null)
                    {
                        DataUser.LastMessage.LastMessageClass.ChatColor ??= AppSettings.MainColor;
                        MainChatColor = !string.IsNullOrEmpty(DataUser.LastMessage.LastMessageClass?.ChatColor) ? DataUser.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(DataUser.LastMessage.LastMessageClass?.ChatColor) : DataUser.LastMessage.LastMessageClass?.ChatColor : AppSettings.MainColor ?? AppSettings.MainColor;
                    }
                    else
                    {
                        MainChatColor = AppSettings.MainColor;
                    }
                }
                else if (TypeChat == "OneSignalNotification")
                {
                    MainChatColor = AppSettings.MainColor;
                }
                else
                {
                    DataUser = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("UserItem") ?? "");
                    if (DataUser?.LastMessage.LastMessageClass != null)
                    {
                        DataUser.LastMessage.LastMessageClass.ChatColor ??= AppSettings.MainColor;
                        MainChatColor = !string.IsNullOrEmpty(DataUser.LastMessage.LastMessageClass?.ChatColor) ? DataUser.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(DataUser.LastMessage.LastMessageClass?.ChatColor) : DataUser.LastMessage.LastMessageClass?.ChatColor : AppSettings.MainColor ?? AppSettings.MainColor;
                    }
                    else
                    {
                        MainChatColor = AppSettings.MainColor;
                    }
                }

                SetTheme(MainChatColor);

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    if (PermissionsController.CheckPermissionStorage())
                    {
                        Methods.Path.Chack_MyFolder(UserId);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(111);
                    }
                }
                else
                {
                    Methods.Path.Chack_MyFolder(UserId);
                }

                StartedMessageList = new ObservableCollection<AdapterModelsClassMessage>();
                PinnedMessageList = new ObservableCollection<AdapterModelsClassMessage>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadDataMessage()
        {
            try
            {
                if (DataUser != null)
                    RunOnUiThread(() => { LoadDataProfileClick(DataUser); });

                StartApiService();

                var sqlEntity = new SqLiteDatabase();
                var localList = sqlEntity.GetMessages_List(UserDetails.UserId, UserId, "0");
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    MAdapter.DifferList = new ObservableCollection<AdapterModelsClassMessage>(localList);

                    RunOnUiThread(() =>
                    {
                        MAdapter.NotifyDataSetChanged();

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        MRecyclerSuggestions.Visibility = ViewStates.Gone;
                        LoadingLayout.Visibility = ViewStates.Gone;
                    });
                }
                else //Or server.. Get Messages Api
                {
                    RunOnUiThread(() =>
                    {
                        LoadingLayout.Visibility = ViewStates.Visible;
                    });
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapter.DifferList.Count == 0 ? LoadMessageApi(true) : LoadMessageApi(false) });

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    UserDetails.Socket?.EmitAsync_SendSeenMessages(UserId, UserDetails.AccessToken, UserDetails.UserId);
                }
                else
                {
                    //Run timer
                    RunOnUiThread(SetTimerMessage);
                }

                var list = ListUtils.MessageUnreadList?.Where(a => a.Sender == UserId).ToList();
                if (list?.Count > 0)
                    ListUtils.MessageUnreadList.RemoveAll(list);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task LoadMessageApi(bool firstRun)
        {
            if (UpdaterRun)
                return;

            if (!Methods.CheckConnectivity())
            {
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                return;
            }

            UpdaterRun = true;

            string limit = "10", lastId = "0";
            if (firstRun)
            {
                if (MAdapter.DifferList.Count > 0)
                {
                    limit = "15";
                    lastId = MAdapter.DifferList.LastOrDefault()?.MesData?.Id ?? "0";
                }
                else
                {
                    limit = "10";
                }
            }

            var countList = MAdapter.DifferList.Count;
            var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", lastId, limit);
            if (apiStatus != 200 || respond is not UserMessagesObject result || result.Messages == null)
            {
                UpdaterRun = false;
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                try
                {
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            var typing = result.Typing.ToString();
                            TxtLastTime.Text = typing == "1" ? GetString(Resource.String.Lbl_Typping) : LastSeen ?? LastSeen;

                            var isRecording = result.IsRecording.ToString();
                            TxtLastTime.Text = isRecording == "1" ? GetString(Resource.String.Lbl_Recording) : LastSeen ?? LastSeen;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    result.Messages.Reverse();

                    bool add = false;
                    foreach (var item in result.Messages)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        var mes = WoWonderTools.MessageFilter(UserId, item, type, true);

                        var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData?.Id == item.Id);
                        if (check == null)
                        {
                            add = true;
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage { TypeView = type, Id = Long.ParseLong(item.Id), MesData = mes });
                        }
                        else if (check.MesData.FromId == UserDetails.UserId && check.MesData.Seen != item.Seen) // right
                        {
                            //check.Id = Convert.ToInt32(item.Id);
                            check.MesData.Seen = item.Seen;
                            check.MesData = mes;
                            check.TypeView = type;

                            RunOnUiThread(() =>
                            {
                                if (check.MesData.Position == "right")
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(check));
                            });

                            // Insert data user in database
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Update_To_one_MessagesTable(check.MesData);
                        }
                    }

                    if (add)
                    {
                        // Insert data user in database
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);

                        RunOnUiThread(() =>
                        {
                            try
                            {
                                if (countList > 0)
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                else
                                    MAdapter.NotifyDataSetChanged();

                                if (firstRun)
                                {
                                    //Scroll Down >> 
                                    MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                }
                                else
                                {
                                    if (UserDetails.SoundControl)
                                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                }

                                MRecyclerSuggestions.Visibility = ViewStates.Gone;
                                LoadingLayout.Visibility = ViewStates.Gone;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                MRecyclerSuggestions.Visibility = MAdapter.DifferList.Count > 0 ? ViewStates.Gone : ViewStates.Visible;
                                LoadingLayout.Visibility = ViewStates.Gone;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    UpdaterRun = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    UpdaterRun = false;
                }
            }
        }

        private async void ScrollLoadMoreFromTop()
        {
            try
            {
                var firstMessageId = MAdapter.DifferList.FirstOrDefault()?.MesData?.Id ?? "0";
                if (FirstMessageId != firstMessageId)
                {
                    FirstMessageId = firstMessageId;
                }
                else
                    return;

                //Code get first Message id where LoadMore >>
                var local = LoadMoreMessage_Database();
                if (local != "1")
                    await LoadMoreMessages_API();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string LoadMoreMessage_Database()
        {
            try
            {
                LoadMoreRun = true;
                var countList = MAdapter.DifferList.Count;
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessageList_loadMore(UserDetails.UserId, UserId, FirstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    bool add = false;
                    foreach (var item in from item in localList let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData?.Id == item.MesData?.Id) where check == null select item)
                    {
                        if (item.TypeView == MessageModelType.None)
                            continue;

                        add = true;
                        MAdapter.DifferList.Insert(0, item);
                    }

                    if (add)
                    {
                        RunOnUiThread(() => { MAdapter?.NotifyItemRangeInserted(0, MAdapter.DifferList.Count - countList - 1); });
                        LoadMoreRun = false;
                        return "1";
                    }
                }
                LoadMoreRun = false;
                return "0";
            }
            catch (Exception e)
            {
                LoadMoreRun = false;
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        private async Task LoadMoreMessages_API()
        {
            if (!Methods.CheckConnectivity())
            {
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                LoadMoreRun = false;
                return;
            }

            LoadMoreRun = true;
            var countList = MAdapter.DifferList.Count;
            var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, FirstMessageId, "0", "15");
            if (apiStatus != 200 || respond is not UserMessagesObject result || result.Messages == null)
            {
                LoadMoreRun = false;
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                try
                {
                    bool add = false;
                    foreach (var item in from item in result.Messages let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData?.Id == item.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        add = true;
                        MAdapter.DifferList.Insert(0, new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                        });
                    }

                    if (add)
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        // Insert data user in database
                        dbDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);

                        RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(0, MAdapter.DifferList.Count - countList - 1); });
                    }

                    LoadMoreRun = false;
                }
                catch (Exception e)
                {
                    LoadMoreRun = false;
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public async Task GetMessagesById(string id)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var countList = MAdapter.DifferList.Count;
                var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", "0", "1", id);
                if (apiStatus != 200 || respond is not UserMessagesObject result || result.Messages == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    bool add = false;
                    foreach (var item in result.Messages)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        var mes = WoWonderTools.MessageFilter(UserId, item, type, true);

                        var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData?.Id == item.Id);
                        if (check == null)
                        {
                            add = true;
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage { TypeView = type, Id = Long.ParseLong(item.Id), MesData = mes });
                        }
                        else
                        {
                            check.Id = Convert.ToInt32(item.Id);
                            check.MesData.Seen = item.Seen;
                            check.MesData = mes;
                            check.TypeView = type;

                            RunOnUiThread(() => { MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(check)); });

                            // Insert data user in database
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Update_To_one_MessagesTable(check.MesData);
                        }
                    }

                    if (add)
                    {
                        // Insert data user in database
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);

                        RunOnUiThread(() =>
                        {
                            try
                            {
                                if (countList > 0)
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                else
                                    MAdapter.NotifyDataSetChanged();

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateOneMessage(MessageData message, bool withBlob = false, bool withScroll = true)
        {
            try
            {
                var type = Holders.GetTypeModel(message);
                if (type == MessageModelType.None)
                    return;

                var checker = MAdapter.DifferList.FirstOrDefault(a => a.MesData?.Id == message.Id);
                if (checker != null)
                {
                    checker.Id = Convert.ToInt32(message.Id);
                    checker.MesData = WoWonderTools.MessageFilter(UserId, message, type, true);
                    checker.TypeView = type;

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (withBlob)
                                MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                            else
                            {
                                switch (checker.TypeView)
                                {
                                    case MessageModelType.RightGif:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobGIF");
                                        break;
                                    case MessageModelType.RightText:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                                        break;
                                    case MessageModelType.RightSticker:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobSticker");
                                        break;
                                    case MessageModelType.RightContact:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                                        break;
                                    case MessageModelType.RightFile:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobFile");
                                        break;
                                    case MessageModelType.RightVideo:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobVideo");
                                        break;
                                    case MessageModelType.RightImage:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobImage");
                                        break;
                                    case MessageModelType.RightAudio:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobAudio");
                                        break;
                                    case MessageModelType.RightMap:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobMap");
                                        break;
                                }
                            }

                            //Scroll Down >> 
                            if (withScroll)
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            CloseReplyUi();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResetMediaPlayerInMessages()
        {
            try
            {
                var list = MAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        item.MesData.MediaIsPlaying = false;

                        if (item.MesData.MediaPlayer != null)
                        {
                            item.MesData.MediaPlayer.Stop();
                            item.MesData.MediaPlayer.Reset();
                        }
                        item.MesData.MediaPlayer?.Release();
                        item.MesData.MediaPlayer = null!;
                        item.MesData.MediaTimer = null!;
                    }
                    MAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Send Message

        public void SendRecordButton()
        {
            try
            {
                if (SendButton?.Tag?.ToString() == "Audio")
                {
                    TopFragmentHolder?.Animate()?.SetInterpolator(Interpolation)?.TranslationY(1200)?.SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(ChatRecordSoundBoxFragment)?.Commit();

                    string filePath = RecorderService.GetRecorded_Sound_Path();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        AddMessageToListAndSend(MessageModelType.RightAudio, "", filePath);
                    }
                }

                if (AppSettings.ShowButtonRecordSound)
                {
                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);
                }
                else
                {
                    //RecordButton.Tag = "Text";
                    //RecordButton.SetTheImageResource(Resource.Drawable.icon_send_vector); 
                    //RecordButton.SetListenForRecord(false);
                }
                SendButton.Tag = "Text";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void AddMessageToListAndSend(MessageModelType modelType, string text = "", string filePath = "", string urlGif = "", Methods.PhoneContactManager.UserContact contact = null, string lat = "", string lng = "")
        {
            try
            {
                string timeNow = DateTime.Now.ToShortTimeString();
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = Convert.ToString(unixTimestamp);

                var dataMyProfile = ListUtils.MyProfileList.FirstOrDefault();

                MessageDataExtra message = new MessageDataExtra
                {
                    Id = time2,
                    FromId = UserDetails.UserId,
                    ToId = UserId,
                    Media = "",
                    Seen = "-1",
                    Time = time2,
                    Position = "right",
                    TimeText = timeNow,
                    ModelType = modelType,
                    ErrorSendMessage = false,
                    ChatColor = MainChatColor,
                    MessageHashId = time2,
                    UserData = dataMyProfile,
                    MessageUser = new MessageUserUnion { UserDataClass = dataMyProfile },
                };

                if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                {
                    message.ReplyId = ReplyId;
                    message.Reply = new ReplyUnion
                    {
                        ReplyClass = SelectedItemPositions.MesData
                    };
                }

                if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                {
                    //remove \n in a string
                    string replacement = Regex.Replace(text, @"\t|\n|\r", "");

                    message.Text = replacement;

                    if (AppSettings.EnableFitchOgLink)
                    {
                        //Check if find website in text 
                        foreach (Match item in Regex.Matches(replacement, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            Console.WriteLine(item.Value);
                            message.FitchOgLink = await Methods.OgLink.FitchOgLink(item.Value);
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filePath);
                    if (!check)
                    {
                        if (info == "AdultImages")
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                        }
                        else
                        {
                            //this file not supported on the server , please select another file 
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                        }
                        return;
                    }

                    string totalSize = Methods.FunString.Format_byte_size(filePath);
                    message.Media = filePath;
                    message.FileSize = totalSize;
                    message.SendFile = true;
                }

                switch (modelType)
                {
                    case MessageModelType.RightAudio:
                        message.MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filePath));
                        message.TimeText = GetText(Resource.String.Lbl_Uploading);
                        break;
                    case MessageModelType.RightGif:
                        message.Stickers = urlGif;
                        break;
                    case MessageModelType.RightMap:
                        message.Lat = lat;
                        message.Lng = lng;
                        break;
                    case MessageModelType.RightContact:
                        {
                            var name = contact?.UserDisplayName ?? "";
                            var phone = contact?.PhoneNumber ?? "";

                            message.ContactName = name;
                            message.ContactNumber = phone;
                            break;
                        }
                }

                //function will send Selected file to the user 
                if (Methods.CheckConnectivity())
                {
                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                    {
                        TypeView = modelType,
                        Id = Long.ParseLong(message.Id),
                        MesData = message
                    });

                    if (modelType == MessageModelType.RightText)
                    {
                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        {
                            UserDetails.Socket?.EmitAsync_SendMessage(UserId, UserDetails.AccessToken, UserDetails.Username, message.Text, MainChatColor, message.ReplyId, message.MessageHashId);
                        }
                        else
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                MessageController.SendMessageTask(this, UserId, ChatId, message.MessageHashId, message.Text, "", "", "", "", "", "", "", "", "", message.ReplyId).ConfigureAwait(false);
                            });
                        }
                    }
                    else
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            switch (modelType)
                            {
                                case MessageModelType.RightAudio:
                                case MessageModelType.RightVideo:
                                case MessageModelType.RightImage:
                                case MessageModelType.RightFile:
                                    MessageController.SendMessageTask(this, UserId, ChatId, message.MessageHashId, "", "", message.Media, "", "", "", "", "", "", "", message.ReplyId).ConfigureAwait(false);
                                    break;
                                case MessageModelType.RightGif:
                                    MessageController.SendMessageTask(this, UserId, ChatId, message.MessageHashId, "", "", "", "", "", message.Stickers, "", "", "", "", message.ReplyId).ConfigureAwait(false);
                                    break;
                                case MessageModelType.RightMap:
                                    MessageController.SendMessageTask(this, UserId, ChatId, message.MessageHashId, "", "", "", "", "", "", "", message.Lat, message.Lng, "", message.ReplyId).ConfigureAwait(false);
                                    break;
                                case MessageModelType.RightContact:
                                    {
                                        var dictionary = new Dictionary<string, string> { { message.ContactName, message.ContactNumber } };
                                        string dataContact = JsonConvert.SerializeObject(dictionary.ToArray().FirstOrDefault(a => a.Key == message.ContactName));
                                        MessageController.SendMessageTask(this, UserId, ChatId, message.MessageHashId, dataContact, "1", "", "", "", "", "", "", "", "", message.ReplyId).ConfigureAwait(false);
                                        break;
                                    }
                            }
                        });
                    }

                    CloseReplyUi();
                }
                else
                {
                    message.ErrorSendMessage = true;
                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                    {
                        TypeView = modelType,
                        Id = Long.ParseLong(message.Id),
                        MesData = message
                    });

                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }

                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                MAdapter.NotifyItemInserted(indexMes);

                //Scroll Down >> 
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                MRecyclerSuggestions.Visibility = ViewStates.Gone;

                SendButton.Tag = "Text";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Reply Messages

        //Reply Messages
        public void ReplyItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    RepliedMessageView.Visibility = ViewStates.Visible;
                    var animation = new TranslateAnimation(0, 0, RepliedMessageView.Height, 0) { Duration = 300 };

                    RepliedMessageView.StartAnimation(animation);

                    ReplyId = SelectedItemPositions.MesData?.Id;

                    TxtOwnerName.Text = SelectedItemPositions.MesData?.MessageUser?.UserDataClass?.UserId == UserDetails.UserId ? GetText(Resource.String.Lbl_You) : TxtUsername.Text;

                    if (SelectedItemPositions.TypeView == MessageModelType.LeftText || SelectedItemPositions.TypeView == MessageModelType.RightText)
                    {
                        MessageFileThumbnail.Visibility = ViewStates.Gone;
                        TxtMessageType.Visibility = ViewStates.Gone;
                        TxtShortMessage.Text = SelectedItemPositions.MesData.Text;
                    }
                    else
                    {
                        MessageFileThumbnail.Visibility = ViewStates.Visible;
                        var fileName = SelectedItemPositions.MesData.Media.Split('/').Last();
                        switch (SelectedItemPositions.TypeView)
                        {
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.video);

                                    var fileNameWithoutExtension = fileName.Split('.').First();

                                    var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + UserId, fileNameWithoutExtension + ".png");
                                    if (videoImage == "File Dont Exists")
                                    {
                                        var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskVideo, fileName, SelectedItemPositions.MesData.Media, "video");
                                        File file2 = new File(mediaFile);
                                        try
                                        {
                                            Uri photoUri = SelectedItemPositions.MesData.Media.Contains("http") ? Uri.Parse(SelectedItemPositions.MesData.Media) : FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                            Glide.With(this)
                                                .AsBitmap()
                                                .Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                                                .Load(photoUri) // or URI/path
                                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(this)
                                                .AsBitmap()
                                                .Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                                                .Load(file2) // or URI/path
                                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                    }
                                    else
                                    {
                                        File file = new File(videoImage);
                                        try
                                        {
                                            Uri photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file);
                                            Glide.With(this).Load(photoUri).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(MessageFileThumbnail);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(this).Load(file).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(MessageFileThumbnail);
                                        }
                                    }
                                    break;
                                }
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Gif);
                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskGif, fileName, SelectedItemPositions.MesData.Media, "image");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Sticker);
                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskSticker, fileName, SelectedItemPositions.MesData.Media, "sticker");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.image);

                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, SelectedItemPositions.MesData.Media, "image");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_VoiceMessage) + " (" + SelectedItemPositions.MesData.MediaDuration + ")";
                                    Glide.With(this).Load(GetDrawable(Resource.Drawable.Audio_File)).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                {
                                    TxtMessageType.Text = GetText(Resource.String.Lbl_File);

                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var fileNameExtension = fileName.Split('.').Last();

                                    TxtShortMessage.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                    Glide.With(this).Load(GetDrawable(Resource.Drawable.Image_File)).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Location);
                                    Glide.With(this).Load(SelectedItemPositions.MesData.MessageMap).Apply(new RequestOptions().Placeholder(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map)).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                {
                                    TxtMessageType.Text = GetText(Resource.String.Lbl_Contact);
                                    TxtShortMessage.Text = SelectedItemPositions.MesData.ContactName;
                                    Glide.With(this).Load(Resource.Drawable.no_profile_image).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Product);
                                    string imageUrl = !string.IsNullOrEmpty(SelectedItemPositions.MesData.Media) ? SelectedItemPositions.MesData.Media : SelectedItemPositions.MesData.Product?.ProductClass?.Images[0]?.Image;
                                    Glide.With(this).Load(imageUrl).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowReplyUi(int position)
        {
            try
            {
                if (position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(position);
                    ReplyItems();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseReplyUi()
        {
            try
            {
                if (RepliedMessageView.Visibility == ViewStates.Visible)
                {
                    Animation animation = new TranslateAnimation(0, 0, 0, RepliedMessageView.Top + RepliedMessageView.Height);
                    animation.Duration = 300;
                    animation.AnimationEnd += (o, args) =>
                    {
                        try
                        {
                            RepliedMessageView.Visibility = ViewStates.Gone;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    };
                    RepliedMessageView.StartAnimation(animation);
                    SelectedItemPositions = null;
                    ReplyId = "0";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Pin Message

        private async Task GetPinMessageApi()
        {
            if (!AppSettings.EnablePinMessageSystem || string.IsNullOrEmpty(ChatId))
                return;

            var (apiStatus, respond) = await RequestsAsync.Message.GetPinMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = PinnedMessageList.FirstOrDefault(a => a.MesData?.Id == item.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        PinnedMessageList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                        });
                    }

                    RunOnUiThread(() =>
                    {
                        AdapterModelsClassMessage message = PinnedMessageList.LastOrDefault();
                        LoadPinnedMessage(message);
                    });
                }
            }
        }

        //Pin Message 
        public void PinMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    SelectedItemPositions.MesData.Pin = SelectedItemPositions.MesData.Pin == "yes" ? "no" : "yes";

                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.NotifyItemChanged(index);
                    }

                    var check = PinnedMessageList.FirstOrDefault(a => a.MesData?.Id == SelectedItemPositions.MesData?.Id);
                    if (check == null && SelectedItemPositions.MesData.Pin == "yes")
                    {
                        PinnedMessageList.Add(SelectedItemPositions);
                    }
                    else if (check != null && SelectedItemPositions.MesData.Pin == "no")
                    {
                        PinnedMessageList.Remove(SelectedItemPositions);
                    }

                    if (SelectedItemPositions.MesData.Pin == "yes")
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.PinMessageAsync(SelectedItemPositions.MesData?.Id, ChatId, "yes", "user") });
                    }
                    else
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.PinMessageAsync(SelectedItemPositions.MesData?.Id, ChatId, "no", "user") });
                    }

                    LoadPinnedMessage(SelectedItemPositions);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPinnedMessage(AdapterModelsClassMessage message)
        {
            try
            {
                if (PinnedMessageList?.Count > 0)
                {
                    PinMessageView.Visibility = ViewStates.Visible;
                    if (message != null)
                    {
                        switch (message?.TypeView)
                        {
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + message.MesData.Text;
                                break;
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.video);
                                break;
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Gif);
                                break;
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Sticker);
                                break;
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.image);
                                break;
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                if (string.IsNullOrEmpty(message?.MesData?.MediaDuration))
                                    ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_VoiceMessage);
                                else
                                    ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_VoiceMessage) + " (" + message?.MesData?.MediaDuration + ")";
                                break;
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_File);
                                break;
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Location);
                                break;
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Contact);
                                break;
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Product);
                                break;
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
                else
                {
                    PinMessageView.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Favorite Message

        private async Task GetFavoriteMessageApi()
        {
            if (!AppSettings.EnableFavoriteMessageSystem || string.IsNullOrEmpty(ChatId))
                return;

            var (apiStatus, respond) = await RequestsAsync.Message.GetFavoriteMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = StartedMessageList.FirstOrDefault(a => a.MesData?.Id == item.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        StartedMessageList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                        });
                    }
                }
            }
        }

        //Star Message 
        public void StarMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    SelectedItemPositions.MesData.Fav = SelectedItemPositions.MesData.Fav == "no" ? "yes" : "no";

                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.NotifyItemChanged(index);
                    }

                    if (SelectedItemPositions.MesData.Fav == "yes")
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.FavoriteMessageAsync(SelectedItemPositions.MesData?.Id, ChatId, "yes", "user") });
                    }
                    else
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.FavoriteMessageAsync(SelectedItemPositions.MesData?.Id, ChatId, "no", "user") });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Forward Message

        //Forward Messages
        public void ForwardItems()
        {
            try
            {
                StopTimerMessage();

                if (SelectedItemPositions != null)
                {
                    var intent = new Intent(this, typeof(ForwardMessagesActivity));
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Delete Message

        //Delete Message
        public void DeleteMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null && Methods.CheckConnectivity())
                {
                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.DifferList.Remove(SelectedItemPositions);

                        MAdapter.NotifyItemRemoved(index);
                        MAdapter.NotifyDataSetChanged();
                    }

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteMessageAsync(SelectedItemPositions.Id.ToString()) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Info Message

        //Message Info 
        public void MessageInfoItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    var intent = new Intent(this, typeof(MessageInfoActivity));
                    intent.PutExtra("UserId", UserId);
                    intent.PutExtra("MainChatColor", MainChatColor);
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Data User 

        //Get Data Profile API
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi, GetPinMessageApi, GetFavoriteMessageApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserId, "user_data");
            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                if (DataUser != null)
                    DataUser.UserData = result.UserData;

                RunOnUiThread(() => { LoadDataProfileClick(result.UserData); });
            }
        }

        //view Profile 
        private void LoadDataProfileClick(UserDataObject userData)
        {
            try
            {
                if (userData == null) return;

                GlideImageLoader.LoadImage(this, userData.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                TxtUsername.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(userData.Name), 15);

                //Online Or offline
                if (userData.LastseenStatus == "on")
                {
                    TxtLastTime.Text = GetString(Resource.String.Lbl_Online);
                    LastSeen = GetString(Resource.String.Lbl_Online);
                }
                else
                {
                    TxtLastTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(userData.LastseenUnixTime), false);
                    LastSeen = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(userData.LastseenUnixTime), false);
                }

                IsBlocked = userData.IsBlocked;
                if (userData.IsBlocked)
                {
                    LayoutBlocked.Visibility = ViewStates.Visible;
                    LayoutEditText.Visibility = ViewStates.Gone;
                }
                else
                {
                    LayoutBlocked.Visibility = ViewStates.Gone;
                    LayoutEditText.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //view Profile 
        private void OpenProfileClick()
        {
            try
            {
                if (DataUser != null)
                {
                    WoWonderTools.OpenProfile(this, UserId, DataUser.UserData);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Theme Color & Wallpaper

        public void SetThemeView(bool change = false)
        {
            try
            {
                TypedValue typedValuePrimary = new TypedValue();
                TypedValue typedValueAccent = new TypedValue();

                Theme?.ResolveAttribute(Resource.Attribute.colorPrimary, typedValuePrimary, true);
                Theme?.ResolveAttribute(Resource.Attribute.colorAccent, typedValueAccent, true);
                var colorPrimary = new Color(typedValuePrimary.Data);
                var colorAccent = new Color(typedValueAccent.Data);

                string hex1 = "#" + Integer.ToHexString(colorPrimary).Remove(0, 2);
                string hex2 = "#" + Integer.ToHexString(colorAccent).Remove(0, 2);

                var chatColor = Color.ParseColor(MainChatColor);

                if (change)
                {
                    Drawable drawable = new DrawableBuilder()
                        .Rectangle()
                        .Gradient()
                        .LinearGradient()
                        .Angle(270)
                        .StartColor(Color.ParseColor(hex2))
                        .EndColor(Color.ParseColor(hex1))
                        .StrokeWidth(0)
                        .Build();

                    MainLayout.Background = drawable;
                }

                if (AppSettings.ChatTheme == ChatTheme.Default)
                {
                    BodyListChatLayout.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#282828") : Color.ParseColor("#F1F1F2"));

                    VideoCallIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(hex1));
                    AudioCallIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(hex1));

                    MediaButton.BackgroundTintList = ColorStateList.ValueOf(chatColor);
                }
                else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                {
                    VideoCallButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(hex1));
                    AudioCallButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(hex1));

                    RecordButton.BackgroundTintList = ColorStateList.ValueOf(chatColor);
                    RecordButton.ImageTintList = ColorStateList.ValueOf(chatColor);

                    MediaButton.ImageTintList = ColorStateList.ValueOf(chatColor);

                    EmojiIcon.ImageTintList = ColorStateList.ValueOf(chatColor);
                }

                UserProfileImage.BorderColor = chatColor;

                FabScrollDown.BackgroundTintList = ColorStateList.ValueOf(chatColor);
                FabScrollDown.SetRippleColor(ColorStateList.ValueOf(chatColor));

                if (change)
                {
                    InitEmojisView();
                }

                if (AppSettings.ShowSettingsWallpaper)
                    GetWallpaper();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetTheme(string color)
        {
            try
            {
                if (color.Contains("b582af"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemeb582af_Dark : Resource.Style.Chatththemeb582af);
                }
                else if (color.Contains("a84849") || color.Contains("a52729"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemea84849_Dark : Resource.Style.Chatththemea84849);
                }
                else if (color.Contains("f9c270"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemef9c270_Dark : Resource.Style.Chatththemef9c270);
                }
                else if (color.Contains("70a0e0"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme70a0e0_Dark : Resource.Style.Chatththeme70a0e0);
                }
                else if (color.Contains("56c4c5"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme56c4c5_Dark : Resource.Style.Chatththeme56c4c5);
                }
                else if (color.Contains("f33d4c"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemef33d4c_Dark : Resource.Style.Chatththemef33d4c);
                }
                else if (color.Contains("a1ce79"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemea1ce79_Dark : Resource.Style.Chatththemea1ce79);
                }
                else if (color.Contains("a085e2"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemea085e2_Dark : Resource.Style.Chatththemea085e2);
                }
                else if (color.Contains("ed9e6a"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemeed9e6a_Dark : Resource.Style.Chatththemeed9e6a);
                }
                else if (color.Contains("2b87ce"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme2b87ce_Dark : Resource.Style.Chatththeme2b87ce);
                }
                else if (color.Contains("f2812b"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemef2812b_Dark : Resource.Style.Chatththemef2812b);
                }
                else if (color.Contains("0ba05d"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme0ba05d_Dark : Resource.Style.Chatththeme0ba05d);
                }
                else if (color.Contains("0e71ea"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme0e71ea_Dark : Resource.Style.Chatththeme0e71ea);
                }
                else if (color.Contains("aa2294"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemeaa2294_Dark : Resource.Style.Chatththemeaa2294);
                }
                else if (color.Contains("f9a722"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemef9a722_Dark : Resource.Style.Chatththemef9a722);
                }
                else if (color.Contains("008484"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme008484_Dark : Resource.Style.Chatththeme008484);
                }
                else if (color.Contains("5462a5"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme5462a5_Dark : Resource.Style.Chatththeme5462a5);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemefc9cde_Dark : Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemefc9cde_Dark : Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("51bcbc"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme51bcbc_Dark : Resource.Style.Chatththeme51bcbc);
                }
                else if (color.Contains("c9605e"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththemec9605e_Dark : Resource.Style.Chatththemec9605e);
                }
                else if (color.Contains("01a5a5"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme01a5a5_Dark : Resource.Style.Chatththeme01a5a5);
                }
                else if (color.Contains("056bba"))
                {
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Chatththeme056bba_Dark : Resource.Style.Chatththeme056bba);
                }
                else
                {
                    //Default Color >> AppSettings.MainColor
                    SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetWallpaper()
        {
            try
            {
                //wael you check when image background  
                string path = MainSettings.SharedData?.GetString("Wallpaper_key", string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    if (type == "Image")
                    {
                        BodyListChatLayout.Background = Drawable.CreateFromPath(path);
                    }
                    else if (path.Contains("#"))
                    {
                        if (AppSettings.ChatTheme == ChatTheme.Default)
                        {
                            BodyListChatLayout.SetBackgroundColor(Color.ParseColor(path));
                        }
                        else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                        {
                            BodyListChatLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(path));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Timer Load Messsage

        private void SetTimerMessage()
        {
            try
            {
                if (TimerMessages == null && !IsBlocked)
                {
                    TimerMessages = new Timer { Interval = AppSettings.MessageRequestSpeed };
                    TimerMessages.Elapsed += TimerMessagesOnElapsed;
                    TimerMessages.Enabled = true;
                    TimerMessages.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TimerMessagesOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!IsBlocked)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMessageApi(false) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StartTimerMessage()
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi && TimerMessages != null)
                {
                    TimerMessages.Enabled = true;
                    TimerMessages.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopTimerMessage()
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi && TimerMessages != null)
                {
                    TimerMessages.Enabled = false;
                    TimerMessages.Stop();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DestroyTimerMessage()
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.RestApi && TimerMessages != null)
                {
                    TimerMessages.Enabled = false;
                    TimerMessages.Stop();
                    TimerMessages.Dispose();
                    TimerMessages = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Mic Record 

        public void OnClick(View v)
        {
            try
            {
                //RECORD BUTTON CLICKED
                SendRecordButton();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStartRecord()
        {
            //record voices 
            try
            {
                Console.WriteLine("OnStartRecord");
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartRecordingVoice();
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        StartRecordingVoice();
                    }
                    else
                    {
                        PermissionsType = "Record";
                        new PermissionsController(this).RequestPermission(102);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCancelRecord()
        {
            try
            {
                Console.WriteLine("OnCancelRecord");
                RecorderService.StopRecording();

                await Task.Delay(1000);

                // reset mic and show editText  
                LayoutEditText.Visibility = ViewStates.Visible;
                MediaButton.Visibility = ViewStates.Visible;

                ApiStatusChat("stopped");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //open Fragment record and show editText
        public async void OnFinishRecord(long recordTime)
        {
            try
            {
                //OnFinishRecord
                if (IsRecording)
                {
                    RecorderService.StopRecording();
                    var filePath = RecorderService.GetRecorded_Sound_Path();

                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);

                    if (recordTime > 0)
                    {
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            Bundle bundle = new Bundle();
                            bundle.PutString("FilePath", filePath);
                            ChatRecordSoundBoxFragment.Arguments = bundle;
                            ReplaceTopFragment(ChatRecordSoundBoxFragment);
                        }
                    }

                    IsRecording = false;
                }

                await Task.Delay(1000);

                LayoutEditText.Visibility = ViewStates.Visible;
                MediaButton.Visibility = ViewStates.Visible;

                ApiStatusChat("stopped");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnLessThanSecond()
        {
            //OnLessThanSecond
        }

        private async void StartRecordingVoice()
        {
            try
            {
                if (RecordButton?.Tag?.ToString() == "Free")
                {
                    //Set Record Style
                    IsRecording = true;

                    LayoutEditText.Visibility = ViewStates.Invisible;
                    MediaButton.Visibility = ViewStates.Invisible;

                    ResetMediaPlayerInMessages();

                    RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                    //Start Audio record
                    await Task.Delay(600);
                    RecorderService.StartRecording();

                    ApiStatusChat("recording");
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
                if (IsBlocked)
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_TextBlockedUser_BoxChat), ToastLength.Short);
                    return;
                }

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

                if (DataUser != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = DataUser.UserId,
                        Avatar = DataUser.Avatar,
                        Name = DataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentCall?.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));
                }

                if (intentCall != null)
                    StartActivity(intentCall);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string text)
        {
            try
            {
                if (text == GetText(Resource.String.Lbl_ImageGallery)) // image 
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("image"))
                        {
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "image");
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_TakeImageFromCamera)) // Camera 
                {
                    PermissionsType = "Camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("image"))
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "image");
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_VideoGallery)) // video  
                {
                    PermissionsType = "Video";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("video"))
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "video");
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_RecordVideoFromCamera)) // video camera
                {
                    PermissionsType = "Video_camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage("video"))
                        {
                            //requestCode >> 513 => video camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108, "video");
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_File)) // File  
                {
                    PermissionsType = "File";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 504 => File
                        new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                    }
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage("file"))
                        {
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(100, "file");
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Music)) // Music  
                {
                    PermissionsType = "Music";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage("audio"))
                            new IntentController(this).OpenIntentAudio(); //505
                        else
                            new PermissionsController(this).RequestPermission(100, "audio");
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Gif)) // Gif  
                {
                    StartActivityForResult(new Intent(this, typeof(GifActivity)), 300);
                }
                else if (text == GetText(Resource.String.Lbl_Contact)) // Contact  
                {
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        //Check to see if any permission in our group is available, if one, then all are
                        if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                        {
                            //request code of result is 506
                            new IntentController(this).OpenIntentGetContactNumberPhone();
                        }
                        else
                        {
                            //101 >> ReadContacts && ReadPhoneNumbers
                            new PermissionsController(this).RequestPermission(101);
                        }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Location)) // Location  
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                        {
                            //Open intent Location when the request code of result is 502
                            new IntentController(this).OpenIntentLocation();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(105);
                        }
                    }
                }

                //================= Menu Section =================
                else if (text == GetText(Resource.String.Lbl_View_Profile)) // Menu View profile
                {
                    OpenProfileClick();
                }
                else if (text == GetText(Resource.String.Lbl_Block)) // Menu Block
                {
                    OnMenuBlock_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Clear_chat)) // Menu Clear Chat
                {
                    OnMenuClearChat_Click();
                }
                else if (text == GetText(Resource.String.Lbl_StartedMessages)) // Menu Started Messages
                {
                    var intent = new Intent(this, typeof(StartedMessagesActivity));
                    intent.PutExtra("ChatId", ChatId);
                    intent.PutExtra("UserId", UserId);
                    intent.PutExtra("MainChatColor", MainChatColor);
                    StartActivity(intent);
                }
                else if (text == GetText(Resource.String.Lbl_Media)) // Menu Media (Shared Files)
                {
                    var intent = new Intent(this, typeof(SharedFilesActivity));
                    intent.PutExtra("UserId", UserId);
                    StartActivity(intent);
                }
                else if (text == GetText(Resource.String.Lbl_ChangeChatTheme)) // Change Chat Theme (color)
                {
                    ChatColorsFragment bottomSheet = new ChatColorsFragment();
                    Bundle bundle = new Bundle();
                    bundle.PutString("userid", UserId);
                    bottomSheet.Arguments = bundle;
                    bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
                }
                else if (text == GetText(Resource.String.Lbl_Wallpaper)) // Wallpaper
                {
                    var intent = new Intent(this, typeof(WallpaperActivity));
                    StartActivityForResult(intent, 23154);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuClearChat_Click()
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(GetText(Resource.String.Lbl_Clear_chat));
                dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureDeleteMessages));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes),(materialDialog, action) =>
                {
                    try
                    {
                        MAdapter.DifferList.Clear();
                        MAdapter.NotifyDataSetChanged();

                        var userToDelete = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == UserId);
                        if (userToDelete != null)
                        {
                            var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(userToDelete);
                            if (index != -1)
                            {
                                GlobalContext?.ChatTab?.LastChatTab.MAdapter.LastChatsList.Remove(userToDelete);
                                GlobalContext?.ChatTab?.LastChatTab.MAdapter.NotifyItemRemoved(index);
                            }
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, UserId);

                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteConversationAsync(UserId) });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_No),new MaterialDialogUtils());
                
                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnMenuBlock_Click()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.BlockUserAsync(UserId, true); //true >> "block" 
                    if (apiStatus == 200)
                    {
                        IsBlocked = DataUser.IsBlocked = true;

                        LayoutBlocked.Visibility = ViewStates.Visible;
                        LayoutEditText.Visibility = ViewStates.Gone;

                        var dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(DataUser.UserData, "Delete");

                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Blocked_successfully), ToastLength.Short);
                        OnMenuClearChat_Click();
                    }
                    else
                    {
                        Methods.DisplayReportResultTrack(respond);
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region OneSignal Notification

        private void GetOneSignalNotification()
        {
            try
            {
                var dataNotifier = Intent?.GetStringExtra("Notifier") ?? "Data not available";
                if (dataNotifier != "Data not available" && !string.IsNullOrEmpty(dataNotifier))
                {
                    if (dataNotifier == "Notifier")
                    {
                        string dataApp = Intent?.GetStringExtra("App") ?? "";
                        if (dataApp == "Timeline")
                        {
                            string name = Intent?.GetStringExtra("Name");
                            UserId = Intent?.GetStringExtra("UserID");
                            string avatar = Intent?.GetStringExtra("Avatar");

                            //string time = Intent?.GetStringExtra("Time");
                            //LastSeen = Intent?.GetStringExtra("LastSeen") ?? "off";

                            TxtUsername.Text = name; // user name

                            GlideImageLoader.LoadImage(this, avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            if (TypeChat == "SendMsgProduct")
                            {
                                //intent.GetStringExtra("ProductId", itemObject.LastMessage.LastMessageClass.ProductId);
                                //intent.GetStringExtra("ProductClass", JsonConvert.SerializeObject(itemObject.LastMessage.LastMessageClass.Product?.ProductClass));

                                string productId = Intent?.GetStringExtra("ProductId");

                                if (!Methods.CheckConnectivity())
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                    return;
                                }

                                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                var time = unixTimestamp.ToString();

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SendMessageAsync(UserId, time, "", "", "", "", "", "", productId) });
                                //ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully),ToastLength.Short);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fragment

        private void ReplaceTopFragment(AndroidX.Fragment.App.Fragment fragmentView)
        {
            try
            {
                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                TopFragmentHolder.TranslationY = 1200;
                TopFragmentHolder?.Animate()?.SetInterpolator(new FastOutSlowInInterpolator())?.TranslationYBy(-1200)?.SetDuration(500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Keyboard

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">stopped,recording,typing</param>
        private void ApiStatusChat(string type)
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (type == "stopped")
                    {
                        UserDetails.Socket?.EmitAsync_StoppedEvent(UserId, UserDetails.AccessToken);
                    }
                    else if (type == "recording")
                    {
                        UserDetails.Socket?.EmitAsync_RecordingEvent(UserId, UserDetails.AccessToken);
                    }
                    else if (type == "typing")
                    {
                        UserDetails.Socket?.EmitAsync_TypingEvent(UserId, UserDetails.AccessToken);
                    }
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SetChatTypingStatusAsync(UserId, type) });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}