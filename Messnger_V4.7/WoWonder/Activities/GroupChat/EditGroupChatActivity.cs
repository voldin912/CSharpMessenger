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
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.GroupChat.Adapter;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using static Com.Canhub.Cropper.CropImageView;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.GroupChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditGroupChatActivity : BaseActivity, IActivityResultCallback
    {
        #region Variables Basic

        private TextView BtnExitGroup;
        private AXEmojiEditText TxtGroupName;
        private ImageView ImageGroup;
        private ImageView BtnImage, ChatEmojImage;
        private AppCompatButton BtnDeleteGroup, BtnSave;
        private GroupMembersAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private string TypePage, GroupPathImage = "", UsersIds, Type = "", GroupId;
        private List<UserDataObject> NewUserList;
        private ChatObject GroupData;
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

                TypePage = Intent?.GetStringExtra("Type") ?? "";
                string obj = Intent?.GetStringExtra("GroupObject") ?? "";
                if (!string.IsNullOrEmpty(obj))
                {
                    GroupData = JsonConvert.DeserializeObject<ChatObject>(obj);
                    GroupId = GroupData.GroupId;
                }

                GlobalContext = ChatTabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GalleryController = new DialogGalleryController(this, this);

                LoadContacts();
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

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.createGroupButton);
                BtnSave.Text = GetText(Resource.String.Lbl_Save);

                BtnDeleteGroup = FindViewById<AppCompatButton>(Resource.Id.deleteGroupButton);
                BtnDeleteGroup.Visibility = ViewStates.Visible;

                BtnExitGroup = FindViewById<TextView>(Resource.Id.exitGroupButton);
                BtnExitGroup.Visibility = ViewStates.Visible;

                Methods.SetColorEditText(TxtGroupName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                InitEmojisView();

                if (TypePage == "Profile")
                {
                    BtnImage.Visibility = ViewStates.Invisible;
                    ChatEmojImage.Visibility = ViewStates.Invisible;
                    BtnDeleteGroup.Visibility = ViewStates.Gone;
                    BtnSave.Visibility = ViewStates.Gone;

                    Methods.SetFocusable(TxtGroupName);
                }
                else
                {
                    BtnDeleteGroup.Visibility = ViewStates.Visible;
                }
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
                MAdapter = new GroupMembersAdapter(this, TypePage == "Edit")
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

                if (TypePage == "Edit")
                {
                    // Add first image Default  
                    MAdapter.UserList.Add(new UserDataObject
                    {
                        UserId = "0",
                        Avatar = "addImage",
                        Name = GetString(Resource.String.Lbl_AddParticipants),
                        About = GetString(Resource.String.Lbl_Group_Add_Description)
                    });
                    MAdapter.NotifyDataSetChanged();
                }
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
                    BtnSave.Click += TxtAddOnClick;

                    BtnImage.Click += BtnImageOnClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    BtnExitGroup.Click += BtnExitGroupOnClick;
                    BtnDeleteGroup.Click += BtnDeleteGroupOnClick;
                    MAdapter.MoreItemClick += MAdapterOnItemLongClick;
                }
                else
                {
                    BtnSave.Click -= TxtAddOnClick;

                    BtnImage.Click -= BtnImageOnClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    BtnExitGroup.Click -= BtnExitGroupOnClick;
                    BtnDeleteGroup.Click -= BtnDeleteGroupOnClick;
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

        //Edit Group
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

                    if (GroupPathImage == GroupData.Avatar)
                    {
                        GroupPathImage = "";
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var list = MAdapter.UserList.Where(a => a.Avatar != "addImage").ToList();
                    if (list.Count == 0)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseSelectUser), ToastLength.Long);
                    }
                    else
                    {
                        try
                        {
                            UsersIds = "";
                            foreach (var user in list)
                            {
                                UsersIds += user.UserId + ",";
                            }

                            UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AddOrRemoveUserToGroupAsync(GroupData.GroupId, UsersIds, "add_user") });
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }

                    var (apiStatus, respond) = await RequestsAsync.GroupChat.EditGroupChatAsync(GroupId, TxtGroupName.Text, GroupPathImage);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateGroupChatObject result)
                        {
                            AndHUD.Shared.ShowSuccess(this);

                            //Add new item to my Group list
                            if (result.Data?.Count > 0)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                                        var data = adapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == GroupData.GroupId);
                                        if (data != null)
                                        {
                                            var index = adapter.LastChatsList.IndexOf(data);
                                            if (index > -1)
                                            {
                                                GroupData = result.Data[0];
                                                adapter.LastChatsList[index].LastChat = result.Data[0];

                                                adapter.NotifyDataSetChanged();
                                            }
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }

                            var resultIntent = new Intent();
                            resultIntent.PutExtra("GroupName", TxtGroupName.Text);
                            SetResult(Result.Ok, resultIntent);

                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //leave group chat
        private void BtnExitGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    Type = "Exit";

                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureExitGroup));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Exit),async (o, args) =>
                    {
                        try
                        {
                            //Show a progress
                            AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                            var (apiStatus, respond) = await RequestsAsync.GroupChat.ExitGroupChatAsync(GroupId);
                            if (apiStatus == 200)
                            {
                                if (respond is AddOrRemoveUserToGroupObject result)
                                {
                                    Console.WriteLine(result.MessageData);

                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_GroupSuccessfullyLeaved), ToastLength.Short);

                                    //remove new item to my Group list  
                                    var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab.MAdapter;
                                    var data = adapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == GroupId);
                                    if (data != null)
                                    {
                                        adapter.LastChatsList.Remove(data);
                                        adapter.NotifyItemRemoved(adapter.LastChatsList.IndexOf(data));
                                    }

                                    AndHUD.Shared.ShowSuccess(this);
                                    Finish();
                                }
                            }
                            else
                            {
                                Methods.DisplayAndHudErrorResult(this, respond);
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel),new MaterialDialogUtils());
                    
                    dialog.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //delete group chat
        private void BtnDeleteGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    Type = "Delete";

                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureDeleteGroup));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_DeleteGroup), async (o, args) =>
                    {
                        try
                        {
                            //Show a progress
                            AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                            var (apiStatus, respond) = await RequestsAsync.GroupChat.DeleteGroupChatAsync(GroupId);
                            if (apiStatus == 200)
                            {
                                AndHUD.Shared.ShowSuccess(this);
                                if (respond is AddOrRemoveUserToGroupObject result)
                                {
                                    Console.WriteLine(result.MessageData);
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_GroupSuccessfullyDeleted), ToastLength.Short);

                                    //remove item to my Group list  
                                    var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab.MAdapter;
                                    var data = adapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == GroupId);
                                    if (data != null)
                                    {
                                        adapter.LastChatsList.Remove(data);
                                        adapter.NotifyItemRemoved(adapter.LastChatsList.IndexOf(data));
                                    }

                                    Finish();
                                }
                            }
                            else
                            {
                                Methods.DisplayAndHudErrorResult(this, respond);
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    } );
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel),new MaterialDialogUtils());
                    
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

                    Type = "RemoveUser";

                    Position = e.Position;
                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetTitle(GetString(Resource.String.Lbl_Remove) + " " + WoWonderTools.GetNameFinal(item));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (o, args) =>
                    {
                        try
                        {
                            var itemUser = MAdapter.GetItem(Position);
                            if (itemUser != null)
                            {
                                MAdapter.UserList.Remove(itemUser);
                                MAdapter.NotifyItemRemoved(Position);

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AddOrRemoveUserToGroupAsync(GroupId, itemUser.UserId, "remove_user") });

                                if (itemUser.UserId == UserDetails.UserId)
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.ExitGroupChatAsync(GroupId) });
                            }

                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_No),new MaterialDialogUtils());
                    
                    dialog.Show();
                }
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
                            NewUserList = MentionActivity.MAdapter.MentionList.Where(a => a.Selected).ToList();

                            foreach (var user in NewUserList)
                            {
                                var dataUser = MAdapter.UserList.FirstOrDefault(attachments => attachments.UserId == user.UserId);
                                if (dataUser == null)
                                {
                                    MAdapter.UserList.Add(user);
                                }
                            }

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
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
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

        private void LoadContacts()
        {
            try
            {
                if (GroupData != null)
                {
                    GroupPathImage = GroupData.Avatar;
                    GlideImageLoader.LoadImage(this, GroupData.Avatar, ImageGroup, ImageStyle.CircleCrop,
                        ImagePlaceholders.DrawableUser);

                    TxtGroupName.Text = Methods.FunString.DecodeString(GroupData.GroupName);

                    if (GroupData?.Parts?.Count == 0) return;

                    var sss = GroupData?.Parts?.Where(dataPart => dataPart != null).ToList();
                    foreach (var dataPart in sss)
                    {
                        MAdapter.UserList.Insert(TypePage == "Edit" ? 1 : 0, dataPart);
                    }

                    MAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
                            GroupPathImage = filepath;
                            //File file2 = new File(resultUri.Path);
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