using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.StickersView;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.ChatWindow
{
    public class AttachmentMediaChatBottomSheet : BottomSheetDialogFragment, IDialogListCallBack
    {
        #region Variables Basic

        private ChatWindowActivity ChatWindowContext;
        private GroupChatWindowActivity GroupChatWindowContext;
        private PageChatWindowActivity PageChatWindowContext;

        private LinearLayout CameraLayout, GalleryLayout, FileLayout, MusicLayout, StickersLayout, GifLayout, ContactLayout, LocationLayout;

        private string Page;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetAttachmentMediaLayout, container, false);
                if (AppSettings.ChatTheme == ChatTheme.Default)
                {
                    view = localInflater?.Inflate(Resource.Layout.BottomSheetAttachmentMediaLayout, container, false);
                }
                else if (AppSettings.ChatTheme == ChatTheme.Tokyo)
                {
                    view = localInflater?.Inflate(Resource.Layout.BottomSheetAttachmentMedia_Style1_Layout, container, false);
                }

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

                LoadDataChat();
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

        private void InitComponent(View view)
        {
            try
            {
                CameraLayout = view.FindViewById<LinearLayout>(Resource.Id.CameraLayout);
                CameraLayout.Click += CameraLayoutOnClick;

                GalleryLayout = view.FindViewById<LinearLayout>(Resource.Id.GalleryLayout);
                GalleryLayout.Click += GalleryLayoutOnClick;

                FileLayout = view.FindViewById<LinearLayout>(Resource.Id.FileLayout);
                FileLayout.Click += FileLayoutOnClick;

                MusicLayout = view.FindViewById<LinearLayout>(Resource.Id.MusicLayout);
                MusicLayout.Click += MusicLayoutOnClick;

                GifLayout = view.FindViewById<LinearLayout>(Resource.Id.GifLayout);
                GifLayout.Click += GifLayoutOnClick;

                StickersLayout = view.FindViewById<LinearLayout>(Resource.Id.StickersLayout);
                StickersLayout.Click += StickersLayoutOnClick;

                ContactLayout = view.FindViewById<LinearLayout>(Resource.Id.ContactLayout);
                ContactLayout.Click += ContactLayoutOnClick;

                LocationLayout = view.FindViewById<LinearLayout>(Resource.Id.LocationLayout);
                LocationLayout.Click += LocationLayoutOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void CameraLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Activity);

                arrayAdapter.Add(GetText(Resource.String.Lbl_TakeImageFromCamera));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.SetTitle(GetString(Resource.String.Camera));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GalleryLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Activity);

                arrayAdapter.Add(GetText(Resource.String.Lbl_ImageGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));

                dialogList.SetTitle(GetString(Resource.String.Lbl_Gallery));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MusicLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StickersLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                BrowseStickersFragment fragment = new BrowseStickersFragment();
                Bundle bundle = new Bundle();

                switch (Page)
                {
                    // Create your fragment here
                    case "ChatWindow":
                        bundle.PutString("TypePage", "ChatWindowActivity");
                        fragment.Arguments = bundle;
                        fragment.Show(ChatWindowContext.SupportFragmentManager, fragment.Tag);
                        break;
                    case "PageChatWindow":
                        bundle.PutString("TypePage", "GroupChatWindowActivity");
                        fragment.Arguments = bundle;
                        fragment.Show(PageChatWindowContext.SupportFragmentManager, fragment.Tag);
                        break;
                    case "GroupChatWindow":
                        bundle.PutString("TypePage", "PageChatWindowActivity");
                        fragment.Arguments = bundle;
                        fragment.Show(GroupChatWindowContext.SupportFragmentManager, fragment.Tag);
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GifLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ContactLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LocationLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(dialog, position, itemString);
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(dialog, position, itemString);
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(dialog, position, itemString);
                        break;
                }
                Dismiss();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                Page = Arguments?.GetString("Page") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext = ChatWindowActivity.GetInstance();
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext = GroupChatWindowActivity.GetInstance();
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext = PageChatWindowActivity.GetInstance();
                        break;
                }
                //wael
                //if (!AppSettings.ShowButtonImage)
                //    ImageLayout.Visibility = ViewStates.Gone;

                //if (!AppSettings.ShowButtonVideo || !WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                //    VideoLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonAttachFile || !WoWonderTools.CheckAllowedFileSharingInServer("File"))
                    FileLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonMusic || !WoWonderTools.CheckAllowedFileSharingInServer("Audio"))
                    MusicLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonGif)
                    GifLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonContact)
                    ContactLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonLocation)
                    LocationLayout.Visibility = ViewStates.Gone;

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}