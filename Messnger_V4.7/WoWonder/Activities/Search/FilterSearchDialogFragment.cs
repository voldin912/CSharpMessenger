using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using Google.Android.Material.Slider;
using WoWonder.Adapters;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using Exception = System.Exception;

namespace WoWonder.Activities.Search
{
    public class FilterSearchDialogFragment : BottomSheetDialogFragment, IDialogListCallBack, IDialogInterfaceOnShowListener, IBaseOnChangeListener
    {
        #region Variables Basic

        private SearchActivity ContextSearch;
        private TextView IconBack, TxtAge, LocationPlace, LocationMoreIcon, TxtVerified, VerifiedMoreIcon, TxtStatus, StatusMoreIcon, TxtProfilePicture, ProfilePictureMoreIcon;
        private AppCompatButton BtnApply;
        private Switch AgeSwitch;
        private RecyclerView GenderRecycler;
        private GendersAdapter GenderAdapter;
        private RangeSlider AgeSeekBar;
        private LinearLayout SeekbarLayout;
        private RelativeLayout LayoutLocation, LayoutVerified, LayoutStatus, LayoutProfilePicture;
        private string Gender = "all", Status = "all", Verified = "all", Location = "all", ProfilePicture = "all", TypeDialog = "";
        private int AgeMin = 10, AgeMax = 70;
        private bool SwitchState;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your fragment here
                ContextSearch = (SearchActivity)Activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                //View view = inflater.Inflate(Resource.Layout.ButtomSheetSearchFilter, container, false);
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.ButtomSheetSearchFilter, container, false);
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

                Dialog.SetOnShowListener(this);

                IconBack.Click += IconBackOnClick;

                LayoutLocation.Click += LayoutLocationOnClick;
                LayoutVerified.Click += LayoutVerifiedOnClick;
                LayoutStatus.Click += LayoutStatusOnClick;
                LayoutProfilePicture.Click += LayoutProfilePictureOnClick;
                AgeSwitch.CheckedChange += AgeSwitchOnCheckedChange;

                BtnApply.Click += BtnApplyOnClick;

                AgeSwitch.Checked = false;

                GetFilter();
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
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                GenderRecycler = view.FindViewById<RecyclerView>(Resource.Id.GenderRecyler);

                TxtAge = view.FindViewById<TextView>(Resource.Id.AgeTextView);

                LocationPlace = view.FindViewById<TextView>(Resource.Id.LocationPlace);
                LocationMoreIcon = view.FindViewById<TextView>(Resource.Id.LocationMoreIcon);

                TxtVerified = view.FindViewById<TextView>(Resource.Id.textVerified);
                VerifiedMoreIcon = view.FindViewById<TextView>(Resource.Id.VerifiedMoreIcon);

                TxtStatus = view.FindViewById<TextView>(Resource.Id.textStatus);
                StatusMoreIcon = view.FindViewById<TextView>(Resource.Id.StatusMoreIcon);

                TxtProfilePicture = view.FindViewById<TextView>(Resource.Id.txtProfilePicture);
                ProfilePictureMoreIcon = view.FindViewById<TextView>(Resource.Id.ProfilePictureMoreIcon);

                LayoutLocation = view.FindViewById<RelativeLayout>(Resource.Id.LayoutLocation);
                LayoutVerified = view.FindViewById<RelativeLayout>(Resource.Id.LayoutVerified);
                LayoutStatus = view.FindViewById<RelativeLayout>(Resource.Id.LayoutStatus);
                LayoutProfilePicture = view.FindViewById<RelativeLayout>(Resource.Id.LayoutProfilePicture);

                SeekbarLayout = view.FindViewById<LinearLayout>(Resource.Id.seekbarLayout);
                AgeSeekBar = view.FindViewById<RangeSlider>(Resource.Id.seekbar);
                AgeSwitch = view.FindViewById<Switch>(Resource.Id.togglebutton);

                BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                AgeSeekBar.TrackHeight = 30;
                AgeSeekBar.SetThumbStrokeColorResource(Resource.Color.gnt_white);
                AgeSeekBar.ThumbStrokeWidth = 5f;
                AgeSeekBar.ValueFrom = 10;
                AgeSeekBar.ValueTo = 70;

                try
                {
                    var method = Java.Lang.Class.ForName("com.google.android.material.slider.BaseSlider").GetDeclaredMethods().FirstOrDefault(x => x.Name == "addOnChangeListener");
                    method?.Invoke(AgeSeekBar, this); // this is implementing IBaseOnChangeListener
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropright : IonIconsFonts.IosArrowDropleft);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LocationMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropleft : IonIconsFonts.IosArrowDropright);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, VerifiedMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropleft : IonIconsFonts.IosArrowDropright);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, StatusMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropleft : IonIconsFonts.IosArrowDropright);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ProfilePictureMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropleft : IonIconsFonts.IosArrowDropright);
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
                GenderRecycler.HasFixedSize = true;
                GenderRecycler.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                GenderAdapter = new GendersAdapter(Activity)
                {
                    GenderList = new ObservableCollection<Classes.Gender>()
                };
                GenderRecycler.SetAdapter(GenderAdapter);
                GenderRecycler.NestedScrollingEnabled = false;
                GenderAdapter.NotifyDataSetChanged();
                GenderRecycler.Visibility = ViewStates.Visible;
                GenderAdapter.ItemClick += GenderAdapterOnItemClick;

                GenderAdapter.GenderList.Add(new Classes.Gender
                {
                    GenderId = "all",
                    GenderName = Activity.GetText(Resource.String.Lbl_All),
                    GenderColor = AppSettings.MainColor,
                    GenderSelect = false
                });

                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    foreach (var (key, value) in ListUtils.SettingsSiteList?.Genders)
                    {
                        GenderAdapter.GenderList.Add(new Classes.Gender
                        {
                            GenderId = key,
                            GenderName = value,
                            GenderColor = WoWonderTools.IsTabDark() ? "#ffffff" : "#444444",
                            GenderSelect = false
                        });
                    }
                }
                else
                {
                    GenderAdapter.GenderList.Add(new Classes.Gender
                    {
                        GenderId = "male",
                        GenderName = Activity.GetText(Resource.String.Radio_Male),
                        GenderColor = WoWonderTools.IsTabDark() ? "#ffffff" : "#444444",
                        GenderSelect = false
                    });
                    GenderAdapter.GenderList.Add(new Classes.Gender
                    {
                        GenderId = "female",
                        GenderName = Activity.GetText(Resource.String.Radio_Female),
                        GenderColor = WoWonderTools.IsTabDark() ? "#ffffff" : "#444444",
                        GenderSelect = false
                    });
                }

                GenderAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Event

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.SearchGender = Gender;
                UserDetails.SearchCountry = Location;
                UserDetails.SearchStatus = Status;
                UserDetails.SearchVerified = Verified;
                UserDetails.SearchProfilePicture = ProfilePicture;
                UserDetails.SearchFilterByAge = SwitchState ? "on" : "off";
                UserDetails.SearchAgeFrom = AgeMin.ToString();
                UserDetails.SearchAgeTo = AgeMax.ToString();

                var dbDatabase = new SqLiteDatabase();
                var newSettingsFilter = new DataTables.SearchFilterTb
                {
                    Gender = Gender,
                    Country = Location,
                    Status = Status,
                    Verified = Verified,
                    ProfilePicture = ProfilePicture,
                    FilterByAge = SwitchState ? "on" : "off",
                    AgeFrom = AgeMin.ToString(),
                    AgeTo = AgeMax.ToString(),
                };
                dbDatabase.InsertOrUpdate_SearchFilter(newSettingsFilter);

                ContextSearch.Search();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Gender
        private void GenderAdapterOnItemClick(object sender, GendersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = GenderAdapter.GetItem(position);
                    if (item != null)
                    {
                        var check = GenderAdapter.GenderList.Where(a => a.GenderSelect).ToList();
                        if (check.Count > 0)
                            foreach (var all in check)
                                all.GenderSelect = false;

                        item.GenderSelect = true;
                        GenderAdapter.NotifyDataSetChanged();

                        Gender = item.GenderId;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }



        //Profile Picture
        private void LayoutProfilePictureOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "ProfilePicture";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

                arrayAdapter.Add(GetText(Resource.String.Lbl_All));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Yes));
                arrayAdapter.Add(GetText(Resource.String.Lbl_No));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Profile_Picture));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Status
        private void LayoutStatusOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Status";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

                arrayAdapter.Add(GetText(Resource.String.Lbl_All));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Offline));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Online));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Status));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Verified
        private void LayoutVerifiedOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Verified";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

                arrayAdapter.Add(GetText(Resource.String.Lbl_All));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Verified));
                arrayAdapter.Add(GetText(Resource.String.Lbl_UnVerified));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Verified));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Location
        private void LayoutLocationOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Location";

                var countriesArray = WoWonderTools.GetCountryList(Activity);

                var dialogList = new MaterialAlertDialogBuilder(Context);

                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();
                arrayAdapter.Insert(0, GetText(Resource.String.Lbl_All));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Location));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Age Switch
        private void AgeSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    //Switch On
                    SwitchState = true;
                    SeekbarLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    //Switch Off
                    SwitchState = false;
                    SeekbarLayout.Visibility = ViewStates.Gone;
                }

                TxtAge.Text = GetString(Resource.String.Lbl_Age);

                UserDetails.SearchFilterByAge = SwitchState ? "on" : "off";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetFilter()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var data = dbDatabase.GetSearchFilterById();
                if (data != null)
                {
                    UserDetails.SearchGender = Gender = data.Gender;
                    UserDetails.SearchCountry = Location = data.Country;
                    UserDetails.SearchStatus = Status = data.Status;
                    UserDetails.SearchVerified = Verified = data.Verified;
                    UserDetails.SearchProfilePicture = ProfilePicture = data.ProfilePicture;
                    UserDetails.SearchFilterByAge = data.FilterByAge;
                    UserDetails.SearchAgeFrom = data.AgeFrom;
                    UserDetails.SearchAgeTo = data.AgeTo;

                    SwitchState = data.FilterByAge == "on";
                    AgeMin = Convert.ToInt32(data.AgeFrom);
                    AgeMax = Convert.ToInt32(data.AgeTo);

                    AgeSeekBar.ValueFrom = AgeMin;
                    AgeSeekBar.ValueTo = AgeMax;

                    TxtStatus.Text = Status switch
                    {
                        "all" => GetText(Resource.String.Lbl_All),
                        "off" => GetText(Resource.String.Lbl_Offline),
                        "on" => GetText(Resource.String.Lbl_Online),
                        _ => GetText(Resource.String.Lbl_All)
                    };

                    TxtVerified.Text = Verified switch
                    {
                        "all" => GetText(Resource.String.Lbl_All),
                        "off" => GetText(Resource.String.Lbl_UnVerified),
                        "on" => GetText(Resource.String.Lbl_Verified),
                        _ => GetText(Resource.String.Lbl_All)
                    };

                    TxtProfilePicture.Text = ProfilePicture switch
                    {
                        "all" => GetText(Resource.String.Lbl_All),
                        "yes" => GetText(Resource.String.Lbl_Yes),
                        "no" => GetText(Resource.String.Lbl_No),
                        _ => GetText(Resource.String.Lbl_All)
                    };

                    var countriesArray = WoWonderTools.GetCountryList(Activity);
                    if (Location == "all")
                    {
                        LocationPlace.Text = GetText(Resource.String.Lbl_All);
                    }
                    else
                    {
                        bool success = int.TryParse(Location, out var number);
                        if (success)
                        {
                            var check = countriesArray.FirstOrDefault(a => a.Key == number.ToString()).Value;
                            if (!string.IsNullOrEmpty(check))
                            {
                                LocationPlace.Text = check;
                            }
                        }
                        else
                        {
                            LocationPlace.Text = GetText(Resource.String.Lbl_All);
                        }
                    }

                    if (SwitchState)
                    {
                        AgeSwitch.Checked = true;
                        SeekbarLayout.Visibility = ViewStates.Visible;
                        TxtAge.Text = GetString(Resource.String.Lbl_Age) + " " + AgeMin + " - " + AgeMax;
                    }
                    else
                    {
                        AgeSwitch.Checked = false;
                        SeekbarLayout.Visibility = ViewStates.Invisible;
                        TxtAge.Text = GetString(Resource.String.Lbl_Age);
                    }

                    //////////////////////////// Gender ////////////////////////////// 
                    var check1 = GenderAdapter.GenderList.Where(a => a.GenderSelect).ToList();
                    if (check1.Count > 0)
                        foreach (var all in check1)
                            all.GenderSelect = false;

                    var check2 = GenderAdapter.GenderList.FirstOrDefault(a => a.GenderId == data.Gender);
                    if (check2 != null)
                    {
                        check2.GenderSelect = true;
                        Gender = check2.GenderId;
                    }

                    GenderAdapter.NotifyDataSetChanged();
                }
                else
                {
                    UserDetails.SearchGender = "all";
                    UserDetails.SearchCountry = "all";
                    UserDetails.SearchStatus = "all";
                    UserDetails.SearchVerified = "all";
                    UserDetails.SearchProfilePicture = "all";
                    UserDetails.SearchFilterByAge = "off";
                    UserDetails.SearchAgeFrom = "10";
                    UserDetails.SearchAgeTo = "70";

                    Gender = UserDetails.SearchGender;
                    Location = UserDetails.SearchCountry;
                    Status = UserDetails.SearchStatus;
                    Verified = UserDetails.SearchVerified;
                    ProfilePicture = UserDetails.SearchProfilePicture;
                    SwitchState = UserDetails.SearchFilterByAge == "on";
                    AgeMin = Convert.ToInt32(UserDetails.SearchAgeFrom);
                    AgeMax = Convert.ToInt32(UserDetails.SearchAgeTo);

                    var check = GenderAdapter.GenderList.FirstOrDefault(a => a.GenderId == "all");
                    if (check != null)
                    {
                        check.GenderSelect = true;
                        Gender = check.GenderId;

                        GenderAdapter.NotifyDataSetChanged();
                    }

                    var newSettingsFilter = new DataTables.SearchFilterTb
                    {
                        Gender = UserDetails.SearchGender,
                        Country = UserDetails.SearchCountry,
                        Status = UserDetails.SearchStatus,
                        Verified = UserDetails.SearchVerified,
                        ProfilePicture = UserDetails.SearchProfilePicture,
                        FilterByAge = UserDetails.SearchFilterByAge,
                        AgeFrom = UserDetails.SearchAgeFrom,
                        AgeTo = UserDetails.SearchAgeTo,
                    };
                    dbDatabase.InsertOrUpdate_SearchFilter(newSettingsFilter);
                }


            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;

                switch (TypeDialog)
                {
                    case "Status":
                        {
                            TxtStatus.Text = text;
                            if (text == GetText(Resource.String.Lbl_All))
                                Status = "all";
                            else if (text == GetText(Resource.String.Lbl_Offline))
                                Status = "off";
                            else if (text == GetText(Resource.String.Lbl_Online))
                                Status = "on";
                            break;
                        }
                    case "Verified":
                        {
                            TxtVerified.Text = text;
                            if (text == GetText(Resource.String.Lbl_All))
                                Verified = "all";
                            else if (text == GetText(Resource.String.Lbl_UnVerified))
                                Verified = "off";
                            else if (text == GetText(Resource.String.Lbl_Verified))
                                Verified = "on";
                            break;
                        }
                    case "ProfilePicture":
                        {
                            TxtProfilePicture.Text = text;
                            if (text == GetText(Resource.String.Lbl_All))
                                ProfilePicture = "all";
                            else if (text == GetText(Resource.String.Lbl_Yes))
                                ProfilePicture = "yes";
                            else if (text == GetText(Resource.String.Lbl_No))
                                ProfilePicture = "no";
                            break;
                        }
                    case "Location":
                        {
                            if (text == GetText(Resource.String.Lbl_All))
                            {
                                Location = "all";
                            }
                            else
                            {
                                var countriesArray = WoWonderTools.GetCountryList(Activity);
                                var check = countriesArray.FirstOrDefault(a => a.Value == text).Key;
                                if (check != null)
                                {
                                    Location = check;
                                }
                            }

                            LocationPlace.Text = itemString;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OnShow(IDialogInterface dialog)
        {
            try
            {
                var d = dialog as BottomSheetDialog;
                var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
                var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
                var layoutParams = bottomSheet.LayoutParameters;

                if (layoutParams != null)
                    layoutParams.Height = Resources.DisplayMetrics.HeightPixels;
                bottomSheet.LayoutParameters = layoutParams;
                bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Age 
        public void OnValueChange(Java.Lang.Object p0, float p1, bool p2)
        {
            try
            {
                var sliderValues = AgeSeekBar.Values;

                AgeMin = sliderValues.FirstOrDefault()?.IntValue() ?? 0;
                AgeMax = sliderValues.LastOrDefault()?.IntValue() ?? 0;

                TxtAge.Text = GetString(Resource.String.Lbl_Age) + " " + AgeMin + " - " + AgeMax;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}