using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.ChatWindow
{
    public class OptionsItemMessageBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private ChatWindowActivity ChatWindowContext;
        private GroupChatWindowActivity GroupChatWindowContext;
        private PageChatWindowActivity PageChatWindowContext;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        private ImageView MImgButtonOne, MImgButtonTwo, MImgButtonThree, MImgButtonFour, MImgButtonFive, MImgButtonSix;
        private LinearLayout ReactLayout;

        private string Page;
        private Holders.TypeClick Type;
        private MessageDataExtra DataMessageObject;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetChatWindowLayout, container, false);
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

        private void InitComponent(View view)
        {
            try
            {
                ReactLayout = view.FindViewById<LinearLayout>(Resource.Id.reactLayout);
                MImgButtonOne = view.FindViewById<ImageView>(Resource.Id.imgButtonOne);
                MImgButtonTwo = view.FindViewById<ImageView>(Resource.Id.imgButtonTwo);
                MImgButtonThree = view.FindViewById<ImageView>(Resource.Id.imgButtonThree);
                MImgButtonFour = view.FindViewById<ImageView>(Resource.Id.imgButtonFour);
                MImgButtonFive = view.FindViewById<ImageView>(Resource.Id.imgButtonFive);
                MImgButtonSix = view.FindViewById<ImageView>(Resource.Id.imgButtonSix);

                MImgButtonOne.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Like);
                MImgButtonTwo.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Love);
                MImgButtonThree.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.HaHa);
                MImgButtonFour.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Wow);
                MImgButtonFive.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Sad);
                MImgButtonSix.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Angry);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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

        private void MAdapterOnItemClick(object sender, ItemOptionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (item?.Id == "1") //Copy
                    {
                        CopyLayoutOnClick();
                    }
                    else if (item?.Id == "2") //MessageInfo
                    {
                        MessageInfoLayoutOnClick();
                    }
                    else if (item?.Id == "3") //DeleteMessage
                    {
                        DeleteMessageLayoutOnClick();
                    }
                    else if (item?.Id == "4") //Reply
                    {
                        ReplyLayoutOnClick();
                    }
                    else if (item?.Id == "5") //Forward
                    {
                        ForwardLayoutOnClick();
                    }
                    else if (item?.Id == "6") //Pin
                    {
                        PinLayoutOnClick();
                    }
                    else if (item?.Id == "7") //Favorite
                    {
                        FavoriteLayoutOnClick();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event

        private string LastReact;
        private void ImgButtonOnClick(object sender, EventArgs e, string reactText)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (LastReact == reactText)
                    return;

                LastReact = reactText;

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("down.mp3");
                        break;
                }

                int resReact = Resource.Drawable.emoji_like;
                DataMessageObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();
                string react = "";
                if (reactText == ReactConstants.Like)
                {
                    DataMessageObject.Reaction.Type = "1";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";

                    resReact = Resource.Drawable.emoji_like;
                }
                else if (reactText == ReactConstants.Love)
                {
                    DataMessageObject.Reaction.Type = "2";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";

                    resReact = Resource.Drawable.emoji_love;
                }
                else if (reactText == ReactConstants.HaHa)
                {
                    DataMessageObject.Reaction.Type = "3";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";

                    resReact = Resource.Drawable.emoji_haha;
                }
                else if (reactText == ReactConstants.Wow)
                {
                    DataMessageObject.Reaction.Type = "4";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";

                    resReact = Resource.Drawable.emoji_wow;
                }
                else if (reactText == ReactConstants.Sad)
                {
                    DataMessageObject.Reaction.Type = "5";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";

                    resReact = Resource.Drawable.emoji_sad;
                }
                else if (reactText == ReactConstants.Angry)
                {
                    DataMessageObject.Reaction.Type = "6";
                    react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";

                    resReact = Resource.Drawable.emoji_angry;
                }

                if (!string.IsNullOrEmpty(react))
                {
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    }
                }

                Console.WriteLine(resReact);

                DataMessageObject.Reaction.IsReacted = true;
                DataMessageObject.Reaction.Count++;

                if (Page == "ChatWindow")
                {
                    var dataClass = ChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        ChatWindowContext?.MAdapter.NotifyItemChanged(ChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }
                else if (Page == "GroupChatWindow")
                {
                    var dataClass = GroupChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        GroupChatWindowContext?.MAdapter.NotifyItemChanged(GroupChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }
                else if (Page == "PageChatWindow")
                {
                    var dataClass = PageChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        PageChatWindowContext?.MAdapter.NotifyItemChanged(PageChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CopyLayoutOnClick()
        {
            try
            {
                if (DataMessageObject != null && !string.IsNullOrEmpty(DataMessageObject.Text))
                {
                    Methods.CopyToClipboard(Activity, DataMessageObject.Text);
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FavoriteLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.StarMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PinLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.PinMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ForwardLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.ForwardItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.ForwardItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.ForwardItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ReplyLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.ReplyItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.ReplyItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.ReplyItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DeleteMessageLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.DeleteMessageItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.DeleteMessageItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.DeleteMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageInfoLayoutOnClick()
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.MessageInfoItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.MessageInfoItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.MessageInfoItems();

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
                Page = Arguments?.GetString("Page") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow
                if (Page == "ChatWindow")
                    ChatWindowContext = ChatWindowActivity.GetInstance();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext = GroupChatWindowActivity.GetInstance();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext = PageChatWindowActivity.GetInstance();

                Type = JsonConvert.DeserializeObject<Holders.TypeClick>(Arguments?.GetString("Type") ?? "");
                DataMessageObject = JsonConvert.DeserializeObject<MessageDataExtra>(Arguments?.GetString("ItemObject") ?? "");
                if (DataMessageObject != null)
                {
                    if (Type == Holders.TypeClick.Text)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "1",
                            Text = GetText(Resource.String.Lbl_Copy),
                            Icon = Resource.Drawable.icon_copy_vector,
                        });
                    }

                    if (DataMessageObject.Position == "right")
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "2",
                            Text = GetText(Resource.String.Lbl_MessageInfo),
                            Icon = Resource.Drawable.icon_info_vector,
                        });

                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "3",
                            Text = GetText(Resource.String.Lbl_DeleteMessage),
                            Icon = Resource.Drawable.icon_delete_vector,
                        });
                    }

                    if (AppSettings.EnableReplyMessageSystem)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "4",
                            Text = GetText(Resource.String.Lbl_Reply),
                            Icon = Resource.Drawable.icon_chat_reply,
                        });
                    }

                    if (AppSettings.EnableForwardMessageSystem)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "5",
                            Text = GetText(Resource.String.Lbl_Forward),
                            Icon = Resource.Drawable.icon_forward_vector,
                        });
                    }

                    if (AppSettings.EnablePinMessageSystem && Page == "ChatWindow")
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "6",
                            Text = DataMessageObject.Pin == "yes" ? GetText(Resource.String.Lbl_UnPin) : GetText(Resource.String.Lbl_Pin),
                            Icon = Resource.Drawable.icon_pin_vector,
                        });
                    }

                    if (AppSettings.EnableFavoriteMessageSystem && Page == "ChatWindow")
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "7",
                            Text = DataMessageObject.Fav == "yes" ? GetText(Resource.String.Lbl_UnFavorite) : GetText(Resource.String.Lbl_Favorite),
                            Icon = Resource.Drawable.icon_star_filled_vector,
                        });
                    }

                    if (AppSettings.EnableReactionMessageSystem && DataMessageObject.Position == "left")
                    {
                        ReactLayout.Visibility = ViewStates.Visible;
                        switch (AppSettings.ReactionTheme)
                        {
                            case PostButtonSystem.ReactionDefault:
                                Glide.With(Activity).Load(Resource.Drawable.gif_like).Apply(new RequestOptions()).Into(MImgButtonOne);
                                Glide.With(Activity).Load(Resource.Drawable.gif_love).Apply(new RequestOptions()).Into(MImgButtonTwo);
                                Glide.With(Activity).Load(Resource.Drawable.gif_haha).Apply(new RequestOptions()).Into(MImgButtonThree);
                                Glide.With(Activity).Load(Resource.Drawable.gif_wow).Apply(new RequestOptions()).Into(MImgButtonFour);
                                Glide.With(Activity).Load(Resource.Drawable.gif_sad).Apply(new RequestOptions()).Into(MImgButtonFive);
                                Glide.With(Activity).Load(Resource.Drawable.gif_angry).Apply(new RequestOptions()).Into(MImgButtonSix);
                                break;
                            case PostButtonSystem.ReactionSubShine:
                            default:
                                Glide.With(Activity).Load(Resource.Drawable.like).Apply(new RequestOptions().FitCenter()).Into(MImgButtonOne);
                                Glide.With(Activity).Load(Resource.Drawable.love).Apply(new RequestOptions().FitCenter()).Into(MImgButtonTwo);
                                Glide.With(Activity).Load(Resource.Drawable.haha).Apply(new RequestOptions().FitCenter()).Into(MImgButtonThree);
                                Glide.With(Activity).Load(Resource.Drawable.wow).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFour);
                                Glide.With(Activity).Load(Resource.Drawable.sad).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFive);
                                Glide.With(Activity).Load(Resource.Drawable.angry).Apply(new RequestOptions().FitCenter()).Into(MImgButtonSix);
                                break;
                        }
                    }
                    else
                    {
                        ReactLayout.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}