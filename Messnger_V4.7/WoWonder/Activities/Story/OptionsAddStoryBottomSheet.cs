using System;
using System.Collections.ObjectModel;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.Tab;
using WoWonder.Adapters;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Story
{
    public class OptionsAddStoryBottomSheet : BottomSheetDialogFragment 
    {
        #region Variables Basic

        private ChatTabbedMainActivity GlobalContext;

        private LinearLayout HeaderLayout;
        private TextView TitleHeader;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;
         
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = ChatTabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetDefaultLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                SetRecyclerViewAdapters(view);

                LoadData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Functions

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                HeaderLayout = (LinearLayout)view.FindViewById(Resource.Id.header);
                HeaderLayout.Visibility = ViewStates.Visible;
                 
                TitleHeader = (TextView)view.FindViewById(Resource.Id.titleHeader);
                TitleHeader.Text = GetText(Resource.String.Lbl_Addnewstory);

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                MAdapter = new ItemOptionAdapter(Activity)
                {
                    ItemOptionList = new ObservableCollection<Classes.ItemOptionObject>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.GetRecycledViewPool().Clear();
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MAdapterOnItemClick(object sender, ItemOptionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (item?.Id == "1") //text
                    {
                        GlobalContext.OpenEditColor();
                    }
                    else if (item?.Id == "2") //image
                    {
                        GlobalContext.OnImage_Button_Click();
                    }
                    else if (item?.Id == "3") //video
                    {
                        GlobalContext.OnVideo_Button_Click();
                    }
                    Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        private void LoadData()
        {
            try
            { 
                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                {
                    Id = "1",
                    Text = GetText(Resource.String.text),
                    Icon = Resource.Drawable.icon_color_vector,
                });

                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                {
                    Id = "2",
                    Text = GetText(Resource.String.image),
                    Icon = Resource.Drawable.icon_image_vector,
                });

                if (WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "3",
                        Text = GetText(Resource.String.video),
                        Icon = Resource.Drawable.icon_video_camera,
                    });

                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}