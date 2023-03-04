using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using AndroidX.RecyclerView.Widget;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;

namespace WoWonder.Activities.SettingsPreferences.Adapters
{
    public class SelectAccountAdapter : RecyclerView.Adapter
    {
        public event EventHandler<SelectAccountAdapterClickEventArgs> ItemClick;
        public event EventHandler<SelectAccountAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<DataTables.LoginTb> AccountList = new ObservableCollection<DataTables.LoginTb>();

        public SelectAccountAdapter(Activity context)
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = context;
                 
                var sqLiteDatabase = new SqLiteDatabase();
                sqLiteDatabase.Get_List_Account_Credentials();
                foreach (var loginTb in ListUtils.DataUserLoginList)
                {
                    if (loginTb.Status == "Active" && string.IsNullOrEmpty(loginTb.Username))
                    {
                        var data = ListUtils.MyProfileList.FirstOrDefault();
                        if (data != null)
                        {
                            loginTb.Avatar = data.Avatar;
                            loginTb.Username = data.Username;
                            loginTb.FullName = data.Name;
                        }

                        AccountList.Add(loginTb);
                    }
                    else
                    {
                        AccountList.Add(loginTb);
                    }
                }

                AccountList.Add(new DataTables.LoginTb
                {
                    UserId = "-1",
                    Username = ActivityContext.GetText(Resource.String.Lbl_AddAccount),
                    Avatar = "addImage",
                    Status = "Add", 
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => AccountList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SelectAccountView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SelectAccountView, parent, false);
                var vh = new SelectAccountAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is SelectAccountAdapterViewHolder holder)
                {
                    var item = AccountList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                        holder.CheckIcon.Visibility = item.Status == "Active" ? ViewStates.Visible : ViewStates.Gone;
                         
                        holder.Name.Text = item.Username; 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public DataTables.LoginTb GetItem(int position)
        {
            return AccountList[position];
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

        private void Click(SelectAccountAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(SelectAccountAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class SelectAccountAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public CircleImageView Image { get; private set; }
        public ImageView CheckIcon { get; private set; }
        public TextView Name { get; private set; }

        #endregion

        public SelectAccountAdapterViewHolder(View itemView, Action<SelectAccountAdapterClickEventArgs> clickListener, Action<SelectAccountAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<CircleImageView>(Resource.Id.imageUser);
                CheckIcon = MainView.FindViewById<ImageView>(Resource.Id.checkIcon);

                Name = MainView.FindViewById<TextView>(Resource.Id.name); 

                //Event  
                itemView.Click += (sender, e) => clickListener(new SelectAccountAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SelectAccountAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }

    public class SelectAccountAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}