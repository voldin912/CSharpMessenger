using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.GroupChat.Adapter
{
    public class GroupMembersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GroupMembersAdapterClickEventArgs> MoreItemClick;
        public event EventHandler<GroupMembersAdapterClickEventArgs> ItemClick;
        public event EventHandler<GroupMembersAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        private readonly bool ShowBtn;

        public GroupMembersAdapter(Activity activity, bool showBtn)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
                ShowBtn = showBtn;
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
                //Setup your layout here >> Style_GroupMembersView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GroupMembersView, parent, false);
                var vh = new GroupMembersAdapterViewHolder(itemView, MoreClick, Click, LongClick);
                return vh;
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
                if (viewHolder is GroupMembersAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        if (item.Avatar == "addImage")
                        {
                            holder.ImageAdd.Visibility = ViewStates.Visible;
                            holder.Image.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                        }

                        holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 25);
                        holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                        holder.About.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetAboutFinal(item), 25);

                        if (item.UserId == UserDetails.UserId || item.Avatar == "addImage" || !ShowBtn)
                            holder.ButtonMore.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(GroupMembersAdapterViewHolder holder, UserDataObject users)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case GroupMembersAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext?.BaseContext).Clear((View) viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public UserDataObject GetItem(int position)
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
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void MoreClick(GroupMembersAdapterClickEventArgs args)
        {
            MoreItemClick?.Invoke(this, args);
        }

        private void Click(GroupMembersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(GroupMembersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
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

    public class GroupMembersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public GroupMembersAdapterViewHolder(View itemView, Action<GroupMembersAdapterClickEventArgs> moreClickListener, Action<GroupMembersAdapterClickEventArgs> clickListener, Action<GroupMembersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ImageAdd = MainView.FindViewById<ImageView>(Resource.Id.add_pic);
                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                ButtonMore = MainView.FindViewById<ImageView>(Resource.Id.more);

                //Event
                ButtonMore.Click += (sender, e) => moreClickListener(new GroupMembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new GroupMembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GroupMembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView ImageAdd { get; private set; }
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public ImageView ButtonMore { get; private set; }
        public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class GroupMembersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}