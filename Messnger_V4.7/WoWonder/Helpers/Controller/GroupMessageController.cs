using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.Tab;
using WoWonder.Adapters;
using WoWonder.Helpers.Jobs;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Services;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using MessageData = WoWonderClient.Classes.Message.MessageData;

namespace WoWonder.Helpers.Controller
{
    public static class GroupMessageController
    {
        //############# DONT'T MODIFY HERE ############# 
        private static GroupChatWindowActivity MainWindowActivity;
        private static ChatTabbedMainActivity GlobalContext;

        //========================= Functions ========================= 
        public static async Task SendMessageTask(GroupChatWindowActivity windowActivity, string id, string chatId, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            try
            {
                MainWindowActivity = windowActivity;
                GlobalContext = ChatTabbedMainActivity.GetInstance();

                if (!string.IsNullOrEmpty(pathFile))
                {
                    new UploadSingleFileToServerWorker(windowActivity, "GroupChatWindowActivity").UploadFileToServer(windowActivity, new FileModel
                    {
                        MessageHashId = messageId,
                        ChatId = chatId,
                        GroupId = id,
                        FilePath = pathFile,
                        ReplyId = replyId,
                    });
                }
                else
                {
                    StartApiService(id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId);
                }
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void StartApiService(string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(MainWindowActivity, MainWindowActivity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SendMessage(id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId) });
        }

        private static async Task SendMessage(string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            var (apiStatus, respond) = await RequestsAsync.GroupChat.Send_MessageToGroupChatAsync(id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId);
            if (apiStatus == 200)
            {
                if (respond is GroupSendMessageObject result)
                {
                    UpdateLastIdMessage(result.Data);
                }
            }
            else Methods.DisplayReportResult(MainWindowActivity, respond);
        }

        public static void UpdateLastIdMessage(List<MessageData> chatMessages)
        {
            try
            {
                MessageData messageInfo = chatMessages?.FirstOrDefault();
                if (messageInfo != null)
                {
                    var typeModel = Holders.GetTypeModel(messageInfo);
                    if (typeModel == MessageModelType.None)
                        return;

                    var checker = MainWindowActivity?.MAdapter.DifferList?.FirstOrDefault(a => a.MesData.Id == messageInfo.MessageHashId);
                    if (checker != null)
                    {
                        var message = WoWonderTools.MessageFilter(messageInfo.ToId, messageInfo, typeModel, true);
                        message.ModelType = typeModel;
                        message.BtnDownload = true;

                        checker.MesData = message;
                        checker.Id = Java.Lang.Long.ParseLong(message.Id);
                        checker.TypeView = typeModel;

                        #region LastChat

                        var updaterGroup = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == message.GroupId);
                        if (updaterGroup?.LastChat.LastMessage.LastMessageClass != null)
                        {
                            var index = GlobalContext.ChatTab.LastGroupChatsTab.MAdapter.LastChatsList.IndexOf(GlobalContext.ChatTab?.LastGroupChatsTab.MAdapter.LastChatsList.FirstOrDefault(x => x.LastChat?.GroupId == message.GroupId));
                            if (index > -1)
                            {
                                if (typeModel == MessageModelType.RightGif)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendGifFile);
                                else if (typeModel == MessageModelType.RightText)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : MainWindowActivity?.GetText(Resource.String.Lbl_SendMessage);
                                else if (typeModel == MessageModelType.RightSticker)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendStickerFile);
                                else if (typeModel == MessageModelType.RightContact)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendContactnumber);
                                else if (typeModel == MessageModelType.RightFile)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendFile);
                                else if (typeModel == MessageModelType.RightVideo)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendVideoFile);
                                else if (typeModel == MessageModelType.RightImage)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendImageFile);
                                else if (typeModel == MessageModelType.RightAudio)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendAudioFile);
                                else if (typeModel == MessageModelType.RightMap)
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = MainWindowActivity?.GetText(Resource.String.Lbl_SendLocationFile);
                                else
                                    updaterGroup.LastChat.LastMessage.LastMessageClass.Text = updaterGroup.LastChat?.LastMessage.LastMessageClass.Text;

                                GlobalContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (!updaterGroup.LastChat.IsPin)
                                        {
                                            var checkPin = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.LastChatsList.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                            if (checkPin != null)
                                            {
                                                var toIndex = GlobalContext.ChatTab.LastGroupChatsTab.MAdapter.LastChatsList.IndexOf(checkPin) + 1;
                                                GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.LastChatsList.Move(index, toIndex);
                                                GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.NotifyItemMoved(index, toIndex);
                                            }
                                            else
                                            {
                                                if (ListUtils.FriendRequestsList.Count > 0)
                                                {
                                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.LastChatsList.Move(index, 1);
                                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.NotifyItemMoved(index, 1);
                                                }
                                                else
                                                {
                                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.LastChatsList.Move(index, 0);
                                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter.NotifyItemMoved(index, 0);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        else
                        {
                            //insert new user  
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                        }

                        #endregion

                        GlobalContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                //Update data RecyclerView Messages.
                                if (message.ModelType != MessageModelType.RightSticker || message.ModelType != MessageModelType.RightImage || message.ModelType != MessageModelType.RightMap || message.ModelType != MessageModelType.RightVideo)
                                    MainWindowActivity.UpdateOneMessage(checker.MesData);

                                if (UserDetails.SoundControl)
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}