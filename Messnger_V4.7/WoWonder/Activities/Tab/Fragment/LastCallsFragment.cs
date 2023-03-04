using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Adcolony.Sdk;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.Call.Agora;
using WoWonder.Activities.Call.Twilio;
using WoWonder.Activities.Tab.Adapter;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab.Fragment
{
    public class LastCallsFragment : AndroidX.Fragment.App.Fragment, IDialogListCallBack
    {
        #region Variables Basic

        public LastCallsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public Classes.CallUser DataUser;
        private AdView BannerAd;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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
                SetRecyclerViewAdapters();
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
                Get_CallUser();
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
                BannerAd?.Destroy();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                LinearLayout adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                if (AppSettings.ShowFbBannerAds)
                    BannerAd = AdsFacebook.InitAdView(Activity, adContainer, MRecycler);
                else
                    AdsColony.InitBannerAd(Activity, adContainer, AdColonyAdSize.Banner, MRecycler);
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
                MAdapter = new LastCallsAdapter(Activity) { MCallUser = new ObservableCollection<Classes.CallUser>() };
                MAdapter.CallClick += MAdapterOnCallClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events 

        private void MAdapterOnCallClick(object sender, LastCallsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DataUser = item;

                        var videoCall = WoWonderTools.CheckAllowedCall(TypeCall.Video);
                        var audioCall = WoWonderTools.CheckAllowedCall(TypeCall.Audio);

                        if (videoCall && audioCall)
                        {
                            var arrayAdapter = new List<string>();
                            var dialogList = new MaterialAlertDialogBuilder(Context);

                            arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Voice_call));
                            arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Video_call));

                            dialogList.SetTitle(GetText(Resource.String.Lbl_Call));
                            //dialogList.SetMessage(GetText(Resource.String.Lbl_Select_Type_Call));
                            dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                            dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close),new MaterialDialogUtils());
                            
                            dialogList.Show();
                        }
                        else if (audioCall == false && videoCall)  // Video Call On
                        {
                            if ((int)Build.VERSION.SdkInt >= 23)
                            {
                                if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                                {
                                    StartCall(TypeCall.Video, item);
                                }
                                else
                                {
                                    new PermissionsController(Activity).RequestPermission(103);
                                }
                            }
                            else
                            {
                                StartCall(TypeCall.Video, item);
                            }
                        }
                        else if (audioCall && !videoCall) // // Audio Call On
                        {
                            if ((int)Build.VERSION.SdkInt >= 23)
                            {
                                if (Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                                {
                                    StartCall(TypeCall.Audio, item);
                                }
                                else
                                {
                                    new PermissionsController(Activity).RequestPermission(102);
                                }
                            }
                            else
                            {
                                StartCall(TypeCall.Audio, item);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Call

        private void Get_CallUser()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.Get_CallUserList();
                if (localList?.Count > 0)
                {
                    var countList = MAdapter.MCallUser.Count;
                    if (countList > 0)
                    {
                        foreach (var item in from item in localList let check = MAdapter.MCallUser.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            MAdapter.MCallUser.Insert(0, item);
                        }
                    }
                    else
                    {
                        MAdapter.MCallUser = new ObservableCollection<Classes.CallUser>(localList.OrderBy(a => a.Id));
                    }

                    MAdapter.NotifyDataSetChanged();
                }


                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.MCallUser.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCall);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == Context.GetText(Resource.String.Lbl_Voice_call))
                {
                    if ((int)Build.VERSION.SdkInt >= 23)
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                        {
                            StartCall(TypeCall.Video, DataUser);
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(103);
                        }
                    }
                    else
                    {
                        StartCall(TypeCall.Video, DataUser);
                    }
                }
                else if (itemString == Context.GetText(Resource.String.Lbl_Video_call))
                {
                    if ((int)Build.VERSION.SdkInt >= 23)
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                        {
                            StartCall(TypeCall.Video, DataUser);
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(103);
                        }
                    }
                    else
                    {
                        StartCall(TypeCall.Video, DataUser);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Call

        public void StartCall(TypeCall type, Classes.CallUser dataUser)
        {
            try
            {
                Intent intentCall = null;
                if (type == TypeCall.Audio)
                {
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentCall = new Intent(Activity, typeof(AgoraAudioCallActivity));
                        intentCall.PutExtra("type", "Agora_audio_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentCall = new Intent(Activity, typeof(TwilioAudioCallActivity));
                        intentCall.PutExtra("type", "Twilio_audio_calling_start");
                    }
                }
                else if (type == TypeCall.Video)
                {
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentCall = new Intent(Activity, typeof(AgoraVideoCallActivity));
                        intentCall.PutExtra("type", "Agora_video_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentCall = new Intent(Activity, typeof(TwilioVideoCallActivity));
                        intentCall.PutExtra("type", "Twilio_video_calling_start");
                    }
                }

                if (dataUser != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = dataUser.UserId,
                        Avatar = dataUser.Avatar,
                        Name = dataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentCall?.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));
                }

                if (intentCall != null)
                    StartActivity(intentCall);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}