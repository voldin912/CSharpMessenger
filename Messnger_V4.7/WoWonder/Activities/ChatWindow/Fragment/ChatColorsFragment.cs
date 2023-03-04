using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Requests;

namespace WoWonder.Activities.ChatWindow.Fragment
{
    public class ChatColorsFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private CircleButton ResetButton;
        private string UserId;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                var contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.ChatColorsFragment, container, false);
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

                UserId = Arguments?.GetString("userid");

                ResetButton = view.FindViewById<CircleButton>(Resource.Id.resetbutton);
                ResetButton.Click += ResetButtonClick;

                var colorButton1 = view.FindViewById<CircleButton>(Resource.Id.colorbutton1);
                var colorButton2 = view.FindViewById<CircleButton>(Resource.Id.colorbutton2);
                var colorButton3 = view.FindViewById<CircleButton>(Resource.Id.colorbutton3);
                var colorButton4 = view.FindViewById<CircleButton>(Resource.Id.colorbutton4);
                var colorButton5 = view.FindViewById<CircleButton>(Resource.Id.colorbutton5);
                var colorButton6 = view.FindViewById<CircleButton>(Resource.Id.colorbutton6);
                var colorButton7 = view.FindViewById<CircleButton>(Resource.Id.colorbutton7);
                var colorButton8 = view.FindViewById<CircleButton>(Resource.Id.colorbutton8);
                var colorButton9 = view.FindViewById<CircleButton>(Resource.Id.colorbutton9);
                var colorButton10 = view.FindViewById<CircleButton>(Resource.Id.colorbutton10);
                var colorButton11 = view.FindViewById<CircleButton>(Resource.Id.colorbutton11);
                var colorButton12 = view.FindViewById<CircleButton>(Resource.Id.colorbutton12);
                var colorButton13 = view.FindViewById<CircleButton>(Resource.Id.colorbutton13);
                var colorButton14 = view.FindViewById<CircleButton>(Resource.Id.colorbutton14);

                colorButton1.Click += SetColorButton_Click;
                colorButton2.Click += SetColorButton_Click;
                colorButton3.Click += SetColorButton_Click;
                colorButton4.Click += SetColorButton_Click;
                colorButton5.Click += SetColorButton_Click;
                colorButton6.Click += SetColorButton_Click;
                colorButton7.Click += SetColorButton_Click;
                colorButton8.Click += SetColorButton_Click;
                colorButton9.Click += SetColorButton_Click;
                colorButton10.Click += SetColorButton_Click;
                colorButton11.Click += SetColorButton_Click;
                colorButton12.Click += SetColorButton_Click;
                colorButton13.Click += SetColorButton_Click;
                colorButton14.Click += SetColorButton_Click;
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

        #endregion

        #region Event

        private void ResetButtonClick(object sender, EventArgs e)
        {
            try
            {
                var chatWindow = ChatWindowActivity.GetInstance();
                if (chatWindow != null)
                {
                    var color = AppSettings.MainColor;
                    ChatWindowActivity.MainChatColor = color;
                    chatWindow.SetTheme(color);
                    chatWindow.SetThemeView(true);

                    var list = chatWindow.MAdapter.DifferList.Where(a => a.MesData?.Position == "right").ToList();
                    foreach (var msg in list)
                    {
                        msg.MesData.ChatColor = color;
                    }
                    chatWindow.MAdapter.NotifyDataSetChanged();

                    //Update All data Messages to database
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Insert_Or_Replace_MessagesTable(chatWindow.MAdapter.DifferList);

                    var dataUser = ChatTabbedMainActivity.GetInstance()?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == UserId);
                    if (dataUser != null)
                    {
                        dataUser.LastChat.ChatColor = color;
                        dataUser.LastChat.LastMessage.LastMessageClass.ChatColor = color;
                    }

                    if (chatWindow.DataUser != null)
                    {
                        chatWindow.DataUser.LastMessage.LastMessageClass.ChatColor = color;
                        chatWindow.DataUser.ChatColor = color;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ChangeChatColorAsync(UserId, color) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetColorButton_Click(object sender, EventArgs e)
        {
            try
            {
                CircleButton btn = (CircleButton)sender;
                string color = (string)btn.Tag;

                var chatWindow = ChatWindowActivity.GetInstance();
                if (chatWindow != null)
                {
                    ChatWindowActivity.MainChatColor = color;
                    chatWindow.SetTheme(color);
                    chatWindow.SetThemeView(true);

                    var list = chatWindow.MAdapter.DifferList.Where(a => a.MesData?.Position == "right").ToList();
                    foreach (var msg in list)
                    {
                        msg.MesData.ChatColor = color;
                    }
                    chatWindow.MAdapter.NotifyDataSetChanged();

                    //Update All data Messages to database
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Insert_Or_Replace_MessagesTable(chatWindow.MAdapter.DifferList);

                    var dataUser = ChatTabbedMainActivity.GetInstance()?.ChatTab?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == UserId);
                    if (dataUser != null)
                    {
                        dataUser.LastChat.ChatColor = color;
                        dataUser.LastChat.LastMessage.LastMessageClass.ChatColor = color;
                    }

                    if (chatWindow.DataUser != null)
                    {
                        chatWindow.DataUser.LastMessage.LastMessageClass.ChatColor = color;
                        chatWindow.DataUser.ChatColor = color;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ChangeChatColorAsync(UserId, color) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}