using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Droid.CustomControls;
using System;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace StyexFleetManagement.Droid.CustomControls
{
        public class BorderlessEntryRenderer : EntryRenderer
        {
            protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
            {
                base.OnElementChanged(e);
                if (e.OldElement == null)
                {
                    Control.Background = null;
                }
            }
        }

}
