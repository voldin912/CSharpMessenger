//Use DoughouzChecker last version 4.2 to 
//build your own certifcate 
//For Full Documention 
//https://doughouzlight.com/?onepage-docs=wowonder-android
//CopyRight DoughouzLight
//For the accuracy of the icon and logo, please use this website " https://appicon.co/ " and add images according to size in folders " mipmap " 
 
using SocketIOClient.Transport;
using WoWonder.Helpers.Model;
using WoWonderClient;

namespace WoWonder
{
    internal static class AppSettings
    {
        public static readonly string TripleDesAppServiceProvider = "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";
    
        //Main Settings >>>>>
        //********************************************************* 
        public static string Version = "4.7";
        public static readonly string ApplicationName = "Chaturnal Messenger";
        public static readonly string DatabaseName = "chaturnalmessenger";

        // Friend system = 0 , follow system = 1
        public static readonly int ConnectivitySystem = 1;
         
        public static readonly InitializeWoWonder.ConnectionType ConnectionTypeChat = InitializeWoWonder.ConnectionType.Socket; 
        public static readonly string PortSocketServer = "449"; 
        public static readonly TransportProtocol Transport = TransportProtocol.Polling;

        //Main Colors >>
        //*********************************************************
        public static readonly string MainColor = "#C83747";
        public static readonly string StoryReadColor = "#808080";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Set Language User on site from phone 
        public static readonly bool SetLangUser = true;  

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "c2f8035b-7058-4c41-af06-6eb179f7fde5";

        //Error Report Mode
        //*********************************************************
        public static readonly bool SetApisReportMode = false;

        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static readonly bool AutoCodeTimeZone = true;
        public static readonly string CodeTimeZone = "UTC";

        public static readonly bool EnableRegisterSystem = true; 

        //Set Theme Full Screen App
        //*********************************************************
        public static readonly bool EnableFullScreenApp = false;
         
        public static readonly bool ShowSettingsUpdateManagerApp = false; 

        public static readonly bool ShowSettingsRateApp = true;  
        public static readonly int ShowRateAppCount = 5;

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //********************************************************* 
        public static readonly ShowAds ShowAds = ShowAds.AllUsers;

        //Three times after entering the ad is displayed
        public static readonly int ShowAdInterstitialCount = 0;
        public static readonly int ShowAdRewardedVideoCount = 0;
        public static int ShowAdNativeCount = 0;
        public static readonly int ShowAdAppOpenCount = 0;
         
        public static readonly bool ShowAdMobBanner = false;
        public static readonly bool ShowAdMobInterstitial = false;
        public static readonly bool ShowAdMobRewardVideo = false;
        public static readonly bool ShowAdMobNative = false;
        public static readonly bool ShowAdMobAppOpen = false; 
        public static readonly bool ShowAdMobRewardedInterstitial = false; 

        public static readonly string AdInterstitialKey = "ca-app-pub-5135691635931982/3442638218";
        public static readonly string AdRewardVideoKey = "ca-app-pub-5135691635931982/3814173301";
        public static readonly string AdAdMobNativeKey = "ca-app-pub-5135691635931982/9452678647";
        public static readonly string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/3836425196";  
        public static readonly string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/7476900652";
         
        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static readonly bool ShowFbBannerAds = false;
        public static readonly bool ShowFbInterstitialAds = false;
        public static readonly bool ShowFbRewardVideoAds = false;
        public static readonly bool ShowFbNativeAds = false;

        //YOUR_PLACEMENT_ID
        public static readonly string AdsFbBannerKey = "250485588986218_554026418632132";
        public static readonly string AdsFbInterstitialKey = "250485588986218_554026125298828";
        public static readonly string AdsFbRewardVideoKey = "250485588986218_554072818627492";
        public static readonly string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static readonly bool ShowColonyBannerAds = false;  
        public static readonly bool ShowColonyInterstitialAds = false; 
        public static readonly bool ShowColonyRewardAds = false; 

        public static readonly string AdsColonyAppId = "appff22269a7a0a4be8aa"; 
        public static readonly string AdsColonyBannerId = "vz85ed7ae2d631414fbd";  
        public static readonly string AdsColonyInterstitialId = "vz39712462b8634df4a8"; 
        public static readonly string AdsColonyRewardedId = "vz32ceec7a84aa4d719a"; 
        //********************************************************* 

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml .. 
        //Google >> ../Properties/AndroidManifest.xml .. line 37
        //*********************************************************
        public static readonly bool EnableSmartLockForPasswords = false; 
         
        public static readonly bool ShowFacebookLogin = false;
        public static readonly bool ShowGoogleLogin = false;

        public static readonly string ClientId = "81603239249-i35mh67livs9gifrlv83e47dd3ohamsg.apps.googleusercontent.com";

        //Chat Window Activity >>
        //*********************************************************
        //if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        //Just replace it with this 5 lines of code
        /*
         <uses-permission android:name="android.permission.READ_CONTACTS" />
         <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" /> 
         */
        public static readonly bool ShowButtonContact = true;
        public static readonly bool InvitationSystem = true;  //Invite friends section
        /////////////////////////////////////
        
        public static readonly ChatTheme ChatTheme = ChatTheme.Default; //#new

        public static readonly GalleryIntentSystem GalleryIntentSystem = GalleryIntentSystem.Matisse; //#new
         
        public static readonly bool ShowButtonCamera = true;
        public static readonly bool ShowButtonImage = true;
        public static readonly bool ShowButtonVideo = true;
        public static readonly bool ShowButtonAttachFile = true;
        public static readonly bool ShowButtonColor = true;
        public static readonly bool ShowButtonStickers = true;
        public static readonly bool ShowButtonMusic = true;
        public static readonly bool ShowButtonGif = true;
        public static readonly bool ShowButtonLocation = true;
         
        public static readonly bool OpenVideoFromApp = true;
        public static readonly bool OpenImageFromApp = true;
        
        //Record Sound Style & Text 
        public static readonly bool ShowButtonRecordSound = true;

        // Options List Message
        public static readonly bool EnableReplyMessageSystem = true; 
        public static readonly bool EnableForwardMessageSystem = true; 
        public static readonly bool EnableFavoriteMessageSystem = true; 
        public static readonly bool EnablePinMessageSystem = true; 
        public static readonly bool EnableReactionMessageSystem = true; 

        public static readonly bool ShowNotificationWithUpload = true;  

        public static readonly bool AllowDownloadMedia = true; 
        public static readonly bool EnableFitchOgLink = true; 

        public static readonly bool EnableSuggestionMessage = true; 

        /// <summary>
        /// https://dashboard.stipop.io/
        /// you can get api key from here https://prnt.sc/26ofmq9
        /// </summary>
        public static readonly string StickersApikey = "0a441b19287cad752e87f6072bb914c0"; 
         
        //List Chat >>
        //*********************************************************
        public static readonly bool EnableChatPage = false; //>> Next update 
        public static readonly bool EnableChatGroup = true;
         
        // Options List Chat
        public static readonly bool EnableChatArchive = true; 
        public static readonly bool EnableChatPin = true;  
        public static readonly bool EnableChatMute = true; 
        public static readonly bool EnableChatMakeAsRead = true; 
         
        // Story >>
        //*********************************************************
        //Set a story duration >> Sec
        public static readonly long StoryImageDuration = 7;
        public static readonly long StoryVideoDuration = 30;  

        /// <summary>
        /// If it is false, it will appear only for the specified time in the value of the StoryVideoDuration
        /// </summary>
        public static readonly bool ShowFullVideo = false;  

        public static readonly bool EnableStorySeenList = true; 
        public static readonly bool EnableReplyStory  = true;

        /// <summary>
        /// you can edit video using FFMPEG 
        /// </summary>
        public static readonly bool EnableVideoEditor = true;  
        public static readonly bool EnableVideoCompress = false;

        //*********************************************************
        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;
        public static readonly string CurrencyIconStatic = "$";
        public static readonly string CurrencyCodeStatic = "USD";

        // Video/Audio Call Settings >>
        //*********************************************************
        public static readonly EnableCall EnableCall = EnableCall.AudioAndVideo;  //#new
        public static readonly SystemCall UseLibrary = SystemCall.Agora;  

        // Walkthrough Settings >>
        //*********************************************************
        public static readonly bool ShowWalkTroutPage = true;
         
        // Register Settings >>
        //*********************************************************
        public static readonly bool ShowGenderOnRegister = true;
         
        //Last Messages Page >>
        //*********************************************************
        public static readonly bool ShowOnlineOfflineMessage = true;

        public static readonly int RefreshAppAPiSeconds = 3500; // 3 Seconds
        public static readonly int MessageRequestSpeed = 4000; // 3 Seconds
         
        public static readonly ToastTheme ToastTheme = ToastTheme.Custom; 
        public static readonly ColorMessageTheme ColorMessageTheme = ColorMessageTheme.Default; 
        public static readonly PostButtonSystem ReactionTheme = PostButtonSystem.ReactionDefault;
          
        //Bypass Web Errors 
        //*********************************************************
        public static readonly bool TurnTrustFailureOnWebException = true;
        public static readonly bool TurnSecurityProtocolType3072On = true;

        public static readonly bool ShowTextWithSpace = false;

        public static TabTheme SetTabDarkTheme = TabTheme.Light;

        public static readonly bool ShowSuggestedUsersOnRegister = true; 

        //Settings Page >> General Account
        public static readonly bool ShowSettingsAccount = true;
        public static readonly bool ShowSettingsPassword = true;
        public static readonly bool ShowSettingsBlockedUsers = true;
        public static readonly bool ShowSettingsDeleteAccount = true;
        public static readonly bool ShowSettingsTwoFactor = true;
        public static readonly bool ShowSettingsManageSessions = true;
        public static readonly bool ShowSettingsWallpaper  = true; 
        public static readonly bool ShowSettingsFingerprintLock = true; 

        //Options chat heads (Bubbles) 
        //*********************************************************
        public static readonly bool ShowChatHeads = true; 

        //Always , Hide , FullScreen
        public static readonly string DisplayModeSettings = "Always";

        //Default , Left  , Right , Nearest , Fix , Thrown
        public static readonly string MoveDirectionSettings = "Right";

        //Circle , Rectangle
        public static readonly string ShapeSettings = "Circle";

        // Last position
        public static readonly bool IsUseLastPosition = true;

        public static readonly int AvatarPostSize = 60; 
        public static readonly int ImagePostSize = 200; 
    }
}