using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.DefaultUser.Adapters
{
    public class AddNewChatAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<AddNewChatAdapterClickEventArgs> ItemClick;
        public event EventHandler<AddNewChatAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.AddNewChatObject> UserList = new ObservableCollection<Classes.AddNewChatObject>();

        public AddNewChatAdapter(Activity context)
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                if (viewType == (int)Classes.ItemType.AddGroup || viewType == (int)Classes.ItemType.AddCall)
                {
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ItemOptionView, parent, false);
                    var vh = new AddItemOptionAdapterViewHolder(itemView, Click, LongClick);
                    return vh;
                }
                else
                {
                    var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);
                    var vh = new AddNewChatAdapterViewHolder(itemView, Click, LongClick);
                    return vh;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = UserList[position];
                if (item.Type == Classes.ItemType.AddGroup)
                {
                    if (viewHolder is AddItemOptionAdapterViewHolder holder)
                    {
                        holder.ContentText.Text = ActivityContext.GetText(Resource.String.Lbl_CreateGroupChat);
                        holder.IconContent.SetImageResource(Resource.Drawable.icon_add_group_vector);
                    }
                }
                if (item.Type == Classes.ItemType.AddCall)
                {
                    if (viewHolder is AddItemOptionAdapterViewHolder holder)
                    {
                        holder.ContentText.Text = ActivityContext.GetText(Resource.String.Lbl_CreateNewVideoCall);
                        holder.IconContent.SetImageResource(Resource.Drawable.icon_video_camera_vector);
                    }
                }
                else
                {
                    if (viewHolder is AddNewChatAdapterViewHolder holder)
                    {
                        if (item.User != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.User.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser, true);

                            holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item.User), 20);

                            holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.User.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                            holder.About.Text = ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(item.User.LastseenUnixTime), false);

                            //Online Or offline
                            var online = WoWonderTools.GetStatusOnline(Convert.ToInt32(item.User.LastseenUnixTime), item.User.LastseenStatus);
                            holder.ImageLastSeen.SetImageResource(online ? Resource.Drawable.icon_online_vector : Resource.Drawable.icon_offline_vector);
                            if (online)
                            {
                                holder.About.Text = ActivityContext.GetString(Resource.String.Lbl_Online);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public Classes.AddNewChatObject GetItem(int position)
        {
            return UserList[position];
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
                var item = UserList[position];

                return item.Type switch
                {
                    Classes.ItemType.AddGroup => (int)Classes.ItemType.AddGroup,
                    Classes.ItemType.AddCall => (int)Classes.ItemType.AddCall,
                    Classes.ItemType.User => (int)Classes.ItemType.User,
                    _ => (int)Classes.ItemType.User
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)Classes.ItemType.User;
            }
        }

        private void Click(AddNewChatAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(AddNewChatAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.User?.Avatar))
                {
                    d.Add(item.User.Avatar);
                    return d;
                }

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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class AddNewChatAdapterViewHolder : RecyclerView.ViewHolder
    {

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton Button { get; private set; }
        public CircleImageView ImageLastSeen { get; private set; }

        #endregion

        public AddNewChatAdapterViewHolder(View itemView, Action<AddNewChatAdapterClickEventArgs> clickListener, Action<AddNewChatAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);

                Button.Visibility = ViewStates.Gone;

                //Event
                itemView.Click += (sender, e) => clickListener(new AddNewChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AddNewChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AddItemOptionAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public LinearLayout ContentLayout { get; set; }
        public ImageView IconContent { get; set; }
        public TextView ContentText { get; set; }

        #endregion

        public AddItemOptionAdapterViewHolder(View itemView, Action<AddNewChatAdapterClickEventArgs> clickListener, Action<AddNewChatAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ContentLayout = itemView.FindViewById<LinearLayout>(Resource.Id.ContentLayout);
                IconContent = itemView.FindViewById<ImageView>(Resource.Id.IconContent);
                ContentText = itemView.FindViewById<TextView>(Resource.Id.ContentText);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new AddNewChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AddNewChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AddNewChatAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}