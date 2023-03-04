using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using System;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Controller
{
    public class PermissionsController
    {
        private readonly Activity Context;

        public PermissionsController(Activity activity)
        {
            try
            {
                Context = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool CheckPermissionStorage(string type = "")
        {
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            //{
            //    return Android.OS.Environment.IsExternalStorageManager;
            //}

            if ((int)Build.VERSION.SdkInt < 23)
                return true;

            //below android 13
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {

                if (type == "image" &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaImages) == Permission.Granted)
                    return true;

                if (type == "video" &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaVideo) == Permission.Granted)
                    return true;

                if (type == "audio" &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaAudio) == Permission.Granted)
                    return true;

                if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaImages) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaVideo) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadMediaAudio) == Permission.Granted)
                    return true;
            }

            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                return true;

            return false;
        }

        /// <summary>
        /// Handle Permission Request
        /// </summary>
        /// <param name="idPermissions"> 100 >> Storage
        /// 101 >> ReadContacts - ReadPhoneNumbers
        /// 102 >> RecordAudio
        /// 103 >> Camera
        /// 104 >> SendSms
        /// 105 >> Location
        /// 106 >> GetAccounts - UseCredentials >> Social Logins
        /// 107 >> AccessWifiState - Internet
        /// 108 >> Storage - Camera</param> 
        /// <param name="type">image,video,audio</param>
        public void RequestPermission(int idPermissions, string type = "")
        {
            switch ((int)Build.VERSION.SdkInt)
            {
                // Check if we're running on Android 5.0 or higher
                case >= 23:
                    switch (idPermissions)
                    {
                        case 100:
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                            {
                                //below android 13
                                switch (type)
                                {
                                    case "image":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaImages,
                                        }, 100);
                                        break;
                                    case "video":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaVideo,
                                        }, 100);
                                        break;
                                    case "audio":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaAudio,
                                        }, 100);
                                        break;
                                    default:
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaImages,
                                            Manifest.Permission.ReadMediaVideo,
                                            Manifest.Permission.ReadMediaAudio,
                                        }, 100);
                                        break;
                                }
                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 100);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 100);
                            }
                            break;

                        case 101:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.ReadContacts,
                                Manifest.Permission.ReadPhoneNumbers
                            }, 101);
                            break;

                        case 102:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.RecordAudio,
                                Manifest.Permission.ModifyAudioSettings
                            }, 102);
                            break;

                        case 103:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.Camera,
                                Manifest.Permission.RecordAudio,
                                Manifest.Permission.ModifyAudioSettings }, 103);
                            break;

                        case 104:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.SendSms,
                                Manifest.Permission.BroadcastSms
                            }, 104);
                            break;

                        case 105:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.AccessFineLocation,
                                Manifest.Permission.AccessCoarseLocation
                            }, 105);
                            break;

                        case 106:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.GetAccounts,
                                Manifest.Permission.UseCredentials
                            }, 106);
                            break;

                        case 107:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.AccessWifiState,
                                Manifest.Permission.Internet
                            }, 107);
                            break;
                        case 108:
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                            {
                                //below android 13
                                switch (type)
                                {
                                    case "image":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.Camera,
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaImages,
                                        }, 108);
                                        break;
                                    case "video":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.Camera,
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaVideo,
                                        }, 108);
                                        break;
                                    case "audio":
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaAudio,
                                        }, 108);
                                        break;
                                    default://Read or Write file  
                                        ActivityCompat.RequestPermissions(Context, new[]
                                        {
                                            Manifest.Permission.Camera,
                                            Manifest.Permission.ReadExternalStorage,
                                            Manifest.Permission.WriteExternalStorage,
                                            Manifest.Permission.ManageExternalStorage,
                                            Manifest.Permission.AccessMediaLocation,
                                            Manifest.Permission.ReadMediaImages,
                                            Manifest.Permission.ReadMediaVideo,
                                            Manifest.Permission.ReadMediaAudio,
                                        }, 108);
                                        break;
                                }

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 108);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 108);
                            }
                            break;
                        case 109:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.ReadProfile,
                                Manifest.Permission.ReadPhoneNumbers,
                                Manifest.Permission.ReadPhoneState
                            }, 109);
                            break;
                        case 110:
                            ActivityCompat.RequestPermissions(Context, new[]
                            {
                                Manifest.Permission.WakeLock
                            }, 110);
                            break;
                        case 111:

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                            {
                                //below android 11
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                    Manifest.Permission.RecordAudio,
                                    Manifest.Permission.ModifyAudioSettings,
                                    Manifest.Permission.ReadMediaImages,
                                    Manifest.Permission.ReadMediaVideo,
                                    Manifest.Permission.ReadMediaAudio,
                                    Manifest.Permission.PostNotifications
                                }, 111);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                    Manifest.Permission.RecordAudio,
                                    Manifest.Permission.ModifyAudioSettings
                                }, 111);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                ActivityCompat.RequestPermissions(Context, new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                    Manifest.Permission.RecordAudio,
                                    Manifest.Permission.ModifyAudioSettings,
                                    Manifest.Permission.PostNotifications
                                }, 111);
                            }
                            break;
                    }

                    break;
            }
        }

        //private void OpenDialog()
        //{
        //    try
        //    {
        //        var dialog = new MaterialAlertDialogBuilder(Context);
        //        dialog.SetTitle(Resource.String.Lbl_Security);
        //        dialog.SetMessage(Resource.String.Lbl_AllowGrantPermission);
        //        dialog.SetPositiveButton(Resource.String.Lbl_GrantPermission,(materialDialog, action) =>
        //        {
        //            try
        //            {
        //                Intent intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
        //                intent.AddCategory(Intent.CategoryDefault);
        //                intent.SetData(Android.Net.Uri.Parse("package:" + Context.PackageName));
        //                Context.StartActivity(intent);
        //            }
        //            catch (Exception e)
        //            {
        //                 Methods.DisplayReportResultTrack(e); 
        //                Intent intent = new Intent();
        //                intent.SetAction(Settings.ActionManageAllFilesAccessPermission);
        //                Context.StartActivity(intent);
        //            }
        //        });
        //        dialog.SetNegativeButton(Resource.String.Lbl_Cancel,new MaterialDialogUtils());
        //       
        //        dialog.Show();
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}
    }
}