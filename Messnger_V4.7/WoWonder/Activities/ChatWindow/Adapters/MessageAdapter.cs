using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content.Res;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.IO;
using Java.Util;
using WoWonder.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Story;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.ChatWindow.Adapters
{
    public class MessageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<Holders.MesClickEventArgs> DownloadItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ErrorLoadingItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ItemLongClick;

        public ObservableCollection<AdapterModelsClassMessage> DifferList = new ObservableCollection<AdapterModelsClassMessage>();

        public readonly Activity ActivityContext;
        private readonly RequestOptions OptionsRoundedCrop;
        public readonly string Id; // to_id 
        private readonly bool ShowGroup;

        public int PositionSound;
        public MessageDataExtra MusicBarMessageData;

        public MessageAdapter(Activity activity, string userid, bool showGroup)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
                Id = userid;
                ShowGroup = showGroup;
                DifferList = new ObservableCollection<AdapterModelsClassMessage>();

                OptionsRoundedCrop = GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)MessageModelType.RightProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return viewHolder;
                        }
                    case (int)MessageModelType.RightText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.LeftText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.RightGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftImage:
                    case (int)MessageModelType.LeftGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightAudio:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Audio, parent, false);
                            Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowGroup);
                            return soundViewHolder;
                        }
                    case (int)MessageModelType.LeftAudio:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Audio, parent, false);
                            Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowGroup);
                            return soundViewHolder;
                        }
                    case (int)MessageModelType.RightContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowGroup);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.LeftContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowGroup);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.RightVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.LeftVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnLongClick, OnDownloadClick, OnErrorLoadingClick, ShowGroup);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.RightSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.LeftSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.RightFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowGroup);
                            return viewHolder;
                        }
                    default:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.NotSupportedViewHolder viewHolder = new Holders.NotSupportedViewHolder(row);
                            return viewHolder;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            try
            {
                var item = DifferList[position];

                var itemViewType = vh.ItemViewType;
                switch (itemViewType)
                {
                    case (int)MessageModelType.RightProduct:
                    case (int)MessageModelType.LeftProduct:
                        {
                            Holders.ProductViewHolder holder = vh as Holders.ProductViewHolder;
                            LoadProductOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightGif:
                    case (int)MessageModelType.LeftGif:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadGifOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightText:
                    case (int)MessageModelType.LeftText:
                        {
                            Holders.TextViewHolder holder = vh as Holders.TextViewHolder;
                            LoadTextOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.LeftImage:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadImageOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightMap:
                    case (int)MessageModelType.LeftMap:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadMapOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightAudio:
                    case (int)MessageModelType.LeftAudio:
                        {
                            Holders.SoundViewHolder holder = vh as Holders.SoundViewHolder;
                            LoadAudioOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightContact:
                    case (int)MessageModelType.LeftContact:
                        {
                            Holders.ContactViewHolder holder = vh as Holders.ContactViewHolder;
                            LoadContactOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightVideo:
                    case (int)MessageModelType.LeftVideo:
                        {
                            Holders.VideoViewHolder holder = vh as Holders.VideoViewHolder;
                            LoadVideoOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightSticker:
                    case (int)MessageModelType.LeftSticker:
                        {
                            Holders.StickerViewHolder holder = vh as Holders.StickerViewHolder;
                            LoadStickerOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightFile:
                    case (int)MessageModelType.LeftFile:
                        {
                            Holders.FileViewHolder holder = vh as Holders.FileViewHolder;
                            LoadFileOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(item.MesData.Text) || !string.IsNullOrWhiteSpace(item.MesData.Text))
                            {
                                if (vh is Holders.TextViewHolder holderText)
                                {
                                    LoadTextOfChatItem(holderText, position, item.MesData);
                                }
                            }
                            else
                            {
                                if (vh is Holders.NotSupportedViewHolder holder)
                                    holder.AutoLinkNotsupportedView.Text = ActivityContext.GetText(Resource.String.Lbl_TextChatNotSupported);
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = DifferList[position];
                    switch (payloads[0].ToString())
                    {
                        case "WithoutBlobImage":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadImageOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobGIF":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadGifOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobSticker":
                            {
                                if (viewHolder is Holders.StickerViewHolder holder)
                                    LoadStickerOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobMap":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadMapOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobVideo":
                            {
                                if (viewHolder is Holders.VideoViewHolder holder)
                                    LoadVideoOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobAudio":
                            {
                                if (viewHolder is Holders.SoundViewHolder holder)
                                    LoadAudioOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobFile":
                            {
                                if (viewHolder is Holders.FileViewHolder holder)
                                    LoadFileOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobUploadProgress":
                            {
                                if (viewHolder is Holders.ImageViewHolder imageViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        imageViewHolder.LoadingProgress.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        imageViewHolder.LoadingProgress.Progress = item.MesData.MessageProgress;
                                }
                                else if (viewHolder is Holders.VideoViewHolder videoViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        videoViewHolder.LoadingProgress.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        videoViewHolder.LoadingProgress.Progress = item.MesData.MessageProgress;
                                }
                                else if (viewHolder is Holders.SoundViewHolder soundViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        soundViewHolder.LoadingProgress.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        soundViewHolder.LoadingProgress.Progress = item.MesData.MessageProgress;
                                }
                                //NotifyItemChanged(position);
                                break;
                            }
                        default:
                            base.OnBindViewHolder(viewHolder, position, payloads);
                            break;
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        #region Function

        //Reply Story Messages
        private void ReplyStoryItems(Holders.RepliedMessageView holder, StoryDataObject.Story story)
        {
            try
            {
                if (!string.IsNullOrEmpty(story?.Id))
                {
                    holder.RepliedMessageLayout.Visibility = ViewStates.Visible;
                    holder.TxtOwnerName.Text = ActivityContext.GetText(Resource.String.Lbl_Story);

                    holder.MessageFileThumbnail.Visibility = ViewStates.Visible;

                    var mediaFile = !story.Thumbnail.Contains("avatar") && story.Videos.Count == 0 ? story.Thumbnail : story.Videos[0].Filename;
                    var fileName = mediaFile.Split('/').Last();

                    var typeView = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                    switch (typeView)
                    {
                        case "Video":
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.video);

                                var fileNameWithoutExtension = fileName.Split('.').First();

                                if (!string.IsNullOrEmpty(mediaFile))
                                {
                                    GlideImageLoader.LoadImage(ActivityContext, mediaFile, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                }
                                else
                                {
                                    var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                                    if (videoImage == "File Dont Exists")
                                    {
                                        File file2 = new File(mediaFile);
                                        try
                                        {
                                            Uri photoUri = mediaFile.Contains("http") ? Uri.Parse(mediaFile) : FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                            Glide.With(ActivityContext?.BaseContext)
                                                .AsBitmap()
                                                .Apply(OptionsRoundedCrop)
                                                .Load(photoUri) // or URI/path
                                                .Into(holder.MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(ActivityContext?.BaseContext)
                                                .AsBitmap()
                                                .Apply(OptionsRoundedCrop)
                                                .Load(file2) // or URI/path
                                                .Into(holder.MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                    }
                                    else
                                    {
                                        GlideImageLoader.LoadImage(ActivityContext, videoImage, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                    }
                                }
                                break;
                            }
                        case "Image":
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.image);

                                GlideImageLoader.LoadImage(ActivityContext, mediaFile, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
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

        //Reply Messages
        private void ReplyItems(Holders.RepliedMessageView holder, MessageModelType typeView, MessageDataExtra messageData, int position)
        {
            try
            {
                var dataReply = messageData.Reply?.ReplyClass;
                if (!string.IsNullOrEmpty(dataReply?.Id))
                {
                    holder.RepliedMessageLayout.Visibility = ViewStates.Visible;
                    holder.RepliedMessageLayout.Click += (sender, args) =>
                    {
                        try
                        {
                            var data = DifferList.FirstOrDefault(a => a.MesData.Id == dataReply.Id);
                            var index = DifferList.IndexOf(data);

                            ChatWindowActivity.GetInstance()?.MRecycler?.ScrollToPosition(index);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    };

                    if (dataReply.MessageUser?.UserDataClass?.UserId == UserDetails.UserId)
                        holder.TxtOwnerName.Text = ActivityContext.GetText(Resource.String.Lbl_You);
                    else
                    {
                        if (!string.IsNullOrEmpty(dataReply.MessageUser?.UserDataClass?.Name))
                        {
                            holder.TxtOwnerName.Text = dataReply.MessageUser?.UserDataClass?.Name;
                        }
                        else
                        {
                            var name = ListUtils.UserList?.FirstOrDefault(a => a.UserId == Id)?.Name ?? "";
                            holder.TxtOwnerName.Text = name;
                        }
                    }

                    //holder.MessageFileThumbnail.Visibility = ViewStates.Visible;

                    var fileName = dataReply.Media.Split('/').Last();
                    var fileNameExtension = fileName.Split('.').Last();
                    var fileNameWithoutExtension = fileName.Split('.').First();

                    switch (typeView)
                    {
                        case MessageModelType.LeftVideo:
                        case MessageModelType.RightVideo:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.video);

                                if (!string.IsNullOrEmpty(dataReply.ImageVideo))
                                {
                                    GlideImageLoader.LoadImage(ActivityContext, dataReply.ImageVideo, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                }
                                else
                                {
                                    var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                                    if (videoImage == "File Dont Exists")
                                    {
                                        File file2 = new File(dataReply.Media);
                                        try
                                        {
                                            Uri photoUri = dataReply.Media.Contains("http") ? Uri.Parse(dataReply.Media) : FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                            Glide.With(ActivityContext?.BaseContext)
                                                .AsBitmap()
                                                .Apply(OptionsRoundedCrop)
                                                .Load(photoUri) // or URI/path
                                                .Into(new MySimpleTarget(this, holder.MessageFileThumbnail, position));  //image view to set thumbnail to 
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(ActivityContext?.BaseContext)
                                                .AsBitmap()
                                                .Apply(OptionsRoundedCrop)
                                                .Load(file2) // or URI/path
                                                .Into(new MySimpleTarget(this, holder.MessageFileThumbnail, position));  //image view to set thumbnail to 
                                        }
                                    }
                                    else
                                    {
                                        GlideImageLoader.LoadImage(ActivityContext, videoImage, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                    }
                                }
                                break;
                            }
                        case MessageModelType.LeftGif:
                        case MessageModelType.RightGif:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.Lbl_Gif);

                                GlideImageLoader.LoadImage(ActivityContext, dataReply.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                                break;
                            }
                        case MessageModelType.LeftSticker:
                        case MessageModelType.RightSticker:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.Lbl_Sticker);

                                GlideImageLoader.LoadImage(ActivityContext, dataReply.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                                break;
                            }
                        case MessageModelType.LeftImage:
                        case MessageModelType.RightImage:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.image);

                                GlideImageLoader.LoadImage(ActivityContext, dataReply.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                                break;
                            }
                        case MessageModelType.LeftAudio:
                        case MessageModelType.RightAudio:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.Lbl_VoiceMessage) + " (" + dataReply.MediaDuration + ")";
                                GlideImageLoader.LoadImage(ActivityContext, "Audio_File", holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                break;
                            }
                        case MessageModelType.LeftFile:
                        case MessageModelType.RightFile:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Visible;
                                holder.TxtMessageType.Text = ActivityContext.GetText(Resource.String.Lbl_File);
                                holder.TxtShortMessage.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                //FullGlideRequestBuilder.Load(MainActivity.GetDrawable(Resource.Drawable.Image_File)).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                break;
                            }
                        case MessageModelType.LeftMap:
                        case MessageModelType.RightMap:
                            {
                                holder.TxtShortMessage.Visibility = ViewStates.Gone;
                                holder.TxtMessageType.Text = ActivityContext.GetText(Resource.String.Lbl_Location);
                                //FullGlideRequestBuilder.Load(message.MessageMap).Apply(new RequestOptions().Placeholder(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map)).Into(holder.MessageFileThumbnail);
                                break;
                            }
                        case MessageModelType.LeftContact:
                        case MessageModelType.RightContact:
                            {
                                holder.TxtMessageType.Text = ActivityContext.GetText(Resource.String.Lbl_Contact);
                                holder.TxtShortMessage.Text = dataReply.ContactName;
                                //FullGlideRequestBuilder.Load(Resource.Drawable.no_profile_image).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                break;
                            }
                        case MessageModelType.LeftProduct:
                        case MessageModelType.RightProduct:
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = ActivityContext.GetText(Resource.String.Lbl_Product);
                                string imageUrl = !string.IsNullOrEmpty(dataReply.Media) ? dataReply.Media : dataReply.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                                GlideImageLoader.LoadImage(ActivityContext, imageUrl, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                break;
                            }
                        case MessageModelType.LeftText:
                        case MessageModelType.RightText:
                            {
                                holder.MessageFileThumbnail.Visibility = ViewStates.Gone;
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = dataReply.Text;
                                break;
                            }
                        case MessageModelType.None:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Reaction Messages
        private void ReactionItems(Holders.GlobalMessageViews holder, MessageDataExtra message)
        {
            try
            {
                if (!AppSettings.EnableReactionMessageSystem)
                {
                    holder.ImageCountLike.Visibility = ViewStates.Gone;
                    return;
                }

                if (message.Reaction != null)
                {
                    if (message.Reaction.IsReacted != null && message.Reaction.IsReacted.Value)
                    {
                        if (!string.IsNullOrEmpty(message.Reaction.Type))
                        {
                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == message.Reaction.Type).Value?.Id ?? "";
                            switch (react)
                            {
                                case "1":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                    break;
                                case "2":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                    break;
                                case "3":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                    break;
                                case "4":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                    break;
                                case "5":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                    break;
                                case "6":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                    break;
                                default:
                                    if (message.Reaction.Count > 0)
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                    break;
                            }
                            holder.ImageCountLike.Visibility = ViewStates.Visible;
                        }
                    }
                    else
                    {
                        if (message.Reaction.Count > 0)
                        {
                            if (message.Reaction.Like != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            }
                            else if (message.Reaction.Love != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                            }
                            else if (message.Reaction.HaHa != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                            }
                            else if (message.Reaction.Wow != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                            }
                            else if (message.Reaction.Sad != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                            }
                            else if (message.Reaction.Angry != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                            }
                            else
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            }
                            holder.ImageCountLike.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            holder.ImageCountLike.Visibility = ViewStates.Gone;
                        }
                    }
                }
                else
                {
                    holder.ImageCountLike.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStartedMessage(Holders.GlobalMessageViews holder, string star)
        {
            try
            {
                if (star == "yes")
                {
                    holder.StarIcon.PlayAnimation();
                    holder.StarIcon.Visibility = ViewStates.Visible;
                    holder.StarImage.SetImageResource(Resource.Drawable.icon_star_filled_vector);
                    holder.StarImage.Visibility = ViewStates.Visible;
                    holder.StarLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.StarIcon.Progress = 0;
                    holder.StarIcon.CancelAnimation();
                    holder.StarIcon.Visibility = ViewStates.Gone;
                    //holder.StarImage.SetImageResource(Resource.Drawable.icon_star_vector);
                    holder.StarImage.Visibility = ViewStates.Gone;
                    holder.StarLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetSeenMessage(ImageView view, string seen)
        {
            try
            {
                switch (seen)
                {
                    //Clock
                    case "-1":
                        view.SetImageResource(Resource.Drawable.icon_clock_vector);
                        break;
                    //Check
                    case "0":
                        view.SetImageResource(Resource.Drawable.icon_done_vector);
                        break;
                    default:
                        {
                            if (seen != "-1" && seen != "0" && !string.IsNullOrEmpty(seen)) //CheckDouble
                            {
                                view.SetImageResource(Resource.Drawable.icon_tick_vector);
                            }

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

        private void LoadGlobalOfChatItem(Holders.GlobalMessageViews holder, int position, MessageDataExtra message)
        {
            try
            {
                if (message.Position == "right")
                    holder.Time.Text = message.TimeText;
                else
                {
                    if (ShowGroup)
                        holder.Time.Text = "• " + message.TimeText;
                    else
                        holder.Time.Text = message.TimeText;
                }

                //Group
                if (holder.UserName != null && ShowGroup)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;

                    GlideImageLoader.LoadImage(ActivityContext, message.UserData.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowGroup)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);
                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            //await Task.Run(() =>
                            //{
                            //    try
                            //    {
                            //        Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(ActivityContext, message.ChatColor);
                            //        if (drawableByTheme != null)
                            //            ActivityContext?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                            //        else
                            //            ActivityContext?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                            //    }
                            //    catch (Exception e)
                            //    {
                            //        Methods.DisplayReportResultTrack(e);
                            //    }
                            //});
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowGroup)
                    SetStartedMessage(holder, message.Fav);

                holder.ForwardLayout.Visibility = message.Forward != null && message.Forward.Value == 1 ? ViewStates.Visible : ViewStates.Gone;

                ReactionItems(holder, message);

                if (message.Story?.StoryClass?.Id != null && !string.IsNullOrEmpty(message.StoryId) && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                    var px = PixelUtil.DpToPx(ActivityContext, 150);
                    holder.BubbleLayout.SetMinimumWidth(px);
                }
                else
                {
                    if (message.Reply?.ReplyClass?.Id != null && !string.IsNullOrEmpty(message.ReplyId) && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                        var px = PixelUtil.DpToPx(ActivityContext, 150);
                        holder.BubbleLayout.SetMinimumWidth(px);
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                        holder.BubbleLayout.SetMinimumWidth(0);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadTextOfChatItem(Holders.TextViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                TextSanitizer textSanitizer = new TextSanitizer(holder.SuperTextView, ActivityContext);
                string lastText = message.Text.Replace(" /", " ");
                textSanitizer.Load(lastText, message.Position);

                if (AppSettings.EnableFitchOgLink)
                {
                    if (message.FitchOgLink?.Count > 0)
                    {
                        var url = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "url").Value ?? "";

                        if (!string.IsNullOrEmpty(url))
                        {
                            var title = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "title").Value ?? "";
                            var description = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "description").Value ?? "";
                            var image = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "image").Value ?? "";

                            var prepareUrl = url.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
                            holder.OgLinkMessageView.OgLinkUrl.Text = prepareUrl?.ToUpper();
                            holder.OgLinkMessageView.OgLinkTitle.Text = title;
                            holder.OgLinkMessageView.OgLinkDescription.Text = description;

                            GlideImageLoader.LoadImage(ActivityContext, image, holder.OgLinkMessageView.OgLinkImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                            if (message.Position == "right")
                            {
                                holder.OgLinkMessageView.OgLinkTitle.SetTextColor(Color.ParseColor("#ffffff"));
                                holder.OgLinkMessageView.OgLinkUrl.SetTextColor(Color.ParseColor("#efefef"));
                                holder.OgLinkMessageView.OgLinkDescription.SetTextColor(Color.ParseColor("#efefef"));
                            }

                            holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Visible;
                            holder.OgLinkMessageView.OgLinkContainerLayout.Click += (sender, args) =>
                            {
                                try
                                {
                                    new IntentController(ActivityContext).OpenBrowserFromApp(url);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };
                        }
                    }
                    else
                    {
                        //Check if find website in text 
                        foreach (Match itemLink in Regex.Matches(message.Text, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            Console.WriteLine(itemLink.Value);
                            message.FitchOgLink = await Methods.OgLink.FitchOgLink(itemLink.Value);
                            break;
                        }

                        if (message.FitchOgLink?.Count > 0)
                        {
                            var url = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "url").Value ?? "";

                            if (!string.IsNullOrEmpty(url))
                            {
                                var title = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "title").Value ?? "";
                                var description = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "description").Value ?? "";
                                var image = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "image").Value ?? "";

                                var prepareUrl = url.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
                                holder.OgLinkMessageView.OgLinkUrl.Text = prepareUrl?.ToUpper();
                                holder.OgLinkMessageView.OgLinkTitle.Text = title;
                                holder.OgLinkMessageView.OgLinkDescription.Text = description;

                                GlideImageLoader.LoadImage(ActivityContext, image, holder.OgLinkMessageView.OgLinkImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                                if (message.Position == "right")
                                {
                                    holder.OgLinkMessageView.OgLinkTitle.SetTextColor(Color.ParseColor("#ffffff"));
                                    holder.OgLinkMessageView.OgLinkUrl.SetTextColor(Color.ParseColor("#efefef"));
                                    holder.OgLinkMessageView.OgLinkDescription.SetTextColor(Color.ParseColor("#efefef"));
                                }

                                holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Visible;
                                holder.OgLinkMessageView.OgLinkContainerLayout.Click += (sender, args) =>
                                {
                                    try
                                    {
                                        new IntentController(ActivityContext).OpenBrowserFromApp(url);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                };
                            }
                        }
                        else
                            holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadMapOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (!update)
                {
                    LatLng latLng = new LatLng(Convert.ToDouble(message.Lat), Convert.ToDouble(message.Lng));

                    var addresses = await WoWonderTools.ReverseGeocodeCurrentLocation(latLng);
                    if (addresses != null)
                    {
                        ActivityContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var deviceAddress = addresses.GetAddressLine(0);

                                string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                                //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                                imageUrlMap += "center=" + deviceAddress;
                                imageUrlMap += "&zoom=13";
                                imageUrlMap += "&scale=2";
                                imageUrlMap += "&size=150x150";
                                imageUrlMap += "&maptype=roadmap";
                                imageUrlMap += "&key=" + ActivityContext.GetText(Resource.String.google_maps_key);
                                imageUrlMap += "&format=png";
                                imageUrlMap += "&visual_refresh=true";
                                imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + deviceAddress;

                                Glide.With(ActivityContext?.BaseContext).Load(imageUrlMap).Apply(OptionsRoundedCrop).Fallback(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map).Into(holder.ImageView);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    else
                    {
                        //default image 
                        Glide.With(ActivityContext?.BaseContext).Load(Resource.Drawable.Image_Map).Apply(OptionsRoundedCrop).Fallback(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map).Into(holder.ImageView);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadImageOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (!update)
                {
                    if (message.Media.Contains("http"))
                    {
                        message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("image");
                        GlideImageLoader.LoadImage(ActivityContext, message.Media, holder.ImageView, message.BtnDownload ? ImageStyle.RoundedCrop : ImageStyle.Blur, ImagePlaceholders.Drawable);
                    }
                    else
                    {
                        message.BtnDownload = true;
                        
                        GlideImageLoader.LoadImage(ActivityContext, message.Media, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                        //var file = Uri.FromFile(new File(message.Media));
                        //GlideImageLoader.LoadImage(ActivityContext, file.Path, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable); 
                    }
                }

                if (message.SendFile)
                {
                    holder.LoadingProgress.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.LoadingProgress.Visibility = ViewStates.Gone;
                        holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadProductOfChatItem(Holders.ProductViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                string imageUrl = !string.IsNullOrEmpty(message.Media) ? message.Media : message.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                GlideImageLoader.LoadImage(ActivityContext, imageUrl, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                holder.Title.Text = message.Product?.ProductClass?.Name;
                holder.Cat.Text = ListUtils.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == message.Product?.ProductClass?.Category)?.CategoriesName;

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(message.Product?.ProductClass?.Currency);
                holder.Price.Text = currencyIcon + " " + message.Product?.ProductClass?.Price;
                Console.WriteLine(currency);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadAudioOfChatItem(Holders.SoundViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (message.SendFile)
                {
                    holder.LoadingProgress.IndeterminateDrawable?.SetColorFilter(new PorterDuffColorFilter(Color.White, PorterDuff.Mode.Multiply));
                    holder.LoadingProgress.Visibility = ViewStates.Visible;
                    holder.PlayButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.PlayButton.Visibility = ViewStates.Visible;
                }

                if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                {
                    var duration = WoWonderTools.GetDuration(message.Media);
                    holder.DurationTextView.Text = message.MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    holder.DurationTextView.Text = message.MediaDuration;

                if (message.MediaIsPlaying)
                {
                    holder.PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                    //if (message.ModelType == MessageModelType.LeftAudio)
                    //    holder.PlayButton.ImageTintList = ColorStateList.ValueOf(WoWonderTools.IsTabDark() ? Color.ParseColor("#efefef") : Color.ParseColor("#444444"));
                    //else
                    //    holder.PlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                }
                else
                {
                    holder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadContactOfChatItem(Holders.ContactViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                holder.ContactName.Text = !string.IsNullOrEmpty(message.ContactName) ? message.ContactName : "Contact Number";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadVideoOfChatItem(Holders.VideoViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (message.Media.Contains("http"))
                {
                    message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("video");
                    if (!message.BtnDownload)
                    {
                        Glide.With(ActivityContext?.BaseContext)
                            .AsBitmap()
                            .Apply(GlideImageLoader.GetOptions(ImageStyle.BlurRounded,ImagePlaceholders.Drawable))
                            .Load(message.Media) // or URI/path
                            .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to
                    }
                    else
                    {
                        Glide.With(ActivityContext?.BaseContext)
                            .AsBitmap()
                            .Apply(OptionsRoundedCrop)
                            .Load(message.Media) // or URI/path
                            .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to
                    }
                }
                else
                {
                    message.BtnDownload = true;
                    if (!string.IsNullOrEmpty(message.ImageVideo))
                    {
                        GlideImageLoader.LoadImage(ActivityContext, message.ImageVideo, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                    }
                    else
                    {
                        var fileName = message.Media.Split('/').Last();
                        var fileNameWithoutExtension = fileName.Split('.').First();

                        var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                        if (videoImage == "File Dont Exists")
                        {
                            File file2 = new File(message.Media);
                            try
                            {
                                Uri photoUri = message.Media.Contains("http") ? Uri.Parse(message.Media) : FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                Glide.With(ActivityContext?.BaseContext)
                                    .AsBitmap()
                                    .Apply(OptionsRoundedCrop)
                                    .Load(photoUri) // or URI/path
                                    .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                                Glide.With(ActivityContext?.BaseContext)
                                    .AsBitmap()
                                    .Apply(OptionsRoundedCrop)
                                    .Load(file2) // or URI/path
                                    .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to 
                            }
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(ActivityContext, videoImage, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                        }
                    }
                }

                if (message.SendFile)
                {
                    holder.LoadingProgress.Visibility = ViewStates.Visible;
                    holder.PlayButton.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.PlayButton.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.PlayButton.Visibility = ViewStates.Gone;
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.PlayButton.Visibility = ViewStates.Gone;
                        holder.LoadingProgress.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySimpleTarget : CustomTarget
        {
            private readonly MessageAdapter MAdapter;
            private readonly ImageView Image;
            private readonly int Position;
            public MySimpleTarget(MessageAdapter adapter, ImageView view, int position)
            {
                try
                {
                    MAdapter = adapter;
                    Image = view;
                    Position = position;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                MAdapter?.ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (MAdapter.DifferList?.Count > 0)
                        {
                            var item = MAdapter.DifferList[Position].MesData;
                            if (item != null)
                            {
                                var fileName = item.Media.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();

                                var pathImage = Methods.Path.FolderDiskVideo + MAdapter.Id + "/" + fileNameWithoutExtension + ".png";

                                if (resource is Bitmap bitmap)
                                {
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmap, fileNameWithoutExtension, Methods.Path.FolderDiskVideo + MAdapter.Id + "/");

                                    //File file2 = new File(pathImage);
                                    //var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                    Glide.With(MAdapter.ActivityContext?.BaseContext).Load(bitmap).Apply(MAdapter.OptionsRoundedCrop).Into(Image);

                                    item.ImageVideo = pathImage;
                                }

                                //var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + MAdapter.Id, fileNameWithoutExtension + ".png");
                                //if (videoImage == "File Dont Exists")
                                //{

                                //}
                                //else
                                //{
                                //    File file2 = new File(pathImage);
                                //    var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                //    Glide.With(MAdapter.ActivityContext).Load(photoUri).Apply(MAdapter.OptionsRoundedCrop).Into(Image);

                                //    item.ImageVideo = photoUri.ToString();
                                //}
                                //MAdapter.NotifyItemChanged(Position);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }

            public override void OnLoadCleared(Drawable p0) { }
        }

        private void LoadStickerOfChatItem(Holders.StickerViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (!update)
                {
                    GlideImageLoader.LoadImage(ActivityContext, message.Media, holder.ImageView, message.BtnDownload ? ImageStyle.RoundedCrop : ImageStyle.Blur, ImagePlaceholders.Drawable);

                    if (message.Media.Contains("http"))
                    {
                        Glide.With(ActivityContext?.BaseContext).Load(message.Media).Apply(new RequestOptions()).Into(holder.ImageView);
                    }
                    else
                    {
                        var file = Uri.FromFile(new File(message.Media));
                        Glide.With(ActivityContext?.BaseContext).Load(file?.Path).Apply(new RequestOptions()).Into(holder.ImageView);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFileOfChatItem(Holders.FileViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                var fileName = message.Media.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();
                var fileNameExtension = fileName.Split('.').Last();

                holder.FileName.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;

                if (message.Media.Contains("http"))
                    message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimFile, fileName, message.Media, "file");

                if (!message.Media.Contains("http"))
                    holder.SizeFile.Text = message.FileSize = Methods.FunString.Format_byte_size(message.Media); // message.FileSize;
                else
                    holder.SizeFile.Text = message.FileSize;

                //if (fileNameExtension.Contains("rar") || fileNameExtension.Contains("RAR") || fileNameExtension.Contains("zip") || fileNameExtension.Contains("ZIP"))
                //{
                //      //ZipBox
                //}
                //else if (fileNameExtension.Contains("txt") || fileNameExtension.Contains("TXT"))
                //{
                //     //NoteText
                //}
                //else if (fileNameExtension.Contains("docx") || fileNameExtension.Contains("DOCX") || fileNameExtension.Contains("doc") || fileNameExtension.Contains("DOC"))
                //{
                //     //FileWord
                //}
                //else if (fileNameExtension.Contains("pdf") || fileNameExtension.Contains("PDF"))
                //{
                //      //FilePdf
                //}
                //else if (fileNameExtension.Contains("apk") || fileNameExtension.Contains("APK"))
                //{
                //   //Fileandroid
                //}
                //else
                //{
                //     //file
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadGifOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                LoadGlobalOfChatItem(holder, position, message);

                if (message.Media != null && message.Media.Contains("http"))
                {
                    message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("image");
                    GlideImageLoader.LoadImage(ActivityContext, message.Media, holder.ImageView, message.BtnDownload ? ImageStyle.RoundedCrop : ImageStyle.Blur, ImagePlaceholders.Drawable);
                }
                else
                {
                    message.BtnDownload = true;
                    GlideImageLoader.LoadImage(ActivityContext, message.Media, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                }

                if (message.SendFile)
                {
                    // holder.LoadingProgress.Indeterminate = true;
                    holder.LoadingProgress.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    //holder.LoadingProgress.Indeterminate = false;
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.LoadingProgress.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.LoadingProgress.Visibility = ViewStates.Gone;
                        holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override int ItemCount => DifferList?.Count ?? 0;

        public AdapterModelsClassMessage GetItem(int position)
        {
            var item = DifferList[position];
            return item;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = DifferList[position];
                if (item == null)
                    return (int)MessageModelType.None;

                switch (item.TypeView)
                {
                    case MessageModelType.RightProduct:
                        return (int)MessageModelType.RightProduct;
                    case MessageModelType.LeftProduct:
                        return (int)MessageModelType.LeftProduct;
                    case MessageModelType.RightGif:
                        return (int)MessageModelType.RightGif;
                    case MessageModelType.LeftGif:
                        return (int)MessageModelType.LeftGif;
                    case MessageModelType.RightText:
                        return (int)MessageModelType.RightText;
                    case MessageModelType.LeftText:
                        return (int)MessageModelType.LeftText;
                    case MessageModelType.RightImage:
                        return (int)MessageModelType.RightImage;
                    case MessageModelType.LeftImage:
                        return (int)MessageModelType.LeftImage;
                    case MessageModelType.RightAudio:
                        return (int)MessageModelType.RightAudio;
                    case MessageModelType.LeftAudio:
                        return (int)MessageModelType.LeftAudio;
                    case MessageModelType.RightContact:
                        return (int)MessageModelType.RightContact;
                    case MessageModelType.LeftContact:
                        return (int)MessageModelType.LeftContact;
                    case MessageModelType.RightVideo:
                        return (int)MessageModelType.RightVideo;
                    case MessageModelType.LeftVideo:
                        return (int)MessageModelType.LeftVideo;
                    case MessageModelType.RightSticker:
                        return (int)MessageModelType.RightSticker;
                    case MessageModelType.LeftSticker:
                        return (int)MessageModelType.LeftSticker;
                    case MessageModelType.RightFile:
                        return (int)MessageModelType.RightFile;
                    case MessageModelType.LeftFile:
                        return (int)MessageModelType.LeftFile;
                    case MessageModelType.RightMap:
                        return (int)MessageModelType.RightMap;
                    case MessageModelType.LeftMap:
                        return (int)MessageModelType.LeftMap;
                    default:
                        return (int)MessageModelType.None;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (int)MessageModelType.None;
            }
        }

        void OnDownloadClick(Holders.MesClickEventArgs args) => DownloadItemClick?.Invoke(this, args);
        void OnErrorLoadingClick(Holders.MesClickEventArgs args) => ErrorLoadingItemClick?.Invoke(this, args);
        void OnClick(Holders.MesClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(Holders.MesClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = DifferList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                string imageUrl = "";

                switch (item.TypeView)
                {
                    case MessageModelType.LeftProduct:
                    case MessageModelType.RightProduct:
                        imageUrl = !string.IsNullOrEmpty(item.MesData.Media) ? item.MesData.Media : item.MesData.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                        break;
                    case MessageModelType.RightGif:
                    case MessageModelType.LeftGif:
                        if (!string.IsNullOrEmpty(item.MesData.Stickers))
                            imageUrl = item.MesData.Stickers;
                        else if (!string.IsNullOrEmpty(item.MesData.Media))
                            imageUrl = item.MesData.Media;
                        else if (!string.IsNullOrEmpty(item.MesData.MediaFileName))
                            imageUrl = item.MesData.MediaFileName;

                        string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200" }, StringSplitOptions.RemoveEmptyEntries);
                        var lastFileName = fileName.Last();
                        var name = fileName[3] + lastFileName;
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, name, imageUrl, "image") : item.MesData.Media;
                        break;
                    case MessageModelType.RightImage:
                    case MessageModelType.LeftImage:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, item.MesData.Media.Split('/').Last(), item.MesData.Media, "image") : item.MesData.Media;
                        break;
                    case MessageModelType.RightVideo:
                    case MessageModelType.LeftVideo:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDcimVideo, item.MesData.Media.Split('/').Last(), imageUrl, "video") : item.MesData.Media;
                        break;
                    case MessageModelType.RightSticker:
                    case MessageModelType.LeftSticker:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, item.MesData.Media.Split('/').Last(), item.MesData.Media, "sticker") : item.MesData.Media;
                        break;
                    case MessageModelType.RightMap:
                    case MessageModelType.LeftMap:
                        if (!string.IsNullOrEmpty(item.MesData.MessageMap) && item.MesData.MessageMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                            imageUrl = item.MesData.MessageMap;
                        break;
                }

                if (!string.IsNullOrEmpty(imageUrl))
                    d.Add(imageUrl);

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(OptionsRoundedCrop);
        }

    }
}