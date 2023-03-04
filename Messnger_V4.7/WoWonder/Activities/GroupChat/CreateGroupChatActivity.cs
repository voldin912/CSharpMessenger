using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using Google.Android.Material.Dialog;
using WoWonder.Activities.Base;
using WoWonder.Activities.GroupChat.Adapter;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using static Com.Canhub.Cropper.CropImageView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.GroupChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateGroupChatActivity : BaseActivity, IActivityResultCallback
    {
        #region Variables Basic

        private TextView BtnExitGroup;
        private AXEmojiEditText TxtGroupName;
        private ImageView ChatEmojImage;
        private ImageView BtnImage, ImageGroup;
        private AppCompatButton BtnDeleteGroup, BtnCreateGroup;
        private GroupMembersAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;

        private string GroupPathImage = "", UsersIds;
        private List<UserDataObject> UserList;

        private int Position;
        private ChatTabbedMainActivity GlobalContext;
        private DialogGalleryController GalleryController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateGroupChatLayout);

                GlobalContext = ChatTabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GalleryController = new DialogGalleryController(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.userRecyler);
                TxtGroupName = FindViewById<AXEmojiEditText>(Resource.Id.groupName);
                ImageGroup = FindViewById<ImageView>(Resource.Id.groupCover);
                BtnImage = FindViewById<ImageView>(Resource.Id.btn_selectimage);

                ChatEmojImage = FindViewById<ImageView>(Resource.Id.emojiicon);

                BtnCreateGroup = FindViewById<AppCompatButton>(Resource.Id.createGroupButton);

                BtnDeleteGroup = FindViewById<AppCompatButton>(Resource.Id.deleteGroupButton);
                BtnDeleteGroup.Visibility = ViewStates.Gone;

                BtnExitGroup = FindViewById<TextView>(Resource.Id.exitGroupButton);
                BtnExitGroup.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtGroupName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                InitEmojisView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_CreateGroup);
                    toolbar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.ParseColor("#060606"));
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new GroupMembersAdapter(this, true)
                {
                    UserList = new ObservableCollection<UserDataObject>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default  
                MAdapter.UserList.Add(new UserDataObject
                {
                    UserId = "0",
                    Avatar = "addImage",
                    Name = GetString(Resource.String.Lbl_AddParticipants),
                    About = GetString(Resource.String.Lbl_Group_Add_Description),
                });
                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnCreateGroup.Click += TxtAddOnClick;
                    BtnImage.Click += BtnImageOnClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.MoreItemClick += MAdapterOnItemLongClick;
                }
                else
                {
                    BtnCreateGroup.Click -= TxtAddOnClick;
                    BtnImage.Click -= BtnImageOnClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.MoreItemClick -= MAdapterOnItemLongClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(AppSettings.MainColor);

                    EmojisViewTools.MStickerView = false;
                    EmojisViewTools.LoadView(this, TxtGroupName, "", ChatEmojImage);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        #endregion

        #region Events

        private void MAdapterOnItemLongClick(object sender, GroupMembersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.Avatar == "addImage") return;

                    Position = e.Position;
                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetTitle(GetString(Resource.String.Lbl_Remove) + " " + WoWonderTools.GetNameFinal(item));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes) , (o, args) =>
                    {
                        try
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var itemUser = MAdapter.GetItem(Position);
                                if (itemUser != null)
                                {
                                    MAdapter.UserList.Remove(itemUser);
                                    MAdapter.NotifyItemRemoved(Position);
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception); 
                        }
                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, GroupMembersAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.Avatar != "addImage") return;
                    StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnImageOnClick(object sender, EventArgs e)
        {
            try
            {
                GalleryController?.OpenDialogGallery(); //requestCode >> 500 => Image Gallery
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtGroupName.Text) || string.IsNullOrWhiteSpace(TxtGroupName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (TxtGroupName.Text.Length < 4 && TxtGroupName.Text.Length > 15)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ErrorLengthGroupName), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(GroupPathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    var list = MAdapter.UserList.Where(a => a.Avatar != "addImage").ToList();
                    if (list.Count == 0)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseSelectUser), ToastLength.Long);
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        UsersIds = "";
                        foreach (var user in list)
                        {
                            UsersIds += user.UserId + ",";
                        }

                        UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                        var (apiStatus, respond) = await RequestsAsync.GroupChat.CreateGroupChatAsync(TxtGroupName.Text, UsersIds, GroupPathImage);
                        if (apiStatus == 200)
                        {
                            if (respond is CreateGroupChatObject result)
                            {
                                AndHUD.Shared.ShowSuccess(this);

                                //Add new item to my Group list 
                                var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                                if (result.Data?.Count > 0 && adapter != null)
                                {
                                    adapter.LastChatsList.Insert(0, new Classes.LastChatsClass
                                    {
                                        LastChat = result.Data.FirstOrDefault(),
                                        Type = Classes.ItemType.LastChatNewV,
                                    });
                                    adapter.NotifyDataSetChanged();

                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.ShowEmptyPage();
                                }

                                Finish();
                            }
                        }
                        else
                        {
                            Methods.DisplayAndHudErrorResult(this, respond);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void TxtAddUserOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    case 3:
                        {
                            UserList = MentionActivity.MAdapter.MentionList.Where(a => a.Selected).ToList();

                            UsersIds = "";
                            foreach (var user in UserList)
                            {
                                UsersIds += user.UserId + ",";

                                var dataUser = MAdapter.UserList.FirstOrDefault(attachments => attachments.UserId == user.UserId);
                                if (dataUser == null)
                                {
                                    MAdapter.UserList.Insert(1, user);
                                }
                            }
                            UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                            MAdapter.NotifyDataSetChanged();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        GalleryController?.OpenDialogGallery();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Gallery
          
        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //File file2 = new File(filepath);
                            //var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(filepath).Apply(new RequestOptions().CircleCrop()).Into(ImageGroup);

                            //GlideImageLoader.LoadImage(this, resultUri.Path, ImageGroup, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
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
}