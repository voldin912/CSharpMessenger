using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.Identity;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.SocialLogins;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnSuccessListener, IOnFailureListener
    {
        #region Variables Basic

        private LinearLayout RegisterButton;
        private AppCompatButton MButtonViewSignIn;
        private EditText UsernameEditText, PasswordEditText;
        private LinearLayout FbLoginButton, GoogleSignInButton;
        private TextView TopTittle, ForgetPass; 
        private ProgressBar ProgressBar; 
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public static GoogleSignInClient MGoogleSignInClient;
        private ImageView EyesIcon;
        private string TimeZone = AppSettings.CodeTimeZone;
        private bool IsActiveUser = true;

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
                SetContentView(Resource.Layout.LoginLayout);

                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnTrustFailureOnWebException, AppSettings.SetApisReportMode);

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();
                InitBackPressed();

                //OneSignal Notification  
                //====================================== 
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.GetSettings_Api(this), GetTimezone});

                if (AppSettings.EnableSmartLockForPasswords)
                    BuildClients();
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
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        protected override void OnDestroy()
        {
            try
            {
                ProfileTracker?.StopTracking();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                //Get values 
                UsernameEditText = FindViewById<EditText>(Resource.Id.usernamefield);
                PasswordEditText = FindViewById<EditText>(Resource.Id.passwordfield);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                EyesIcon = FindViewById<ImageView>(Resource.Id.imageShowPass);
                EyesIcon.Click += EyesIconOnClick;
                EyesIcon.Tag = "hide";

                MButtonViewSignIn = FindViewById<AppCompatButton>(Resource.Id.loginButton);
                RegisterButton = FindViewById<LinearLayout>(Resource.Id.SignLayout);
                 
                TopTittle = FindViewById<TextView>(Resource.Id.titile);
                TopTittle.Text = GetText(Resource.String.Lbl_LoginTo) + " " + AppSettings.ApplicationName;

                ForgetPass = FindViewById<TextView>(Resource.Id.forgetpassButton);
               
                ProgressBar.Visibility = ViewStates.Invisible;

                if (!AppSettings.EnableRegisterSystem)
                    RegisterButton.Visibility = ViewStates.Gone;
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
                    ForgetPass.Click += ForgetPassOnClick;  
                    RegisterButton.Click += RegisterButton_Click;
                    MButtonViewSignIn.Click += BtnLoginOnClick;

                }
                else
                {
                    //Close Event
                    ForgetPass.Click -= ForgetPassOnClick;  
                    RegisterButton.Click -= RegisterButton_Click;
                    MButtonViewSignIn.Click -= BtnLoginOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this); 
                    LoginButton loginButton = new LoginButton(this);
                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.ll_fblogin);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.Click += FbLoginButtonOnClick;

                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    loginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    LoginManager.Instance.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.ll_fblogin);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.ll_Googlelogin);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.ll_Googlelogin);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FbLoginButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                LoginManager.Instance.LogInWithReadPermissions(this, new List<string>
                {
                    "email",
                    "public_profile"
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Login With Facebook
        private void ProfileTrackerOnMOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //var FbFirstName = e.MProfile.FirstName;
                    //var FbLastName = e.MProfile.LastName;
                    //var FbName = e.MProfile.Name;
                    //var FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }
         
        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Events

        private void EyesIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (EyesIcon.Tag?.ToString() == "hide")
                {
                    EyesIcon.SetImageResource(Resource.Drawable.icon_eye_show_vector);
                    EyesIcon.Tag = "show";
                    PasswordEditText.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
                    PasswordEditText.SetSelection(PasswordEditText.Text.Length);
                }
                else
                {
                    EyesIcon.SetImageResource(Resource.Drawable.icon_eye_vector);
                    EyesIcon.Tag = "hide";
                    PasswordEditText.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
                    PasswordEditText.SetSelection(PasswordEditText.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Click Button Login
        private async void BtnLoginOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
                else
                {
                    if (!string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        HideKeyboard();

                        ProgressBar.Visibility = ViewStates.Visible;
                        MButtonViewSignIn.Visibility = ViewStates.Gone;

                        if (string.IsNullOrEmpty(TimeZone))
                           await GetTimezone();

                        await AuthApi(UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text);
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task AuthApi(string email, string password)
        {
            var (apiStatus, respond) = await RequestsAsync.Auth.AuthAsync(email, password, TimeZone, UserDetails.DeviceId);
            switch (apiStatus)
            {
                case 200 when respond is AuthObject auth:
                    {
                        var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                        if (emailValidation == "1")
                        {
                            IsActiveUser = await CheckIsActiveUser(auth.UserId);
                        }

                        if (IsActiveUser)
                        { 
                            SetDataLogin(auth);

                            if (AppSettings.ShowWalkTroutPage)
                            {
                                Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                                newIntent.PutExtra("class", "login");
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                            FinishAffinity();
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                case 200:
                    {
                        if (respond is AuthMessageObject messageObject)
                        {
                            UserDetails.Username = UsernameEditText.Text;
                            UserDetails.FullName = UsernameEditText.Text;
                            UserDetails.Password = PasswordEditText.Text;
                            UserDetails.UserId = messageObject.UserId;
                            UserDetails.Status = "Active";
                            UserDetails.Email = UsernameEditText.Text;
                             
                            Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                            newIntent.PutExtra("TypeCode", "TwoFactor");
                            StartActivity(newIntent);
                        }

                        break;
                    }
                case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            switch (errorId)
                            {
                                case "3":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "4":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "5":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_5), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        break;
                    }
                case 404:
                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    break;
            } 
        }

        private void PrivacyOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                string url = InitializeWoWonder.WebsiteUrl + "/terms/privacy-policy";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TermsOfServiceOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                string url = InitializeWoWonder.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void ForgetPassOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity)); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this ,typeof(RegisterActivity)));
                OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom,Resource.Animation.abc_shrink_fade_out_from_bottom);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Main_LoginPage_Click(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result
         
        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                // Logins Facebook
                MFbCallManager?.OnActivityResult(requestCode, (int)resultCode, data);
                base.OnActivityResult(requestCode, resultCode, data);

                //Log.Debug("Login_Activity", "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);

                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else if (requestCode == RcCredentialsHint)
                {
                    if (resultCode == Result.Ok)
                    {
                        SignInCredential credential = OneTapClient.GetSignInCredentialFromIntent(data);
                        string idToken = credential.GoogleIdToken;
                        string username = credential.Id;
                        string password = credential.Password;

                        if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                        {
                            // Email/password account
                            Console.WriteLine("Signed in as {0}", credential.Id);

                            //send api auth  
                            HideKeyboard();

                            ProgressBar.Visibility = ViewStates.Visible;
                            MButtonViewSignIn.Visibility = ViewStates.Gone;

                            await AuthApi(credential.Id, credential.Password);
                        }
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                        break;
                    case 16248:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                MButtonViewSignIn.Visibility = ViewStates.Gone;

                //SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is AuthObject auth)
                        {
                            //if (AppSettings.EnableSmartLockForPasswords && !string.IsNullOrEmpty(json?.ToString()))
                            //{
                            //    var data = json.ToString();
                            //    var result = JsonConvert.DeserializeObject<FacebookResult>(data);

                            //    //FbEmail = result.Email; 
                            //}

                            SetDataLogin(auth);

                            if (AppSettings.ShowWalkTroutPage)
                            {
                                Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                                newIntent.PutExtra("class", "login");
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                            }

                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;

                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else if (apiStatus == 404)
                    {
                        var error = respond.ToString();
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                    }

                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #region Google
         
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    MButtonViewSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                     
                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        switch (apiStatus)
                        {
                            case 200:
                            {
                                if (respond is AuthObject auth)
                                {  
                                    SetDataLogin(auth);

                                    if (AppSettings.ShowWalkTroutPage)
                                    {
                                        Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                                        newIntent.PutExtra("class", "login");
                                        StartActivity(newIntent);
                                    }
                                    else
                                    {
                                        StartActivity(new Intent(this, typeof(ChatTabbedMainActivity)));
                                    }
                                    Finish();
                                }

                                break;
                            }
                            case 400:
                            {
                                if (respond is ErrorObject error)
                                {
                                    var errorText = error.Error.ErrorText;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                }

                                break;
                            }
                            case 404:
                            {
                                var error = respond.ToString();
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                                break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible; 
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #endregion

        #region Cross App Authentication

        private static readonly int RcCredentialsHint = 10;

        private ISignInClient OneTapClient;
        private BeginSignInRequest SignInRequest;

        public async void BuildClients()
        {
            try
            {
                OneTapClient = Identity.GetSignInClient(this);
                SignInRequest = new BeginSignInRequest.Builder()
                    .SetPasswordRequestOptions(new BeginSignInRequest.PasswordRequestOptions.Builder().SetSupported(true).Build())
                    .SetGoogleIdTokenRequestOptions(new BeginSignInRequest.GoogleIdTokenRequestOptions.Builder()
                        .SetSupported(true)
                        // Your server's client ID, not your Android client ID.
                        .SetServerClientId(AppSettings.ClientId)
                        // true : Only show accounts previously used to sign in.
                        // false : Show all accounts on the device.
                        .SetFilterByAuthorizedAccounts(false)
                        .Build())
                    // true : Automatically sign in when exactly one credential is retrieved.
                    //.SetAutoSelectEnabled(true)
                    .Build();

                await OneTapClient.BeginSignIn(SignInRequest).AddOnSuccessListener(this).AddOnFailureListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        void IOnSuccessListener.OnSuccess(Object result)
        {
            try
            {
                if (result is BeginSignInResult results)
                {
                    StartIntentSenderForResult(results.PendingIntent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                }
            }
            catch (IntentSender.SendIntentException e)
            {
                Console.WriteLine("Couldn't start One Tap UI: " + e.LocalizedMessage);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }
            
        #endregion

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task GetTimezone()
        {
            try
            {
                TimeZone = await ApiRequest.GetTimeZoneAsync();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.IsActiveUserAsync(userId);
                switch (apiStatus)
                {
                    case 200 when respond is MessageObject auth:
                        Console.WriteLine(auth);
                        return true;
                    case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            switch (errorId)
                            {
                                case "5":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "4":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_UserNotFound), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        break;
                    }
                    case 404:
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        break;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }
         
        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                ApiRequest.SwitchAccount(this);

                Current.AccessToken = UserDetails.AccessToken = auth.AccessToken;

                UserDetails.Username = UsernameEditText.Text;
                UserDetails.FullName = UsernameEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Active";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = UsernameEditText.Text;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UsernameEditText.Text,
                    Password = UsernameEditText.Text,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                }; 

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                 
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnActivityResult(Object result)
        {
            try
            {
                // handle the result in your own way 
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}