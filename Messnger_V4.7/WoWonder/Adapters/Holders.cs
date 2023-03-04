using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Com.Airbnb.Lottie;
using Java.IO;
using Java.Lang;
using JetBrains.Annotations;
using WoWonder.Activities.ChatWindow.Adapters;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Socket;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace WoWonder.Adapters
{
    public class Holders
    {
        public enum TypeClick
        {
            Text, Image, Sound, Contact, Video, Sticker, File, Product, Map
        }

        public static MessageModelType GetTypeModel(MessageData item)
        {
            try
            {
                MessageModelType modelType;

                if (item.FromId == UserDetails.UserId) // right
                {
                    item.Position = "right";
                }
                else if (item.ToId == UserDetails.UserId) // left
                {
                    item.Position = "left";
                }

                string imageUrl = "", text = "";
                if (!string.IsNullOrEmpty(item.Stickers))
                {
                    item.Stickers = item.Stickers.Replace(".mp4", ".gif");
                    imageUrl = item.Stickers;
                }

                if (!string.IsNullOrEmpty(item.Media))
                    imageUrl = item.Media;

                if (!string.IsNullOrEmpty(item.Text))
                    text = ChatUtils.GetMessage(item.Text, item.Time);

                if (!string.IsNullOrEmpty(text))
                    modelType = item.TypeTwo == "contact" ? item.Position == "left" ? MessageModelType.LeftContact : MessageModelType.RightContact : item.Position == "left" ? MessageModelType.LeftText : MessageModelType.RightText;
                else if (item.Product?.ProductClass != null && !string.IsNullOrEmpty(item.ProductId) && item.ProductId != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftProduct : MessageModelType.RightProduct;
                else if (!string.IsNullOrEmpty(item.Lat) && !string.IsNullOrEmpty(item.Lng) && item.Lat != "0" && item.Lng != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftMap : MessageModelType.RightMap;
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(imageUrl);
                    switch (type)
                    {
                        case "Audio":
                            modelType = item.Position == "left" ? MessageModelType.LeftAudio : MessageModelType.RightAudio;
                            break;
                        case "Video":
                            modelType = item.Position == "left" ? MessageModelType.LeftVideo : MessageModelType.RightVideo;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.Media) && !item.Media.Contains(".gif"):
                            modelType = item.Media.Contains("sticker") ? item.Position == "left" ? MessageModelType.LeftSticker : MessageModelType.RightSticker : item.Position == "left" ? MessageModelType.LeftImage : MessageModelType.RightImage;
                            break;
                        case "File" when !string.IsNullOrEmpty(item.Stickers) && item.Stickers.Contains(".gif"):
                        case "File" when !string.IsNullOrEmpty(item.Media) && item.Media.Contains(".gif"):
                        case "Image" when !string.IsNullOrEmpty(item.Stickers) && item.Stickers.Contains(".gif"):
                        case "Image" when !string.IsNullOrEmpty(item.Media) && item.Media.Contains(".gif"):
                            modelType = item.Position == "left" ? MessageModelType.LeftGif : MessageModelType.RightGif;
                            break;
                        case "File":
                            modelType = item.Position == "left" ? MessageModelType.LeftFile : MessageModelType.RightFile;
                            break;
                        default:
                            modelType = MessageModelType.None;
                            break;
                    }
                }
                else
                {
                    modelType = MessageModelType.None;
                }

                return modelType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return MessageModelType.None;
            }
        }

        public static MessageModelType GetTypeModel(PrivateMessageObject item)
        {
            try
            {
                MessageModelType modelType;

                if (item.Sender == UserDetails.UserId) // right
                {
                    item.Position = "right";
                }
                else if (item.Receiver == UserDetails.UserId) // left
                {
                    item.Position = "left";
                }

                string text = "";

                if (!string.IsNullOrEmpty(item.Message))
                    text = ChatUtils.GetMessage(item.Message, item.Time);

                if (!string.IsNullOrEmpty(text))
                    modelType = item.Position == "left" ? MessageModelType.LeftText : MessageModelType.RightText;

                else if (!string.IsNullOrEmpty(item.Lat) && !string.IsNullOrEmpty(item.Lng) && item.Lat != "0" && item.Lng != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftMap : MessageModelType.RightMap;

                //if (item == "contact")
                //    modelType = item.Position == "left" ? MessageModelType.LeftContact : MessageModelType.RightContact;

                else if (!string.IsNullOrEmpty(item.MediaLink))
                {
                    var typeFile = Methods.AttachmentFiles.Check_FileExtension(item.MediaLink);
                    switch (typeFile)
                    {
                        case "Audio":
                            modelType = item.Position == "left" ? MessageModelType.LeftAudio : MessageModelType.RightAudio;
                            break;
                        case "Video":
                            modelType = item.Position == "left" ? MessageModelType.LeftVideo : MessageModelType.RightVideo;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.MediaLink) && !item.MediaLink.Contains(".gif") && !item.MessagesHtml.Contains(".gif"):
                            modelType = item.MediaLink.Contains("sticker") ? item.Position == "left" ? MessageModelType.LeftSticker : MessageModelType.RightSticker : item.Position == "left" ? MessageModelType.LeftImage : MessageModelType.RightImage;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.MediaLink) && item.MediaLink.Contains(".gif") || item.MessagesHtml.Contains(".gif"):
                            modelType = item.Position == "left" ? MessageModelType.LeftGif : MessageModelType.RightGif;
                            break;
                        case "File":
                            modelType = item.Position == "left" ? MessageModelType.LeftFile : MessageModelType.RightFile;
                            break;
                        default:
                            modelType = MessageModelType.None;
                            break;
                    }
                }
                else
                {
                    modelType = MessageModelType.None;
                }

                return modelType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return MessageModelType.None;
            }
        }

        public static string GetSmileTypeIcon(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    {"smile", "😀"},
                    {"joy", "😂"},
                    {"relaxed", "😚"},
                    {"stuck-out-tongue-winking-eye", "😛"},
                    {"stuck-out-tongue", ":😜"},
                    {"sunglasses", "😎"},
                    {"wink", "😉"},
                    {"grin", "😁"},
                    {"smirk", "😏"},
                    {"innocent", "😇"},
                    {"cry", ":😢"},
                    {"sob", ":😭"},
                    {"disappointed", "😞"},
                    {"kissing-heart", "😘"},
                    {"heart", "❤️"},
                    {"broken-heart", "💔"},
                    {"heart-eyes", "😍"},
                    {"star", "⭐"},
                    {"open-mouth", "😦"},
                    {"scream", "😱"},
                    {"anguished", "😨"},
                    {"unamused", "😒"},
                    {"angry", "😡"},
                    {"rage", "😡"},
                    {"expressionless", "😑"},
                    {"confused", ":😕"},
                    {"neutral-face", "😐"},
                    {"exclamation", "❗"},
                    {"yum", "😋"},
                    {"triumph", "😤"},
                    {"imp", "😈"},
                    {"hear-no-evil", "🙉"},
                    {"alien", "👽"},
                    {"yellow-heart", "💛"},
                    {"sleeping", "😴"},
                    {"mask", "😷"},
                    {"no-mouth", "😈"},
                    {"weary", "😩"},
                    {"dizzy-face", "😵"},
                    {"man", "👨"},
                    {"woman", "👩"},
                    {"boy", "👦"},
                    {"girl", "👧"},
                    {"older-man", "👴"},
                    {"older-woman", "👵"},
                    {"cop", "👨‍✈️"},
                    {"dancers", "👯"},
                    {"speak-no-evil", "🙊"},
                    {"lips", "👄"},
                    {"see-no-evil", "🙈"},
                    {"dog", "🐕"},
                    {"bear", "🐻"},
                    {"rose", "🌹"},
                    {"gift-heart", "💝"},
                    {"ghost", "👻"},
                    {"bell", "🔔"},
                    {"video-game", "🎮"},
                    {"soccer", "⚽"},
                    {"books", "📚"},
                    {"moneybag", "💰"},
                    {"mortar-board", "🎓"},
                    {"hand", "🤚"},
                    {"tiger", "🐅"},
                    {"elephant", "🐘"},
                    {"scream-cat", "🙀"},
                    {"monkey", "🐒"},
                    {"bird", "🐦"},
                    {"snowflake", "❄️"},
                    {"sunny", "☀️"},
                    {"оcean", "🌊"},
                    {"umbrella", "☂️"},
                    {"hibiscus", "🌺"},
                    {"tulip", "🌷"},
                    {"computer", "💻"},
                    {"bomb", "💣"},
                    {"gem", "💎"},
                    {"ring", "💍"}
                };

                string pattern = @"(<i class=[""']twa-lg twa twa-(.*?)[""']>)";
                var aa = Regex.Matches(text, pattern);
                if (aa.Count > 0)
                {
                    foreach (var item in aa)
                    {
                        //<i class="twa-lg twa twa-joy">
                        var type = item.ToString().Split("twa-").Last().Replace(">", "").Replace('"', ' ').Replace(" ", "");

                        var containsValue = dictionary.ContainsKey(type);
                        if (containsValue)
                        {
                            var value = dictionary.FirstOrDefault(a => a.Key == type).Value;
                            text = text.Replace(item.ToString(), value);
                        }
                    }
                }

                return text;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public class MesClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
            public TypeClick Type { get; set; }
        }

        public class GlobalMessageViews : RecyclerView.ViewHolder
        {
            #region Variables Basic 

            public ConstraintLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }

            public LinearLayout ForwardLayout { get; private set; }
            public RepliedMessageView RepliedMessageView { get; set; }

            public ImageView ImageCountLike { get; private set; }

            public TextView UserName { get; private set; }
            public ImageView ImageUser { get; private set; }
            public TextView Time { get; private set; }

            public ImageView Seen { get; private set; }

            public FrameLayout StarLayout { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }
            public ImageView StarImage { get; private set; }

            #endregion

            public GlobalMessageViews([NotNull] View itemView, bool chatGroup) : base(itemView)
            {
                try
                {
                    RepliedMessageView = new RepliedMessageView(itemView);

                    LytParent = itemView.FindViewById<ConstraintLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);

                    ForwardLayout = itemView.FindViewById<LinearLayout>(Resource.Id.ForwardLayout);
                    ForwardLayout.Visibility = ViewStates.Gone;

                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Seen = itemView.FindViewById<ImageView>(Resource.Id.seen);

                    ImageCountLike = itemView.FindViewById<ImageView>(Resource.Id.ImageCountLike);
                    ImageCountLike.Visibility = ViewStates.Gone;

                    StarLayout = itemView.FindViewById<FrameLayout>(Resource.Id.starLayout);
                    StarLayout.Visibility = ViewStates.Gone;

                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;

                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    ImageUser = itemView.FindViewById<ImageView>(Resource.Id.imageUser);
                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);

                    if (UserName != null)
                    {
                        if (chatGroup)
                        {
                            UserName.Visibility = ViewStates.Visible;
                            ImageUser.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            UserName.Visibility = ViewStates.Gone;
                            ImageUser.Visibility = ViewStates.Gone;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class TextViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public SuperTextView SuperTextView { get; private set; }
            public OgLinkMessageView OgLinkMessageView { get; private set; }

            #endregion

            public TextViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    SuperTextView = itemView.FindViewById<SuperTextView>(Resource.Id.active);

                    OgLinkMessageView = new OgLinkMessageView(itemView);

                    if (RepliedMessageView != null)
                        RepliedMessageView.MessageFileThumbnail.Visibility = ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Text });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Text });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class ImageViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView ImageView { get; private set; }
            public ProgressBar LoadingProgress { get; private set; }
            public TextView TxtErrorLoading { get; private set; }
            public TextView TxtDownload { get; private set; }

            #endregion

            public ImageViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, Action<MesClickEventArgs> downloadClickListener, Action<MesClickEventArgs> errorLoadingClickListener, bool chatGroup, TypeClick typeClick) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);

                    TxtErrorLoading = itemView.FindViewById<TextView>(Resource.Id.textErrorLoading);
                    TxtDownload = itemView.FindViewById<TextView>(Resource.Id.textDownload);

                    LoadingProgress = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);

                    if (TxtDownload != null)
                        TxtDownload.Click += (sender, args) => downloadClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = typeClick });

                    if (TxtErrorLoading != null)
                        TxtErrorLoading.Click += (sender, args) => errorLoadingClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = typeClick });

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = typeClick });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = typeClick });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class VideoViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView ImageView { get; private set; }
            public ProgressBar LoadingProgress { get; private set; }
            public TextView TxtErrorLoading { get; private set; }
            public TextView TxtDownload { get; private set; }
            public CircleButton PlayButton { get; private set; }

            #endregion

            public VideoViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, Action<MesClickEventArgs> downloadClickListener, Action<MesClickEventArgs> errorLoadingClickListener, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);

                    TxtErrorLoading = itemView.FindViewById<TextView>(Resource.Id.textErrorLoading);
                    TxtDownload = itemView.FindViewById<TextView>(Resource.Id.textDownload);

                    LoadingProgress = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);

                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);

                    if (TxtDownload != null)
                        TxtDownload.Click += (sender, args) => downloadClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Video });

                    if (TxtErrorLoading != null)
                        TxtErrorLoading.Click += (sender, args) => errorLoadingClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Video });

                    PlayButton.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Video });

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Video });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Video });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class SoundViewHolder : GlobalMessageViews, View.IOnClickListener
        {
            #region Variables Basic

            public View MainView { get; private set; }
            private readonly MessageAdapter MessageAdapter;

            public ProgressBar LoadingProgress { get; private set; }
            public CircleButton PlayButton { get; private set; }
            public AppCompatSeekBar SeekBar { get; private set; }

            public TextView DurationTextView { get; private set; }

            #endregion

            public SoundViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, MessageAdapter mAdapter, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;
                    MessageAdapter = mAdapter;

                    DurationTextView = itemView.FindViewById<TextView>(Resource.Id.Duration);
                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);

                    LoadingProgress = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    SeekBar = itemView.FindViewById<AppCompatSeekBar>(Resource.Id.seek_song_progressbar);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        SeekBar.SetProgress(0, true);
                    else
                        // For API < 24 
                        SeekBar.Progress = 0;

                    PlayButton?.SetOnClickListener(this);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Sound });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Sound });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }


            public void OnClick(View v)
            {
                try
                {
                    if (BindingAdapterPosition != RecyclerView.NoPosition)
                    {
                        MessageAdapter.MusicBarMessageData = MessageAdapter.DifferList[BindingAdapterPosition]?.MesData;
                        if (v.Id == PlayButton.Id && MessageAdapter.MusicBarMessageData != null)
                        {
                            PlaySound(BindingAdapterPosition, MessageAdapter.MusicBarMessageData);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void PlaySound(int position, MessageDataExtra message)
            {
                try
                {
                    if (MessageAdapter.PositionSound != position)
                    {
                        var list = MessageAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
                        if (list.Count > 0)
                            foreach (var item in list)
                            {
                                item.MesData.MediaIsPlaying = false;

                                if (item.MesData.MediaPlayer != null)
                                {
                                    item.MesData.MediaPlayer.Stop();
                                    item.MesData.MediaPlayer.Reset();
                                }

                                item.MesData.MediaPlayer = null!;
                                item.MesData.MediaTimer = null!;

                                item.MesData.MediaPlayer?.Release();
                                item.MesData.MediaPlayer = null!;
                            }
                    }

                    var fileName = message.Media.Split('/').Last();

                    var mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media, "audio");
                    if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                    {
                        var duration = WoWonderTools.GetDuration(mediaFile);
                        DurationTextView.Text = message.MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                    }
                    else
                        DurationTextView.Text = message.MediaDuration;

                    if (message.MediaPlayer == null)
                    {
                        MessageAdapter.PositionSound = position;

                        message.MediaPlayer = new MediaPlayer();
                        message.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());
                        message.MediaPlayer.Completion += (sender, args) =>
                        {
                            try
                            {
                                LoadingProgress.Visibility = ViewStates.Gone;
                                PlayButton.Visibility = ViewStates.Visible;

                                PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                PlayButton.Tag = "Play";
                                message.MediaPlayer.Stop();

                                message.MediaIsPlaying = false;

                                message.MediaPlayer.Stop();
                                message.MediaPlayer.Reset();
                                message.MediaPlayer = null;

                                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                    SeekBar.SetProgress(0, true);
                                else
                                    // For API < 24 
                                    SeekBar.Progress = 0;

                                if (message.MediaTimer == null) return;

                                message.MediaTimer.Enabled = false;
                                message.MediaTimer.Stop();
                                message.MediaTimer = null;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        message.MediaPlayer.Prepared += (o, eventArgs) =>
                        {
                            try
                            {
                                message.MediaIsPlaying = true;

                                message.MediaPlayer.Start();
                                PlayButton.Tag = "Pause";
                                PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                                LoadingProgress.Visibility = ViewStates.Gone;
                                PlayButton.Visibility = ViewStates.Visible;

                                if (message.MediaTimer == null)
                                {
                                    message.MediaTimer = new Timer { Interval = 1000, Enabled = true };
                                    message.MediaTimer.Elapsed += (sender, eventArgs) =>
                                    {
                                        MessageAdapter.ActivityContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                if (message.MediaPlayer != null && message.MediaTimer.Enabled)
                                                {
                                                    int totalDuration = message.MediaPlayer.Duration;
                                                    int currentDuration = message.MediaPlayer.CurrentPosition;

                                                    // Updating progress bar
                                                    int progress = GetProgressSeekBar(currentDuration, totalDuration);

                                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                                        SeekBar.SetProgress(progress, true);
                                                    else
                                                        // For API < 24 
                                                        SeekBar.Progress = progress;

                                                    if (message.MediaPlayer.CurrentPosition <= message.MediaPlayer.Duration)
                                                    {
                                                        DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.CurrentPosition.ToString());
                                                    }
                                                    else
                                                    {
                                                        DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.Duration.ToString());

                                                        PlayButton.Tag = "Play";
                                                        PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                                        //PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.icon_play_vector);
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                                PlayButton.Tag = "Play";
                                            }
                                        });
                                    };
                                    message.MediaTimer?.Start();
                                }
                                else
                                {
                                    message.MediaTimer.Enabled = true;
                                    message.MediaTimer.Start();
                                }

                                if (Methods.CheckConnectivity())
                                    RequestsAsync.Message.ListeningMessageAsync(message.Id).ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        PlayButton.Visibility = ViewStates.Gone;
                        LoadingProgress.Visibility = ViewStates.Visible;

                        if (mediaFile.Contains("http"))
                            mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media, "audio");

                        if (!string.IsNullOrEmpty(mediaFile) && (mediaFile.Contains("file://") || mediaFile.Contains("content://") || mediaFile.Contains("storage") || mediaFile.Contains("/data/user/0/")))
                        {
                            File file2 = new File(mediaFile);
                            var photoUri = FileProvider.GetUriForFile(MessageAdapter.ActivityContext, MessageAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                            message.MediaPlayer.SetDataSource(MessageAdapter.ActivityContext, photoUri);
                            message.MediaPlayer.Prepare();
                        }
                        else
                        {
                            message.MediaPlayer.SetDataSource(MessageAdapter.ActivityContext, Uri.Parse(mediaFile));
                            message.MediaPlayer.PrepareAsync();
                        }

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                            SeekBar.SetProgress(0, true);
                        else
                            // For API < 24 
                            SeekBar.Progress = 0;

                        SeekBar.StartTrackingTouch += (sender, args) =>
                        {
                            try
                            {
                                if (message.MediaTimer != null)
                                {
                                    message.MediaTimer.Enabled = false;
                                    message.MediaTimer.Stop();
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        SeekBar.StopTrackingTouch += (sender, args) =>
                        {
                            try
                            {
                                if (message.MediaTimer != null)
                                {
                                    message.MediaTimer.Enabled = false;
                                    message.MediaTimer.Stop();
                                }

                                int seek = args.SeekBar.Progress;

                                int totalDuration = message.MediaPlayer.Duration;
                                var currentPosition = ProgressToTimer(seek, totalDuration);

                                // forward or backward to certain seconds
                                message.MediaPlayer.SeekTo((int)currentPosition);

                                if (message.MediaTimer != null)
                                {
                                    // update timer progress again
                                    message.MediaTimer.Enabled = true;
                                    message.MediaTimer.Start();
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };
                    }
                    else
                    {
                        if (PlayButton?.Tag?.ToString() == "Play")
                        {
                            PlayButton.Visibility = ViewStates.Visible;
                            PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                            PlayButton.Tag = "Pause";
                            message.MediaPlayer?.Start();

                            message.MediaIsPlaying = true;

                            if (message.MediaTimer != null)
                            {
                                message.MediaTimer.Enabled = true;
                                message.MediaTimer.Start();
                            }
                        }
                        else if (PlayButton?.Tag?.ToString() == "Pause")
                        {
                            PlayButton.Visibility = ViewStates.Visible;
                            PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                            PlayButton.Tag = "Play";
                            message.MediaPlayer?.Pause();

                            message.MediaIsPlaying = false;

                            if (message.MediaTimer == null) return;

                            message.MediaTimer.Enabled = false;
                            message.MediaTimer.Stop();
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private static readonly int MaxProgress = 10000;
            private long ProgressToTimer(int progress, int totalDuration)
            {
                try
                {
                    totalDuration /= 1000;
                    var currentDuration = (int)((double)progress / MaxProgress * totalDuration);

                    // return current duration in milliseconds
                    return currentDuration * 1000;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return 0;
                }
            }

            private int GetProgressSeekBar(int currentDuration, int totalDuration)
            {
                try
                {
                    // calculating percentage
                    double progress = (double)currentDuration / totalDuration * MaxProgress;
                    return progress switch
                    {
                        >= 0 =>
                            // return percentage
                            Convert.ToInt32(progress),
                        _ => 0
                    };
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return 0;
                }
            }

        }

        public class StickerViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView ImageView { get; private set; }

            #endregion

            public StickerViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Sticker });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Sticker });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class ContactViewHolder : GlobalMessageViews, View.IOnClickListener
        {
            #region Variables Basic

            public View MainView { get; private set; }
            private readonly MessageAdapter MessageAdapter;
            public TextView ContactName { get; private set; }

            #endregion

            public ContactViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, MessageAdapter mAdapter, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;
                    MessageAdapter = mAdapter;

                    ContactName = itemView.FindViewById<TextView>(Resource.Id.contactName);

                    BubbleLayout.SetOnClickListener(this);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Contact });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Contact });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (BindingAdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = MessageAdapter.DifferList[BindingAdapterPosition]?.MesData;
                        if (v.Id == BubbleLayout.Id && item != null)
                        {
                            Methods.App.SaveContacts(MessageAdapter.ActivityContext, item.ContactNumber, item.ContactName, "2");
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class FileViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView IconTypeFile { get; private set; }
            public TextView FileName { get; private set; }
            public TextView SizeFile { get; private set; }

            #endregion

            public FileViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    IconTypeFile = itemView.FindViewById<ImageView>(Resource.Id.IconTypeFile);
                    FileName = itemView.FindViewById<TextView>(Resource.Id.fileName);
                    SizeFile = itemView.FindViewById<TextView>(Resource.Id.sizeFileText);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.File });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.File });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class ProductViewHolder : GlobalMessageViews
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public ImageView ImageView { get; private set; }
            public TextView Title { get; private set; }
            public TextView Cat { get; private set; }
            public TextView Price { get; private set; }

            #endregion

            public ProductViewHolder([NotNull] View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool chatGroup) : base(itemView, chatGroup)
            {
                try
                {
                    MainView = itemView;

                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                    Title = itemView.FindViewById<TextView>(Resource.Id.title);
                    Cat = itemView.FindViewById<TextView>(Resource.Id.cat);
                    Price = itemView.FindViewById<TextView>(Resource.Id.price);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Product });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = BindingAdapterPosition, Type = TypeClick.Product });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class NotSupportedViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public RelativeLayout LytParent { get; private set; }
            public View MainView { get; private set; }
            public SuperTextView AutoLinkNotsupportedView { get; private set; }
            public LinearLayout ForwardLayout { get; private set; }

            #endregion

            public NotSupportedViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<RelativeLayout>(Resource.Id.main);
                    AutoLinkNotsupportedView = itemView.FindViewById<SuperTextView>(Resource.Id.active);
                    var time = itemView.FindViewById<TextView>(Resource.Id.time);

                    time.Visibility = ViewStates.Gone;

                    var userName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (userName != null) userName.Visibility = ViewStates.Gone;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class RepliedMessageView : Object
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public LinearLayout ColorView { get; private set; }
            public LinearLayout RepliedMessageLayout { get; private set; }
            public TextView TxtOwnerName { get; private set; }
            public TextView TxtMessageType { get; private set; }
            public TextView TxtShortMessage { get; private set; }
            public ImageView MessageFileThumbnail { get; private set; }
            public ImageView BtnCloseReply { get; private set; }

            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="itemView"></param>
            public RepliedMessageView([NotNull] View itemView)
            {
                try
                {
                    MainView = itemView;

                    RepliedMessageLayout = itemView.FindViewById<LinearLayout>(Resource.Id.replied_message_view);
                    ColorView = itemView.FindViewById<LinearLayout>(Resource.Id.color_view);
                    TxtOwnerName = itemView.FindViewById<TextView>(Resource.Id.owner_name);
                    TxtMessageType = itemView.FindViewById<TextView>(Resource.Id.message_type);
                    TxtShortMessage = itemView.FindViewById<TextView>(Resource.Id.short_message);
                    MessageFileThumbnail = itemView.FindViewById<ImageView>(Resource.Id.message_file_thumbnail);
                    BtnCloseReply = itemView.FindViewById<ImageView>(Resource.Id.clear_btn_reply_view);

                    RepliedMessageLayout.Visibility = ViewStates.Gone;
                    BtnCloseReply.Visibility = ViewStates.Gone;

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class OgLinkMessageView : Object
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public LinearLayout OgLinkContainerLayout { get; private set; }
            public ImageView OgLinkImage { get; private set; }
            public TextView OgLinkUrl { get; private set; }
            public TextView OgLinkTitle { get; private set; }
            public TextView OgLinkDescription { get; private set; }


            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="itemView"></param>
            public OgLinkMessageView(View itemView)
            {
                try
                {
                    MainView = itemView;

                    OgLinkContainerLayout = MainView.FindViewById<LinearLayout>(Resource.Id.info_container);
                    OgLinkImage = MainView.FindViewById<ImageView>(Resource.Id.Image);
                    OgLinkUrl = MainView.FindViewById<TextView>(Resource.Id.url);
                    OgLinkTitle = MainView.FindViewById<TextView>(Resource.Id.Title);
                    OgLinkDescription = MainView.FindViewById<TextView>(Resource.Id.description);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public sealed class MsgPreCachingLayoutManager : LinearLayoutManager
        {
            private readonly Context Context;
            private int ExtraLayoutSpace = -1;
            private readonly int DefaultExtraLayoutSpace = 600;
            private OrientationHelper MOrientationHelper;
            private int MAdditionalAdjacentPrefetchItemCount;

            public MsgPreCachingLayoutManager(Activity context) : base(context)
            {
                try
                {
                    Context = context;
                    Init();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void Init()
            {
                try
                {
                    MOrientationHelper = OrientationHelper.CreateOrientationHelper(this, Orientation);
                    ItemPrefetchEnabled = true;
                    InitialPrefetchItemCount = 30;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void SetExtraLayoutSpace(int space)
            {
                ExtraLayoutSpace = space;
            }

            [Obsolete("deprecated")]
            protected override int GetExtraLayoutSpace(RecyclerView.State state)
            {
                return ExtraLayoutSpace switch
                {
                    > 0 => ExtraLayoutSpace,
                    _ => DefaultExtraLayoutSpace
                };
            }

            public void SetPreloadItemCount(int preloadItemCount)
            {
                MAdditionalAdjacentPrefetchItemCount = preloadItemCount switch
                {
                    < 1 => throw new IllegalArgumentException("adjacentPrefetchItemCount must not smaller than 1!"),
                    _ => preloadItemCount - 1
                };
            }

            public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
            {
                try
                {
                    base.OnLayoutChildren(recycler, state);
                }
                catch
                {
                    // Methods.DisplayReportResultTrack(e);  
                }
            }

            //public override bool SupportsPredictiveItemAnimations()
            //{
            //    try
            //    {
            //        base.SupportsPredictiveItemAnimations();
            //        return false;
            //    }
            //    catch (Exception e)
            //    {
            //        Methods.DisplayReportResultTrack(e);  
            //        return base.SupportsPredictiveItemAnimations();
            //    }
            //}

            public override void CollectAdjacentPrefetchPositions(int dx, int dy, RecyclerView.State state, ILayoutPrefetchRegistry layoutPrefetchRegistry)
            {
                try
                {
                    base.CollectAdjacentPrefetchPositions(dx, dy, state, layoutPrefetchRegistry);

                    var delta = Orientation == Horizontal ? dx : dy;
                    if (ChildCount == 0 || delta == 0)
                        return;

                    var layoutDirection = delta > 0 ? 1 : -1;
                    var child = GetChildClosest(layoutDirection);
                    var currentPosition = GetPosition(child) + layoutDirection;

                    if (layoutDirection != 1)
                        return;

                    var scrollingOffset = MOrientationHelper.GetDecoratedEnd(child) - MOrientationHelper.EndAfterPadding;
                    for (var i = currentPosition + 1; i < currentPosition + MAdditionalAdjacentPrefetchItemCount + 1; i++)
                    {
                        switch (i)
                        {
                            case >= 0 when i < state.ItemCount:
                                layoutPrefetchRegistry.AddPosition(i, Java.Lang.Math.Max(0, scrollingOffset));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private View GetChildClosest(int layoutDirection)
            {
                return GetChildAt(layoutDirection == -1 ? 0 : ChildCount - 1);
            }
        }
    }
}