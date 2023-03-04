using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Exoplayer2.UI;
using Com.Plattysoft.Leonids;
using Com.Plattysoft.Leonids.Modifiers;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.Stories.StoriesProgressView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers.Exo;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using IList = System.Collections.IList;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Story.Adapter
{
    public class StoryShowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, RecyclerView.IOnChildAttachStateChangeListener
    {
        public readonly Activity ActivityContext;
        public readonly StoriesProgressView StoriesProgress;
        public readonly ViewStoryFragment StoryFragment;
        public ObservableCollection<StoryDataObject.Story> StoryList = new ObservableCollection<StoryDataObject.Story>();
        private readonly StReadMoreOption ReadMoreOption;
        private readonly RecyclerView RecyclerView;
        private View ViewHolderParent;
        public static StyledPlayerView PlayerView { get; set; }
        public static ExoController ExoController { get; set; }

        public StoryShowAdapter(Activity context, StoriesProgressView storyProgressView, ViewStoryFragment storyFragment)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                StoriesProgress = storyProgressView;
                StoryFragment = storyFragment;
                RecyclerView = storyFragment.MRecycler;

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(250, StReadMoreOption.TypeCharacter)
                    .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StoryList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                if (viewType == 11) //Video
                {
                    //Setup your layout here >> ViewStoryLayout
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewStoryVideoLayout, parent, false);
                    var vh = new StoryShowAdapterViewHolder(itemView, this, "Video");
                    return vh;
                }
                else   //Image
                {
                    //Setup your layout here >> ViewStoryLayout
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewStoryLayout, parent, false);
                    var vh = new StoryShowAdapterViewHolder(itemView, this, "Image");
                    return vh;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is StoryShowAdapterViewHolder holder)
                {
                    var item = StoryList[position];
                    if (item != null)
                    {
                        string caption = "";
                        if (!string.IsNullOrEmpty(item.Description))
                            caption = item.Description;
                        else if (!string.IsNullOrEmpty(item.Title))
                            caption = item.Title;

                        if (string.IsNullOrEmpty(caption) || string.IsNullOrWhiteSpace(caption))
                        {
                            holder.CaptionStoryTextView.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            holder.CaptionStoryTextView.Visibility = ViewStates.Visible;
                            ReadMoreOption.AddReadMoreTo(holder.CaptionStoryTextView, new Java.Lang.String(Methods.FunString.DecodeString(caption)));
                        }

                        StoryFragment.SetLastSeenTextView(item);

                        bool isOwner = item.IsOwner;
                        if (item.UserId == UserDetails.UserId)
                        {
                            holder.OpenReply.Visibility = ViewStates.Gone;

                            holder.OpenSeenListLayout.Visibility = ViewStates.Visible;
                            holder.SeenCounterTextView.Visibility = ViewStates.Visible;
                            holder.SeenCounterTextView.Text = item.ViewCount;
                        }
                        else
                        {
                            holder.OpenReply.Visibility = ViewStates.Visible;

                            holder.OpenSeenListLayout.Visibility = ViewStates.Gone;
                            holder.SeenCounterTextView.Visibility = ViewStates.Gone;
                        }

                        string mediaFile = item.Thumbnail;
                        //image and video 
                        if (!item.Thumbnail.Contains("avatar") && item.Videos.Count == 0)
                            mediaFile = item.Thumbnail;
                        else if (item.Videos.Count > 0)
                            mediaFile = item.Videos[0].Filename;

                        var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                        if (type == "Image")
                        {
                            Glide.With(ActivityContext?.BaseContext).Load(mediaFile).Apply(new RequestOptions()).Into(holder.StoryImageView);
                        }
                        else if (type == "Video")
                        {
                            ViewHolderParent = holder.MainView;

                            if (PlayerView == null)
                                holder.InitVideoView(holder.MainView);

                            var fileName = mediaFile.Split('/').Last();
                            mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile, "other");

                            if (mediaFile.Contains("http"))
                            {
                                Uri uri = Uri.Parse(mediaFile);
                                ExoController?.FirstPlayVideo(uri);
                            }
                            else
                            {
                                var file = Uri.FromFile(new File(mediaFile));
                                ExoController?.FirstPlayVideo(file);
                            }
                        }

                        Glide.With(ActivityContext?.BaseContext).Load(mediaFile).Apply(new RequestOptions()).Into(new ColorGenerate(ActivityContext, holder.ImageBlurView));

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.GetStoryByIdAsync(item.Id) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public StoryDataObject.Story GetItem(int position)
        {
            return StoryList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = GetItem(position);

                return item.TypeView switch
                {
                    "Image" => 10,
                    "Video" => 11,
                    _ => 10
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 10;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];
                if (item == null)
                    return d;
                else
                {
                    string mediaFile = "";
                    //image and video 
                    if (!item.Thumbnail.Contains("avatar") && item.Videos.Count == 0)
                        mediaFile = item.Thumbnail;

                    if (!string.IsNullOrEmpty(mediaFile))
                        d.Add(mediaFile);

                    return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

        #region ChildAttachState

        public void OnChildViewAttachedToWindow(View view)
        {
            try
            {
                //if (ViewHolderParent != null && ViewHolderParent.Equals(view))
                //{
                //    var mainHolder = RecyclerView.GetChildViewHolder(view);
                //    if (mainHolder is StoryShowAdapterViewHolder holder)
                //    {
                //        if (PlayerView != null)
                //        {
                //            holder.Play();
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnChildViewDetachedFromWindow(View view)
        {
            try
            {
                if (ViewHolderParent != null && ViewHolderParent.Equals(view))
                {
                    var mainHolder = RecyclerView.GetChildViewHolder(view);
                    if (mainHolder is StoryShowAdapterViewHolder holder)
                    {
                        if (PlayerView != null)
                        {
                            holder.Destroy();
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
    }

    public class StoryShowAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
    {
        #region Variables Basic

        public View MainView { get; private set; }

        //public FrameLayout StoryDisplayLayout { get; private set; }

        public ImageView StoryImageView { get; private set; }


        public View ReverseView { get; private set; }
        public View CenterView { get; private set; }
        public View SkipView { get; private set; }


        public LinearLayout StoryBodyLayout { get; private set; }
        public TextView CaptionStoryTextView { get; private set; }

        public LinearLayout OpenReply { get; private set; }
        public LinearLayout SendMessagePanel { get; private set; }

        public ImageView MImgButtonOne { get; private set; }
        public ImageView MImgButtonTwo { get; private set; }
        public ImageView MImgButtonThree { get; private set; }
        public ImageView MImgButtonFour { get; private set; }
        public ImageView MImgButtonFive { get; private set; }
        public ImageView MImgButtonSix { get; private set; }

        public LinearLayout OpenSeenListLayout { get; private set; }
        public TextView SeenCounterTextView { get; private set; }
        public TextView IconSeen { get; private set; }

        public ImageView ImageBlurView { get; private set; }

        #endregion

        private long PressTime;
        private readonly long Limit = 500L;
        private bool PlayerPaused, Paused;
        private readonly StoryShowAdapter MAdapter;

        public StoryShowAdapterViewHolder(View itemView, StoryShowAdapter adapter, string type) : base(itemView)
        {
            try
            {
                MAdapter = adapter;
                MainView = itemView;

                // StoryDisplayLayout = itemView.FindViewById<FrameLayout>(Resource.Id.storyDisplay);
                if (type == "Image")
                {
                    StoryImageView = itemView.FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                }
                else
                {
                    InitVideoView(itemView);
                }

                ReverseView = itemView.FindViewById<View>(Resource.Id.reverse);
                CenterView = itemView.FindViewById<View>(Resource.Id.center);
                SkipView = itemView.FindViewById<View>(Resource.Id.skip);

                StoryBodyLayout = itemView.FindViewById<LinearLayout>(Resource.Id.story_body_layout);
                CaptionStoryTextView = itemView.FindViewById<TextView>(Resource.Id.story_body);

                OpenSeenListLayout = itemView.FindViewById<LinearLayout>(Resource.Id.open_seen_list_layout);
                SeenCounterTextView = itemView.FindViewById<TextView>(Resource.Id.seen_counter);
                IconSeen = itemView.FindViewById<TextView>(Resource.Id.iconSeen);

                ImageBlurView = itemView.FindViewById<ImageView>(Resource.Id.imageBlur);

                OpenReply = itemView.FindViewById<LinearLayout>(Resource.Id.open_reply);
                SendMessagePanel = itemView.FindViewById<LinearLayout>(Resource.Id.send_message_panel);
                InitializingReactImages(itemView);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconSeen, FontAwesomeIcon.Eye);

                //Event
                //ReverseView?.SetOnTouchListener(new MyTouchListener(this));
                //SkipView?.SetOnTouchListener(new MyTouchListener(this));
                //CenterView?.SetOnTouchListener(new MyTouchListener(this));

                if (AppSettings.EnableStorySeenList)
                {
                    OpenSeenListLayout?.SetOnClickListener(this);
                    //new ViewSwipeTouchListener(adapter.ActivityContext, OpenSeenListLayout, new MySwipeListener(this));
                }

                if (AppSettings.EnableReplyStory)
                {
                    SendMessagePanel?.SetOnClickListener(this);
                    //new ViewSwipeTouchListener(adapter.ActivityContext, OpenReply, new MySwipeListener(this));
                }
                else
                {
                    OpenReply.Visibility = ViewStates.Gone;
                }

                ReverseView?.SetOnClickListener(this);
                SkipView?.SetOnClickListener(this);
                CenterView?.SetOnClickListener(this);
                CenterView?.SetOnLongClickListener(this);

                MAdapter.StoryFragment.SetStoryStateListener(new MyStoryStateListener(this));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitializingReactImages(View view)
        {
            try
            {
                MImgButtonOne = view.FindViewById<ImageView>(Resource.Id.imgButtonOne);
                MImgButtonTwo = view.FindViewById<ImageView>(Resource.Id.imgButtonTwo);
                MImgButtonThree = view.FindViewById<ImageView>(Resource.Id.imgButtonThree);
                MImgButtonFour = view.FindViewById<ImageView>(Resource.Id.imgButtonFour);
                MImgButtonFive = view.FindViewById<ImageView>(Resource.Id.imgButtonFive);
                MImgButtonSix = view.FindViewById<ImageView>(Resource.Id.imgButtonSix);

                switch (AppSettings.ReactionTheme)
                {
                    case PostButtonSystem.ReactionDefault:
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_like).Apply(new RequestOptions()).Into(MImgButtonOne);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_love).Apply(new RequestOptions()).Into(MImgButtonTwo);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_haha).Apply(new RequestOptions()).Into(MImgButtonThree);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_wow).Apply(new RequestOptions()).Into(MImgButtonFour);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_sad).Apply(new RequestOptions()).Into(MImgButtonFive);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.gif_angry).Apply(new RequestOptions()).Into(MImgButtonSix);
                        break;
                    case PostButtonSystem.ReactionSubShine:
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.like).Apply(new RequestOptions().FitCenter()).Into(MImgButtonOne);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.love).Apply(new RequestOptions().FitCenter()).Into(MImgButtonTwo);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.haha).Apply(new RequestOptions().FitCenter()).Into(MImgButtonThree);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.wow).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFour);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.sad).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFive);
                        Glide.With(MAdapter.ActivityContext?.BaseContext).Load(Resource.Drawable.angry).Apply(new RequestOptions().FitCenter()).Into(MImgButtonSix);
                        break;
                }

                MImgButtonOne?.SetOnClickListener(this);
                MImgButtonTwo?.SetOnClickListener(this);
                MImgButtonThree?.SetOnClickListener(this);
                MImgButtonFour?.SetOnClickListener(this);
                MImgButtonFive?.SetOnClickListener(this);
                MImgButtonSix?.SetOnClickListener(this);
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
                StoryDataObject.Story dataNowStory = MAdapter.StoryList[BindingAdapterPosition];

                if (v.Id == ReverseView.Id)
                {
                    MAdapter.StoriesProgress?.Reverse();
                }
                else if (v.Id == CenterView.Id || v.Id == SkipView.Id)
                {
                    MAdapter.StoriesProgress?.Skip();
                }
                else if (v.Id == OpenSeenListLayout.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        Paused = true;
                    }

                    if (dataNowStory != null)
                    {
                        StorySeenListFragment bottomSheet = new StorySeenListFragment(MAdapter.StoryFragment);
                        Bundle bundle = new Bundle();
                        bundle.PutString("recipientId", dataNowStory.UserId);
                        bundle.PutString("StoryId", dataNowStory.Id);
                        bundle.PutString("DataNowStory", JsonConvert.SerializeObject(dataNowStory));
                        bottomSheet.Arguments = bundle;
                        bottomSheet.Show(MAdapter.StoryFragment.ChildFragmentManager, bottomSheet.Tag);
                    }
                }
                else if (v.Id == SendMessagePanel.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        Paused = true;
                    }
                    OpenReply.Visibility = ViewStates.Invisible;

                    if (dataNowStory != null)
                    {
                        Intent mIntent = new Intent(MAdapter.StoryFragment.Context, typeof(StoryReplyActivity));
                        mIntent.PutExtra("recipientId", dataNowStory.UserId);
                        mIntent.PutExtra("StoryId", dataNowStory.Id);
                        mIntent.PutExtra("DataNowStory", JsonConvert.SerializeObject(dataNowStory));
                        MAdapter.ActivityContext.StartActivityForResult(mIntent, 5326);
                        MAdapter.ActivityContext.OverridePendingTransition(Resource.Animation.appear, Resource.Animation.disappear);
                    }
                }
                else if (v.Id == MImgButtonOne.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.Like, dataNowStory);
                }
                else if (v.Id == MImgButtonTwo.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.Love, dataNowStory);
                }
                else if (v.Id == MImgButtonThree.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.HaHa, dataNowStory);
                }
                else if (v.Id == MImgButtonFour.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.Wow, dataNowStory);
                }
                else if (v.Id == MImgButtonFive.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.Sad, dataNowStory);
                }
                else if (v.Id == MImgButtonSix.Id)
                {
                    ImgButtonOnClick(v, ReactConstants.Angry, dataNowStory);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private string LastReact;
        private void ImgButtonOnClick(View v, string reactText, StoryDataObject.Story dataNowStory)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(MAdapter.ActivityContext, MAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (LastReact == reactText)
                    return;

                LastReact = reactText;

                if (UserDetails.SoundControl)
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("down.mp3");

                var scale = AnimationUtils.LoadAnimation(MAdapter.ActivityContext, Resource.Animation.react_button_animation);
                v.StartAnimation(scale);

                int resReact = Resource.Drawable.emoji_like;
                dataNowStory.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                if (reactText == ReactConstants.Like)
                {
                    dataNowStory.Reaction.Type = "1";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_like;
                }
                else if (reactText == ReactConstants.Love)
                {
                    dataNowStory.Reaction.Type = "2";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_love;
                }
                else if (reactText == ReactConstants.HaHa)
                {
                    dataNowStory.Reaction.Type = "3";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_haha;
                }
                else if (reactText == ReactConstants.Wow)
                {
                    dataNowStory.Reaction.Type = "4";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_wow;
                }
                else if (reactText == ReactConstants.Sad)
                {
                    dataNowStory.Reaction.Type = "5";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_sad;
                }
                else if (reactText == ReactConstants.Angry)
                {
                    dataNowStory.Reaction.Type = "6";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Story.ReactStoryAsync(dataNowStory.Id, react) });
                    resReact = Resource.Drawable.emoji_angry;
                }

                if (dataNowStory.Reaction.IsReacted != null && !dataNowStory.Reaction.IsReacted.Value)
                {
                    dataNowStory.Reaction.IsReacted = true;
                    dataNowStory.Reaction.Count++;
                }

                new ParticleSystem(MAdapter.ActivityContext, 10, resReact, 3000)
                    .SetSpeedByComponentsRange(-0.1f, 0.1f, -0.1f, 0.02f)
                    .SetAcceleration(0.000003f, 90)
                    .SetInitialRotationRange(0, 360)
                    .SetRotationSpeed(144)
                    .SetFadeOut(2000)
                    .AddModifier(new ScaleModifier(0f, 1.5f, 0, 1500))
                    .OneShot(v, 10);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool OnLongClick(View v)
        {
            try
            {
                if (v.Id == CenterView.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        MenuNavigation(false);
                        Paused = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
            return false;
        }

        #region MediaPlayer 
        public void InitVideoView(View view)
        {
            try
            {
                StoryShowAdapter.PlayerView = view.FindViewById<StyledPlayerView>(Resource.Id.player_video_view);

                StoryShowAdapter.ExoController = new ExoController(MAdapter.ActivityContext, StoryShowAdapter.PlayerView);
                StoryShowAdapter.ExoController.SetPlayer(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayPlayer()
        {
            try
            {
                if (PlayerPaused)
                    StoryShowAdapter.ExoController?.PlayVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PausePlayer()
        {
            try
            {
                StoryShowAdapter.ExoController?.StopVideo();
                PlayerPaused = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Destroy()
        {
            try
            {
                StoryShowAdapter.ExoController?.ReleaseVideo();
                PlayerPaused = true;
                StoryShowAdapter.ExoController = null;

                StoryShowAdapter.PlayerView?.Player?.Stop();
                StoryShowAdapter.PlayerView = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void MenuNavigation(bool show)
        {
            try
            {
                var isOwner = MAdapter.StoryList[BindingAdapterPosition]?.IsOwner ?? false;
                MAdapter.StoryFragment.OnEventMainThread(show);
                if (show)
                {
                    MAdapter.StoryFragment.FadeInAnimation(StoryBodyLayout, 200);
                    MAdapter.StoryFragment.FadeInAnimation(isOwner ? OpenSeenListLayout : OpenReply, 200);
                }
                else
                {
                    MAdapter.StoryFragment.FadeOutAnimation(StoryBodyLayout, 200);
                    MAdapter.StoryFragment.FadeOutAnimation(isOwner ? OpenSeenListLayout : OpenReply, 200);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyStoryStateListener : StoriesProgressView.IStoryStateListener
        {
            private readonly StoryShowAdapterViewHolder Holder;
            public MyStoryStateListener(StoryShowAdapterViewHolder holder)
            {
                Holder = holder;
            }

            public void OnPause()
            {
                try
                {
                    if (!Holder.Paused)
                    {
                        Holder.MAdapter.StoriesProgress?.Pause();
                        Holder.PausePlayer();
                        Holder.Paused = true;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public void OnResume()
            {
                try
                {
                    if (Holder.Paused)
                    {
                        Holder.MAdapter.StoriesProgress?.Resume();
                        Holder.PlayPlayer();
                        Holder.Paused = false;
                    }

                    var item = Holder.MAdapter.StoryList[Holder.BindingAdapterPosition];
                    bool isOwner = item?.IsOwner ?? false;
                    if (isOwner)
                    {
                        Holder.OpenReply.Visibility = ViewStates.Gone;

                        Holder.OpenSeenListLayout.Visibility = ViewStates.Visible;
                        Holder.SeenCounterTextView.Visibility = ViewStates.Visible;
                        Holder.SeenCounterTextView.Text = item.ViewCount;
                    }
                    else
                    {
                        Holder.OpenReply.Visibility = ViewStates.Visible;

                        Holder.OpenSeenListLayout.Visibility = ViewStates.Gone;
                        Holder.SeenCounterTextView.Visibility = ViewStates.Gone;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        private class MyTouchListener : Java.Lang.Object, View.IOnTouchListener
        {
            private readonly StoryShowAdapterViewHolder Holder;
            public MyTouchListener(StoryShowAdapterViewHolder holder)
            {
                Holder = holder;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                try
                {
                    switch (e.Action)
                    {
                        case MotionEventActions.Down:
                            Holder.PressTime = Methods.Time.CurrentTimeMillis();

                            return false;

                        case MotionEventActions.Up:
                            long now = Methods.Time.CurrentTimeMillis();
                            if (Holder.Paused)
                            {
                                Holder.MAdapter.StoriesProgress?.Resume();
                                Holder.PlayPlayer();
                                Holder.MenuNavigation(true);
                                Holder.Paused = false;
                            }

                            return Holder.Limit < now - Holder.PressTime;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
                return false;
            }
        }

        private class MySwipeListener : Java.Lang.Object, ViewSwipeTouchListener.IOnSwipeListener
        {
            private readonly StoryShowAdapterViewHolder Holder;
            public MySwipeListener(StoryShowAdapterViewHolder holder)
            {
                Holder = holder;
            }

            public void Swipe(View v, ViewSwipeTouchListener.SwipeType type)
            {
                try
                {
                    if (type == ViewSwipeTouchListener.SwipeType.Top)
                    {
                        if (Holder.BindingAdapterPosition >= 0)
                        {
                            if (!Holder.Paused)
                            {
                                Holder.MAdapter.StoriesProgress?.Pause();
                                Holder.PausePlayer();
                                Holder.Paused = true;
                            }

                            Holder.OpenReply.Visibility = ViewStates.Invisible;

                            var dataNowStory = Holder.MAdapter.StoryList[Holder.BindingAdapterPosition];
                            if (dataNowStory != null)
                            {
                                if (dataNowStory.IsOwner)
                                {
                                    //Show list
                                    StorySeenListFragment bottomSheet = new StorySeenListFragment(Holder.MAdapter.StoryFragment);
                                    Bundle bundle = new Bundle();
                                    bundle.PutString("recipientId", dataNowStory.UserId);
                                    bundle.PutString("StoryId", dataNowStory.Id);
                                    bundle.PutString("DataNowStory", JsonConvert.SerializeObject(dataNowStory));
                                    bottomSheet.Arguments = bundle;
                                    bottomSheet.Show(Holder.MAdapter.StoryFragment.ChildFragmentManager, bottomSheet.Tag);
                                }
                                else
                                {
                                    Intent mIntent = new Intent(Holder.MAdapter.StoryFragment.Context, typeof(StoryReplyActivity));
                                    mIntent.PutExtra("recipientId", dataNowStory.UserId);
                                    mIntent.PutExtra("StoryId", dataNowStory.Id);
                                    mIntent.PutExtra("DataNowStory", JsonConvert.SerializeObject(dataNowStory));
                                    Holder.MAdapter.ActivityContext.StartActivityForResult(mIntent, 5326);
                                    Holder.MAdapter.ActivityContext.OverridePendingTransition(Resource.Animation.appear, Resource.Animation.disappear);
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
        }
    }
}