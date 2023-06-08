using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public class EntryPopupPage : PopupPage
    {
        public EntryPopupPage()
        {
            var entry = new Entry { Keyboard = Keyboard.Numeric };
            Content = entry;
        }
    }
}
