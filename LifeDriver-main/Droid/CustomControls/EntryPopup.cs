using Android.App;
using Android.Widget;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Droid.CustomControls;
using Xamarin.Forms;

[assembly: Dependency(typeof(EntryPopupLoader))]
namespace StyexFleetManagement.Droid.CustomControls
{
    public class EntryPopupLoader : IEntryPopupLoader
    {
        public void ShowPopup(EntryPopup popup)
        {
            var alert = new AlertDialog.Builder(Forms.Context);

            var edit = new EditText(Forms.Context) { Text = popup.Text, InputType = Android.Text.InputTypes.NumberFlagDecimal };
            alert.SetView(edit);

            alert.SetTitle(popup.Title);

            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
                popup.OnPopupClosed(new EntryPopupClosedArgs
                {
                    Button = "OK",
                    Text = edit.Text
                });
            });

            alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                popup.OnPopupClosed(new EntryPopupClosedArgs
                {
                    Button = "Cancel",
                    Text = edit.Text
                });
            });
            alert.Show();
        }
    }
}