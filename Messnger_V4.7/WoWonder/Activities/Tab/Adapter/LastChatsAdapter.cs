using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AmulyaKhare.TextDrawableLib;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Tab.Adapter
{
    public class LastChatsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<LastChatsAdapterClickEventArgs> ItemClick;
        public event EventHandler<LastChatsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.LastChatsClass> LastChatsList = new ObservableCollection<Classes.LastChatsClass>();
        private readonly List<string> ListOnline = new List<string>();
        private readonly string TypePage;
        public LastChatsAdapter(Activity context, string type)
        {
            try
            {
                ActivityContext = context;
                TypePage = type;
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
                    case (int)Classes.ItemType.LastChatNewV:
                        {
                            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastMessageView, parent, false);
                            var holder = new LastChatsAdapterViewHolder(itemView, OnClick, OnLongClick);
                            return holder;
                        }
                    case (int)Classes.ItemType.EmptyPage:
                        {
                            View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmptyStateLayout, parent, false);
                            var vh = new EmptyStateViewHolder(itemView);
                            return vh;
                        }
                    default:
                        return null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = LastChatsList[position];
                    switch (item.Type)
                    {
                        case Classes.ItemType.LastChatNewV:
                            {
                                if (viewHolder is LastChatsAdapterViewHolder holder)
                                {
                                    switch (payloads[0].ToString())
                                    {
                                        case "WithoutBlobMute" when item.LastChat.IsMute:
                                            holder.IconMute.Visibility = ViewStates.Visible;
                                            holder.IconMute.SetImageResource(Resource.Drawable.icon_mute_vector);

                                            break;
                                        case "WithoutBlobMute":
                                            holder.IconMute.Visibility = ViewStates.Gone;
                                            break;
                                        case "WithoutBlobPin" when item.LastChat.IsPin && TypePage != "Archived":
                                            holder.IconPin.Visibility = ViewStates.Visible;
                                            holder.IconPin.SetImageResource(Resource.Drawable.icon_pin_vector);
                                            break;
                                        case "WithoutBlobPin":
                                            holder.IconPin.Visibility = ViewStates.Gone;
                                            break;
                                        case "WithoutBlobLastSeen":
                                            holder.TxtTimestamp.Text = item.LastChat.LastseenTimeText;
                                            if (item.LastChat.LastseenStatus == "on" && UserDetails.OnlineUsers)
                                            {
                                                holder.TxtTimestamp.Text = ActivityContext.GetText(Resource.String.Lbl_Online);
                                                holder.ImageLastseen.SetBackgroundResource(Resource.Drawable.icon_online_vector);

                                                if (AppSettings.ShowOnlineOfflineMessage)
                                                {
                                                    var data = ListOnline.Contains(item.LastChat.Name);
                                                    if (data == false && !item.LastChat.IsMute)
                                                    {
                                                        ListOnline.Add(item.LastChat.Name);

                                                        Toast toast = Toast.MakeText(ActivityContext, item.LastChat.Name + " " + ActivityContext.GetText(Resource.String.Lbl_Online), ToastLength.Short);
                                                        toast?.SetGravity(GravityFlags.Center, 0, 0);
                                                        toast?.Show();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                holder.ImageLastseen.SetBackgroundResource(Resource.Drawable.icon_offline_vector);
                                            }
                                            break;
                                        case "WithoutBlobText":
                                            holder.TxtLastMessages.Text = item.LastChat.LastMessage.LastMessageClass.Text;
                                            switch (string.IsNullOrEmpty(item.LastChat.LastMessage.LastMessageClass.Media))
                                            {
                                                //If message contains Media files 
                                                case false when item.LastChat.LastMessage.LastMessageClass.Media.Contains("image"):
                                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_image_vector);
                                                    break;
                                                case false when item.LastChat.LastMessage.LastMessageClass.Media.Contains("video"):
                                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_video_player_vector);
                                                    break;
                                                case false when item.LastChat.LastMessage.LastMessageClass.Media.Contains("sticker"):
                                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_sticker_vector);
                                                    break;
                                                case false when item.LastChat.LastMessage.LastMessageClass.Media.Contains("sounds"):
                                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_radios_vector);
                                                    break;
                                                case false when item.LastChat.LastMessage.LastMessageClass.Media.Contains("file"):
                                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_document_vector);
                                                    break;
                                                default:
                                                    {
                                                        if (!string.IsNullOrEmpty(item.LastChat.LastMessage.LastMessageClass.Stickers) && item.LastChat.LastMessage.LastMessageClass.Stickers.Contains(".gif"))
                                                        {
                                                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_gif_vector);
                                                        }
                                                        else if (!string.IsNullOrEmpty(item.LastChat.LastMessage.LastMessageClass.Lat) && !string.IsNullOrEmpty(item.LastChat.LastMessage.LastMessageClass.Lng) && item.LastChat.LastMessage.LastMessageClass.Lat != "0" && item.LastChat.LastMessage.LastMessageClass.Lng != "0")
                                                        {
                                                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_search_location_vector);
                                                        }
                                                        else
                                                        {
                                                            holder.LastMessagesIcon.Visibility = ViewStates.Gone;
                                                        }

                                                        break;
                                                    }
                                            }
                                            //Check read message
                                            if (item.LastChat.LastMessage.LastMessageClass.FromId != null && item.LastChat.LastMessage.LastMessageClass.ToId != null && item.LastChat.LastMessage.LastMessageClass.ToId != UserDetails.UserId && item.LastChat.LastMessage.LastMessageClass.FromId == UserDetails.UserId)
                                            {
                                                if (item.LastChat.LastMessage.LastMessageClass.Seen == "0")
                                                {
                                                    holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                                                    //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                    holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                }
                                                else
                                                {
                                                    holder.IconCheckCountMessages.Visibility = ViewStates.Visible;
                                                    // holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                    holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                    holder.IconCheckCountMessages.SetImageResource(Resource.Drawable.icon_tick_vector);
                                                }
                                            }
                                            else if (item.LastChat.LastMessage.LastMessageClass.FromId != null && item.LastChat.LastMessage.LastMessageClass.ToId != null && item.LastChat.LastMessage.LastMessageClass.ToId == UserDetails.UserId && item.LastChat.LastMessage.LastMessageClass.FromId != UserDetails.UserId)
                                            {
                                                if (item.LastChat.LastMessage.LastMessageClass.Seen == "0")
                                                {
                                                    //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                                                    holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Bold);

                                                    //var drawable = new TextDrawable.Builder().BeginConfig().FontSize(25).EndConfig().BuildRound("N", Color.ParseColor(AppSettings.MainColor));
                                                    //holder.IconCheckCountMessages.SetImageDrawable(drawable);
                                                    holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                                                }
                                                else
                                                {
                                                    holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                                                    //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                    holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                                }
                                            }
                                            break;
                                    }
                                }
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

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = LastChatsList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.LastChatNewV:
                            {
                                if (viewHolder is LastChatsAdapterViewHolder holder)
                                {
                                    InitializeLastChatNewV(holder, item.LastChat);
                                }
                                break;
                            }
                        case Classes.ItemType.EmptyPage:
                            {
                                if (viewHolder is EmptyStateViewHolder holder)
                                {
                                    switch (TypePage)
                                    {
                                        case "user":
                                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.EmptyStateIcon, IonIconsFonts.Chatbubbles);
                                            holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Lastmessages);
                                            holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Lastmessages);
                                            break;
                                        case "group":
                                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, holder.EmptyStateIcon, FontAwesomeIcon.UserFriends);
                                            holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Group);
                                            holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Group);
                                            break;
                                        case "page":
                                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                                            holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Page);
                                            holder.DescriptionText.Text = "";
                                            break;
                                    }
                                }
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

        #region Init

        private void InitializeLastChatNewV(LastChatsAdapterViewHolder holder, ChatObject item)
        {
            try
            {
                if (item != null)
                {
                    GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                    //last seen time  
                    holder.TxtTimestamp.Text = Methods.Time.TimeAgo(int.Parse(item.ChatTime ?? item.Time), true);

                    if (item.LastMessage.LastMessageClass != null)
                    {
                        var lastMessage = item.LastMessage.LastMessageClass;

                        lastMessage.Stickers = lastMessage.Stickers?.Replace(".mp4", ".gif") ?? "";

                        holder.TxtLastMessages.Text = item.LastMessage.LastMessageClass.Text;

                        switch (string.IsNullOrEmpty(lastMessage.Media))
                        {
                            //If message contains Media files 
                            case false when lastMessage.Media.Contains("image"):
                                holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_image_vector);
                                break;
                            case false when lastMessage.Media.Contains("video"):
                                holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_video_player_vector);

                                break;
                            case false when lastMessage.Media.Contains("sticker"):
                                holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_sticker_vector);
                                break;
                            case false when lastMessage.Media.Contains("sounds"):
                                holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_radios_vector);
                                break;
                            case false when lastMessage.Media.Contains("file"):
                                holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_document_vector);

                                break;
                            default:
                                {
                                    if (!string.IsNullOrEmpty(lastMessage.Stickers) && lastMessage.Stickers.Contains(".gif"))
                                    {
                                        holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                        holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_gif_vector);
                                    }
                                    else if (!string.IsNullOrEmpty(lastMessage.Lat) && !string.IsNullOrEmpty(lastMessage.Lng) && lastMessage.Lat != "0" && lastMessage.Lng != "0")
                                    {
                                        holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                        holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_search_location_vector);
                                    }
                                    else
                                    {
                                        holder.LastMessagesIcon.Visibility = ViewStates.Gone;
                                    }

                                    break;
                                }
                        }

                        //Check read message
                        if (lastMessage.FromId != null && lastMessage.ToId != null && lastMessage.ToId != UserDetails.UserId && lastMessage.FromId == UserDetails.UserId)
                        {
                            if (lastMessage.Seen == "0")
                            {
                                holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                                //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            }
                            else
                            {
                                holder.IconCheckCountMessages.Visibility = ViewStates.Visible;
                                //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                holder.IconCheckCountMessages.SetImageResource(Resource.Drawable.icon_tick_vector);
                            }
                        }
                        else if (lastMessage.FromId != null && lastMessage.ToId != null && lastMessage.ToId == UserDetails.UserId && lastMessage.FromId != UserDetails.UserId)
                        {
                            if (lastMessage.Seen == "0")
                            {
                                //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                                holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Bold);

                                holder.IconCheckCountMessages.Visibility = ViewStates.Visible;
                                if (item.ChatType == "user")
                                {
                                    if (!string.IsNullOrEmpty(item.MessageCount))
                                    {
                                        var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(25).EndConfig().BuildRound(item.MessageCount, Color.ParseColor(AppSettings.MainColor));
                                        holder.IconCheckCountMessages.SetImageDrawable(drawable);
                                    }
                                    else
                                    {
                                        var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(25).EndConfig().BuildRound("N", Color.ParseColor(AppSettings.MainColor));
                                        holder.IconCheckCountMessages.SetImageDrawable(drawable);
                                    }
                                }
                            }
                            else
                            {
                                holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                                //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                                holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            }
                        }
                    }

                    if (item.IsMute)
                    {
                        holder.IconMute.Visibility = ViewStates.Visible;
                        holder.IconMute.SetImageResource(Resource.Drawable.icon_mute_vector);
                    }
                    else
                    {
                        holder.IconMute.Visibility = ViewStates.Gone;
                    }

                    if (item.IsPin && TypePage != "Archived")
                    {
                        holder.IconPin.Visibility = ViewStates.Visible;
                        holder.IconPin.SetImageResource(Resource.Drawable.icon_pin_vector);
                    }
                    else
                    {
                        holder.IconPin.Visibility = ViewStates.Gone;
                    }

                    switch (item.ChatType)
                    {
                        case "user":
                            InitializeUser(holder, item);
                            break;
                        case "page":
                            InitializePage(holder, item);
                            break;
                        case "group":
                            InitializeGroup(holder, item);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitializeUser(LastChatsAdapterViewHolder holder, ChatObject item)
        {
            try
            {
                holder.TxtUsername.Text = item.Name;

                holder.TxtUsername.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                //Online Or offline
                if (item.LastseenStatus?.ToLower() == "on" && item.Showlastseen == "1")
                {
                    holder.TxtTimestamp.Text = ActivityContext.GetText(Resource.String.Lbl_Online);
                    holder.ImageLastseen.Visibility = ViewStates.Visible;
                    holder.ImageLastseen.SetBackgroundResource(Resource.Drawable.icon_online_vector);

                    if (!AppSettings.ShowOnlineOfflineMessage) return;
                    var data = ListOnline.Contains(item.Name);
                    if (data == false && !item.IsMute)
                    {
                        ListOnline.Add(item.Name);

                        Toast toast = Toast.MakeText(ActivityContext, item.Name + " " + ActivityContext.GetText(Resource.String.Lbl_Online), ToastLength.Short);
                        toast?.SetGravity(GravityFlags.Center, 0, 0);
                        toast?.Show();
                    }
                }
                else if (item.LastseenStatus?.ToLower() == "on" && item.Showlastseen == "0")
                {
                    holder.ImageLastseen.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.ImageLastseen.Visibility = ViewStates.Visible;
                    holder.ImageLastseen.SetBackgroundResource(Resource.Drawable.icon_offline_vector);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializePage(LastChatsAdapterViewHolder holder, ChatObject item)
        {
            try
            {
                holder.TxtUsername.Text = item.PageName;
                holder.TxtUsername.SetCompoundDrawablesWithIntrinsicBounds(0, 0, 0, 0);

                holder.ImageLastseen.Visibility = ViewStates.Gone;
                holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializeGroup(LastChatsAdapterViewHolder holder, ChatObject item)
        {
            try
            {
                holder.TxtUsername.Text = item.GroupName;
                holder.TxtUsername.SetCompoundDrawablesWithIntrinsicBounds(0, 0, 0, 0);

                holder.ImageLastseen.Visibility = ViewStates.Gone;
                holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializeLastChatPage(LastChatsAdapterViewHolder holder, PageDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                holder.TxtUsername.Text = item.PageName;
                holder.TxtUsername.SetCompoundDrawablesWithIntrinsicBounds(0, 0, 0, 0);

                //last seen time  
                holder.TxtTimestamp.Visibility = ViewStates.Gone;

                //Online Or offline
                holder.ImageLastseen.Visibility = ViewStates.Gone;

                if (item.LastMessage != null)
                {
                    holder.TxtLastMessages.Text = item.LastMessage.Text;

                    switch (string.IsNullOrEmpty(item.LastMessage.Media))
                    {
                        //If message contains Media files 
                        case false when item.LastMessage.Media.Contains("image"):
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_image_vector);
                            break;
                        case false when item.LastMessage.Media.Contains("video"):
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_video_player_vector);
                            break;
                        case false when item.LastMessage.Media.Contains("sticker"):
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_sticker_vector);
                            break;
                        case false when item.LastMessage.Media.Contains("sounds"):
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_radios_vector);
                            break;
                        case false when item.LastMessage.Media.Contains("file"):
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_document_vector);
                            break;
                        default:
                            {
                                if (!string.IsNullOrEmpty(item.LastMessage.Stickers) && item.LastMessage.Stickers.Contains(".gif"))
                                {
                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_gif_vector);
                                }
                                else if (!string.IsNullOrEmpty(item.LastMessage.Lat) && !string.IsNullOrEmpty(item.LastMessage.Lng) && item.LastMessage.Lat != "0" && item.LastMessage.Lng != "0")
                                {
                                    holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                                    holder.LastMessagesIcon.SetImageResource(Resource.Drawable.icon_search_location_vector);
                                }
                                else
                                {
                                    holder.LastMessagesIcon.Visibility = ViewStates.Gone;
                                }

                                break;
                            }
                    }

                    //Check read message
                    if (item.LastMessage.FromId != null && item.LastMessage.ToId != null && item.LastMessage.ToId != UserDetails.UserId && item.LastMessage.FromId == UserDetails.UserId)
                    {
                        if (item.LastMessage.Seen == "0")
                        {
                            holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                            //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                        }
                        else
                        {
                            holder.IconCheckCountMessages.Visibility = ViewStates.Visible;
                            // holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.IconCheckCountMessages.SetImageResource(Resource.Drawable.icon_tick_vector);
                        }
                    }
                    else if (item.LastMessage.FromId != null && item.LastMessage.ToId != null && item.LastMessage.ToId == UserDetails.UserId && item.LastMessage.FromId != UserDetails.UserId)
                    {
                        if (item.LastMessage.Seen == "0")
                        {
                            //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Bold);

                            //var drawable = new TextDrawable.Builder().BeginConfig().FontSize(25).EndConfig().BuildRound("N", Color.ParseColor(AppSettings.MainColor));
                            //holder.IconCheckCountMessages.SetImageDrawable(drawable);
                            holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            holder.IconCheckCountMessages.Visibility = ViewStates.Gone;
                            //holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                        }
                    }
                }

                if (item.IsMute)
                {
                    holder.IconMute.Visibility = ViewStates.Visible;
                    holder.IconMute.SetImageResource(Resource.Drawable.icon_mute_vector);
                }
                else
                {
                    holder.IconMute.Visibility = ViewStates.Gone;
                }

                if (item.IsPin && TypePage != "Archived")
                {
                    holder.IconPin.Visibility = ViewStates.Visible;
                    holder.IconPin.SetImageResource(Resource.Drawable.icon_pin_vector);
                }
                else
                {
                    holder.IconPin.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override int ItemCount => LastChatsList?.Count ?? 0;

        public Classes.LastChatsClass GetItem(int position)
        {
            return LastChatsList[position];
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
                var item = LastChatsList[position];
                if (item != null)
                {
                    return item.Type switch
                    {
                        Classes.ItemType.LastChatNewV => (int)Classes.ItemType.LastChatNewV,
                        Classes.ItemType.EmptyPage => (int)Classes.ItemType.EmptyPage,
                        _ => (int)Classes.ItemType.EmptyPage,
                    };
                }

                return (int)Classes.ItemType.EmptyPage;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)Classes.ItemType.EmptyPage;
            }
        }

        void OnClick(LastChatsAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
        void OnLongClick(LastChatsAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = LastChatsList[p0];
                if (item == null)
                    return d;
                else
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.LastChatNewV:
                            {
                                if (!string.IsNullOrEmpty(item?.LastChat?.Avatar))
                                    d.Add(item.LastChat.Avatar);
                                break;
                            }
                    }

                    return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var d = new List<string>();
                return d;
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class LastChatsAdapterViewHolder : RecyclerView.ViewHolder
    {

        #region Variables Basic

        public View MainView { get; private set; }

        public RelativeLayout RelativeLayoutMain { get; private set; }
        public ImageView IconCheckCountMessages { get; private set; }
        public ImageView IconPin { get; private set; }
        public ImageView IconMute { get; private set; }

        public ImageView LastMessagesIcon { get; private set; }
        public TextView TxtUsername { get; private set; }
        public TextView TxtLastMessages { get; private set; }
        public TextView TxtTimestamp { get; private set; }
        public ImageView ImageAvatar { get; private set; } //ImageView
        public View ImageLastseen { get; private set; }

        #endregion

        public LastChatsAdapterViewHolder(View itemView, Action<LastChatsAdapterClickEventArgs> clickListener, Action<LastChatsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                RelativeLayoutMain = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
                ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.ImageAvatar);
                ImageLastseen = (View)MainView.FindViewById(Resource.Id.ImageLastseen);
                TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
                LastMessagesIcon = (ImageView)MainView.FindViewById(Resource.Id.IconLastMessages);
                TxtLastMessages = (TextView)MainView.FindViewById(Resource.Id.Txt_LastMessages);
                TxtTimestamp = (TextView)MainView.FindViewById(Resource.Id.Txt_timestamp);
                IconCheckCountMessages = (ImageView)MainView.FindViewById(Resource.Id.IconCheckRead);
                IconPin = (ImageView)MainView.FindViewById(Resource.Id.IconPin);
                IconMute = (ImageView)MainView.FindViewById(Resource.Id.IconMute);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LastChatsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LastChatsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
    
    public class EmptyStateViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public AppCompatButton EmptyStateButton { get; private set; }
        public TextView EmptyStateIcon { get; private set; }
        public TextView DescriptionText { get; private set; }
        public TextView TitleText { get; private set; }
        public ImageView EmptyImage { get; private set; }

        #endregion

        public EmptyStateViewHolder(View itemView) : base(itemView)
        {
            try
            {
                MainView = itemView;

                EmptyStateIcon = (TextView)itemView.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)itemView.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)itemView.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (AppCompatButton)itemView.FindViewById(Resource.Id.button);
                EmptyImage = itemView.FindViewById<ImageView>(Resource.Id.iv_empty);

                EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                EmptyStateButton.Visibility = ViewStates.Gone;
                EmptyImage.Visibility = ViewStates.Gone;
                EmptyStateIcon.Visibility = ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LastChatsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}