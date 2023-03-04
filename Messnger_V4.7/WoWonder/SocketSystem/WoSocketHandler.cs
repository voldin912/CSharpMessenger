using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SocketIOClient;
using SocketIOClient.Ext_Json;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Tab;
using WoWonder.Activities.Tab.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Socket;
using Exception = System.Exception;

namespace WoWonder.SocketSystem
{
    public class WoSocketHandler
    {
        public SocketIO Client;
        public static bool IsJoined;
        private static ChatTabbedMainActivity GlobalContext;
        private int MTries;

        public void InitStart()
        {
            try
            {
                GlobalContext = ChatTabbedMainActivity.GetInstance();

                DisconnectSocket();

                //string port = ListUtils.SettingsSiteList?.NodejsSsl == "1" ? ListUtils.SettingsSiteList?.NodejsSslPort : ListUtils.SettingsSiteList?.NodejsPort;

                var options = new SocketIOOptions
                {
                    // low-level engine options
                    Transport = AppSettings.Transport,
                    AutoUpgrade = false,
                    SslProtocols = AppSettings.TurnSecurityProtocolType3072On,
                    Path = "/socket.io",
                    Query = null,
                    //ExtraHeaders = new Dictionary<string, string>()
                    //{
                    //    {"Connection", "Upgrade"},
                    //    {"Upgrade", "websocket"},
                    //    {"Sec-WebSocket-Version", "13"},
                    //    {"Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits"},
                    //},

                    // Manager options
                    EIO = EngineIO.V3,
                    Reconnection = true,
                    ReconnectionAttempts = int.MaxValue,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 5000,
                    RandomizationFactor = 0.5,
                    ConnectionTimeout = TimeSpan.FromSeconds(20),

                    Auth = null,
                };

                string website = InitializeWoWonder.WebsiteUrl;
                char last = InitializeWoWonder.WebsiteUrl.Last();
                if (last.Equals('/'))
                {
                    website = InitializeWoWonder.WebsiteUrl.Remove(InitializeWoWonder.WebsiteUrl.Length - 1, 1);
                }

                Client = new SocketIO(new Uri($"{website}:{AppSettings.PortSocketServer}"), options);
                if (Client != null)
                {
                    var jsonSerializer = new NewtonsoftJsonSerializer
                    {
                        OptionsProvider = () => new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            }
                        }
                    };
                    Client.JsonSerializer = jsonSerializer;
                    
                    WoSocketEvents events = new WoSocketEvents();
                    events.InitEvents(Client);
                     
                    Emit_Connect(UserDetails.Username, UserDetails.AccessToken);
                }
            }
            catch (OperationCanceledException e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async void Emit_Connect(string username, string accessToken)
        {
            try
            {
                if (Client != null)
                {
                    Client.OnConnected += null;
                    Client.OnConnected += (sender, args) =>
                    {
                        try
                        {
                            Console.WriteLine("Socket_OnConnected");
                            Console.WriteLine("Socket.Id:" + Client.Id);
                             
                            //Add all On_ functions here 
                            //Socket_On_Alert(Client);
                            Socket_On_Private_Message(Client);
                            Socket_On_Private_PageMessage(Client);
                            Socket_On_Private_GroupMessage(Client);
                            //Socket_On_Private_Message_page(Client); 
                            Socket_On_User_Status_Change(Client);
                            Socket_On_RecordingEvent(Client);
                            Socket_On_TypingEvent(Client);
                            Socket_On_StopTypingEvent(Client);
                            Socket_On_Seen_Messages(Client);
                            Socket_On_new_video_call(Client);
                            Socket_On_loggedintEvent(Client);
                            Socket_On_loggedoutEvent(Client);
                            Socket_On_message_reaction(Client);
                            //Socket_On_Private_PageMessage(Client);

                            Emit_Join(username, accessToken);
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    };
                    await Client.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
         
        public async void Emit_Join(string username, string accessToken)
        {
            try
            {
                if (Client == null)
                    InitStart();

                if (Client != null)
                {
                    if (!Client.Connected)
                    {
                        Emit_Connect(username, accessToken);
                        return;
                    }

                    if (IsJoined)
                        return;

                    Dictionary<string, string> value = new Dictionary<string, string>
                    {
                        {"username", username}, {"user_id", accessToken}
                    };

                    await Client.EmitAsync("join", response =>
                    {
                        try
                        {
                            Console.WriteLine("Socket.Id:" + Client.Id);
                            Console.WriteLine("Socket_joined");
                            IsJoined = true;

                            var result = response;
                            Console.WriteLine(result);
                            MTries = 0;
                            Socket_ping_for_lastseen(UserDetails.AccessToken);

                            if (UserDetails.OnlineUsers)
                                Emit_loggedintEvent(UserDetails.AccessToken);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }, value);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #region User

        //======================= Emit Async ==========================

        //set type text
        public async void EmitAsync_RecordingEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                //Console.WriteLine("Socket.Id:" + Client?.Id);

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("recording", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set type text
        public async void EmitAsync_TypingEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                //Console.WriteLine("Socket.Id:" + Client?.Id);

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("typing", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set Stop text
        public async void EmitAsync_StoppedEvent(string recipientId, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}
                };

                await Client?.EmitAsync("typing_done", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //set seen messages
        public async void EmitAsync_SendSeenMessages(string recipientId, string accessToken, string fromUserId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"recipient_id", recipientId}, {"user_id", accessToken}, {"current_user_id", fromUserId}
                };

                await Client?.EmitAsync("seen_messages", value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Send Message text
        public async void EmitAsync_SendMessage(string toId, string accessToken, string username, string msg, string color, string messageReplyId, string messageHashId, string storyId = "", string lat = "", string lng = "")
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                if (string.IsNullOrEmpty(messageReplyId))
                    messageReplyId = "0";

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"to_id", toId},
                    {"from_id", accessToken},
                    {"username", username},
                    {"msg", msg},
                    {"color", color},
                    {"message_reply_id", messageReplyId},
                    {"story_id", storyId},
                    {"lat", lat},
                    {"lng", lng},
                    {"isSticker", "false"}
                };

                await Client?.EmitAsync("private_message", response =>
                {
                    try
                    {
                        var json = response.GetValue();
                        var result = response.GetValue<PrivateMessageObject>();
                        if (result != null)
                        { 
                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(async () =>
                            {
                                try
                                {
                                    AdapterModelsClassMessage checker = chatWindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == messageHashId);
                                    if (checker != null)
                                    {
                                        //Update data message and get type
                                        checker.Id = Long.ParseLong(result.MessageId);
                                        checker.MesData.Id = result.MessageId;
                                        checker.MesData.Seen = "0";

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Update_To_one_MessagesTable(checker.MesData);

                                        chatWindowActivity?.UpdateOneMessage(checker.MesData);

                                        if (UserDetails.SoundControl)
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");

                                        await Task.Delay(1500);

                                        if (Methods.CheckConnectivity())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });
                                    }

                                    #region LastChat

                                    var typeModel = Holders.GetTypeModel(result);
                                    if (typeModel == MessageModelType.None)
                                        return;
                                     
                                    var updaterUser = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.Receiver && a.LastChat?.ChatType == "user");
                                    if (updaterUser?.LastChat != null)
                                    {
                                        var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterUser);
                                        if (index > -1)
                                        {
                                            switch (typeModel)
                                            {
                                                case MessageModelType.LeftGif:
                                                case MessageModelType.RightGif:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                                    break;
                                                case MessageModelType.LeftText:
                                                case MessageModelType.RightText:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(result.Message) ? Methods.FunString.DecodeString(result.Message) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                                    break;
                                                case MessageModelType.LeftSticker:
                                                case MessageModelType.RightSticker:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                                    break;
                                                case MessageModelType.LeftContact:
                                                case MessageModelType.RightContact:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                                    break;
                                                case MessageModelType.LeftFile:
                                                case MessageModelType.RightFile:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                                    break;
                                                case MessageModelType.LeftVideo:
                                                case MessageModelType.RightVideo:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                                    break;
                                                case MessageModelType.LeftImage:
                                                case MessageModelType.RightImage:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                                    break;
                                                case MessageModelType.LeftAudio:
                                                case MessageModelType.RightAudio:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                                    break;
                                                case MessageModelType.LeftMap:
                                                case MessageModelType.RightMap:
                                                    updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);
                                                    break;
                                            }
                                             
                                            GlobalContext?.RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    if (updaterUser.LastChat.Mute?.Pin == "no")
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
                                                            if (index != 0)
                                                            {
                                                                GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                                GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                            }

                                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
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
                                            dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterUser?.LastChat);

                                        }
                                    }
                                    else
                                    {
                                        //insert new user  
                                        LastChatFragment.ApiRun = false;
                                        if (Methods.CheckConnectivity())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                                    }

                                    #endregion 
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }); 
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        public static void Socket_On_Seen_Messages(SocketIO client)
        {
            try
            {
                if (client != null)
                { 
                    client?.On("seen_messages", response =>
                    {
                        try
                        {
                            var result = response.GetValue();
                            Console.WriteLine(result);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });

                    client?.On("lastseen", response =>
                    {
                        try
                        {
                            var result = response.GetValue<ChatLastSeenObject>();
                            if (result != null)
                            { 
                                var chatWindowActivity = ChatWindowActivity.GetInstance();
                                chatWindowActivity?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (Methods.CheckConnectivity())
                                        {
                                            if (!string.IsNullOrEmpty(result.MessageId))
                                            {
                                                AdapterModelsClassMessage checker = chatWindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == result.MessageId);
                                                if (checker != null)
                                                {
                                                    //Update data message and get type 
                                                    checker.MesData.Seen = result.Seen;

                                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.Insert_Or_Update_To_one_MessagesTable(checker.MesData);

                                                    chatWindowActivity?.UpdateOneMessage(checker.MesData);
                                                     
                                                    //await Task.Delay(1500);

                                                    //if (Methods.CheckConnectivity())
                                                    //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });
                                                }
                                            } 
                                            else
                                            {
                                                if (chatWindowActivity.UserId == result.UserId)
                                                {
                                                    chatWindowActivity.UpdaterRun = false;
                                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.LoadMessageApi(false) });
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
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get new user in last user messages 
        public static void Socket_On_User_Status_Change(SocketIO client)
        {
            try
            {
                client?.On("user_status_change", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is recording now
        public void Socket_On_RecordingEvent(SocketIO client)
        {
            try
            {
                client?.On("recording", response =>
                {
                    try
                    {
                        var json = response.GetValue();

                        var result = response.GetValue<ChatTypingObject>();
                        Console.WriteLine(result);
                        if (result != null)
                        {
                            GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                            var data = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.SenderId);
                            if (data != null)
                            {
                                Console.WriteLine(data);
                            }

                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (chatWindowActivity.UserId == result.SenderId)
                                    {
                                        var typing = result.IsTyping;
                                        chatWindowActivity.TxtLastTime.Text = typing == "200" ? chatWindowActivity.GetString(Resource.String.Lbl_Typping) : chatWindowActivity.LastSeen ?? chatWindowActivity.LastSeen;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is Typing now
        public void Socket_On_TypingEvent(SocketIO client)
        {
            try
            {
                client?.On("typing", response =>
                {
                    try
                    {
                        var result = response.GetValue<ChatTypingObject>();
                        Console.WriteLine(result);
                        if (result != null)
                        {
                            GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                            var data = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.SenderId);
                            if (data != null)
                            {
                                Console.WriteLine(data);
                            }

                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (chatWindowActivity.UserId == result.SenderId)
                                    {
                                        var typing = result.IsTyping;
                                        chatWindowActivity.TxtLastTime.Text = typing == "200" ? chatWindowActivity.GetString(Resource.String.Lbl_Typping) : chatWindowActivity.LastSeen ?? chatWindowActivity.LastSeen;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check who is finish Typing
        public void Socket_On_StopTypingEvent(SocketIO client)
        {
            try
            {
                client?.On("typing_done", response =>
                {
                    try
                    {
                        var result = response?.GetValue<ChatTypingObject>();
                        if (result != null)
                        {
                            GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                            var data = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.SenderId);
                            if (data != null)
                            {
                                Console.WriteLine(data);
                            }

                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (chatWindowActivity.UserId == result.SenderId)
                                    {
                                        chatWindowActivity.TxtLastTime.Text = chatWindowActivity.LastSeen;
                                    } 
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get New Message 
        public static void Socket_On_Private_Message(SocketIO client)
        {
            try
            {
                client?.On("private_message", response =>
                {
                    try
                    {
                        var json = response.GetValue();
                        var result = response.GetValue<PrivateMessageObject>();
                        if (result != null)
                        { 
                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            if (chatWindowActivity != null)
                            {
                                if (Methods.AppLifecycleObserver.AppState == "Background")
                                    AppNotificationsManager.Instance?.ShowUserNotification("user", result.MessageId, result.Username, result.Message, result.Sender, result.Sender, result.Avatar, AppSettings.MainColor);

                                var typeModel = Holders.GetTypeModel(result); 
                                if (chatWindowActivity.UserId == result.Sender || chatWindowActivity.UserId == result.Receiver || chatWindowActivity.UserId == result.Id)
                                {
                                    var dataProfile = new UserDataObject();

                                    MessageDataExtra message = new MessageDataExtra
                                    {
                                        Id = result.MessageId,
                                        Seen = "0",
                                        Time = result.TimeApi?.ToString(),
                                        TimeText = Methods.Time.TimeAgo(result.TimeApi.Value),
                                        ModelType = typeModel,
                                        ErrorSendMessage = false,
                                    };
                                     
                                    if (result.Sender == UserDetails.UserId) // right
                                    {
                                        message.ToId = result.Receiver;
                                        message.FromId = UserDetails.UserId;
                                        message.Position = "right";
                                        dataProfile = ListUtils.MyProfileList.FirstOrDefault();
                                    }
                                    else if (result.Receiver == UserDetails.UserId) // left
                                    {
                                        message.ToId = UserDetails.UserId;
                                        message.FromId = result.Sender;
                                        message.Position = "left";
                                        dataProfile = chatWindowActivity.DataUser.UserData;
                                    }

                                    message.UserData = dataProfile;
                                    message.MessageUser = new MessageUserUnion {UserDataClass = dataProfile}; 

                                    switch (typeModel)
                                    {
                                        case MessageModelType.LeftText:
                                        case MessageModelType.RightText:
                                            message.Text = result.Message;
                                            break;
                                        case MessageModelType.LeftAudio:
                                        case MessageModelType.RightAudio:
                                        case MessageModelType.LeftImage:
                                        case MessageModelType.RightImage:
                                        case MessageModelType.LeftVideo:
                                        case MessageModelType.RightVideo:
                                        case MessageModelType.LeftFile:
                                        case MessageModelType.RightFile:
                                            message.Media = result.MediaLink;
                                            break;
                                        case MessageModelType.LeftMap:
                                        case MessageModelType.RightMap:
                                            message.Lat = result.Lat;
                                            message.Lng = result.Lng;
                                            break;
                                        case MessageModelType.LeftSticker:
                                        case MessageModelType.RightSticker:
                                        case MessageModelType.LeftGif:
                                        case MessageModelType.RightGif:
                                        case MessageModelType.LeftContact:
                                        case MessageModelType.RightContact:
                                        case MessageModelType.LeftProduct:
                                        case MessageModelType.RightProduct:
                                        case MessageModelType.None:
                                        default:
                                            break;
                                    }

                                    chatWindowActivity.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                    {
                                        TypeView = typeModel,
                                        Id = Long.ParseLong(result.MessageId),
                                        MesData = WoWonderTools.MessageFilter(chatWindowActivity.UserId, message, typeModel, true)
                                    });

                                    chatWindowActivity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var indexMes = chatWindowActivity.MAdapter.DifferList.IndexOf(chatWindowActivity.MAdapter.DifferList.Last());
                                            chatWindowActivity.MAdapter.NotifyItemInserted(indexMes);

                                            //Scroll Down >> 
                                            chatWindowActivity.MRecycler.ScrollToPosition(chatWindowActivity.MAdapter.ItemCount - 1);
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                    
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });

                                    UserDetails.Socket?.EmitAsync_SendSeenMessages(result.Sender, UserDetails.AccessToken, UserDetails.UserId);
                                }
                                else
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("user", result.MessageId, result.Username, result.Message, result.Sender, result.Sender, result.Avatar, AppSettings.MainColor);
                                }
                                     
                                #region LastChat
                                 
                                if (typeModel == MessageModelType.None)
                                    return;

                                var updaterUser = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.Receiver && a.LastChat?.ChatType == "user");
                                if (updaterUser?.LastChat != null)
                                {
                                    var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterUser);
                                    if (index > -1)
                                    { 
                                        switch (typeModel)
                                        {
                                            case MessageModelType.LeftGif:
                                            case MessageModelType.RightGif:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                                break;
                                            case MessageModelType.LeftText:
                                            case MessageModelType.RightText:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(result.Message) ? Methods.FunString.DecodeString(result.Message) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                                break;
                                            case MessageModelType.LeftSticker:
                                            case MessageModelType.RightSticker:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                                break;
                                            case MessageModelType.LeftContact:
                                            case MessageModelType.RightContact:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                                break;
                                            case MessageModelType.LeftFile:
                                            case MessageModelType.RightFile:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                                break;
                                            case MessageModelType.LeftVideo:
                                            case MessageModelType.RightVideo:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                                break;
                                            case MessageModelType.LeftImage:
                                            case MessageModelType.RightImage:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                                break;
                                            case MessageModelType.LeftAudio:
                                            case MessageModelType.RightAudio:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                                break;
                                            case MessageModelType.LeftMap:
                                            case MessageModelType.RightMap:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);
                                                break; 
                                        }
                                         
                                        GlobalContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                if (updaterUser.LastChat.Mute?.Pin == "no")
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
                                                        if (index != 0)
                                                        {
                                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                        }

                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
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
                                        dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterUser?.LastChat);

                                    }
                                }
                                else
                                {
                                    //insert new user  
                                    LastChatFragment.ApiRun = false;
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                                }

                                #endregion
                            }
                            else
                            {
                                ListUtils.MessageUnreadList ??= new ObservableCollection<PrivateMessageObject>();

                                GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                                var updaterUser = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.Sender && a.LastChat?.ChatType == "user");
                                if (updaterUser?.LastChat != null)
                                {
                                    var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterUser);
                                    if (index > -1)
                                    {
                                        var typeModel = Holders.GetTypeModel(result);

                                        switch (typeModel)
                                        {
                                            case MessageModelType.LeftGif:
                                            case MessageModelType.RightGif:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                                break;
                                            case MessageModelType.LeftText:
                                            case MessageModelType.RightText:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(result.Message) ? Methods.FunString.DecodeString(result.Message) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                                break;
                                            case MessageModelType.LeftSticker:
                                            case MessageModelType.RightSticker:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                                break;
                                            case MessageModelType.LeftContact:
                                            case MessageModelType.RightContact:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                                break;
                                            case MessageModelType.LeftFile:
                                            case MessageModelType.RightFile:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                                break;
                                            case MessageModelType.LeftVideo:
                                            case MessageModelType.RightVideo:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                                break;
                                            case MessageModelType.LeftImage:
                                            case MessageModelType.RightImage:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                                break;
                                            case MessageModelType.LeftAudio:
                                            case MessageModelType.RightAudio:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                                break;
                                            case MessageModelType.LeftMap:
                                            case MessageModelType.RightMap:
                                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);
                                                break; 
                                        }

                                        GlobalContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                if (updaterUser.LastChat.Mute?.Pin == "no")
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
                                                        if (index != 0)
                                                        {
                                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                            GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                        }

                                                        GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
                                                    }
                                                }
                                                else
                                                {
                                                    GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.NotifyItemChanged(index, "WithoutBlobText");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });

                                        SqLiteDatabase dbSqLite = new SqLiteDatabase();
                                        //Update All data users to database
                                        dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterUser?.LastChat);

                                    }
                                     
                                    if (result.IsMedia != null && result.IsMedia.Value)
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Sender,
                                                Message = GlobalContext.GetText(Resource.String.Lbl_SendMessage)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("user", result.MessageId, Methods.FunString.DecodeString(result.Username), GlobalContext.GetText(Resource.String.Lbl_SendMessage), result.Sender, result.Sender, result.Avatar, AppSettings.MainColor, Convert.ToInt32(updaterUser.LastChat.MessageCount));
                                    }
                                    else
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Sender,
                                                Message = Methods.FunString.DecodeString(result.Message)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("user", result.MessageId, Methods.FunString.DecodeString(result.Username), Methods.FunString.DecodeString(result.Message), result.Sender, result.Sender, result.Avatar, AppSettings.MainColor, Convert.ToInt32(updaterUser.LastChat.MessageCount));
                                    }
                                }
                                else 
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("user", result.MessageId, result.Username, result.Message, result.Sender, result.Sender, result.Avatar, AppSettings.MainColor);
                                  
                                    //insert new user  
                                    LastChatFragment.ApiRun = false;
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                                }
                            }  
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Page

        //======================= Emit Async ==========================

        //Send Page Message text
        public async void EmitAsync_SendPageMessage(string pageId, string toId, string accessToken, string username, string msg, string messageReplyId, string messageHashId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                if (string.IsNullOrEmpty(messageReplyId))
                    messageReplyId = "0";

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"page_id", pageId},
                    {"to_id", toId},
                    {"from_id", accessToken},
                    {"username", username},
                    {"msg", msg},
                    {"message_reply_id", messageReplyId },
                    {"isSticker", "false"}
                };

                await Client?.EmitAsync("page_message", response =>
                {
                    try
                    {
                        //var json = response.GetValue();
                        var result = response.GetValue<PrivateMessageObject>();
                        if (result != null)
                        { 
                            var chatWindowActivity = PageChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(async () =>
                            {
                                try
                                {
                                    AdapterModelsClassMessage checker = chatWindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == messageHashId);
                                    if (checker != null)
                                    {
                                        //Update data mesasage and get type
                                        checker.Id = Long.ParseLong(result.MessageId);
                                        checker.MesData.Id = result.MessageId;
                                        checker.MesData.Seen = "0";

                                        chatWindowActivity?.UpdateOneMessage(checker.MesData);

                                        if (UserDetails.SoundControl)
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");

                                        await Task.Delay(1500);

                                        if (Methods.CheckConnectivity())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                             
                            #region LastChat

                            var typeModel = Holders.GetTypeModel(result);
                            if (typeModel == MessageModelType.None)
                                return;

                            var updaterPage = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.PageId == pageId && a.LastChat?.ChatType == "page");
                            if (updaterPage?.LastChat != null)
                            {
                                var index = GlobalContext.ChatTab.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterPage);
                                if (index > -1)
                                {
                                    updaterPage.LastChat.LastMessage.LastMessageClass.Text = typeModel switch
                                    {
                                        MessageModelType.RightGif => GlobalContext?.GetText(Resource.String.Lbl_SendGifFile),
                                        MessageModelType.RightText => !string.IsNullOrEmpty(result.Message) ? Methods.FunString.DecodeString(result.Message) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage),
                                        MessageModelType.RightSticker => GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile),
                                        MessageModelType.RightContact => GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber),
                                        MessageModelType.RightFile => GlobalContext?.GetText(Resource.String.Lbl_SendFile),
                                        MessageModelType.RightVideo => GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile),
                                        MessageModelType.RightImage => GlobalContext?.GetText(Resource.String.Lbl_SendImageFile),
                                        MessageModelType.RightAudio => GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile),
                                        MessageModelType.RightMap => GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile),
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

                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        //Get New PageMessage 
        public static void Socket_On_Private_PageMessage(SocketIO client)
        {
            try
            {
                client?.On("page_message", response =>
                {
                    try
                    {
                        var json = response.GetValue();
                        var result = response.GetValue<PrivateMessageObject>();
                        if (result != null)
                        { 
                            var typeModel = Holders.GetTypeModel(result);

                            var chatWindowActivity = PageChatWindowActivity.GetInstance();
                            if (chatWindowActivity != null)
                            { 
                                if (Methods.AppLifecycleObserver.AppState == "Background")
                                    AppNotificationsManager.Instance?.ShowUserNotification("page", result.MessageId, result.Username, result.Message, result.Sender, result.Sender, result.Avatar, AppSettings.MainColor);

                                //wael check PageID 
                                if (chatWindowActivity.UserId == result.Sender || chatWindowActivity.UserId == result.Receiver || chatWindowActivity.UserId == result.Id)
                                {
                                    var dataProfile = new UserDataObject();

                                    MessageDataExtra message = new MessageDataExtra
                                    {
                                        Id = result.MessageId,
                                        Seen = "0",
                                        Time = result.TimeApi?.ToString(),
                                        TimeText = Methods.Time.TimeAgo(result.TimeApi.Value),
                                        ModelType = typeModel,
                                        ErrorSendMessage = false,
                                    };

                                    if (result.Sender == UserDetails.UserId) // right
                                    {
                                        message.ToId = result.Receiver;
                                        message.FromId = UserDetails.UserId;
                                        message.Position = "right";
                                        dataProfile = ListUtils.MyProfileList.FirstOrDefault();
                                    }
                                    else if (result.Receiver == UserDetails.UserId) // left
                                    {
                                        message.ToId = result.Sender;
                                        message.FromId = UserDetails.UserId;
                                        message.Position = "left";
                                        dataProfile = chatWindowActivity.PageData.UserData;
                                    }

                                    message.UserData = dataProfile;
                                    message.MessageUser = new MessageUserUnion { UserDataClass = dataProfile };

                                    switch (typeModel)
                                    {
                                        case MessageModelType.LeftText:
                                        case MessageModelType.RightText:
                                            message.Text = result.Message;
                                            break;
                                        case MessageModelType.LeftAudio:
                                        case MessageModelType.RightAudio:
                                        case MessageModelType.LeftImage:
                                        case MessageModelType.RightImage:
                                        case MessageModelType.LeftVideo:
                                        case MessageModelType.RightVideo:
                                        case MessageModelType.LeftFile:
                                        case MessageModelType.RightFile:
                                            message.Media = result.MediaLink;
                                            break;
                                        case MessageModelType.LeftMap:
                                        case MessageModelType.RightMap:
                                            message.Lat = result.Lat;
                                            message.Lng = result.Lng;
                                            break;
                                        case MessageModelType.LeftSticker:
                                        case MessageModelType.RightSticker:
                                        case MessageModelType.LeftGif:
                                        case MessageModelType.RightGif:
                                        case MessageModelType.LeftContact:
                                        case MessageModelType.RightContact:
                                        case MessageModelType.LeftProduct:
                                        case MessageModelType.RightProduct:
                                        case MessageModelType.None:
                                        default:
                                            break;
                                    }

                                    chatWindowActivity.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                    {
                                        TypeView = typeModel,
                                        Id = Long.ParseLong(result.MessageId),
                                        MesData = WoWonderTools.MessageFilter(chatWindowActivity.PageId, message, typeModel, true)
                                    });

                                    chatWindowActivity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var indexMes = chatWindowActivity.MAdapter.DifferList.IndexOf(chatWindowActivity.MAdapter.DifferList.Last());
                                            chatWindowActivity.MAdapter.NotifyItemInserted(indexMes);

                                            //Scroll Down >> 
                                            chatWindowActivity.MRecycler.ScrollToPosition(chatWindowActivity.MAdapter.ItemCount - 1);
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });

                                    UserDetails.Socket?.EmitAsync_SendSeenMessages(result.Sender, UserDetails.AccessToken, UserDetails.UserId);
                                } 
                                else
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("page", result.MessageId, result.Username, result.Message, result.Sender, result.Sender, result.Avatar, AppSettings.MainColor);
                                }
                            }
                            else
                            {
                                ListUtils.MessageUnreadList ??= new ObservableCollection<PrivateMessageObject>();

                                GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                                var updaterPage = GlobalContext?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == result.Sender && a.LastChat?.ChatType == "page");
                                if (updaterPage != null)
                                {
                                    if (result.IsMedia != null && result.IsMedia.Value)
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Sender,
                                                Message = GlobalContext.GetText(Resource.String.Lbl_SendMessage)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("page", result.MessageId, Methods.FunString.DecodeString(result.Username), GlobalContext.GetText(Resource.String.Lbl_SendMessage), result.Sender, result.Sender, result.Avatar, AppSettings.MainColor, Convert.ToInt32(updaterPage.LastChat.MessageCount));
                                    }
                                    else
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Sender && a.Message == Methods.FunString.DecodeString(result.Message));
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Sender,
                                                Message = Methods.FunString.DecodeString(result.Message)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("page", result.MessageId, Methods.FunString.DecodeString(result.Username), Methods.FunString.DecodeString(result.Message), result.Sender, result.Sender, result.Avatar, AppSettings.MainColor, Convert.ToInt32(updaterPage.LastChat.MessageCount));
                                    }
                                }
                                else  
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("page", result.MessageId, "", result.Message, result.Sender, result.Sender, "", AppSettings.MainColor);
                                   
                                    //insert new user  
                                    LastChatFragment.ApiRun = false;
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AppUpdaterHelper.LoadChatAsync() });
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Group

        //======================= Emit Async ==========================

        //Send Group Message text
        public async void EmitAsync_SendGroupMessage(string groupId, string accessToken, string username, string msg, string messageReplyId, string messageHashId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                if (string.IsNullOrEmpty(messageReplyId))
                    messageReplyId = "0";

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"group_id", groupId},
                    {"from_id", accessToken},
                    {"username", username},
                    {"msg", msg},
                    {"message_reply_id", messageReplyId},
                    {"isSticker", "false"}
                };

                await Client?.EmitAsync("group_message", response =>
                {
                    try
                    {
                        //var json = response.GetValue();
                        var result = response.GetValue<PrivateGroupMessageObject>();
                        if (result != null)
                        { 
                            var chatWindowActivity = GroupChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(async () =>
                            {
                                try
                                {
                                    AdapterModelsClassMessage checker = chatWindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == messageHashId);
                                    if (checker != null)
                                    {
                                        var type = Holders.GetTypeModel(result.NewMessage);

                                        //Update data mesasage and get type
                                        checker.Id = Convert.ToInt32(result.NewMessage.Id);
                                        checker.MesData = WoWonderTools.MessageFilter(result.GroupData.GroupId, result.NewMessage, type, true);
                                        checker.TypeView = type;

                                        chatWindowActivity?.UpdateOneMessage(checker.MesData);

                                        if (UserDetails.SoundControl)
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");

                                        await Task.Delay(1500);

                                        if (Methods.CheckConnectivity())
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.MessageId) });
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                             
                            #region LastChat

                            var typeModel = Holders.GetTypeModel(result.NewMessage);
                            if (typeModel == MessageModelType.None)
                                return;

                            var updaterGroup = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == result.NewMessage.GroupId);
                            if (updaterGroup?.LastChat?.LastMessage.LastMessageClass != null)
                            {
                                var index = GlobalContext.ChatTab.LastGroupChatsTab.MAdapter.LastChatsList.IndexOf(GlobalContext.ChatTab?.LastGroupChatsTab.MAdapter.LastChatsList.FirstOrDefault(x => x.LastChat?.GroupId == result.NewMessage.GroupId));
                                if (index > -1)
                                {
                                    switch (typeModel)
                                    {
                                        case MessageModelType.LeftGif:
                                        case MessageModelType.RightGif:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                            break;
                                        case MessageModelType.LeftText:
                                        case MessageModelType.RightText:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(result.NewMessage.Text) ? Methods.FunString.DecodeString(result.NewMessage.Text) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                            break;
                                        case MessageModelType.LeftSticker:
                                        case MessageModelType.RightSticker:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                            break;
                                        case MessageModelType.LeftContact:
                                        case MessageModelType.RightContact:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                            break;
                                        case MessageModelType.LeftFile:
                                        case MessageModelType.RightFile:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                            break;
                                        case MessageModelType.LeftVideo:
                                        case MessageModelType.RightVideo:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                            break;
                                        case MessageModelType.LeftImage:
                                        case MessageModelType.RightImage:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                            break;
                                        case MessageModelType.LeftAudio:
                                        case MessageModelType.RightAudio:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                            break;
                                        case MessageModelType.LeftMap:
                                        case MessageModelType.RightMap:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);
                                            break;
                                    }
                                     
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
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        //Get New GroupMessage 
        public static void Socket_On_Private_GroupMessage(SocketIO client)
        {
            try
            {
                client?.On("group_message",response =>
                {
                    try
                    {
                        //var json = response.GetValue();
                        var result = response.GetValue<PrivateGroupMessageObject>();
                        if (result != null)
                        { 
                            var chatWindowActivity = GroupChatWindowActivity.GetInstance();
                            if (chatWindowActivity != null)
                            {
                                if (Methods.AppLifecycleObserver.AppState == "Background")
                                    AppNotificationsManager.Instance?.ShowUserNotification("group", result.MessageId, result.GroupData.Username, result.NewMessage.Text, result.Id, result.Id, result.GroupData.Avatar, AppSettings.MainColor);

                                if (chatWindowActivity.GroupId == result.GroupData.GroupId)
                                {
                                    var type = Holders.GetTypeModel(result.NewMessage);
                                    if (type == MessageModelType.None)
                                        return;

                                    var mes = WoWonderTools.MessageFilter(chatWindowActivity.GroupId, result.NewMessage, type);

                                    var check = chatWindowActivity.MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == result.MessageId);
                                    if (check == null)
                                    {
                                        chatWindowActivity.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                        {
                                            TypeView = type,
                                            Id = Long.ParseLong(result.MessageId),
                                            MesData = mes
                                        });

                                        //if (countList > 0)
                                        //    MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                        //else
                                        chatWindowActivity.MAdapter.NotifyDataSetChanged();

                                        //Scroll Down >> 
                                        chatWindowActivity.MRecycler.ScrollToPosition(chatWindowActivity.MAdapter.ItemCount - 1);

                                        if (UserDetails.SoundControl)
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                    }
                                    else if (check.MesData.Seen == "0" && check.MesData.Seen != result.NewMessage.Seen)
                                    {
                                        check.Id = Convert.ToInt32(result.MessageId);
                                        check.MesData = mes;
                                        check.TypeView = type;

                                        if (check.MesData.Position == "right")
                                            chatWindowActivity.MAdapter.NotifyItemChanged(chatWindowActivity.MAdapter.DifferList.IndexOf(check));
                                    }
                                         
                                    //Wael add data Messages and get type 
                                    //UserDetails.Socket?.EmitAsync_SendSeenMessages(result.Sender, UserDetails.AccessToken, UserDetails.UserId);
                                } 
                                else
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("group", result.MessageId, result.GroupData.Username, result.NewMessage.Text, result.Id, result.Id, result.GroupData.Avatar, AppSettings.MainColor);
                                }    
                            }
                            else
                            {
                                ListUtils.MessageUnreadList ??= new ObservableCollection<PrivateMessageObject>();

                                GlobalContext ??= ChatTabbedMainActivity.GetInstance();
                                var checkGroup = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == result.GroupData?.GroupId && a.LastChat?.ChatType == "group");
                                if (checkGroup != null)
                                {
                                    if (!string.IsNullOrEmpty(result.NewMessage.Media))
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Id);
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Id,
                                                Message = GlobalContext.GetText(Resource.String.Lbl_SendMessage)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("group", result.MessageId, Methods.FunString.DecodeString(checkGroup.LastChat.Name), GlobalContext.GetText(Resource.String.Lbl_SendMessage), result.Id, result.Id, result.GroupData.Avatar, AppSettings.MainColor, Convert.ToInt32(checkGroup.LastChat.MessageCount));
                                    }
                                    else
                                    {
                                        var data = ListUtils.MessageUnreadList.FirstOrDefault(a => a.Sender == result.Id && a.Message == Methods.FunString.DecodeString(result.NewMessage.Text));
                                        if (data == null)
                                        {
                                            ListUtils.MessageUnreadList.Add(new PrivateMessageObject
                                            {
                                                Sender = result.Id,
                                                Message = Methods.FunString.DecodeString(result.NewMessage.Text)
                                            });
                                        }

                                        AppNotificationsManager.Instance?.ShowUserNotification("group", result.MessageId, Methods.FunString.DecodeString(checkGroup.LastChat.Name), Methods.FunString.DecodeString(result.NewMessage.Text), result.Id, result.Id, result.GroupData.Avatar, AppSettings.MainColor, Convert.ToInt32(checkGroup.LastChat.MessageCount));
                                    }
                                }
                                else
                                {
                                    AppNotificationsManager.Instance?.ShowUserNotification("group", result.MessageId, "", result.NewMessage.Text, result.Id, result.Id, "", AppSettings.MainColor);

                                    //insert new Group  
                                    GlobalContext?.ChatTab?.LastGroupChatsTab?.StartApiService();
                                }
                            }
                                  
                            #region LastChat

                            var typeModel = Holders.GetTypeModel(result.NewMessage);
                            if (typeModel == MessageModelType.None)
                                return;

                            var updaterGroup = GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == result.NewMessage.GroupId);
                            if (updaterGroup?.LastChat?.LastMessage.LastMessageClass != null)
                            {
                                var index = GlobalContext.ChatTab.LastGroupChatsTab.MAdapter.LastChatsList.IndexOf(GlobalContext.ChatTab?.LastGroupChatsTab.MAdapter.LastChatsList.FirstOrDefault(x => x.LastChat?.GroupId == result.NewMessage.GroupId));
                                if (index > -1)
                                {
                                    switch (typeModel)
                                    {
                                        case MessageModelType.LeftGif:
                                        case MessageModelType.RightGif:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                            break;
                                        case MessageModelType.LeftText:
                                        case MessageModelType.RightText:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(result.NewMessage.Text) ? Methods.FunString.DecodeString(result.NewMessage.Text) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                            break;
                                        case MessageModelType.LeftSticker:
                                        case MessageModelType.RightSticker:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                            break;
                                        case MessageModelType.LeftContact:
                                        case MessageModelType.RightContact:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                            break;
                                        case MessageModelType.LeftFile:
                                        case MessageModelType.RightFile:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                            break;
                                        case MessageModelType.LeftVideo:
                                        case MessageModelType.RightVideo:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                            break;
                                        case MessageModelType.LeftImage:
                                        case MessageModelType.RightImage:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                            break;
                                        case MessageModelType.LeftAudio:
                                        case MessageModelType.RightAudio:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                            break;
                                        case MessageModelType.LeftMap:
                                        case MessageModelType.RightMap:
                                            updaterGroup.LastChat.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);
                                            break;
                                    }

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
                                            else
                                            {
                                                GlobalContext?.ChatTab?.LastGroupChatsTab?.MAdapter?.NotifyItemChanged(index, "WithoutBlobText");
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
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region call

        //Check new video call 
        public void Socket_On_new_video_call(SocketIO client)
        {
            try
            {
                client?.On("new_video_call", response =>
                {
                    try
                    {
                        //var result = response.GetValue<NewVideoCallObject>();
                        LastChatFragment.ApiRun = false;

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => AppUpdaterHelper.LoadChatAsync(true)});
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Check new video call 
        public async void EmitAsync_Create_callEvent(string recipientId)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                //{to_id: toUSERID, type: 'create_video'}
                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"to_id", recipientId}, {"type", "create_video"}
                };

                await Client?.EmitAsync("user_notification", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Reaction

        public async void EmitAsync_message_reaction(string id, string reaction, string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                Dictionary<string, string> value = new Dictionary<string, string>
                {
                    {"type", "messages"},
                    {"id", id},
                    {"reaction", reaction},
                    {"user_id", accessToken},
                };

                await Client?.EmitAsync("register_reaction", response =>
                {
                    try
                    {
                        var result = response.GetValue();
                        Console.WriteLine(result);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public static void Socket_On_message_reaction(SocketIO client)
        {
            try
            {
                client?.On("register_reaction", response =>
                {
                    try
                    {
                        var json = response.GetValue();
                        var result = response.GetValue<ReactionMessageObject>();
                        if (result != null)
                        {
                            var chatWindowActivity = ChatWindowActivity.GetInstance();
                            chatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    //wael check what is value result.Reactions .. and add in list when work the event 
                                    //var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == result.Reactions).Value?.Id ?? "1";
                                         
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => chatWindowActivity.GetMessagesById(result.Id) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            var groupChatWindowActivity = GroupChatWindowActivity.GetInstance();
                            groupChatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => groupChatWindowActivity.GetMessagesById(result.Id) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            var pageChatWindowActivity = PageChatWindowActivity.GetInstance();
                            pageChatWindowActivity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => pageChatWindowActivity.GetMessagesById(result.Id) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region General

        //======================= Emit Async ==========================

        //online
        public async void Emit_loggedintEvent(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                Dictionary<string, string> value = new Dictionary<string, string> { { "from_id", accessToken } };

                await Client?.EmitAsync("on_user_loggedin", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //offline
        public async void Emit_loggedoutEvent(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                Dictionary<string, string> value = new Dictionary<string, string> { { "from_id", accessToken } };

                await Client?.EmitAsync("on_user_loggedoff", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, value)!;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //======================= On Async ==========================

        //online
        public void Socket_On_loggedintEvent(SocketIO client)
        {
            try
            {
                client?.On("on_user_loggedin", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //offline
        public void Socket_On_loggedoutEvent(SocketIO client)
        {
            try
            {
                client?.On("on_user_loggedoff", response =>
                {
                    try
                    {
                        //var result = response.GetValue();

                        Console.WriteLine(response);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        private static Timer Timer;

        //UPDATE USER LAST SEEN
        public async void Socket_ping_for_lastseen(string accessToken)
        {
            try
            {
                if (!Client.Connected || !IsJoined)
                    await Client?.ConnectAsync()!;

                //Console.WriteLine("Socket.Id:" + Client?.Id);

                if (Timer != null)
                    return;

                Timer = new Timer { Interval = 2000 };
                Timer.Elapsed += (o, eventArgs) =>
                {
                    try
                    {
                        Dictionary<string, string> valueDictionary = new Dictionary<string, string>
                        {
                            {"user_id", accessToken}
                        };

                        Client?.EmitAsync("ping_for_lastseen", valueDictionary);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };
                Timer.Enabled = true;
                Timer.Start();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void DisconnectSocket()
        {
            try
            {
                if (Client != null)
                {
                    //Client?.Off("alert");
                    Client?.Off("join");
                    Client?.Off("private_message");
                    Client?.Off("page_message");
                    Client?.Off("group_message");
                    Client?.Off("seen_messages");
                    Client?.Off("lastseen");
                    Client?.Off("user_status_change");
                    Client?.Off("recording");
                    Client?.Off("typing");
                    Client?.Off("typing_done");
                    Client?.Off("new_video_call");
                    Client?.Off("user_notification");
                    Client?.Off("on_user_loggedin");
                    Client?.Off("on_user_loggedoff");
                    Client?.Off("ping_for_lastseen");
                    Client?.Off("register_reaction");
                    Client?.Off("group_message_page");

                    Client?.DisconnectAsync();
                }
                Client = null;

                if (Timer != null)
                {
                    Timer.Stop();
                    Timer = null;
                }

                IsJoined = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ReconnectSocket()
        {
            try
            {
                if (MTries < 5)
                {
                    MTries++;

                    DisconnectSocket();

                    //Connect to socket with access token
                    UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                }
                else
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Error_connectServer), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get New Message 
        //public static void Socket_On_Private_Message_page(SocketIO client)
        //{
        //    try
        //    {
        //        client?.On("private_message_page", response =>
        //        {
        //            try
        //            {
        //                var json = response.GetValue();

        //                var result = response.GetValue<PrivateMessageObject>();
        //                if (result != null)
        //                {

        //                }
        //            }
        //            catch (Exception exception)
        //            {
        //                Methods.DisplayReportResultTrack(exception);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Methods.DisplayReportResultTrack(ex);
        //    }
        //}

        //public static void Socket_On_Alert(SocketIO client)
        //{
        //    try
        //    { 
        //        client?.On("alert", response =>
        //        {
        //            try
        //            {
        //                var result = response.GetValue(); 
        //                Console.WriteLine(result);
        //            } 
        //            catch (Exception exception)
        //            {
        //                Methods.DisplayReportResultTrack(exception);
        //            }
        //        });
        //    } 
        //    catch (Exception ex)
        //    {
        //        Methods.DisplayReportResultTrack(ex);
        //    }
        //}

    }
}