using System;
using Android.App;
using Android.Graphics;
using Android.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Fonts
{
    public static class FontUtils
    { 
        //Changes the TextView To IconFrameWork Fonts
        public static void SetTextViewIcon(FontsIconFrameWork type, TextView textViewUi, string iconUnicode)
        {
            try
            {
                if (type == FontsIconFrameWork.IonIcons)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeSolid)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-solid-900.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeRegular)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-regular-400.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeBrands)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-brands-400.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeLight)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-light-300.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }  
                else if (type == FontsIconFrameWork.FontAwesomeDuotone)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-duotone-900.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeThin)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-thin-100.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
                else if (type == FontsIconFrameWork.FontAwesomeV4Compatibility)
                {
                    var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-v4compatibility.ttf");
                    textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                    if (!string.IsNullOrEmpty(iconUnicode))
                        textViewUi.Text = iconUnicode;
                    else
                        textViewUi.Text = textViewUi.Text;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Set_TextViewIcon Function ERROR " + e);
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}