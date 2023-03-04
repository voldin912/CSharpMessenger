using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.GroupChat;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab
{
    public class OptionsLastMessagesBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private ChatTabbedMainActivity GlobalContext;
        //wael  add Mute call
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        private string Type, Page;
        private ChatObject DataChatObject;


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

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
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
                    if (item?.Id == "1") //Archive
                    {
                        ArchiveLayoutOnClick();
                    }
                    else if (item?.Id == "2") //DeleteMessage
                    {
                        DeleteLayoutOnClick();
                    }
                    else if (item?.Id == "3") //Pin
                    {
                        PinLayoutOnClick();
                    }
                    else if (item?.Id == "4") //MuteNotification
                    {
                        MuteLayoutOnClick();
                    }
                    else if (item?.Id == "5") //MarkAsRead
                    {
                        ReadLayoutOnClick();
                    }
                    else if (item?.Id == "6") //Block
                    {
                        BlockLayoutOnClick();
                    }
                    else if (item?.Id == "7") //View Profile
                    {
                        ProfileLayoutOnClick();
                    }
                    else if (item?.Id == "9") //GroupInfo
                    {
                        GroupInfoLayoutOnClick();
                    }
                    else if (item?.Id == "10") //ExitGroup
                    {
                        ExitGroupLayoutOnClick();
                    }
                    else if (item?.Id == "11") //AddMembers
                    {
                        AddMembersLayoutOnClick();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Add Members to group
        private void AddMembersLayoutOnClick()
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(EditGroupChatActivity));
                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(DataChatObject));
                intent.PutExtra("Type", "Edit");
                Activity.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Exit Group
        private void ExitGroupLayoutOnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var dialog = new MaterialAlertDialogBuilder(Activity);
                    dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureExitGroup));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Exit),async (materialDialog, action) =>
                    {
                        try
                        {
                            //Show a progress
                            AndHUD.Shared.Show(Activity, GetText(Resource.String.Lbl_Loading));

                            var (apiStatus, respond) = await RequestsAsync.GroupChat.ExitGroupChatAsync(DataChatObject.GroupId);
                            if (apiStatus == 200)
                            {
                                if (respond is AddOrRemoveUserToGroupObject result)
                                {
                                    Console.WriteLine(result.MessageData);

                                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_GroupSuccessfullyLeaved), ToastLength.Short);

                                    //remove item to my Group list  
                                    var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab.MAdapter;
                                    var data = adapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                                    if (data != null)
                                    {
                                        adapter.LastChatsList.Remove(data);
                                        adapter.NotifyItemRemoved(adapter.LastChatsList.IndexOf(data));
                                    }

                                    AndHUD.Shared.ShowSuccess(Activity);
                                }
                            }
                            else Methods.DisplayReportResult(Activity, respond);

                            AndHUD.Shared.Dismiss();

                            Dismiss();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
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

        //Group Info (Profile)  
        private void GroupInfoLayoutOnClick()
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(EditGroupChatActivity));
                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(DataChatObject));
                intent.PutExtra("Type", "Profile");
                Activity.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //View Profile
        private void ProfileLayoutOnClick()
        {
            try
            {
                WoWonderTools.OpenProfile(Activity, DataChatObject.UserId, DataChatObject.UserData);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Block User
        private async void BlockLayoutOnClick()
        {
            try
            {
                string userId = DataChatObject.UserId;

                if (Methods.CheckConnectivity())
                {
                    if (!DataChatObject.IsBlocked)
                    {
                        DataChatObject.IsBlocked = true;
                        var (apiStatus, respond) = await RequestsAsync.Global.BlockUserAsync(userId, true); //true >> "block" 
                        if (apiStatus == 200)
                        {
                            var dbDatabase = new SqLiteDatabase();
                            //dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(DataUserChat, "Delete"); 
                            dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, userId);

                            Methods.Path.DeleteAll_FolderUser(userId);

                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short);

                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            var checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                            if (checkUser != null)
                            {
                                checkUser.LastChat.IsBlocked = true;
                            }


                            DeleteLayoutOnClick();
                        }
                        else
                            Methods.DisplayReportResultTrack(respond);
                    }
                    else
                    {
                        DataChatObject.IsBlocked = false;

                        var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                        var checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                        if (checkUser != null)
                        {
                            var index = mAdapter.LastChatsList.IndexOf(checkUser);

                            checkUser.LastChat.IsBlocked = false;

                            mAdapter.NotifyItemChanged(index);
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.BlockUserAsync(DataChatObject.UserId, false) });//false >> "un-block"
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Unblock_successfully), ToastLength.Short);
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Mark As Read/UnRead //wael
        //if (Seen == "0") //not read Change to read (Normal) >> Seen = "1"; 
        //else //read Change to unread (Bold) >> Seen = "0";
        private void ReadLayoutOnClick()
        {
            try
            {
                //wael add api  
                switch (Type)
                {
                    case "user":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            var seen = DataChatObject.LastMessage.LastMessageClass.Seen == "0" ? DataChatObject.LastMessage.LastMessageClass.Seen = "1" : DataChatObject.LastMessage.LastMessageClass.Seen = "0";


                            var checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                            if (checkUser != null)
                            {
                                checkUser.LastChat.LastMessage.LastMessageClass.Seen = seen;
                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkUser), "WithoutBlobRead");
                            }
                            break;
                        }
                    case "page":
                        {
                            Classes.LastChatsClass checkUser = null!;

                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            var seen = DataChatObject.LastMessage.LastMessageClass.Seen == "0" ? DataChatObject.LastMessage.LastMessageClass.Seen = "1" : DataChatObject.LastMessage.LastMessageClass.Seen = "0";

                            var checkPage = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId && a.LastChat?.LastMessage.LastMessageClass?.ToData?.UserId == DataChatObject.LastMessage.LastMessageClass?.ToData?.UserId);
                            if (checkPage != null)
                            {
                                var userAdminPage = DataChatObject.UserId;
                                if (userAdminPage == DataChatObject.LastMessage.LastMessageClass.ToData.UserId)
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.UserData?.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.UserData?.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);
                                }
                                else
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.ToData.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.ToData.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.ToData.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);
                                }
                            }
                            else
                                checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId);

                            if (checkUser != null)
                            {
                                checkUser.LastChat.LastMessage.LastMessageClass.Seen = seen;
                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkUser), "WithoutBlobRead");
                            }
                            break;
                        }
                    //break;
                    case "group":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                            var seen = DataChatObject.LastMessage.LastMessageClass.Seen == "0" ? DataChatObject.LastMessage.LastMessageClass.Seen = "1" : DataChatObject.LastMessage.LastMessageClass.Seen = "0";

                            var checkGroup = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                            if (checkGroup?.LastChat != null)
                            {
                                checkGroup.LastChat.LastMessage.LastMessageClass.Seen = seen;
                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkGroup), "WithoutBlobRead");
                            }
                            break;
                        }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Mark As Mute/UnMute
        private void MuteLayoutOnClick()
        {
            try
            {
                bool isMute = false;
                Classes.OptionLastChat muteObject = null!;
                Mute globalMute = null!;
                string idChat = null!;

                switch (Type)
                {
                    case "user":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isMute = !DataChatObject.IsMute;
                            idChat = DataChatObject.ChatId;
                            globalMute = DataChatObject.Mute;

                            var checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                            if (checkUser != null)
                            {
                                checkUser.LastChat.IsMute = isMute;

                                checkUser.LastChat.Mute.Notify = isMute ? "no" : "yes";
                                globalMute = checkUser.LastChat.Mute;

                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkUser), "WithoutBlobMute");
                                muteObject = new Classes.OptionLastChat
                                {
                                    ChatType = "user",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = DataChatObject.UserId,
                                    GroupId = "",
                                    PageId = "",
                                    Name = DataChatObject.Name
                                };
                            }
                            break;
                        }
                    case "page":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isMute = !DataChatObject.IsMute;
                            idChat = DataChatObject.ChatId;
                            globalMute = DataChatObject.Mute;

                            Classes.LastChatsClass checkUser = null!;

                            var checkPage = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId && a.LastChat?.LastMessage.LastMessageClass?.ToData?.UserId == DataChatObject.LastMessage.LastMessageClass?.ToData?.UserId);
                            if (checkPage != null)
                            {
                                var userAdminPage = DataChatObject.UserId;
                                if (userAdminPage == DataChatObject.LastMessage.LastMessageClass.ToData.UserId)
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.UserData?.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.UserData?.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    muteObject = new Classes.OptionLastChat
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name
                                    };
                                }
                                else
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.ToData.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.ToData.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.ToData.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    muteObject = new Classes.OptionLastChat
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name
                                    };
                                }
                            }
                            else
                            {
                                checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId);
                                muteObject = new Classes.OptionLastChat
                                {
                                    ChatType = "page",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = "",
                                    PageId = DataChatObject.PageId,
                                    Name = DataChatObject.PageName
                                };
                            }

                            if (checkUser != null)
                            {
                                checkUser.LastChat.IsMute = isMute;
                                checkUser.LastChat.Mute.Notify = isMute ? "no" : "yes";
                                globalMute = checkUser.LastChat.Mute;

                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkUser), "WithoutBlobMute");
                            }
                            break;
                        }
                    case "group":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                            isMute = !DataChatObject.IsMute;
                            idChat = DataChatObject.GroupId;

                            var checkGroup = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                            if (checkGroup != null)
                            {
                                checkGroup.LastChat.IsMute = isMute;

                                checkGroup.LastChat.Mute.Notify = isMute ? "no" : "yes";
                                globalMute = checkGroup.LastChat.Mute;

                                mAdapter?.NotifyItemChanged(mAdapter.LastChatsList.IndexOf(checkGroup), "WithoutBlobMute");

                                muteObject = new Classes.OptionLastChat
                                {
                                    ChatType = "group",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = DataChatObject.GroupId,
                                    PageId = "",
                                    Name = DataChatObject.GroupName
                                };
                            }
                            break;
                        }
                }

                if (isMute)
                {
                    if (muteObject != null)
                    {
                        ListUtils.MuteList.Add(muteObject);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Mute(muteObject);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"notify", "no"},
                    };

                    //if (globalMute != null)
                    //{
                    //    dictionary.Add("call_chat", globalMute.CallChat);
                    //    dictionary.Add("archive", globalMute.Archive);
                    //    dictionary.Add("pin", globalMute.Pin);
                    //}

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_AddedMute), ToastLength.Long);
                }
                else
                {
                    var checkMute = ListUtils.MuteList.FirstOrDefault(a => muteObject != null && a.ChatId == muteObject.ChatId && a.ChatType == muteObject.ChatType);
                    if (checkMute != null)
                    {
                        ListUtils.MuteList.Remove(checkMute);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Mute(checkMute);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"notify", "yes"},
                    };

                    //if (globalMute != null)
                    //{
                    //    dictionary.Add("call_chat", globalMute.CallChat);
                    //    dictionary.Add("archive", globalMute.Archive);
                    //    dictionary.Add("pin", globalMute.Pin);
                    //}

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_RemovedMute), ToastLength.Long);
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Mark Pin
        private void PinLayoutOnClick()
        {
            try
            {
                bool isPin = false;
                Classes.OptionLastChat pinObject = null!;
                Mute globalMute = null!;
                string idChat = null!;

                switch (Type)
                {
                    case "user":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isPin = !DataChatObject.IsPin;
                            idChat = DataChatObject.ChatId;
                            globalMute = DataChatObject.Mute;

                            var checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                            if (checkUser != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkUser);
                                checkUser.LastChat.IsPin = isPin;

                                checkUser.LastChat.Mute.Pin = isPin ? "yes" : "no";
                                globalMute = checkUser.LastChat.Mute;

                                if (isPin)
                                {
                                    var checkPin = mAdapter.LastChatsList.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                    if (checkPin != null)
                                    {
                                        var toIndex = mAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                        if (ListUtils.FriendRequestsList.Count > 0)
                                            toIndex++;

                                        if (mAdapter.LastChatsList.Count > toIndex)
                                        {
                                            mAdapter.LastChatsList.Move(index, toIndex);
                                            mAdapter.NotifyItemMoved(index, toIndex);
                                        }

                                        mAdapter.NotifyItemChanged(toIndex, "WithoutBlobPin");
                                    }
                                    else
                                    {
                                        if (ListUtils.FriendRequestsList.Count > 0)
                                        {
                                            mAdapter.LastChatsList.Move(index, 1);
                                            mAdapter.NotifyItemMoved(index, 1);
                                            mAdapter.NotifyItemChanged(1, "WithoutBlobPin");
                                        }
                                        else
                                        {
                                            mAdapter.LastChatsList.Move(index, 0);
                                            mAdapter.NotifyItemMoved(index, 0);
                                            mAdapter.NotifyItemChanged(0, "WithoutBlobPin");
                                        }
                                    }
                                }
                                else
                                {
                                    mAdapter.NotifyItemChanged(index, "WithoutBlobPin");
                                }

                                pinObject = new Classes.OptionLastChat
                                {
                                    ChatType = "user",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = DataChatObject.UserId,
                                    GroupId = "",
                                    PageId = "",
                                    Name = DataChatObject.Name
                                };
                            }
                            break;
                        }
                    case "page":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isPin = !DataChatObject.IsPin;
                            idChat = DataChatObject.ChatId;
                            globalMute = DataChatObject.Mute;

                            Classes.LastChatsClass checkUser = null!;

                            var checkPage = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId && a.LastChat?.LastMessage.LastMessageClass?.ToData?.UserId == DataChatObject.LastMessage.LastMessageClass?.ToData?.UserId);
                            if (checkPage != null)
                            {
                                var userAdminPage = DataChatObject.UserId;
                                if (userAdminPage == DataChatObject.LastMessage.LastMessageClass.ToData.UserId)
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.UserData?.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.UserData?.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    pinObject = new Classes.OptionLastChat
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name
                                    };
                                }
                                else
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.ToData.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.ToData.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.ToData.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    pinObject = new Classes.OptionLastChat
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name
                                    };
                                }
                            }
                            else
                            {
                                checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId);
                                pinObject = new Classes.OptionLastChat
                                {
                                    ChatType = "page",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = "",
                                    PageId = DataChatObject.PageId,
                                    Name = DataChatObject.PageName
                                };
                            }

                            if (checkUser != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkUser);
                                checkUser.LastChat.IsPin = isPin;
                                checkUser.LastChat.Mute.Pin = isPin ? "yes" : "no";
                                globalMute = checkUser.LastChat.Mute;

                                if (isPin)
                                {
                                    if (ListUtils.FriendRequestsList.Count > 0)
                                    {
                                        mAdapter.LastChatsList.Move(index, 1);
                                        mAdapter.NotifyItemMoved(index, 1);
                                        mAdapter.NotifyItemChanged(1, "WithoutBlobPin");
                                    }
                                    else
                                    {
                                        mAdapter.LastChatsList.Move(index, 0);
                                        mAdapter.NotifyItemMoved(index, 0);
                                        mAdapter.NotifyItemChanged(0, "WithoutBlobPin");
                                    }

                                    //var checkPin = mAdapter.LastChatsList.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                    //if (checkPin != null)
                                    //{
                                    //    var toIndex = mAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                    //    mAdapter.LastChatsList.Move(index, toIndex);
                                    //    mAdapter.NotifyItemMoved(index, toIndex);
                                    //    mAdapter.NotifyItemChanged(toIndex);
                                    //}
                                    //else
                                    //{
                                    //    if (ListUtils.FriendRequestsList.Count > 0)
                                    //    {
                                    //        mAdapter.LastChatsList.Move(index, 1);
                                    //        mAdapter.NotifyItemMoved(index, 1);
                                    //        mAdapter.NotifyItemChanged(1);
                                    //    }
                                    //    else
                                    //    {
                                    //        mAdapter.LastChatsList.Move(index, 0);
                                    //        mAdapter.NotifyItemMoved(index, 0);
                                    //        mAdapter.NotifyItemChanged(0);
                                    //    }
                                    //}
                                }
                                else
                                {
                                    mAdapter.NotifyItemChanged(index, "WithoutBlobPin");
                                }
                            }
                            break;
                        }
                    //break;
                    case "group":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                            isPin = !DataChatObject.IsPin;
                            idChat = DataChatObject.GroupId;
                            globalMute = DataChatObject.Mute;

                            var checkGroup = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                            if (checkGroup?.LastChat != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkGroup);

                                checkGroup.LastChat.IsPin = isPin;
                                checkGroup.LastChat.Mute.Pin = isPin ? "yes" : "no";
                                globalMute = checkGroup.LastChat.Mute;

                                if (isPin)
                                {
                                    var checkPin = mAdapter.LastChatsList.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                    if (checkPin != null)
                                    {
                                        var toIndex = mAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                        if (ListUtils.FriendRequestsList.Count > 0)
                                            toIndex++;

                                        if (mAdapter.LastChatsList.Count > toIndex)
                                        {
                                            mAdapter.LastChatsList.Move(index, toIndex);
                                            mAdapter.NotifyItemMoved(index, toIndex);
                                        }
                                        mAdapter.NotifyItemChanged(toIndex, "WithoutBlobPin");
                                    }
                                    else
                                    {
                                        if (ListUtils.FriendRequestsList.Count > 0)
                                        {
                                            mAdapter.LastChatsList.Move(index, 1);
                                            mAdapter.NotifyItemMoved(index, 1);
                                            mAdapter.NotifyItemChanged(1, "WithoutBlobPin");
                                        }
                                        else
                                        {
                                            mAdapter.LastChatsList.Move(index, 0);
                                            mAdapter.NotifyItemMoved(index, 0);
                                            mAdapter.NotifyItemChanged(0, "WithoutBlobPin");
                                        }
                                    }
                                }
                                else
                                {
                                    mAdapter.NotifyItemChanged(index, "WithoutBlobPin");
                                }

                                pinObject = new Classes.OptionLastChat
                                {
                                    ChatType = "group",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = DataChatObject.GroupId,
                                    PageId = "",
                                    Name = DataChatObject.GroupName
                                };
                            }
                            break;
                        }
                }

                if (isPin)
                {
                    if (pinObject != null)
                    {
                        ListUtils.PinList.Add(pinObject);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Pin(pinObject);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"pin", "yes"},
                    };

                    //if (globalMute != null)
                    //{
                    //    dictionary.Add("call_chat", globalMute.CallChat);
                    //    dictionary.Add("archive", globalMute.Archive);
                    //    dictionary.Add("notify", globalMute.Notify);
                    //}

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_MessagePinned), ToastLength.Long);
                }
                else
                {
                    var checkPin = ListUtils.PinList.FirstOrDefault(a => pinObject != null && a.ChatId == pinObject.ChatId && a.ChatType == pinObject.ChatType);
                    if (checkPin != null)
                    {
                        ListUtils.PinList.Remove(checkPin);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Pin(checkPin);
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"pin", "no"},
                    };

                    //if (globalMute != null)
                    //{
                    //    dictionary.Add("call_chat", globalMute.CallChat);
                    //    dictionary.Add("archive", globalMute.Archive);
                    //    dictionary.Add("notify", globalMute.Notify);
                    //}

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_MessageUnPinned), ToastLength.Long);
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Delete Chat
        private void DeleteLayoutOnClick()
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(Context);
                dialog.SetTitle(GetText(Resource.String.Lbl_DeleteTheEntireConversation));
                dialog.SetMessage(GetText(Resource.String.Lbl_OnceYouDeleteConversation));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes),(materialDialog, action) =>
                {
                    try
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            return;
                        }

                        switch (Type)
                        {
                            case "user":
                                {
                                    var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                                    var userToDelete = mAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                                    if (userToDelete != null)
                                    {
                                        var index = mAdapter.LastChatsList.IndexOf(userToDelete);
                                        if (index > -1)
                                        {
                                            mAdapter?.LastChatsList?.Remove(userToDelete);
                                            mAdapter?.NotifyItemRemoved(index);
                                        }
                                    }

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Delete_LastUsersChat(DataChatObject.UserId, "user");
                                    dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, DataChatObject.UserId);

                                    Methods.Path.DeleteAll_FolderUser(DataChatObject.UserId);

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteConversationAsync(DataChatObject.UserId) });
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_TheConversationHasBeenDeleted), ToastLength.Long);
                                    break;
                                }
                            case "page":
                                {
                                    string userId;
                                    //remove item to my page list  
                                    var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;

                                    var checkPage = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId);
                                    if (checkPage != null)
                                    {
                                        var userAdminPage = DataChatObject.UserId;
                                        if (userAdminPage == DataChatObject.LastMessage.LastMessageClass.ToData.UserId)
                                        {
                                            userId = DataChatObject.LastMessage.LastMessageClass.UserData?.UserId;
                                            var data = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.UserData?.UserId == userId);
                                            if (data != null)
                                            {
                                                mAdapter?.LastChatsList.Remove(data);
                                                mAdapter?.NotifyItemRemoved(mAdapter.LastChatsList.IndexOf(data));
                                            }
                                        }
                                        else
                                        {
                                            userId = DataChatObject.LastMessage.LastMessageClass.ToData.UserId;
                                            var data = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.ToData.UserId == userId);
                                            if (data != null)
                                            {
                                                mAdapter?.LastChatsList.Remove(data);
                                                mAdapter?.NotifyItemRemoved(mAdapter.LastChatsList.IndexOf(data));
                                            }
                                        }

                                        var dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Delete_LastUsersChat(DataChatObject.PageId, "page", userId);

                                        Methods.Path.DeleteAll_FolderUser(DataChatObject.PageId);

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.PageChat.DeletePageChatAsync(DataChatObject.PageId, userId) });
                                    }


                                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_TheConversationHasBeenDeleted), ToastLength.Short);

                                    break;
                                }
                            case "group":
                                {
                                    //remove item to my Group list  
                                    var adapter = GlobalContext?.ChatTab?.LastGroupChatsTab.MAdapter;
                                    var data = adapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                                    if (data != null)
                                    {
                                        adapter.LastChatsList.Remove(data);
                                        adapter.NotifyItemRemoved(adapter.LastChatsList.IndexOf(data));
                                    }

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Delete_LastUsersChat(DataChatObject.GroupId, "group");
                                    dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, DataChatObject.GroupId);

                                    Methods.Path.DeleteAll_FolderUser(DataChatObject.GroupId);

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.DeleteGroupChatAsync(DataChatObject.GroupId) });

                                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_GroupSuccessfullyLeaved), ToastLength.Short);
                                    break;
                                }
                        }

                        Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_No),new MaterialDialogUtils());
                
                dialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Archive chat 
        private void ArchiveLayoutOnClick()
        {
            try
            {
                bool isArchive = false;
                Classes.LastChatArchive archiveObject = null!;
                Mute globalMute = null!;
                Classes.LastChatsClass checkUser = null!;
                string idChat = null!;

                switch (Type)
                {
                    case "user":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isArchive = !DataChatObject.IsArchive;
                            DataChatObject.IsArchive = isArchive;
                            globalMute = DataChatObject.Mute;

                            checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.UserId == DataChatObject.UserId);
                            if (checkUser?.LastChat != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkUser);
                                checkUser.LastChat.IsArchive = isArchive;

                                checkUser.LastChat.Mute.Archive = isArchive ? "yes" : "no";
                                globalMute = checkUser.LastChat.Mute;

                                mAdapter.LastChatsList.Remove(checkUser);
                                mAdapter.NotifyItemRemoved(index);

                                idChat = DataChatObject.UserId;
                                archiveObject = new Classes.LastChatArchive
                                {
                                    ChatType = "user",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = DataChatObject.UserId,
                                    GroupId = "",
                                    PageId = "",
                                    Name = DataChatObject.Name,
                                    IdLastMessage = DataChatObject?.LastMessage.LastMessageClass?.Id ?? "",
                                    LastChat = DataChatObject
                                };
                            }
                            break;
                        }
                    case "page":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastChatTab?.MAdapter;
                            isArchive = !DataChatObject.IsArchive;
                            DataChatObject.IsArchive = isArchive;
                            idChat = DataChatObject.ChatId;
                            globalMute = DataChatObject.Mute;

                            var checkPage = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId && a.LastChat?.LastMessage.LastMessageClass?.ToData?.UserId == DataChatObject.LastMessage.LastMessageClass?.ToData?.UserId);
                            if (checkPage != null)
                            {
                                var userAdminPage = DataChatObject.UserId;
                                if (userAdminPage == DataChatObject.LastMessage.LastMessageClass.ToData.UserId)
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.UserData?.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.UserData?.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    archiveObject = new Classes.LastChatArchive
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name,
                                        IdLastMessage = DataChatObject.LastMessage.LastMessageClass?.Id ?? "",
                                        LastChat = DataChatObject
                                    };
                                }
                                else
                                {
                                    var userId = DataChatObject.LastMessage.LastMessageClass.ToData.UserId;
                                    checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.LastMessage.LastMessageClass.ToData.UserId == userId);

                                    var name = DataChatObject.LastMessage.LastMessageClass.ToData.Name + "(" + DataChatObject.PageName + ")";
                                    Console.WriteLine(name);

                                    archiveObject = new Classes.LastChatArchive
                                    {
                                        ChatType = "page",
                                        ChatId = DataChatObject.ChatId,
                                        UserId = userId,
                                        GroupId = "",
                                        PageId = DataChatObject.PageId,
                                        Name = name,
                                        IdLastMessage = DataChatObject.LastMessage.LastMessageClass?.Id ?? "",
                                        LastChat = DataChatObject
                                    };
                                }
                            }
                            else
                            {
                                checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.PageId == DataChatObject.PageId);
                                archiveObject = new Classes.LastChatArchive
                                {
                                    ChatType = "page",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = "",
                                    PageId = DataChatObject.PageId,
                                    Name = DataChatObject.PageName,
                                    IdLastMessage = DataChatObject.LastMessage.LastMessageClass?.Id ?? "",
                                    LastChat = DataChatObject
                                };
                            }

                            if (checkUser != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkUser);

                                checkUser.LastChat.IsArchive = isArchive;
                                checkUser.LastChat.Mute.Archive = isArchive ? "yes" : "no";
                                globalMute = checkUser.LastChat.Mute;

                                mAdapter.LastChatsList.Remove(checkUser);
                                mAdapter.NotifyItemRemoved(index);
                            }
                            break;
                        }
                    //break;
                    case "group":
                        {
                            var mAdapter = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter;
                            isArchive = !DataChatObject.IsArchive;
                            DataChatObject.IsArchive = isArchive;
                            globalMute = DataChatObject.Mute;

                            checkUser = mAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == DataChatObject.GroupId);
                            if (checkUser?.LastChat != null)
                            {
                                var index = mAdapter.LastChatsList.IndexOf(checkUser);
                                checkUser.LastChat.IsArchive = isArchive;

                                checkUser.LastChat.Mute.Archive = isArchive ? "yes" : "no";
                                globalMute = checkUser.LastChat.Mute;

                                mAdapter.LastChatsList.Remove(checkUser);
                                mAdapter.NotifyItemRemoved(index);

                                idChat = DataChatObject.GroupId;
                                archiveObject = new Classes.LastChatArchive
                                {
                                    ChatType = "group",
                                    ChatId = DataChatObject.ChatId,
                                    UserId = "",
                                    GroupId = DataChatObject.GroupId,
                                    PageId = "",
                                    Name = DataChatObject.GroupName,
                                    IdLastMessage = DataChatObject.LastMessage.LastMessageClass?.Id ?? "",
                                    LastChat = DataChatObject
                                };
                            }
                            break;
                        }
                }

                if (isArchive)
                {
                    if (archiveObject != null)
                    {
                        ListUtils.ArchiveList.Add(archiveObject);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Archive(archiveObject);

                        GlobalContext?.ChatTab?.ArchivedChatsTab?.GetArchivedList();

                        var dictionary = new Dictionary<string, string>
                        {
                            {"archive", "yes"},
                        };

                        if (globalMute != null)
                        {
                            dictionary.Add("call_chat", globalMute.CallChat);
                            dictionary.Add("pin", "no");
                            dictionary.Add("notify", globalMute.Notify);
                        }

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });
                    }

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Archive), ToastLength.Long);
                }
                else
                {
                    var checkArchive = ListUtils.ArchiveList.FirstOrDefault(a => archiveObject != null && a.ChatId == archiveObject.ChatId && a.ChatType == archiveObject.ChatType);
                    if (checkArchive != null)
                    {
                        ListUtils.ArchiveList.Remove(checkArchive);

                        var sqLiteDatabase = new SqLiteDatabase();
                        sqLiteDatabase.InsertORDelete_Archive(checkArchive);

                        GlobalContext?.ChatTab?.ArchivedChatsTab?.GetArchivedList();

                        var dictionary = new Dictionary<string, string>
                        {
                            {"archive", "no"},
                        };

                        if (globalMute != null)
                        {
                            dictionary.Add("call_chat", globalMute.CallChat);
                            dictionary.Add("pin", "yes");
                            dictionary.Add("notify", globalMute.Notify);
                        }

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.MuteChatsInfoAsync(idChat, Type, dictionary) });
                    }

                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_UnArchive), ToastLength.Long);
                }

                GlobalContext?.ChatTab?.LastChatTab?.ShowEmptyPage();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                Page = Arguments?.GetString("Page") ?? "";
                Type = Arguments?.GetString("Type") ?? "";

                switch (Type)
                {
                    case "user":
                        {
                            DataChatObject = JsonConvert.DeserializeObject<ChatObject>(Arguments?.GetString("ItemObject") ?? "");
                            if (DataChatObject != null) //not read Change to read (Normal)
                            {
                                if (AppSettings.EnableChatArchive)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "1",
                                        Text = GetText(DataChatObject.IsArchive ? Resource.String.Lbl_UnArchive : Resource.String.Lbl_Archive),
                                        Icon = Resource.Drawable.icon_archive_vector,

                                    });
                                }

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "2",
                                    Text = GetText(Resource.String.Btn_DeleteMessage),
                                    Icon = Resource.Drawable.icon_delete_vector,

                                });

                                if (AppSettings.EnableChatPin && Page != "Archived")
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "3",
                                        Text = GetText(DataChatObject.IsPin ? Resource.String.Lbl_UnPin : Resource.String.Lbl_Pin),
                                        Icon = Resource.Drawable.icon_pin_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMute)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "4",
                                        Text = GetText(DataChatObject.IsMute ? Resource.String.Lbl_UnMuteNotification : Resource.String.Lbl_MuteNotification),
                                        Icon = Resource.Drawable.icon_mute_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMakeAsRead)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "5",
                                        Text = GetText(DataChatObject.LastMessage.LastMessageClass?.Seen == "0" ? Resource.String.Lbl_MarkAsRead : Resource.String.Lbl_MarkAsUnRead),
                                        Icon = Resource.Drawable.icon_mark_chat_unread_vector,

                                    });
                                }

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "6",
                                    Text = GetText(DataChatObject.IsBlocked ? Resource.String.Btn_UnBlock : Resource.String.Lbl_Block),
                                    Icon = Resource.Drawable.icon_block_vector,

                                });

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "7",
                                    Text = GetText(Resource.String.Lbl_View_Profile),
                                    Icon = Resource.Drawable.icon_user_vector,

                                });
                            }

                            break;
                        }
                    case "page":
                        {
                            DataChatObject = JsonConvert.DeserializeObject<ChatObject>(Arguments?.GetString("ItemObject") ?? "");
                            if (DataChatObject != null) //not read Change to read (Normal)  
                            {
                                if (AppSettings.EnableChatArchive)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "1",
                                        Text = GetText(DataChatObject.IsArchive ? Resource.String.Lbl_UnArchive : Resource.String.Lbl_Archive),
                                        Icon = Resource.Drawable.icon_archive_vector,

                                    });
                                }

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "2",
                                    Text = GetText(Resource.String.Btn_DeleteMessage),
                                    Icon = Resource.Drawable.icon_delete_vector,

                                });

                                if (AppSettings.EnableChatPin && Page != "Archived")
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "3",
                                        Text = GetText(DataChatObject.IsPin ? Resource.String.Lbl_UnPin : Resource.String.Lbl_Pin),
                                        Icon = Resource.Drawable.icon_pin_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMute)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "4",
                                        Text = GetText(DataChatObject.IsMute ? Resource.String.Lbl_UnMuteNotification : Resource.String.Lbl_MuteNotification),
                                        Icon = Resource.Drawable.icon_mute_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMakeAsRead)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "5",
                                        Text = GetText(DataChatObject.LastMessage.LastMessageClass?.Seen == "0" ? Resource.String.Lbl_MarkAsRead : Resource.String.Lbl_MarkAsUnRead),
                                        Icon = Resource.Drawable.icon_mark_chat_unread_vector,

                                    });
                                }
                            }
                            break;
                        }
                    case "group":
                        {
                            DataChatObject = JsonConvert.DeserializeObject<ChatObject>(Arguments?.GetString("ItemObject") ?? "");
                            if (DataChatObject != null) //not read Change to read (Normal)  
                            {
                                if (AppSettings.EnableChatArchive)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "1",
                                        Text = GetText(DataChatObject.IsArchive ? Resource.String.Lbl_UnArchive : Resource.String.Lbl_Archive),
                                        Icon = Resource.Drawable.icon_archive_vector,

                                    });
                                }

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "2",
                                    Text = GetText(Resource.String.Btn_DeleteMessage),
                                    Icon = Resource.Drawable.icon_delete_vector,

                                });

                                if (AppSettings.EnableChatPin)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "3",
                                        Text = GetText(DataChatObject.IsPin ? Resource.String.Lbl_UnPin : Resource.String.Lbl_Pin),
                                        Icon = Resource.Drawable.icon_pin_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMute)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "4",
                                        Text = GetText(DataChatObject.IsMute ? Resource.String.Lbl_UnMuteNotification : Resource.String.Lbl_MuteNotification),
                                        Icon = Resource.Drawable.icon_mute_vector,

                                    });
                                }

                                if (AppSettings.EnableChatMakeAsRead)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "5",
                                        Text = GetText(DataChatObject.LastMessage.LastMessageClass?.Seen == "0" ? Resource.String.Lbl_MarkAsRead : Resource.String.Lbl_MarkAsUnRead),
                                        Icon = Resource.Drawable.icon_mark_chat_unread_vector,

                                    });
                                }

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "9",
                                    Text = GetText(Resource.String.Lbl_GroupInfo),
                                    Icon = Resource.Drawable.icon_info_vector,

                                });

                                MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                {
                                    Id = "10",
                                    Text = GetText(Resource.String.Lbl_ExitGroup),
                                    Icon = Resource.Drawable.icon_logout_vector,
                                });

                                if (DataChatObject?.Owner != null && DataChatObject.Owner.Value)
                                {
                                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                                    {
                                        Id = "11",
                                        Text = GetText(Resource.String.Lbl_AddMembers),
                                        Icon = Resource.Drawable.icon_user_vector,
                                    });
                                }
                            }

                            break;
                        }
                }

                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}