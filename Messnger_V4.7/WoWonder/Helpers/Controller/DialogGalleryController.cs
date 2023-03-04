using Android;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using Com.Canhub.Cropper;
using Java.IO;
using System;
using WoWonder.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace WoWonder.Helpers.Controller
{
    public class DialogGalleryController
    {
        private readonly AppCompatActivity Activity;
        private readonly IActivityResultCallback Callback;

        private ActivityResultLauncher CropImage;

        public string ImageType;

        public DialogGalleryController(AppCompatActivity activity, IActivityResultCallback callback)
        {
            try
            {
                Activity = activity;
                Callback = callback;

                InitCropImage(activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitCropImage(AppCompatActivity activity)
        {
            try
            {
                CropImage = activity.RegisterForActivityResult(new CropImageContract(), Callback);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenDialogGallery(string typeImage = "")
        {
            try
            {
                if (!WoWonderTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_Security), Activity.GetText(Resource.String.Lbl_Error_AllowedFileUpload), Activity.GetText(Resource.String.Lbl_Ok));
                    return;
                }

                ImageType = typeImage;

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();
                    var myUri = Android.Net.Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                    var option = new CropImageContractOptions(null, new CropImageOptions()
                    {
                        ImageSourceIncludeGallery = true,
                        ImageSourceIncludeCamera = true,
                        ShowIntentChooser = true,
                        ActivityBackgroundColor = Color.Black,
                        AllowFlipping = true,
                        AllowRotation = true,
                        Guidelines = CropImageView.Guidelines.On,
                        MaxZoom = 4,
                        OutputCompressFormat = Bitmap.CompressFormat.Jpeg,
                    });
                    //Open Image 
                    CropImage.Launch(option);
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage("image") && ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        var myUri = Android.Net.Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                        var option = new CropImageContractOptions(null, new CropImageOptions()
                        {
                            ImageSourceIncludeGallery = true,
                            ImageSourceIncludeCamera = true,
                            ShowIntentChooser = true,
                            ActivityBackgroundColor = Color.Black,
                            AllowFlipping = true,
                            AllowRotation = true,
                            Guidelines = CropImageView.Guidelines.On,
                            MaxZoom = 4,
                            OutputCompressFormat = Bitmap.CompressFormat.Jpeg,
                        });
                        //Open Image 
                        CropImage.Launch(option);
                    }
                    else
                    {
                        new PermissionsController(Activity).RequestPermission(108, "image");
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenCropDialog(Uri myUri)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    var option = new CropImageContractOptions(myUri, new CropImageOptions()
                    {
                        ImageSourceIncludeGallery = false,
                        ImageSourceIncludeCamera = false,
                        ShowIntentChooser = false,
                        ActivityBackgroundColor = Color.Black,
                        AllowFlipping = true,
                        AllowRotation = true,
                        Guidelines = CropImageView.Guidelines.On,
                        MaxZoom = 4,
                        OutputCompressFormat = Bitmap.CompressFormat.Jpeg,

                    });
                    //Open Image 
                    CropImage.Launch(option);
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage("image") && ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        var option = new CropImageContractOptions(myUri, new CropImageOptions()
                        {
                            ImageSourceIncludeGallery = false,
                            ImageSourceIncludeCamera = false,
                            ShowIntentChooser = false,
                            ActivityBackgroundColor = Color.Black,
                            AllowFlipping = true,
                            AllowRotation = true,
                            Guidelines = CropImageView.Guidelines.On,
                            MaxZoom = 4,
                            OutputCompressFormat = Bitmap.CompressFormat.Jpeg,
                        });
                        //Open Image 
                        CropImage.Launch(option);
                    }
                    else
                    {
                        new PermissionsController(Activity).RequestPermission(108, "image");
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
