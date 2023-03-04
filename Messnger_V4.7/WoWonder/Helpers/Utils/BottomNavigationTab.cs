using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Q.Rorbin.Badgeview;
using WoWonder.Activities.DefaultUser;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Ads;
using WoWonderClient.Classes.Call;

namespace WoWonder.Helpers.Utils
{
    public class BottomNavigationTab : Java.Lang.Object, View.IOnClickListener
    {
        private readonly ChatTabbedMainActivity MainActivity;

        private LinearLayout Tab, ChatLayout, StoryLayout, CallLayout, MoreLayout, AddLayout;
        private ImageView ImageChat, ImageStory, ImageCall, ImageMore;
       
        private ImageView FloatingActionImageView;

        private readonly Color UnSelectColor = Color.ParseColor("#dddddd");

        public BottomNavigationTab(ChatTabbedMainActivity activity)
        {
            try
            {
                MainActivity = activity;

                Initialize();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize()
        {
            try
            {
                Tab = MainActivity.FindViewById<LinearLayout>(Resource.Id.bottomnavigationtab);
                Tab.BackgroundTintList = ColorStateList.ValueOf(WoWonderTools.IsTabDark() ? Color.Black : Color.White);

                ChatLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llChat);
                ImageChat = MainActivity.FindViewById<ImageView>(Resource.Id.ivChat);

                StoryLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llStory);
                ImageStory = MainActivity.FindViewById<ImageView>(Resource.Id.ivStory);

                AddLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llAdd);
                FloatingActionImageView = MainActivity.FindViewById<ImageView>(Resource.Id.Image);
                 
                CallLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llCall);
                ImageCall = MainActivity.FindViewById<ImageView>(Resource.Id.ivCall);
                          
                MoreLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llMore);
                ImageMore = MainActivity.FindViewById<ImageView>(Resource.Id.ivMore);

                ChatLayout?.SetOnClickListener(this);
                StoryLayout?.SetOnClickListener(this);
                AddLayout?.SetOnClickListener(this);
                CallLayout?.SetOnClickListener(this);
                MoreLayout?.SetOnClickListener(this);

                float weightSum = 5;

                var videoCall = WoWonderTools.CheckAllowedCall(TypeCall.Video);
                var audioCall = WoWonderTools.CheckAllowedCall(TypeCall.Audio);
                
                if (!videoCall && !audioCall)
                {
                    CallLayout.Visibility = ViewStates.Gone;
                    weightSum--;
                }

                Tab.WeightSum = weightSum;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SelectItem(int index)
        {
            try
            {
                ImageChat.SetColorFilter(UnSelectColor);
                ImageStory.SetColorFilter(UnSelectColor);
                ImageCall.SetColorFilter(UnSelectColor);
                ImageMore.SetColorFilter(UnSelectColor);

                switch (index)
                {
                    //Chat
                    case 0:
                        {
                            ImageChat.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                            MainActivity.ViewPager.SetCurrentItem(0, false);

                            AdsGoogle.Ad_Interstitial(MainActivity);
                            break;
                        }
                    //Story
                    case 1:
                        ImageStory.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        MainActivity.ViewPager.SetCurrentItem(1, false);

                        AdsGoogle.Ad_AppOpenManager(MainActivity);
                        break;
                    //Call
                    case 2:
                        ImageCall.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        MainActivity.ViewPager.SetCurrentItem(2, false);

                        AdsGoogle.Ad_RewardedVideo(MainActivity);

                        MainActivity.InAppReview();
                        break;
                    //More
                    case 3:
                       
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowBadge(int id, int count, bool showBadge)
        {
            try
            {
                if (id < 0) return;

                if (showBadge)
                {
                    if (id == 0)
                        ShowOrHideBadgeViewIcon(MainActivity, ChatLayout, count, true);
                    else if (id == 1)
                        ShowOrHideBadgeViewIcon(MainActivity, StoryLayout, count, true);
                    else if (id == 2)
                        ShowOrHideBadgeViewIcon(MainActivity, CallLayout, count, true);
                    else if (id == 3) ShowOrHideBadgeViewIcon(MainActivity, MoreLayout, count, true);
                }
                else if (id == 0)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, ChatLayout);
                }
                else if (id == 1)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, StoryLayout);
                }
                else if (id == 2)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, CallLayout);
                }
                else if (id == 3)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, MoreLayout);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private QBadgeView BadgeStory, BadgeCall;
        private void ShowOrHideBadgeViewIcon(Activity mainActivity, LinearLayout linearLayoutImage, int count = 0, bool show = false)
        {
            try
            {
                mainActivity?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (linearLayoutImage != null)
                            {
                                if (linearLayoutImage.Id == StoryLayout.Id)
                                {
                                    BadgeStory = new QBadgeView(mainActivity);
                                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                    BadgeStory.BindTarget(linearLayoutImage);
                                    BadgeStory.SetBadgeNumber(count);
                                    BadgeStory.SetBadgeGravity(gravity);
                                    BadgeStory.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                    BadgeStory.SetGravityOffset(10, true);
                                }
                                else if (linearLayoutImage.Id == CallLayout.Id)
                                {
                                    BadgeCall = new QBadgeView(mainActivity);
                                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                    BadgeCall.BindTarget(linearLayoutImage);
                                    BadgeCall.SetBadgeNumber(count);
                                    BadgeCall.SetBadgeGravity(gravity);
                                    BadgeCall.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                    BadgeCall.SetGravityOffset(10, true);
                                }
                            }
                        }
                        else
                        {
                            if (linearLayoutImage?.Id == StoryLayout.Id)
                                BadgeStory?.BindTarget(linearLayoutImage).Hide(true);
                            else if (linearLayoutImage?.Id == CallLayout.Id)
                                BadgeCall?.BindTarget(linearLayoutImage).Hide(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == ChatLayout.Id)
                {
                    SelectItem(0);
                }
                else if (v.Id == StoryLayout?.Id)
                {
                    SelectItem(1);

                   //ShowBadge(1, 0, false);

                } 
                else if (v.Id == CallLayout?.Id)
                {
                    SelectItem(2);
                     
                    //ShowBadge(2, 0, false); 
                }
                else if (v.Id == MoreLayout?.Id)
                {
                    var intent = new Intent(MainActivity, typeof(SettingsActivity));
                    MainActivity.StartActivity(intent);
                }
                else if (v.Id == AddLayout?.Id)
                {
                    var intent = new Intent(MainActivity, typeof(AddNewChatActivity));
                    MainActivity.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}