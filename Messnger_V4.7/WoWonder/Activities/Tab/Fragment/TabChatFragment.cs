using System;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using WoWonder.Adapters;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tab.Fragment
{
    public class TabChatFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private ViewPager2 ViewPager;
        private MainTabAdapter TabAdapter;
        private LinearLayout Tab;
        private TextView TxtChats, TxtGroups, TxtArchives;
        private LinearLayout ButtonChats, ButtonGroups, ButtonArchives;

        public LastChatFragment LastChatTab;
        public LastGroupChatsFragment LastGroupChatsTab;
        public ArchivedChatsFragment ArchivedChatsTab;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TabChatLayout, container, false);
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

                InitComponent(view);
                AddFragmentsTabs();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
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

        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                TabAdapter = new MainTabAdapter(this);
                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.viewPagerChat);
                Tab = view.FindViewById<LinearLayout>(Resource.Id.tabsChat);

                ButtonChats = view.FindViewById<LinearLayout>(Resource.Id.llChats);
                ButtonGroups = view.FindViewById<LinearLayout>(Resource.Id.llGroups);
                ButtonArchives = view.FindViewById<LinearLayout>(Resource.Id.llArchive);

                TxtChats = view.FindViewById<TextView>(Resource.Id.ivChat);
                TxtGroups = view.FindViewById<TextView>(Resource.Id.ivGroups);
                TxtArchives = view.FindViewById<TextView>(Resource.Id.ivArchive);

                if (!ButtonChats.HasOnClickListeners)
                {
                    ButtonChats.Click += ButtonChats_Click;
                    ButtonGroups.Click += ButtonGroups_Click;
                    ButtonArchives.Click += ButtonArchives_Click;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void AddFragmentsTabs()
        {
            try
            {
                TabAdapter.ClaerFragment();

                LastChatTab = new LastChatFragment();

                if (AppSettings.EnableChatGroup)
                    LastGroupChatsTab = new LastGroupChatsFragment();

                if (AppSettings.EnableChatArchive)
                    ArchivedChatsTab = new ArchivedChatsFragment();

                if (TabAdapter is { ItemCount: <= 0 })
                {
                    TabAdapter.AddFragment(LastChatTab, GetText(Resource.String.Lbl_User));

                    if (AppSettings.EnableChatGroup)
                        TabAdapter.AddFragment(LastGroupChatsTab, GetText(Resource.String.Lbl_Group));

                    if (AppSettings.EnableChatArchive)
                        TabAdapter.AddFragment(ArchivedChatsTab, GetText(Resource.String.Lbl_Archive));

                    ViewPager.UserInputEnabled = false;
                    ViewPager.CurrentItem = TabAdapter.ItemCount;
                    ViewPager.OffscreenPageLimit = TabAdapter.ItemCount;

                    ViewPager.Orientation = ViewPager2.OrientationHorizontal;
                    ViewPager.Adapter = TabAdapter;
                    ViewPager.Adapter.NotifyDataSetChanged();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Event

        private void ButtonChats_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonGroups.SetBackgroundResource(0);
                ButtonArchives.SetBackgroundResource(0);
                TxtGroups.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));
                TxtArchives.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));

                ButtonChats.SetBackgroundResource(Resource.Drawable.bg_tab_button);
                TxtChats.SetTextColor(Color.White);

                //Show UsersList 
                ViewPager.SetCurrentItem(0, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ButtonGroups_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonChats.SetBackgroundResource(0);
                ButtonArchives.SetBackgroundResource(0);
                TxtChats.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));
                TxtArchives.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));

                ButtonGroups.SetBackgroundResource(Resource.Drawable.bg_tab_button);
                TxtGroups.SetTextColor(Color.White);

                //Show GroupsList 
                ViewPager.SetCurrentItem(1, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ButtonArchives_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonChats.SetBackgroundResource(0);
                ButtonGroups.SetBackgroundResource(0);
                TxtChats.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));
                TxtGroups.SetTextColor(Resources.GetColor(Resource.Color.text_color_in_between, null));

                ButtonArchives.SetBackgroundResource(Resource.Drawable.bg_tab_button);
                TxtArchives.SetTextColor(Color.White);

                //Show Archive page 
                ViewPager.SetCurrentItem(2, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}