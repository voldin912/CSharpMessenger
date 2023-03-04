using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Tab;
using WoWonder.Adapters;
using WoWonder.Helpers.Jobs;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.PageChat;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using MessageData = WoWonderClient.Classes.Message.MessageData;

namespace WoWonder.Helpers.Controller
{
    public static class PageMessageController
    {
        //############# DONT'T MODIFY HERE ############# 
        private static PageChatWindowActivity MainWindowActivity;

        private static ChatTabbedMainActivity GlobalContext;

        //========================= Functions ========================= 
        public static async Task SendMessageTask(PageChatWindowActivity windowActivity, string pageId, string id, string chatId, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            try
            {
                MainWindowActivity = windowActivity;

                GlobalContext = ChatTabbedMainActivity.GetInstance();

                if (!string.IsNullOrEmpty(pathFile))
                {
                    new UploadSingleFileToServerWorker(windowActivity, "PageChatWindowActivity").UploadFileToServer(windowActivity, new FileModel
                    {
                        MessageHashId = messageId,
                        ChatId = chatId,
                        UserId = id,
                        PageId = pageId,
                        FilePath = pathFile,
                        ReplyId = replyId,
                    });
                }
                else
                {
                    StartApiService(pageId, id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId);
                }
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void StartApiService(string pageId, string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(MainWindowActivity, MainWindowActivity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SendMessage(pageId, id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId) });
        }

        private static async Task SendMessage(string pageId, string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "", string replyId = "")
        {
            var (apiStatus, respond) = await RequestsAsync.PageChat.SendMessageToPageChatAsync(pageId, id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng, replyId);
            if (apiStatus == 200)
            {
                if (respond is PageSendMessageObject result)
                {
                    UpdateLastIdMessage(result.Data, pageId, id);
                }
            }
            else Methods.DisplayReportResult(MainWindowActivity, respond);
        }

        public static void UpdateLastIdMessage(List<MessageData> chatMessages, string pageId, string id)
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

                        var updaterPage = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.PageId == message.PageId && a.LastChat?.ChatType == "page");
                        if (updaterPage?.LastChat != null)
                        {
                            var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterPage);
                            if (index > -1)
                            {
                                updaterPage.LastChat.LastMessage.LastMessageClass.Text = typeModel switch
                                {
                                    MessageModelType.RightGif => MainWindowActivity?.GetText(Resource.String.Lbl_SendGifFile),
                                    MessageModelType.RightText => !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : MainWindowActivity?.GetText(Resource.String.Lbl_SendMessage),
                                    MessageModelType.RightSticker => MainWindowActivity?.GetText(Resource.String.Lbl_SendStickerFile),
                                    MessageModelType.RightContact => MainWindowActivity?.GetText(Resource.String.Lbl_SendContactnumber),
                                    MessageModelType.RightFile => MainWindowActivity?.GetText(Resource.String.Lbl_SendFile),
                                    MessageModelType.RightVideo => MainWindowActivity?.GetText(Resource.String.Lbl_SendVideoFile),
                                    MessageModelType.RightImage => MainWindowActivity?.GetText(Resource.String.Lbl_SendImageFile),
                                    MessageModelType.RightAudio => MainWindowActivity?.GetText(Resource.String.Lbl_SendAudioFile),
                                    MessageModelType.RightMap => MainWindowActivity?.GetText(Resource.String.Lbl_SendLocationFile),
                                    _ => updaterPage.LastChat?.LastMessage.LastMessageClass.Text
                                };

                                GlobalContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (updaterPage.LastChat.Mute?.Pin == "no")
                                        {
                                            var checkPin = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.Mute?.Pin == "yes");
                                            if (checkPin != null)
                                            {
                                                var toIndex = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                if (index != toIndex)
                                                {
                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, toIndex);
                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, toIndex);
                                                }

                                                GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(toIndex, "WithoutBlobText");
                                            }
                                            else
                                            {
                                                if (ListUtils.FriendRequestsList.Count > 0)
                                                {
                                                    if (index != 1)
                                                    {
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 1);
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 1);
                                                    }

                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(1, "WithoutBlobText");
                                                }
                                                else
                                                {
                                                    if (index != 0)
                                                    {
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                    }

                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });

                                SqLiteDatabase dbSqLite = new SqLiteDatabase();
                                //Update All data users to database
                                dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterPage.LastChat);
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