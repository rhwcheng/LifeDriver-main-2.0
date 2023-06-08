using System;
using System.Linq;
using UIKit;
using StyexFleetManagement.iOS;
using StyexFleetManagement.CustomControls;

[assembly: Xamarin.Forms.Dependency(typeof(EntryPopupLoader))]
namespace StyexFleetManagement.iOS
{
    public class EntryPopupLoader : IEntryPopupLoader
    {
        public void ShowPopup(EntryPopup popup)
        {
            var alert = new UIAlertView
            {
                Title = popup.Title,
                Message = popup.Text,
                AlertViewStyle = UIAlertViewStyle.PlainTextInput
            };
            foreach (var b in popup.Buttons)
                alert.AddButton(b);

            alert.Clicked += (s, args) =>
            {
                popup.OnPopupClosed(new EntryPopupClosedArgs
                {
                    Button = popup.Buttons.ElementAt(Convert.ToInt32(args.ButtonIndex)),
                    Text = alert.GetTextField(0).Text
                });
            };
            alert.Show();
        }
    }
}